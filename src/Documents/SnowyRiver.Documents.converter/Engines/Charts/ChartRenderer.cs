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

    [System.ThreadStatic] private static string? _currentFontFamily;

    private static void DrawTextSmart(SKCanvas canvas, string text, float x, float y, SKTextAlign align, SKFont baseFont, SKPaint paint)
    {
        if (string.IsNullOrEmpty(text)) return;
        var fallback = SkiaFontFallback.ResolveForText(_currentFontFamily, text);
        if (ReferenceEquals(fallback, baseFont.Typeface))
        {
            canvas.DrawText(text, x, y, align, baseFont, paint);
            return;
        }
        using var f = new SKFont(fallback, baseFont.Size) { Edging = baseFont.Edging };
        canvas.DrawText(text, x, y, align, f, paint);
    }

    private static float MeasureSmart(SKFont baseFont, string text)
    {
        if (string.IsNullOrEmpty(text)) return 0;
        var fallback = SkiaFontFallback.ResolveForText(_currentFontFamily, text);
        if (ReferenceEquals(fallback, baseFont.Typeface)) return baseFont.MeasureText(text);
        using var f = new SKFont(fallback, baseFont.Size);
        return f.MeasureText(text);
    }

    private static void DrawChart(SKCanvas canvas, ChartData data, SKRect bounds, float scale, string fontFamily)
    {
        _currentFontFamily = fontFamily;
        var typeface = SkiaFontFallback.ResolvePrimary(fontFamily);

        float padding = 8 * scale;
        var area = SKRect.Inflate(bounds, -padding, -padding);

        // 1) 标题
        if (!string.IsNullOrEmpty(data.Title))
        {
            using var titlePaint = new SKPaint { Color = SKColors.Black, IsAntialias = true };
            using var titleFont = new SKFont(typeface, 14 * scale) { Edging = SKFontEdging.SubpixelAntialias };
            float titleHeight = 18 * scale;
            var w = MeasureSmart(titleFont, data.Title);
            DrawTextSmart(canvas, data.Title, area.MidX - w / 2, area.Top + titleHeight - 4 * scale, SKTextAlign.Left, titleFont, titlePaint);
            area = new SKRect(area.Left, area.Top + titleHeight + 4 * scale, area.Right, area.Bottom);
        }

        // 2) 图例（饼/环图也需要图例显示分类）
        var legendItems = BuildLegendItems(data);
        var (plotArea, legendArea) = SplitForLegend(area, data.Legend, legendItems.Count, scale, typeface);

        // 3) 轴标题占位（仅 axis-based 图表）
        bool axisBased = data.Kind is ChartKind.Column or ChartKind.BarHorizontal
            or ChartKind.Line or ChartKind.Area or ChartKind.Scatter;
        if (axisBased)
        {
            float axisTitleH = 14 * scale;
            float axisTitleW = 14 * scale;
            if (!string.IsNullOrEmpty(data.AxisTitleX))
            {
                using var ap = new SKPaint { Color = SKColors.Black, IsAntialias = true };
                using var af = new SKFont(typeface, 11 * scale) { Edging = SKFontEdging.SubpixelAntialias };
                var w = MeasureSmart(af, data.AxisTitleX!);
                DrawTextSmart(canvas, data.AxisTitleX!, plotArea.MidX - w / 2, plotArea.Bottom - 2 * scale, SKTextAlign.Left, af, ap);
                plotArea = new SKRect(plotArea.Left, plotArea.Top, plotArea.Right, plotArea.Bottom - axisTitleH);
            }
            if (!string.IsNullOrEmpty(data.AxisTitleY))
            {
                using var ap = new SKPaint { Color = SKColors.Black, IsAntialias = true };
                using var af = new SKFont(typeface, 11 * scale) { Edging = SKFontEdging.SubpixelAntialias };
                canvas.Save();
                canvas.Translate(plotArea.Left + 4 * scale, plotArea.MidY);
                canvas.RotateDegrees(-90);
                var w = MeasureSmart(af, data.AxisTitleY!);
                DrawTextSmart(canvas, data.AxisTitleY!, -w / 2, 0, SKTextAlign.Left, af, ap);
                canvas.Restore();
                plotArea = new SKRect(plotArea.Left + axisTitleW, plotArea.Top, plotArea.Right, plotArea.Bottom);
            }
        }

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
            case ChartKind.Radar:
                DrawRadar(canvas, data, plotArea, scale, typeface);
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
                DrawTextSmart(canvas, it.Text, x, y + 4 * scale, SKTextAlign.Left, f, textPaint);
                x += MeasureSmart(f, it.Text) + 12 * scale;
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
                DrawTextSmart(canvas, it.Text, x + swatch + 4 * scale, y, SKTextAlign.Left, f, textPaint);
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
        bool stacked = data.Stack == StackMode.Stacked;
        bool percent = data.Stack == StackMode.PercentStacked;

        int catCount = data.Categories.Count;
        int serCount = data.Series.Count;

        // 计算数值范围
        double allMin = 0, allMax = 0;
        if (percent)
        {
            allMin = 0; allMax = 1;
        }
        else if (stacked)
        {
            for (int ci = 0; ci < catCount; ci++)
            {
                double posSum = 0, negSum = 0;
                foreach (var s in data.Series)
                {
                    if (ci >= s.Values.Count) continue;
                    double v = s.Values[ci];
                    if (double.IsNaN(v)) continue;
                    if (v >= 0) posSum += v; else negSum += v;
                }
                if (posSum > allMax) allMax = posSum;
                if (negSum < allMin) allMin = negSum;
            }
            if (allMin == 0 && allMax == 0) allMax = 1;
        }
        else
        {
            allMin = double.PositiveInfinity; allMax = double.NegativeInfinity;
            foreach (var s in data.Series)
                foreach (var v in s.Values)
                    if (!double.IsNaN(v))
                    {
                        if (v < allMin) allMin = v;
                        if (v > allMax) allMax = v;
                    }
            if (double.IsInfinity(allMin)) { allMin = 0; allMax = 1; }
        }
        var (vMin, vMax, vStep) = percent
            ? (0.0, 1.0, 0.2)
            : NiceRange(allMin, allMax);

        float axisLabelW = 36 * scale;
        float axisLabelH = 16 * scale;
        var inner = vertical
            ? new SKRect(plot.Left + axisLabelW, plot.Top + 4 * scale, plot.Right - 4 * scale, plot.Bottom - axisLabelH)
            : new SKRect(plot.Left + axisLabelW, plot.Top + 4 * scale, plot.Right - 4 * scale, plot.Bottom - axisLabelH);

        DrawAxes(canvas, inner, scale, tf, data.ShowGridLines, vMin, vMax, vStep, vertical, percent);

        if (vertical)
        {
            float catW = inner.Width / catCount;
            float groupPad = catW * 0.15f;
            float barW = stacked || percent ? (catW - 2 * groupPad) : (catW - 2 * groupPad) / serCount;

            for (int ci = 0; ci < catCount; ci++)
            {
                float xBase = inner.Left + ci * catW + groupPad;
                if (stacked || percent)
                {
                    double total = 0;
                    if (percent)
                    {
                        for (int si = 0; si < serCount; si++)
                        {
                            if (ci >= data.Series[si].Values.Count) continue;
                            var v = data.Series[si].Values[ci];
                            if (!double.IsNaN(v) && v > 0) total += v;
                        }
                        if (total <= 0) continue;
                    }
                    double posAcc = 0, negAcc = 0;
                    for (int si = 0; si < serCount; si++)
                    {
                        var s = data.Series[si];
                        if (ci >= s.Values.Count) continue;
                        double v = s.Values[ci];
                        if (double.IsNaN(v)) continue;
                        double vv = percent ? (v > 0 ? v / total : 0) : v;
                        double from = vv >= 0 ? posAcc : negAcc + vv;
                        double to = vv >= 0 ? posAcc + vv : negAcc;
                        float yFrom = MapValueToY(from, vMin, vMax, inner);
                        float yTo = MapValueToY(to, vMin, vMax, inner);
                        var rect = new SKRect(xBase, Math.Min(yFrom, yTo), xBase + barW, Math.Max(yFrom, yTo));
                        using var p = CreateSeriesFill(s, si, rect, ci);
                        canvas.DrawRect(rect, p);
                        if (vv >= 0) posAcc += vv; else negAcc += vv;
                    }
                }
                else
                {
                    for (int si = 0; si < serCount; si++)
                    {
                        var s = data.Series[si];
                        if (ci >= s.Values.Count) continue;
                        double v = s.Values[ci];
                        if (double.IsNaN(v)) continue;
                        float yZero = MapValueToY(0, vMin, vMax, inner);
                        float yV = MapValueToY(v, vMin, vMax, inner);
                        var rect = new SKRect(xBase + si * barW, Math.Min(yZero, yV), xBase + (si + 1) * barW - 1, Math.Max(yZero, yV));
                        using var p = CreateSeriesFill(s, si, rect, ci);
                        canvas.DrawRect(rect, p);
                        if (data.ShowDataLabels)
                            DrawValueLabel(canvas, v, new SKPoint(rect.MidX, rect.Top - 2 * scale), scale, tf, SKTextAlign.Center);
                    }
                }
                using var f = new SKFont(tf, 10 * scale);
                using var tp = new SKPaint { Color = SKColors.Black, IsAntialias = true };
                var label = data.Categories[ci];
                var w = MeasureSmart(f, label);
                DrawTextSmart(canvas, label, inner.Left + (ci + 0.5f) * catW - w / 2, inner.Bottom + 12 * scale, SKTextAlign.Left, f, tp);
            }
        }
        else
        {
            float catH = inner.Height / catCount;
            float groupPad = catH * 0.15f;
            float barH = stacked || percent ? (catH - 2 * groupPad) : (catH - 2 * groupPad) / serCount;

            for (int ci = 0; ci < catCount; ci++)
            {
                float yBase = inner.Top + ci * catH + groupPad;
                if (stacked || percent)
                {
                    double total = 0;
                    if (percent)
                    {
                        for (int si = 0; si < serCount; si++)
                        {
                            if (ci >= data.Series[si].Values.Count) continue;
                            var v = data.Series[si].Values[ci];
                            if (!double.IsNaN(v) && v > 0) total += v;
                        }
                        if (total <= 0) continue;
                    }
                    double posAcc = 0, negAcc = 0;
                    for (int si = 0; si < serCount; si++)
                    {
                        var s = data.Series[si];
                        if (ci >= s.Values.Count) continue;
                        double v = s.Values[ci];
                        if (double.IsNaN(v)) continue;
                        double vv = percent ? (v > 0 ? v / total : 0) : v;
                        double from = vv >= 0 ? posAcc : negAcc + vv;
                        double to = vv >= 0 ? posAcc + vv : negAcc;
                        float xFrom = MapValueToX(from, vMin, vMax, inner);
                        float xTo = MapValueToX(to, vMin, vMax, inner);
                        var rect = new SKRect(Math.Min(xFrom, xTo), yBase, Math.Max(xFrom, xTo), yBase + barH);
                        using var p = CreateSeriesFill(s, si, rect, ci);
                        canvas.DrawRect(rect, p);
                        if (vv >= 0) posAcc += vv; else negAcc += vv;
                    }
                }
                else
                {
                    for (int si = 0; si < serCount; si++)
                    {
                        var s = data.Series[si];
                        if (ci >= s.Values.Count) continue;
                        double v = s.Values[ci];
                        if (double.IsNaN(v)) continue;
                        float xZero = MapValueToX(0, vMin, vMax, inner);
                        float xV = MapValueToX(v, vMin, vMax, inner);
                        var rect = new SKRect(Math.Min(xZero, xV), yBase + si * barH, Math.Max(xZero, xV), yBase + (si + 1) * barH - 1);
                        using var p = CreateSeriesFill(s, si, rect, ci);
                        canvas.DrawRect(rect, p);
                        if (data.ShowDataLabels)
                            DrawValueLabel(canvas, v, new SKPoint(rect.Right + 2 * scale, rect.MidY + 3 * scale), scale, tf, SKTextAlign.Left);
                    }
                }
                using var f = new SKFont(tf, 10 * scale);
                using var tp = new SKPaint { Color = SKColors.Black, IsAntialias = true };
                var label = data.Categories[ci];
                var w = MeasureSmart(f, label);
                DrawTextSmart(canvas, label, inner.Left - w - 4 * scale, inner.Top + (ci + 0.5f) * catH + 4 * scale, SKTextAlign.Left, f, tp);
            }
        }

        // 组合图叠加 + 副坐标轴：仅在垂直主图（Column/簇状/堆叠均可）时启用
        if (vertical)
            DrawOverlaySeries(canvas, data, inner, scale, tf, vMin, vMax);
    }

    /// <summary>
    /// 在主柱状图之上叠加 OverrideKind=Line 的系列；同时若任意叠加系列 UseSecondaryAxis=true，
    /// 则用副坐标轴重新映射并在右侧绘制副轴刻度。
    /// </summary>
    private static void DrawOverlaySeries(SKCanvas canvas, ChartData data, SKRect inner, float scale, SKTypeface tf, double primaryMin, double primaryMax)
    {
        var overlay = data.Series
            .Select((s, i) => (s, i))
            .Where(t => t.s.OverrideKind == ChartKind.Line)
            .ToList();
        if (overlay.Count == 0) return;

        // 计算副轴范围
        bool anySecondary = overlay.Any(t => t.s.UseSecondaryAxis);
        double sMin = double.PositiveInfinity, sMax = double.NegativeInfinity;
        if (anySecondary)
        {
            foreach (var (s, _) in overlay.Where(t => t.s.UseSecondaryAxis))
                foreach (var v in s.Values)
                    if (!double.IsNaN(v))
                    {
                        if (v < sMin) sMin = v;
                        if (v > sMax) sMax = v;
                    }
            if (double.IsInfinity(sMin)) { sMin = 0; sMax = 1; }
        }
        var (s2Min, s2Max, s2Step) = anySecondary ? NiceRange(sMin, sMax) : (0.0, 1.0, 0.2);

        int catCount = data.Categories.Count;
        if (catCount == 0) return;
        float catW = inner.Width / catCount;

        foreach (var (s, si) in overlay)
        {
            using var stroke = new SKPaint
            {
                Color = new SKColor(s.ColorArgb ?? OfficePalette.Get(si)),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2 * scale,
                IsAntialias = true,
            };
            using var dot = new SKPaint
            {
                Color = stroke.Color,
                IsAntialias = true,
            };
            using var path = new SKPath();
            bool useSec = s.UseSecondaryAxis && anySecondary;
            double mn = useSec ? s2Min : primaryMin;
            double mx = useSec ? s2Max : primaryMax;
            bool started = false;
            for (int ci = 0; ci < catCount && ci < s.Values.Count; ci++)
            {
                double v = s.Values[ci];
                if (double.IsNaN(v)) continue;
                float x = inner.Left + (ci + 0.5f) * catW;
                float y = MapValueToY(v, mn, mx, inner);
                if (!started) { path.MoveTo(x, y); started = true; } else path.LineTo(x, y);
                canvas.DrawCircle(x, y, 2.5f * scale, dot);
            }
            canvas.DrawPath(path, stroke);
        }

        if (anySecondary)
        {
            using var axis = new SKPaint { Color = SKColors.Gray, StrokeWidth = 1 * scale, IsAntialias = true };
            using var f = new SKFont(tf, 9 * scale);
            using var tp = new SKPaint { Color = SKColors.DimGray, IsAntialias = true };
            canvas.DrawLine(inner.Right, inner.Top, inner.Right, inner.Bottom, axis);
            for (double v = s2Min; v <= s2Max + 1e-9; v += s2Step)
            {
                float y = MapValueToY(v, s2Min, s2Max, inner);
                canvas.DrawLine(inner.Right, y, inner.Right + 3 * scale, y, axis);
                var label = FormatAxis(v);
                canvas.DrawText(label, inner.Right + 5 * scale, y + 3 * scale, SKTextAlign.Left, f, tp);
            }
            if (!string.IsNullOrEmpty(data.AxisTitleY2))
            {
                using var ap = new SKPaint { Color = SKColors.Black, IsAntialias = true };
                using var af = new SKFont(tf, 11 * scale) { Edging = SKFontEdging.SubpixelAntialias };
                canvas.Save();
                canvas.Translate(inner.Right + 36 * scale, inner.MidY);
                canvas.RotateDegrees(90);
                var w = MeasureSmart(af, data.AxisTitleY2!);
                DrawTextSmart(canvas, data.AxisTitleY2!, -w / 2, 0, SKTextAlign.Left, af, ap);
                canvas.Restore();
            }
        }
    }

    /// <summary>根据系列填充模式创建画笔（实色/线性渐变/径向渐变 + dPt 点级覆盖）。</summary>
    private static SKPaint CreateSeriesFill(ChartSeries s, int seriesIndex, SKRect rect, int pointIndex)
    {
        // 数据点颜色优先（dPt 仅适用于实色）
        if (s.PointColors.TryGetValue(pointIndex, out var pc))
            return new SKPaint { Color = new SKColor(pc), IsAntialias = true };

        if (s.Fill == FillKind.LinearGradient && s.GradientStops.Count >= 2)
        {
            var colors = s.GradientStops.Select(g => new SKColor(g.ColorArgb)).ToArray();
            var pos = s.GradientStops.Select(g => (float)g.Position).ToArray();
            var shader = SKShader.CreateLinearGradient(
                new SKPoint(rect.Left, rect.Top),
                new SKPoint(rect.Left, rect.Bottom),
                colors, pos, SKShaderTileMode.Clamp);
            return new SKPaint { Shader = shader, IsAntialias = true };
        }
        if (s.Fill == FillKind.RadialGradient && s.GradientStops.Count >= 2)
        {
            var colors = s.GradientStops.Select(g => new SKColor(g.ColorArgb)).ToArray();
            var pos = s.GradientStops.Select(g => (float)g.Position).ToArray();
            var shader = SKShader.CreateRadialGradient(
                new SKPoint(rect.MidX, rect.MidY),
                Math.Max(rect.Width, rect.Height) / 2,
                colors, pos, SKShaderTileMode.Clamp);
            return new SKPaint { Shader = shader, IsAntialias = true };
        }
        return new SKPaint { Color = new SKColor(s.ColorArgb ?? OfficePalette.Get(seriesIndex)), IsAntialias = true };
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

                // 标记点 + 数据标签
                using var marker = new SKPaint { Color = stroke.Color, IsAntialias = true };
                for (int i = 0; i < s.Values.Count && i < data.Categories.Count; i++)
                {
                    double v = s.Values[i];
                    if (double.IsNaN(v)) continue;
                    float x = inner.Left + i * stepX;
                    float y = MapValueToY(v, vMin, vMax, inner);
                    canvas.DrawCircle(x, y, 2.5f * scale, marker);
                    if (data.ShowDataLabels)
                        DrawValueLabel(canvas, v, new SKPoint(x, y - 4 * scale), scale, tf, SKTextAlign.Center);
                }

                // 趋势线
                if (s.Trend != null && s.Trend.Kind != TrendlineKind.None)
                    DrawTrendline(canvas, s, inner, stepX, vMin, vMax, scale);
            }
        }

        // 类别标签（与柱状图一致）
        using var f = new SKFont(tf, 10 * scale);
        using var tp = new SKPaint { Color = SKColors.Black, IsAntialias = true };
        for (int ci = 0; ci < n && ci < data.Categories.Count; ci++)
        {
            float x = inner.Left + ci * stepX;
            var label = data.Categories[ci];
            var w = MeasureSmart(f, label);
            DrawTextSmart(canvas, label, x - w / 2, inner.Bottom + 12 * scale, SKTextAlign.Left, f, tp);
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
        using var sliceBorder = new SKPaint { Color = SKColors.White, IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f * scale };
        for (int i = 0; i < s.Values.Count; i++)
        {
            double v = s.Values[i];
            if (double.IsNaN(v) || v <= 0) continue;
            float sweep = (float)(v / total * 360.0);
            var sliceColor = new SKColor(OfficePalette.Get(i));
            using var paint = new SKPaint { Color = sliceColor, IsAntialias = true, Style = SKPaintStyle.Fill };
            using var path = new SKPath();
            path.MoveTo(rect.MidX, rect.MidY);
            path.ArcTo(rect, startAngle, sweep, false);
            path.Close();
            canvas.DrawPath(path, paint);
            canvas.DrawPath(path, sliceBorder);

            if (data.ShowDataLabels)
            {
                double mid = (startAngle + sweep / 2) * Math.PI / 180.0;
                float r = size / 2 * 0.7f;
                float lx = (float)(rect.MidX + r * Math.Cos(mid));
                float ly = (float)(rect.MidY + r * Math.Sin(mid));
                var pct = (v / total * 100).ToString("0.#", CultureInfo.InvariantCulture) + "%";
                using var f = new SKFont(tf, 10 * scale);
                // 根据切片亮度选用黑/白文字以保证可读性
                double luma = 0.299 * sliceColor.Red + 0.587 * sliceColor.Green + 0.114 * sliceColor.Blue;
                using var tp = new SKPaint { Color = luma < 140 ? SKColors.White : SKColors.Black, IsAntialias = true };
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
                if (data.ShowDataLabels)
                    DrawValueLabel(canvas, y, new SKPoint(px, py - 5 * scale), scale, tf, SKTextAlign.Center);
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

    private static void DrawAxes(SKCanvas canvas, SKRect inner, float scale, SKTypeface tf, bool grid, double vMin, double vMax, double vStep, bool vertical, bool percentLabel = false)
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
                var label = percentLabel ? (v * 100).ToString("0", CultureInfo.InvariantCulture) + "%" : FormatAxis(v);
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
                var label = percentLabel ? (v * 100).ToString("0", CultureInfo.InvariantCulture) + "%" : FormatAxis(v);
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

    // ---------- 雷达图 ----------

    private static void DrawRadar(SKCanvas canvas, ChartData data, SKRect plot, float scale, SKTypeface tf)
    {
        int axes = data.Categories.Count;
        if (axes < 3) return;

        double allMax = double.NegativeInfinity;
        foreach (var s in data.Series)
            foreach (var v in s.Values)
                if (!double.IsNaN(v) && v > allMax) allMax = v;
        if (double.IsInfinity(allMax) || allMax <= 0) allMax = 1;
        var (vMin, vMax, vStep) = NiceRange(0, allMax);
        if (vMin < 0) vMin = 0;

        float cx = plot.MidX;
        float cy = plot.MidY;
        float radius = Math.Min(plot.Width, plot.Height) / 2 - 24 * scale;
        if (radius <= 0) return;

        using var grid = new SKPaint { Color = new SKColor(0xFFD0D0D0), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1 * scale };
        using var spoke = new SKPaint { Color = new SKColor(0xFFB0B0B0), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1 * scale };
        using var labelPaint = new SKPaint { Color = SKColors.DimGray, IsAntialias = true };
        using var labelFont = new SKFont(tf, 10 * scale);

        // 同心多边形网格
        for (double v = vStep; v <= vMax + 1e-9; v += vStep)
        {
            double t = (v - vMin) / (vMax - vMin);
            using var path = new SKPath();
            for (int i = 0; i < axes; i++)
            {
                double angle = -Math.PI / 2 + 2 * Math.PI * i / axes;
                float x = (float)(cx + Math.Cos(angle) * radius * t);
                float y = (float)(cy + Math.Sin(angle) * radius * t);
                if (i == 0) path.MoveTo(x, y); else path.LineTo(x, y);
            }
            path.Close();
            canvas.DrawPath(path, grid);
        }

        // 轴线 + 类别标签
        for (int i = 0; i < axes; i++)
        {
            double angle = -Math.PI / 2 + 2 * Math.PI * i / axes;
            float ex = (float)(cx + Math.Cos(angle) * radius);
            float ey = (float)(cy + Math.Sin(angle) * radius);
            canvas.DrawLine(cx, cy, ex, ey, spoke);

            var label = data.Categories[i];
            float lx = (float)(cx + Math.Cos(angle) * (radius + 10 * scale));
            float ly = (float)(cy + Math.Sin(angle) * (radius + 10 * scale)) + 4 * scale;
            var w = MeasureSmart(labelFont, label);
            DrawTextSmart(canvas, label, lx - w / 2, ly, SKTextAlign.Left, labelFont, labelPaint);
        }

        // 系列
        int sIdx = 0;
        foreach (var s in data.Series)
        {
            var color = new SKColor(s.ColorArgb ?? OfficePalette.Get(sIdx));
            using var stroke = new SKPaint { Color = color, IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 2 * scale };
            using var fill = new SKPaint { Color = new SKColor((uint)(color.WithAlpha(0x60))), IsAntialias = true, Style = SKPaintStyle.Fill };
            using var marker = new SKPaint { Color = color, IsAntialias = true };

            using var path = new SKPath();
            bool started = false;
            for (int i = 0; i < axes; i++)
            {
                if (i >= s.Values.Count) break;
                double v = s.Values[i];
                if (double.IsNaN(v)) { started = false; continue; }
                double t = (Math.Clamp(v, vMin, vMax) - vMin) / (vMax - vMin);
                double angle = -Math.PI / 2 + 2 * Math.PI * i / axes;
                float x = (float)(cx + Math.Cos(angle) * radius * t);
                float y = (float)(cy + Math.Sin(angle) * radius * t);
                if (!started) { path.MoveTo(x, y); started = true; } else path.LineTo(x, y);
            }
            path.Close();
            if (data.RadarFilled) canvas.DrawPath(path, fill);
            canvas.DrawPath(path, stroke);

            for (int i = 0; i < axes && i < s.Values.Count; i++)
            {
                double v = s.Values[i];
                if (double.IsNaN(v)) continue;
                double t = (Math.Clamp(v, vMin, vMax) - vMin) / (vMax - vMin);
                double angle = -Math.PI / 2 + 2 * Math.PI * i / axes;
                float x = (float)(cx + Math.Cos(angle) * radius * t);
                float y = (float)(cy + Math.Sin(angle) * radius * t);
                canvas.DrawCircle(x, y, 2.5f * scale, marker);
            }
            sIdx++;
        }
    }

    // ---------- 趋势线 ----------

    private static void DrawTrendline(SKCanvas canvas, ChartSeries s, SKRect inner, float stepX, double vMin, double vMax, float scale)
    {
        if (s.Trend == null || s.Trend.Kind == TrendlineKind.None) return;

        // 收集 (x,y) 有效点（x 用索引）
        var xs = new List<double>();
        var ys = new List<double>();
        for (int i = 0; i < s.Values.Count; i++)
        {
            var v = s.Values[i];
            if (double.IsNaN(v)) continue;
            xs.Add(i);
            ys.Add(v);
        }
        if (xs.Count < 2) return;

        Func<double, double>? f = s.Trend.Kind switch
        {
            TrendlineKind.Linear => FitPolynomial(xs, ys, 1),
            TrendlineKind.Polynomial => FitPolynomial(xs, ys, Math.Max(2, s.Trend.Order)),
            TrendlineKind.Logarithmic => FitLogarithmic(xs, ys),
            TrendlineKind.Exponential => FitExponential(xs, ys),
            TrendlineKind.Power => FitPower(xs, ys),
            TrendlineKind.MovingAverage => MovingAverage(xs, ys, Math.Max(2, s.Trend.Order)),
            _ => null,
        };
        if (f == null) return;

        var color = s.Trend.ColorArgb.HasValue
            ? new SKColor(s.Trend.ColorArgb.Value)
            : new SKColor(s.ColorArgb ?? OfficePalette.Get(0));
        using var pen = new SKPaint
        {
            Color = color,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1.5f * scale,
            PathEffect = SKPathEffect.CreateDash(new float[] { 6 * scale, 4 * scale }, 0),
        };

        using var path = new SKPath();
        bool started = false;
        double xMin = xs[0], xMax = xs[xs.Count - 1];
        int samples = 64;
        for (int k = 0; k <= samples; k++)
        {
            double xv = xMin + (xMax - xMin) * k / samples;
            double yv;
            try { yv = f(xv); } catch { continue; }
            if (double.IsNaN(yv) || double.IsInfinity(yv)) continue;
            float px = inner.Left + (float)xv * stepX;
            float py = MapValueToY(Math.Clamp(yv, vMin, vMax), vMin, vMax, inner);
            if (!started) { path.MoveTo(px, py); started = true; } else path.LineTo(px, py);
        }
        canvas.DrawPath(path, pen);
    }

    /// <summary>用最小二乘法拟合 y = a0 + a1*x + ... + ak*x^k。</summary>
    private static Func<double, double>? FitPolynomial(IList<double> xs, IList<double> ys, int order)
    {
        int n = xs.Count;
        int m = order + 1;
        if (n < m) return null;
        // 正规方程 A^T A c = A^T y
        var ata = new double[m, m];
        var atb = new double[m];
        for (int i = 0; i < n; i++)
        {
            double xi = xs[i];
            double[] row = new double[m];
            row[0] = 1;
            for (int j = 1; j < m; j++) row[j] = row[j - 1] * xi;
            for (int r = 0; r < m; r++)
            {
                atb[r] += row[r] * ys[i];
                for (int c = 0; c < m; c++) ata[r, c] += row[r] * row[c];
            }
        }
        var coef = SolveLinear(ata, atb);
        if (coef == null) return null;
        return x =>
        {
            double y = 0, p = 1;
            for (int i = 0; i < m; i++) { y += coef[i] * p; p *= x; }
            return y;
        };
    }

    private static Func<double, double>? FitLogarithmic(IList<double> xs, IList<double> ys)
    {
        var lx = new List<double>(); var fy = new List<double>();
        for (int i = 0; i < xs.Count; i++)
        {
            if (xs[i] <= 0) continue;
            lx.Add(Math.Log(xs[i])); fy.Add(ys[i]);
        }
        var lin = FitPolynomial(lx, fy, 1);
        if (lin == null) return null;
        return x => x > 0 ? lin(Math.Log(x)) : double.NaN;
    }

    private static Func<double, double>? FitExponential(IList<double> xs, IList<double> ys)
    {
        var fx = new List<double>(); var ly = new List<double>();
        for (int i = 0; i < xs.Count; i++)
        {
            if (ys[i] <= 0) continue;
            fx.Add(xs[i]); ly.Add(Math.Log(ys[i]));
        }
        var lin = FitPolynomial(fx, ly, 1);
        if (lin == null) return null;
        return x => Math.Exp(lin(x));
    }

    private static Func<double, double>? FitPower(IList<double> xs, IList<double> ys)
    {
        var lx = new List<double>(); var ly = new List<double>();
        for (int i = 0; i < xs.Count; i++)
        {
            if (xs[i] <= 0 || ys[i] <= 0) continue;
            lx.Add(Math.Log(xs[i])); ly.Add(Math.Log(ys[i]));
        }
        var lin = FitPolynomial(lx, ly, 1);
        if (lin == null) return null;
        return x => x > 0 ? Math.Exp(lin(Math.Log(x))) : double.NaN;
    }

    private static Func<double, double> MovingAverage(IList<double> xs, IList<double> ys, int period)
    {
        // 简化：以 x 索引附近 period 范围求平均
        return x =>
        {
            double sum = 0; int cnt = 0;
            for (int i = 0; i < xs.Count; i++)
            {
                if (Math.Abs(xs[i] - x) <= period / 2.0)
                { sum += ys[i]; cnt++; }
            }
            return cnt == 0 ? double.NaN : sum / cnt;
        };
    }

    /// <summary>高斯消元解 n×n 线性方程组。</summary>
    private static double[]? SolveLinear(double[,] a, double[] b)
    {
        int n = b.Length;
        var m = new double[n, n + 1];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++) m[i, j] = a[i, j];
            m[i, n] = b[i];
        }
        for (int i = 0; i < n; i++)
        {
            int pivot = i;
            for (int k = i + 1; k < n; k++)
                if (Math.Abs(m[k, i]) > Math.Abs(m[pivot, i])) pivot = k;
            if (Math.Abs(m[pivot, i]) < 1e-12) return null;
            if (pivot != i)
                for (int j = 0; j <= n; j++) (m[i, j], m[pivot, j]) = (m[pivot, j], m[i, j]);
            for (int k = i + 1; k < n; k++)
            {
                double f = m[k, i] / m[i, i];
                for (int j = i; j <= n; j++) m[k, j] -= f * m[i, j];
            }
        }
        var x = new double[n];
        for (int i = n - 1; i >= 0; i--)
        {
            double s = m[i, n];
            for (int j = i + 1; j < n; j++) s -= m[i, j] * x[j];
            x[i] = s / m[i, i];
        }
        return x;
    }
}
