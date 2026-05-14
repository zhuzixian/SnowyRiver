using SkiaSharp;
using System.Globalization;

namespace SnowyRiver.Documents.Converter.Engines.Charts;

/// <summary>
/// 基于 SkiaSharp 的轻量图表渲染器。把 <see cref="ChartData"/> 绘制成 PNG 字节。
/// 完全免费、不依赖任何商业组件或系统 OLE/Office 引擎。
/// </summary>
internal static class ChartRenderer
{
    public static byte[]? Render(ChartData data, int dpi, string fontFamily)
    {
        if (data == null || data.Series.Count == 0) return null;

        // 由原始 96dpi 锚点像素尺寸放大到目标 DPI
        double scale = Math.Max(1.0, dpi / 96.0);
        int width = (int)(data.PixelWidth * scale);
        int height = (int)(data.PixelHeight * scale);
        if (width <= 0 || height <= 0) { width = 800; height = 480; }

        using var bmp = new SKBitmap(width, height);
        using (var canvas = new SKCanvas(bmp))
        {
            canvas.Clear(SKColors.White);
            DrawChart(canvas, data, new SKRect(0, 0, width, height), (float)scale, fontFamily);
        }

        using var img = SKImage.FromBitmap(bmp);
        using var enc = img.Encode(SKEncodedImageFormat.Png, 95);
        return enc.ToArray();
    }

    private static void DrawChart(SKCanvas canvas, ChartData data, SKRect bounds, float scale, string fontFamily)
    {
        var typeface = SKTypeface.FromFamilyName(fontFamily) ?? SKTypeface.Default;

        float padding = 8 * scale;
        var area = SKRect.Inflate(bounds, -padding, -padding);

        // 1) 标题
        if (!string.IsNullOrEmpty(data.Title))
        {
            using var titlePaint = new SKPaint { Color = SKColors.Black, IsAntialias = true };
            using var titleFont = new SKFont(typeface, 14 * scale) { Edging = SKFontEdging.SubpixelAntialias };
            float titleHeight = 18 * scale;
            var w = titleFont.MeasureText(data.Title);
            canvas.DrawText(data.Title, area.MidX - w / 2, area.Top + titleHeight - 4 * scale, SKTextAlign.Left, titleFont, titlePaint);
            area = new SKRect(area.Left, area.Top + titleHeight + 4 * scale, area.Right, area.Bottom);
        }

        // 2) 图例（饼/环图也需要图例显示分类）
        var legendItems = BuildLegendItems(data);
        var (plotArea, legendArea) = SplitForLegend(area, data.Legend, legendItems.Count, scale, typeface);

        switch (data.Kind)
        {
            case ChartKind.Column:
                DrawCategoryChart(canvas, data, plotArea, scale, typeface, vertical: true);
                break;
            case ChartKind.BarHorizontal:
                DrawCategoryChart(canvas, data, plotArea, scale, typeface, vertical: false);
                break;
            case ChartKind.Line:
                DrawLineOrArea(canvas, data, plotArea, scale, typeface, fill: false);
                break;
            case ChartKind.Area:
                DrawLineOrArea(canvas, data, plotArea, scale, typeface, fill: true);
                break;
            case ChartKind.Pie:
                DrawPie(canvas, data, plotArea, scale, typeface, doughnut: false);
                break;
            case ChartKind.Doughnut:
                DrawPie(canvas, data, plotArea, scale, typeface, doughnut: true);
                break;
            case ChartKind.Scatter:
                DrawScatter(canvas, data, plotArea, scale, typeface);
                break;
        }

        if (data.Legend != LegendPos.None && legendItems.Count > 0)
            DrawLegend(canvas, legendArea, legendItems, scale, typeface, data.Legend);
    }

    // ---------- 图例 ----------

    private record LegendItem(string Text, uint ColorArgb);

    private static List<LegendItem> BuildLegendItems(ChartData data)
    {
        var list = new List<LegendItem>();
        if (data.Kind == ChartKind.Pie || data.Kind == ChartKind.Doughnut)
        {
            // 单系列饼图：每个分类一项
            var s = data.Series[0];
            for (int i = 0; i < s.Values.Count; i++)
            {
                var name = i < data.Categories.Count ? data.Categories[i] : (i + 1).ToString();
                list.Add(new LegendItem(name, OfficePalette.Get(i)));
            }
        }
        else
        {
            int idx = 0;
            foreach (var s in data.Series)
                list.Add(new LegendItem(s.Name ?? $"系列 {++idx}", s.ColorArgb ?? OfficePalette.Get(idx - 1)));
        }
        return list;
    }

    private static (SKRect plot, SKRect legend) SplitForLegend(SKRect area, LegendPos pos, int itemCount, float scale, SKTypeface tf)
    {
        if (pos == LegendPos.None || itemCount == 0) return (area, SKRect.Empty);

        using var f = new SKFont(tf, 10 * scale);
        float lineH = 14 * scale;
        float legendW = 120 * scale; // 简化：固定宽度
        float legendH = lineH * itemCount + 6 * scale;

        return pos switch
        {
            LegendPos.Right => (
                new SKRect(area.Left, area.Top, area.Right - legendW - 6 * scale, area.Bottom),
                new SKRect(area.Right - legendW, area.Top, area.Right, area.Bottom)),
            LegendPos.Left => (
                new SKRect(area.Left + legendW + 6 * scale, area.Top, area.Right, area.Bottom),
                new SKRect(area.Left, area.Top, area.Left + legendW, area.Bottom)),
            LegendPos.Top => (
                new SKRect(area.Left, area.Top + legendH + 4 * scale, area.Right, area.Bottom),
                new SKRect(area.Left, area.Top, area.Right, area.Top + legendH)),
            LegendPos.Bottom => (
                new SKRect(area.Left, area.Top, area.Right, area.Bottom - legendH - 4 * scale),
                new SKRect(area.Left, area.Bottom - legendH, area.Right, area.Bottom)),
            _ => (area, SKRect.Empty),
        };
    }

    private static void DrawLegend(SKCanvas canvas, SKRect rect, List<LegendItem> items, float scale, SKTypeface tf, LegendPos pos)
    {
        using var f = new SKFont(tf, 10 * scale) { Edging = SKFontEdging.SubpixelAntialias };
        using var textPaint = new SKPaint { Color = SKColors.Black, IsAntialias = true };
        float lineH = 14 * scale;
        float swatch = 10 * scale;

        if (pos == LegendPos.Top || pos == LegendPos.Bottom)
        {
            // 横向排列
            float x = rect.Left + 4 * scale;
            float y = rect.MidY;
            foreach (var it in items)
            {
                using var fill = new SKPaint { Color = new SKColor(it.ColorArgb), IsAntialias = true };
                canvas.DrawRect(x, y - swatch / 2, swatch, swatch, fill);
                x += swatch + 4 * scale;
                canvas.DrawText(it.Text, x, y + 4 * scale, SKTextAlign.Left, f, textPaint);
                x += f.MeasureText(it.Text) + 12 * scale;
                if (x > rect.Right) break;
            }
        }
        else
        {
            float x = rect.Left + 4 * scale;
            float y = rect.Top + lineH;
            foreach (var it in items)
            {
                using var fill = new SKPaint { Color = new SKColor(it.ColorArgb), IsAntialias = true };
                canvas.DrawRect(x, y - swatch, swatch, swatch, fill);
                canvas.DrawText(it.Text, x + swatch + 4 * scale, y, SKTextAlign.Left, f, textPaint);
                y += lineH;
                if (y > rect.Bottom) break;
            }
        }
    }

    // ---------- 通用坐标轴 ----------

    private static (double min, double max, double step) NiceRange(double min, double max, int targetTicks = 6)
    {
        if (double.IsNaN(min) || double.IsNaN(max) || min == max)
        {
            if (min == max) { min -= 1; max += 1; }
            else { min = 0; max = 1; }
        }
        if (min > 0 && max > 0) min = 0;
        if (min < 0 && max < 0) max = 0;
        double range = NiceNum(max - min, false);
        double step = NiceNum(range / (targetTicks - 1), true);
        double niceMin = Math.Floor(min / step) * step;
        double niceMax = Math.Ceiling(max / step) * step;
        return (niceMin, niceMax, step);
    }

    private static double NiceNum(double range, bool round)
    {
        double exp = Math.Floor(Math.Log10(range));
        double frac = range / Math.Pow(10, exp);
        double nice = round
            ? (frac < 1.5 ? 1 : frac < 3 ? 2 : frac < 7 ? 5 : 10)
            : (frac <= 1 ? 1 : frac <= 2 ? 2 : frac <= 5 ? 5 : 10);
        return nice * Math.Pow(10, exp);
    }

    // ---------- 柱/条 ----------

    private static void DrawCategoryChart(SKCanvas canvas, ChartData data, SKRect plot, float scale, SKTypeface tf, bool vertical)
    {
        if (data.Categories.Count == 0) return;

        double allMin = double.PositiveInfinity, allMax = double.NegativeInfinity;
        foreach (var s in data.Series)
            foreach (var v in s.Values)
                if (!double.IsNaN(v))
                {
                    if (v < allMin) allMin = v;
                    if (v > allMax) allMax = v;
                }
        if (double.IsInfinity(allMin)) { allMin = 0; allMax = 1; }
        var (vMin, vMax, vStep) = NiceRange(allMin, allMax);

        float axisLabelW = 36 * scale;
        float axisLabelH = 16 * scale;
        var inner = vertical
            ? new SKRect(plot.Left + axisLabelW, plot.Top + 4 * scale, plot.Right - 4 * scale, plot.Bottom - axisLabelH)
            : new SKRect(plot.Left + axisLabelW, plot.Top + 4 * scale, plot.Right - 4 * scale, plot.Bottom - axisLabelH);

        DrawAxes(canvas, inner, scale, tf, data.ShowGridLines, vMin, vMax, vStep, vertical);

        int catCount = data.Categories.Count;
        int serCount = data.Series.Count;

        if (vertical)
        {
            float catW = inner.Width / catCount;
            float groupPad = catW * 0.15f;
            float barW = (catW - 2 * groupPad) / serCount;

            for (int ci = 0; ci < catCount; ci++)
            {
                float xBase = inner.Left + ci * catW + groupPad;
                for (int si = 0; si < serCount; si++)
                {
                    var s = data.Series[si];
                    if (ci >= s.Values.Count) continue;
                    double v = s.Values[ci];
                    if (double.IsNaN(v)) continue;
                    float yZero = MapValueToY(0, vMin, vMax, inner);
                    float yV = MapValueToY(v, vMin, vMax, inner);
                    var rect = new SKRect(xBase + si * barW, Math.Min(yZero, yV), xBase + (si + 1) * barW - 1, Math.Max(yZero, yV));
                    using var p = new SKPaint { Color = new SKColor(s.ColorArgb ?? OfficePalette.Get(si)), IsAntialias = true };
                    canvas.DrawRect(rect, p);
                    if (data.ShowDataLabels)
                        DrawValueLabel(canvas, v, new SKPoint(rect.MidX, rect.Top - 2 * scale), scale, tf, SKTextAlign.Center);
                }
                // 类别标签
                using var f = new SKFont(tf, 10 * scale);
                using var tp = new SKPaint { Color = SKColors.Black, IsAntialias = true };
                var label = data.Categories[ci];
                var w = f.MeasureText(label);
                canvas.DrawText(label, inner.Left + (ci + 0.5f) * catW - w / 2, inner.Bottom + 12 * scale, SKTextAlign.Left, f, tp);
            }
        }
        else
        {
            float catH = inner.Height / catCount;
            float groupPad = catH * 0.15f;
            float barH = (catH - 2 * groupPad) / serCount;

            for (int ci = 0; ci < catCount; ci++)
            {
                float yBase = inner.Top + ci * catH + groupPad;
                for (int si = 0; si < serCount; si++)
                {
                    var s = data.Series[si];
                    if (ci >= s.Values.Count) continue;
                    double v = s.Values[ci];
                    if (double.IsNaN(v)) continue;
                    float xZero = MapValueToX(0, vMin, vMax, inner);
                    float xV = MapValueToX(v, vMin, vMax, inner);
                    var rect = new SKRect(Math.Min(xZero, xV), yBase + si * barH, Math.Max(xZero, xV), yBase + (si + 1) * barH - 1);
                    using var p = new SKPaint { Color = new SKColor(s.ColorArgb ?? OfficePalette.Get(si)), IsAntialias = true };
                    canvas.DrawRect(rect, p);
                    if (data.ShowDataLabels)
                        DrawValueLabel(canvas, v, new SKPoint(rect.Right + 2 * scale, rect.MidY + 3 * scale), scale, tf, SKTextAlign.Left);
                }
                using var f = new SKFont(tf, 10 * scale);
                using var tp = new SKPaint { Color = SKColors.Black, IsAntialias = true };
                var label = data.Categories[ci];
                var w = f.MeasureText(label);
                canvas.DrawText(label, inner.Left - w - 4 * scale, inner.Top + (ci + 0.5f) * catH + 4 * scale, SKTextAlign.Left, f, tp);
            }
        }
    }

    // ---------- 折线/面积 ----------

    private static void DrawLineOrArea(SKCanvas canvas, ChartData data, SKRect plot, float scale, SKTypeface tf, bool fill)
    {
        if (data.Categories.Count == 0) return;
        double allMin = double.PositiveInfinity, allMax = double.NegativeInfinity;
        foreach (var s in data.Series)
            foreach (var v in s.Values)
                if (!double.IsNaN(v))
                {
                    if (v < allMin) allMin = v;
                    if (v > allMax) allMax = v;
                }
        if (double.IsInfinity(allMin)) { allMin = 0; allMax = 1; }
        var (vMin, vMax, vStep) = NiceRange(allMin, allMax);

        float axisLabelW = 36 * scale;
        float axisLabelH = 16 * scale;
        var inner = new SKRect(plot.Left + axisLabelW, plot.Top + 4 * scale, plot.Right - 4 * scale, plot.Bottom - axisLabelH);

        DrawAxes(canvas, inner, scale, tf, data.ShowGridLines, vMin, vMax, vStep, vertical: true);

        int n = data.Categories.Count;
        if (n < 2) n = Math.Max(n, 2);
        float stepX = inner.Width / (n - 1);

        foreach (var s in data.Series)
        {
            using var stroke = new SKPaint
            {
                Color = new SKColor(s.ColorArgb ?? OfficePalette.Get(0)),
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2 * scale,
            };
            using var fillPaint = new SKPaint
            {
                Color = new SKColor((s.ColorArgb ?? 0xFF4472C4) & 0x80FFFFFF),
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
            };

            using var path = new SKPath();
            using var areaPath = new SKPath();
            float yZero = MapValueToY(Math.Max(0, vMin), vMin, vMax, inner);
            bool started = false;
            for (int i = 0; i < s.Values.Count && i < data.Categories.Count; i++)
            {
                double v = s.Values[i];
                if (double.IsNaN(v)) continue;
                float x = inner.Left + i * stepX;
                float y = MapValueToY(v, vMin, vMax, inner);
                if (!started)
                {
                    path.MoveTo(x, y);
                    areaPath.MoveTo(x, yZero);
                    areaPath.LineTo(x, y);
                    started = true;
                }
                else
                {
                    path.LineTo(x, y);
                    areaPath.LineTo(x, y);
                }
            }
            if (started)
            {
                if (fill)
                {
                    var lastX = inner.Left + (Math.Min(s.Values.Count, data.Categories.Count) - 1) * stepX;
                    areaPath.LineTo(lastX, yZero);
                    areaPath.Close();
                    canvas.DrawPath(areaPath, fillPaint);
                }
                canvas.DrawPath(path, stroke);

                // 标记点
                using var marker = new SKPaint { Color = stroke.Color, IsAntialias = true };
                for (int i = 0; i < s.Values.Count && i < data.Categories.Count; i++)
                {
                    double v = s.Values[i];
                    if (double.IsNaN(v)) continue;
                    float x = inner.Left + i * stepX;
                    float y = MapValueToY(v, vMin, vMax, inner);
                    canvas.DrawCircle(x, y, 2.5f * scale, marker);
                }
            }
        }

        // 类别标签（与柱状图一致）
        using var f = new SKFont(tf, 10 * scale);
        using var tp = new SKPaint { Color = SKColors.Black, IsAntialias = true };
        for (int ci = 0; ci < n && ci < data.Categories.Count; ci++)
        {
            float x = inner.Left + ci * stepX;
            var label = data.Categories[ci];
            var w = f.MeasureText(label);
            canvas.DrawText(label, x - w / 2, inner.Bottom + 12 * scale, SKTextAlign.Left, f, tp);
        }
    }

    // ---------- 饼/环 ----------

    private static void DrawPie(SKCanvas canvas, ChartData data, SKRect plot, float scale, SKTypeface tf, bool doughnut)
    {
        var s = data.Series[0];
        double total = 0;
        foreach (var v in s.Values) if (!double.IsNaN(v) && v > 0) total += v;
        if (total <= 0) return;

        float size = Math.Min(plot.Width, plot.Height) - 16 * scale;
        var rect = new SKRect(plot.MidX - size / 2, plot.MidY - size / 2, plot.MidX + size / 2, plot.MidY + size / 2);

        float startAngle = -90f;
        for (int i = 0; i < s.Values.Count; i++)
        {
            double v = s.Values[i];
            if (double.IsNaN(v) || v <= 0) continue;
            float sweep = (float)(v / total * 360.0);
            using var paint = new SKPaint { Color = new SKColor(OfficePalette.Get(i)), IsAntialias = true, Style = SKPaintStyle.Fill };
            using var path = new SKPath();
            path.MoveTo(rect.MidX, rect.MidY);
            path.ArcTo(rect, startAngle, sweep, false);
            path.Close();
            canvas.DrawPath(path, paint);

            if (data.ShowDataLabels)
            {
                double mid = (startAngle + sweep / 2) * Math.PI / 180.0;
                float r = size / 2 * 0.7f;
                float lx = (float)(rect.MidX + r * Math.Cos(mid));
                float ly = (float)(rect.MidY + r * Math.Sin(mid));
                var pct = (v / total * 100).ToString("0.#", CultureInfo.InvariantCulture) + "%";
                using var f = new SKFont(tf, 10 * scale);
                using var tp = new SKPaint { Color = SKColors.White, IsAntialias = true };
                var w = f.MeasureText(pct);
                canvas.DrawText(pct, lx - w / 2, ly + 4 * scale, SKTextAlign.Left, f, tp);
            }
            startAngle += sweep;
        }

        if (doughnut)
        {
            using var hole = new SKPaint { Color = SKColors.White, IsAntialias = true };
            float hr = size / 2 * 0.55f;
            canvas.DrawCircle(rect.MidX, rect.MidY, hr, hole);
        }
    }

    // ---------- 散点 ----------

    private static void DrawScatter(SKCanvas canvas, ChartData data, SKRect plot, float scale, SKTypeface tf)
    {
        double xMin = double.PositiveInfinity, xMax = double.NegativeInfinity;
        double yMin = double.PositiveInfinity, yMax = double.NegativeInfinity;
        foreach (var s in data.Series)
        {
            for (int i = 0; i < s.Values.Count; i++)
            {
                double y = s.Values[i];
                double x = i < s.XValues.Count ? s.XValues[i] : i + 1;
                if (double.IsNaN(x) || double.IsNaN(y)) continue;
                if (x < xMin) xMin = x; if (x > xMax) xMax = x;
                if (y < yMin) yMin = y; if (y > yMax) yMax = y;
            }
        }
        if (double.IsInfinity(xMin)) return;
        var (xnMin, xnMax, xStep) = NiceRange(xMin, xMax);
        var (ynMin, ynMax, yStep) = NiceRange(yMin, yMax);

        float axisLabelW = 36 * scale;
        float axisLabelH = 16 * scale;
        var inner = new SKRect(plot.Left + axisLabelW, plot.Top + 4 * scale, plot.Right - 4 * scale, plot.Bottom - axisLabelH);
        DrawAxes(canvas, inner, scale, tf, data.ShowGridLines, ynMin, ynMax, yStep, vertical: true);

        // X 轴刻度（数值）
        DrawXNumericTicks(canvas, inner, scale, tf, xnMin, xnMax, xStep);

        foreach (var s in data.Series)
        {
            using var paint = new SKPaint { Color = new SKColor(s.ColorArgb ?? OfficePalette.Get(0)), IsAntialias = true };
            for (int i = 0; i < s.Values.Count; i++)
            {
                double y = s.Values[i];
                double x = i < s.XValues.Count ? s.XValues[i] : i + 1;
                if (double.IsNaN(x) || double.IsNaN(y)) continue;
                float px = MapNumericToX(x, xnMin, xnMax, inner);
                float py = MapValueToY(y, ynMin, ynMax, inner);
                canvas.DrawCircle(px, py, 3 * scale, paint);
            }
        }
    }

    // ---------- 坐标转换 / 轴 ----------

    private static float MapValueToY(double v, double vMin, double vMax, SKRect inner)
    {
        double t = (v - vMin) / (vMax - vMin);
        return (float)(inner.Bottom - t * inner.Height);
    }

    private static float MapValueToX(double v, double vMin, double vMax, SKRect inner)
    {
        double t = (v - vMin) / (vMax - vMin);
        return (float)(inner.Left + t * inner.Width);
    }

    private static float MapNumericToX(double v, double vMin, double vMax, SKRect inner) =>
        MapValueToX(v, vMin, vMax, inner);

    private static void DrawAxes(SKCanvas canvas, SKRect inner, float scale, SKTypeface tf, bool grid, double vMin, double vMax, double vStep, bool vertical)
    {
        using var axis = new SKPaint { Color = SKColors.Gray, StrokeWidth = 1 * scale, IsAntialias = true };
        using var gridPaint = new SKPaint { Color = new SKColor(0xFFE6E6E6), StrokeWidth = 1 * scale, IsAntialias = true };
        using var f = new SKFont(tf, 9 * scale);
        using var tp = new SKPaint { Color = SKColors.DimGray, IsAntialias = true };

        canvas.DrawLine(inner.Left, inner.Top, inner.Left, inner.Bottom, axis);
        canvas.DrawLine(inner.Left, inner.Bottom, inner.Right, inner.Bottom, axis);

        if (vertical)
        {
            for (double v = vMin; v <= vMax + 1e-9; v += vStep)
            {
                float y = MapValueToY(v, vMin, vMax, inner);
                if (grid) canvas.DrawLine(inner.Left, y, inner.Right, y, gridPaint);
                canvas.DrawLine(inner.Left - 3 * scale, y, inner.Left, y, axis);
                var label = FormatAxis(v);
                var w = f.MeasureText(label);
                canvas.DrawText(label, inner.Left - 5 * scale - w, y + 3 * scale, SKTextAlign.Left, f, tp);
            }
        }
        else
        {
            for (double v = vMin; v <= vMax + 1e-9; v += vStep)
            {
                float x = MapValueToX(v, vMin, vMax, inner);
                if (grid) canvas.DrawLine(x, inner.Top, x, inner.Bottom, gridPaint);
                canvas.DrawLine(x, inner.Bottom, x, inner.Bottom + 3 * scale, axis);
                var label = FormatAxis(v);
                var w = f.MeasureText(label);
                canvas.DrawText(label, x - w / 2, inner.Bottom + 12 * scale, SKTextAlign.Left, f, tp);
            }
        }
    }

    private static void DrawXNumericTicks(SKCanvas canvas, SKRect inner, float scale, SKTypeface tf, double min, double max, double step)
    {
        using var f = new SKFont(tf, 9 * scale);
        using var tp = new SKPaint { Color = SKColors.DimGray, IsAntialias = true };
        using var axis = new SKPaint { Color = SKColors.Gray, StrokeWidth = 1 * scale, IsAntialias = true };
        for (double v = min; v <= max + 1e-9; v += step)
        {
            float x = MapValueToX(v, min, max, inner);
            canvas.DrawLine(x, inner.Bottom, x, inner.Bottom + 3 * scale, axis);
            var label = FormatAxis(v);
            var w = f.MeasureText(label);
            canvas.DrawText(label, x - w / 2, inner.Bottom + 12 * scale, SKTextAlign.Left, f, tp);
        }
    }

    private static void DrawValueLabel(SKCanvas canvas, double v, SKPoint pos, float scale, SKTypeface tf, SKTextAlign align)
    {
        using var f = new SKFont(tf, 9 * scale);
        using var tp = new SKPaint { Color = SKColors.Black, IsAntialias = true };
        canvas.DrawText(FormatAxis(v), pos.X, pos.Y, align, f, tp);
    }

    private static string FormatAxis(double v)
    {
        double abs = Math.Abs(v);
        if (abs >= 1_000_000) return (v / 1_000_000).ToString("0.##", CultureInfo.InvariantCulture) + "M";
        if (abs >= 1_000) return (v / 1_000).ToString("0.##", CultureInfo.InvariantCulture) + "K";
        if (abs >= 100) return v.ToString("0", CultureInfo.InvariantCulture);
        if (abs >= 1) return v.ToString("0.##", CultureInfo.InvariantCulture);
        return v.ToString("0.###", CultureInfo.InvariantCulture);
    }
}
