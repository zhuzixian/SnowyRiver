using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using System.Globalization;

namespace SnowyRiver.Documents.Converter.Engines.Charts;

/// <summary>
/// 从 EPPlus 的 <see cref="ExcelChart"/> 提取出与具体绘图无关的通用 <see cref="ChartData"/>。
/// 支持柱/条/折线/饼/环/面积/散点；不支持的类型返回 <c>null</c>。
/// </summary>
internal static class ChartDataExtractor
{
    public static ChartData? Extract(ExcelChart chart)
    {
        if (chart == null) return null;
        var workbook = chart.WorkSheet?.Workbook;
        if (workbook == null) return null;

        var data = new ChartData
        {
            Kind = MapKind(chart.ChartType),
            Title = SafeTitle(chart),
            Legend = MapLegend(chart),
            ShowDataLabels = TryHasDataLabel(chart),
        };

        if (data.Kind == ChartKind.Unknown) return null;

        try
        {
            var sz = chart.Size;
            if (sz != null)
            {
                int px(long emu) => (int)Math.Max(1, emu / 9525);
                if (sz.Width > 0) data.PixelWidth = px(sz.Width);
                if (sz.Height > 0) data.PixelHeight = px(sz.Height);
            }
        }
        catch { }

        try
        {
            int idx = 0;
            foreach (var s in chart.Series)
            {
                var ser = new ChartSeries
                {
                    Name = SafeSeriesName(s),
                    ColorArgb = OfficePalette.Get(idx++),
                };

                var values = ParseDoubles(workbook, s.Series);
                ser.Values.AddRange(values);

                if (data.Kind == ChartKind.Scatter)
                {
                    var xs = ParseDoubles(workbook, s.XSeries);
                    if (xs.Count == 0)
                        for (int i = 0; i < ser.Values.Count; i++) xs.Add(i + 1);
                    ser.XValues.AddRange(xs);
                }
                else
                {
                    if (data.Categories.Count == 0)
                    {
                        var cats = ParseStrings(workbook, s.XSeries);
                        if (cats.Count == 0)
                            for (int i = 0; i < ser.Values.Count; i++)
                                cats.Add((i + 1).ToString(CultureInfo.InvariantCulture));
                        data.Categories.AddRange(cats);
                    }
                }

                if (ser.Values.Count > 0) data.Series.Add(ser);
            }
        }
        catch
        {
        }

        if (data.Series.Count == 0) return null;
        return data;
    }

    private static ChartKind MapKind(eChartType t)
    {
        var n = t.ToString();
        if (n.StartsWith("Doughnut", StringComparison.Ordinal)) return ChartKind.Doughnut;
        if (n.StartsWith("Pie", StringComparison.Ordinal)) return ChartKind.Pie;
        if (n.StartsWith("BarOfPie", StringComparison.Ordinal)) return ChartKind.Pie;
        if (n.StartsWith("Bar", StringComparison.Ordinal)) return ChartKind.BarHorizontal;
        if (n.StartsWith("Column", StringComparison.Ordinal)) return ChartKind.Column;
        if (n.StartsWith("Line", StringComparison.Ordinal)) return ChartKind.Line;
        if (n.StartsWith("Area", StringComparison.Ordinal)) return ChartKind.Area;
        if (n.StartsWith("XYScatter", StringComparison.Ordinal) || n.StartsWith("Scatter", StringComparison.Ordinal))
            return ChartKind.Scatter;
        return ChartKind.Unknown;
    }

    private static LegendPos MapLegend(ExcelChart chart)
    {
        try
        {
            var leg = chart.Legend;
            if (leg == null) return LegendPos.None;
            return leg.Position switch
            {
                eLegendPosition.Right => LegendPos.Right,
                eLegendPosition.Left => LegendPos.Left,
                eLegendPosition.Top => LegendPos.Top,
                eLegendPosition.Bottom => LegendPos.Bottom,
                eLegendPosition.TopRight => LegendPos.Right,
                _ => LegendPos.Right,
            };
        }
        catch { return LegendPos.Right; }
    }

    private static string? SafeTitle(ExcelChart chart)
    {
        try { return chart.Title?.Text; }
        catch { return null; }
    }

    private static bool TryHasDataLabel(ExcelChart chart)
    {
        try
        {
            foreach (var s in chart.Series)
            {
                var dl = (s as ExcelChartSerie)?.GetType().GetProperty("DataLabel")?.GetValue(s);
                if (dl == null) continue;
                var show = dl.GetType().GetProperty("ShowValue")?.GetValue(dl) as bool?;
                if (show == true) return true;
            }
        }
        catch { }
        return false;
    }

    private static string? SafeSeriesName(ExcelChartSerie s)
    {
        try
        {
            if (!string.IsNullOrEmpty(s.Header)) return s.Header;
        }
        catch { }
        try { return s.HeaderAddress?.Address; }
        catch { return null; }
    }

    private static List<double> ParseDoubles(ExcelWorkbook wb, string? address)
    {
        var result = new List<double>();
        var range = ResolveRange(wb, address);
        if (range == null) return result;
        foreach (var cell in range)
        {
            var v = cell.Value;
            if (v == null) { result.Add(double.NaN); continue; }
            switch (v)
            {
                case double d: result.Add(d); break;
                case float f: result.Add(f); break;
                case int i: result.Add(i); break;
                case long l: result.Add(l); break;
                case short sh: result.Add(sh); break;
                case decimal m: result.Add((double)m); break;
                case DateTime dt: result.Add(dt.ToOADate()); break;
                case bool b: result.Add(b ? 1 : 0); break;
                default:
                    if (double.TryParse(v.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var dd))
                        result.Add(dd);
                    else
                        result.Add(double.NaN);
                    break;
            }
        }
        return result;
    }

    private static List<string> ParseStrings(ExcelWorkbook wb, string? address)
    {
        var result = new List<string>();
        var range = ResolveRange(wb, address);
        if (range == null) return result;
        foreach (var cell in range)
            result.Add(cell.Text ?? cell.Value?.ToString() ?? string.Empty);
        return result;
    }

    private static ExcelRange? ResolveRange(ExcelWorkbook wb, string? address)
    {
        if (string.IsNullOrWhiteSpace(address)) return null;
        try
        {
            // 解析 sheet 名（"Sheet1!$A$1:$A$10" 或带引号 "'My Sheet'!$A$1:$B$2"）
            string? sheetName = null;
            string rangePart = address;
            int bang = address.LastIndexOf('!');
            if (bang > 0)
            {
                sheetName = address.Substring(0, bang).Trim('\'', ' ');
                rangePart = address.Substring(bang + 1);
            }
            var a = new ExcelAddress(rangePart);
            ExcelWorksheet? ws = null;
            if (!string.IsNullOrEmpty(sheetName))
                ws = wb.Worksheets[sheetName];
            ws ??= wb.Worksheets.FirstOrDefault();
            if (ws == null) return null;
            return ws.Cells[a.Start.Row, a.Start.Column, a.End.Row, a.End.Column];
        }
        catch
        {
            return null;
        }
    }
}
