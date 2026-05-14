using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using A = DocumentFormat.OpenXml.Drawing;
using C = DocumentFormat.OpenXml.Drawing.Charts;

namespace SnowyRiver.Documents.Converter.Engines.Charts;

/// <summary>
/// 直接从 OpenXML <see cref="ChartPart"/>（即 xl/charts/chart*.xml 的 c:chartSpace）提取
/// 通用 <see cref="ChartData"/>。优点：完全开源 (MIT)，可商用，且数据来源是绘图缓存
/// （c:numCache / c:strCache），即便引用的工作表数据缺失也能拿到原始数值。
/// 为了兼容 OpenXml SDK 3.x 强类型属性差异，这里统一使用 GetFirstChild&lt;T&gt;() 与 Elements&lt;T&gt;()。
/// </summary>
internal static class ChartDataExtractor
{
    public static ChartData? Extract(ChartPart part)
    {
        if (part?.ChartSpace == null) return null;
        var chart = part.ChartSpace.GetFirstChild<C.Chart>();
        var plotArea = chart?.GetFirstChild<C.PlotArea>();
        if (chart == null || plotArea == null) return null;

        var data = new ChartData
        {
            Title = ExtractTitle(chart),
            Legend = ExtractLegend(chart),
            AxisTitleX = ExtractAxisTitle(plotArea.Elements<C.CategoryAxis>().FirstOrDefault()?.GetFirstChild<C.Title>())
                          ?? ExtractAxisTitle(plotArea.Elements<C.DateAxis>().FirstOrDefault()?.GetFirstChild<C.Title>()),
            AxisTitleY = ExtractAxisTitle(plotArea.Elements<C.ValueAxis>().FirstOrDefault()?.GetFirstChild<C.Title>()),
        };

        foreach (var element in plotArea.ChildElements)
        {
            switch (element)
            {
                case C.BarChart bar:
                    data.Kind = (bar.GetFirstChild<C.BarDirection>()?.Val?.Value == C.BarDirectionValues.Bar)
                        ? ChartKind.BarHorizontal : ChartKind.Column;
                    data.Stack = MapBarGrouping(bar.GetFirstChild<C.BarGrouping>()?.Val?.Value);
                    FillCategorySeries(data, bar.Elements<C.BarChartSeries>());
                    data.ShowDataLabels |= HasDataLabels(bar.GetFirstChild<C.DataLabels>());
                    return Done(data);

                case C.Bar3DChart bar3D:
                    data.Kind = (bar3D.GetFirstChild<C.BarDirection>()?.Val?.Value == C.BarDirectionValues.Bar)
                        ? ChartKind.BarHorizontal : ChartKind.Column;
                    data.Stack = MapBarGrouping(bar3D.GetFirstChild<C.BarGrouping>()?.Val?.Value);
                    FillCategorySeries(data, bar3D.Elements<C.BarChartSeries>());
                    data.ShowDataLabels |= HasDataLabels(bar3D.GetFirstChild<C.DataLabels>());
                    return Done(data);

                case C.LineChart line:
                    data.Kind = ChartKind.Line;
                    data.Stack = MapGrouping(line.GetFirstChild<C.Grouping>()?.Val?.Value);
                    FillCategorySeries(data, line.Elements<C.LineChartSeries>());
                    data.ShowDataLabels |= HasDataLabels(line.GetFirstChild<C.DataLabels>());
                    return Done(data);

                case C.Line3DChart line3D:
                    data.Kind = ChartKind.Line;
                    data.Stack = MapGrouping(line3D.GetFirstChild<C.Grouping>()?.Val?.Value);
                    FillCategorySeries(data, line3D.Elements<C.LineChartSeries>());
                    data.ShowDataLabels |= HasDataLabels(line3D.GetFirstChild<C.DataLabels>());
                    return Done(data);

                case C.AreaChart area:
                    data.Kind = ChartKind.Area;
                    data.Stack = MapGrouping(area.GetFirstChild<C.Grouping>()?.Val?.Value);
                    FillCategorySeries(data, area.Elements<C.AreaChartSeries>());
                    data.ShowDataLabels |= HasDataLabels(area.GetFirstChild<C.DataLabels>());
                    return Done(data);

                case C.Area3DChart area3D:
                    data.Kind = ChartKind.Area;
                    data.Stack = MapGrouping(area3D.GetFirstChild<C.Grouping>()?.Val?.Value);
                    FillCategorySeries(data, area3D.Elements<C.AreaChartSeries>());
                    data.ShowDataLabels |= HasDataLabels(area3D.GetFirstChild<C.DataLabels>());
                    return Done(data);

                case C.PieChart pie:
                    data.Kind = ChartKind.Pie;
                    FillPie(data, pie.Elements<C.PieChartSeries>());
                    data.ShowDataLabels |= HasDataLabels(pie.GetFirstChild<C.DataLabels>());
                    return Done(data);

                case C.Pie3DChart pie3D:
                    data.Kind = ChartKind.Pie;
                    FillPie(data, pie3D.Elements<C.PieChartSeries>());
                    data.ShowDataLabels |= HasDataLabels(pie3D.GetFirstChild<C.DataLabels>());
                    return Done(data);

                case C.DoughnutChart doughnut:
                    data.Kind = ChartKind.Doughnut;
                    FillPie(data, doughnut.Elements<C.PieChartSeries>());
                    data.ShowDataLabels |= HasDataLabels(doughnut.GetFirstChild<C.DataLabels>());
                    return Done(data);

                case C.ScatterChart scatter:
                    data.Kind = ChartKind.Scatter;
                    FillScatter(data, scatter.Elements<C.ScatterChartSeries>());
                    data.ShowDataLabels |= HasDataLabels(scatter.GetFirstChild<C.DataLabels>());
                    return Done(data);

                case C.RadarChart radar:
                    data.Kind = ChartKind.Radar;
                    data.RadarFilled = radar.GetFirstChild<C.RadarStyle>()?.Val?.Value == C.RadarStyleValues.Filled;
                    FillCategorySeries(data, radar.Elements<C.RadarChartSeries>());
                    data.ShowDataLabels |= HasDataLabels(radar.GetFirstChild<C.DataLabels>());
                    return Done(data);
            }
        }
        return null;
    }

    private static StackMode MapBarGrouping(C.BarGroupingValues? v)
    {
        if (v == null) return StackMode.Clustered;
        if (v.Value == C.BarGroupingValues.Stacked) return StackMode.Stacked;
        if (v.Value == C.BarGroupingValues.PercentStacked) return StackMode.PercentStacked;
        return StackMode.Clustered;
    }

    private static StackMode MapGrouping(C.GroupingValues? v)
    {
        if (v == null) return StackMode.Clustered;
        if (v.Value == C.GroupingValues.Stacked) return StackMode.Stacked;
        if (v.Value == C.GroupingValues.PercentStacked) return StackMode.PercentStacked;
        return StackMode.Clustered;
    }

    private static ChartData? Done(ChartData d) => d.Series.Count == 0 ? null : d;

    private static string? ExtractTitle(C.Chart chart)
    {
        var t = chart.GetFirstChild<C.Title>();
        return ExtractAxisTitle(t);
    }

    private static string? ExtractAxisTitle(C.Title? t)
    {
        if (t == null) return null;
        var sb = new System.Text.StringBuilder();
        foreach (var run in t.Descendants<DocumentFormat.OpenXml.Drawing.Text>())
            sb.Append(run.Text);
        var s = sb.ToString();
        return string.IsNullOrWhiteSpace(s) ? null : s;
    }

    private static LegendPos ExtractLegend(C.Chart chart)
    {
        var leg = chart.GetFirstChild<C.Legend>();
        if (leg == null) return LegendPos.None;
        var pos = leg.GetFirstChild<C.LegendPosition>()?.Val?.Value;
        if (pos == null) return LegendPos.Right;
        if (pos.Value == C.LegendPositionValues.Right) return LegendPos.Right;
        if (pos.Value == C.LegendPositionValues.Left) return LegendPos.Left;
        if (pos.Value == C.LegendPositionValues.Top) return LegendPos.Top;
        if (pos.Value == C.LegendPositionValues.Bottom) return LegendPos.Bottom;
        if (pos.Value == C.LegendPositionValues.TopRight) return LegendPos.Right;
        return LegendPos.Right;
    }

    private static int ReadIndex(OpenXmlElement series, int fallback)
    {
        var idx = series.GetFirstChild<C.Index>()?.Val?.Value;
        return idx.HasValue ? (int)idx.Value : fallback;
    }

    private static void FillCategorySeries<TSer>(ChartData data, IEnumerable<TSer> series)
        where TSer : OpenXmlElement
    {
        int autoIdx = 0;
        foreach (var s in series)
        {
            int idx = ReadIndex(s, autoIdx);
            var ser = new ChartSeries
            {
                Name = ReadSeriesName(s.GetFirstChild<C.SeriesText>()),
                ColorArgb = OfficePalette.Get(idx),
            };
            ApplySeriesShape(ser, s.GetFirstChild<C.ChartShapeProperties>());
            ApplyDataPoints(ser, s.Elements<C.DataPoint>());
            ser.Trend = ReadTrendline(s.Elements<C.Trendline>().FirstOrDefault());
            autoIdx++;
            ser.Values.AddRange(ReadValues(s.GetFirstChild<C.Values>()));
            if (data.Categories.Count == 0)
                data.Categories.AddRange(ReadCategories(s.GetFirstChild<C.CategoryAxisData>(), ser.Values.Count));
            if (ser.Values.Count > 0) data.Series.Add(ser);
        }
    }

    private static void FillPie(ChartData data, IEnumerable<C.PieChartSeries> series)
    {
        var first = series.FirstOrDefault();
        if (first == null) return;
        var ser = new ChartSeries
        {
            Name = ReadSeriesName(first.GetFirstChild<C.SeriesText>()),
            ColorArgb = OfficePalette.Get(0),
        };
        ApplySeriesShape(ser, first.GetFirstChild<C.ChartShapeProperties>());
        ApplyDataPoints(ser, first.Elements<C.DataPoint>());
        ser.Values.AddRange(ReadValues(first.GetFirstChild<C.Values>()));
        data.Categories.AddRange(ReadCategories(first.GetFirstChild<C.CategoryAxisData>(), ser.Values.Count));
        if (ser.Values.Count > 0) data.Series.Add(ser);
    }

    private static void FillScatter(ChartData data, IEnumerable<C.ScatterChartSeries> series)
    {
        int autoIdx = 0;
        foreach (var s in series)
        {
            int idx = ReadIndex(s, autoIdx);
            var ser = new ChartSeries
            {
                Name = ReadSeriesName(s.GetFirstChild<C.SeriesText>()),
                ColorArgb = OfficePalette.Get(idx),
            };
            ApplySeriesShape(ser, s.GetFirstChild<C.ChartShapeProperties>());
            ApplyDataPoints(ser, s.Elements<C.DataPoint>());
            ser.Trend = ReadTrendline(s.Elements<C.Trendline>().FirstOrDefault());
            autoIdx++;
            var yVals = s.GetFirstChild<C.YValues>();
            ser.Values.AddRange(ReadDoubleSource(yVals));
            var xVals = s.GetFirstChild<C.XValues>();
            ser.XValues.AddRange(ReadDoubleSource(xVals));
            if (ser.XValues.Count == 0)
                for (int i = 0; i < ser.Values.Count; i++) ser.XValues.Add(i + 1);
            if (ser.Values.Count > 0) data.Series.Add(ser);
        }
    }

    /// <summary>从 c:spPr 中提取实色 / 渐变填充。</summary>
    private static void ApplySeriesShape(ChartSeries ser, C.ChartShapeProperties? sp)
    {
        if (sp == null) return;
        var solid = sp.GetFirstChild<A.SolidFill>();
        if (solid != null)
        {
            var c = ReadFillColor(solid);
            if (c.HasValue) { ser.ColorArgb = c.Value; ser.Fill = FillKind.Solid; }
            return;
        }
        var grad = sp.GetFirstChild<A.GradientFill>();
        if (grad != null)
        {
            var stops = grad.GetFirstChild<A.GradientStopList>();
            if (stops != null)
            {
                foreach (var st in stops.Elements<A.GradientStop>())
                {
                    var pos = (st.Position?.Value ?? 0) / 100000.0;
                    var c = ReadFillColor(st);
                    if (c.HasValue) ser.GradientStops.Add(new GradientStop(Math.Clamp(pos, 0, 1), c.Value));
                }
            }
            if (ser.GradientStops.Count >= 2)
            {
                ser.Fill = grad.GetFirstChild<A.PathGradientFill>() != null
                    ? FillKind.RadialGradient : FillKind.LinearGradient;
                ser.ColorArgb = ser.GradientStops[0].ColorArgb;
            }
        }
    }

    /// <summary>从 c:dPt 中按 idx 覆盖颜色。</summary>
    private static void ApplyDataPoints(ChartSeries ser, IEnumerable<C.DataPoint> points)
    {
        foreach (var dp in points)
        {
            var idx = (int?)(dp.GetFirstChild<C.Index>()?.Val?.Value);
            if (!idx.HasValue) continue;
            var sp = dp.GetFirstChild<C.ChartShapeProperties>();
            var solid = sp?.GetFirstChild<A.SolidFill>();
            var c = solid != null ? ReadFillColor(solid) : null;
            if (c.HasValue) ser.PointColors[idx.Value] = c.Value;
        }
    }

    /// <summary>从 a:solidFill 或 a:gs 等容器读取颜色。</summary>
    private static uint? ReadFillColor(OpenXmlElement container)
    {
        var srgb = container.GetFirstChild<A.RgbColorModelHex>();
        if (srgb?.Val?.Value is { Length: 6 } hex && uint.TryParse(hex, System.Globalization.NumberStyles.HexNumber,
                System.Globalization.CultureInfo.InvariantCulture, out var rgb))
        {
            return 0xFF000000u | rgb;
        }
        var scheme = container.GetFirstChild<A.SchemeColor>();
        if (scheme?.Val?.Value != null)
        {
            // 简化映射：accent1..6 -> 调色板对应项
            var v = scheme.Val.Value;
            if (v == A.SchemeColorValues.Accent1) return OfficePalette.Get(0);
            if (v == A.SchemeColorValues.Accent2) return OfficePalette.Get(1);
            if (v == A.SchemeColorValues.Accent3) return OfficePalette.Get(2);
            if (v == A.SchemeColorValues.Accent4) return OfficePalette.Get(3);
            if (v == A.SchemeColorValues.Accent5) return OfficePalette.Get(4);
            if (v == A.SchemeColorValues.Accent6) return OfficePalette.Get(5);
        }
        return null;
    }

    private static Trendline? ReadTrendline(C.Trendline? tl)
    {
        if (tl == null) return null;
        var t = new Trendline();
        var k = tl.GetFirstChild<C.TrendlineType>()?.Val?.Value;
        if (k == null) t.Kind = TrendlineKind.Linear;
        else if (k.Value == C.TrendlineValues.Linear) t.Kind = TrendlineKind.Linear;
        else if (k.Value == C.TrendlineValues.Polynomial) t.Kind = TrendlineKind.Polynomial;
        else if (k.Value == C.TrendlineValues.Logarithmic) t.Kind = TrendlineKind.Logarithmic;
        else if (k.Value == C.TrendlineValues.Exponential) t.Kind = TrendlineKind.Exponential;
        else if (k.Value == C.TrendlineValues.Power) t.Kind = TrendlineKind.Power;
        else if (k.Value == C.TrendlineValues.MovingAverage) t.Kind = TrendlineKind.MovingAverage;
        else t.Kind = TrendlineKind.Linear;

        var ord = tl.GetFirstChild<C.PolynomialOrder>()?.Val?.Value;
        var per = tl.GetFirstChild<C.Period>()?.Val?.Value;
        if (ord.HasValue) t.Order = ord.Value;
        else if (per.HasValue) t.Order = (int)per.Value;

        var sp = tl.GetFirstChild<C.ChartShapeProperties>();
        var line = sp?.GetFirstChild<A.Outline>();
        var solid = line?.GetFirstChild<A.SolidFill>();
        if (solid != null) t.ColorArgb = ReadFillColor(solid);
        return t;
    }

    private static string? ReadSeriesName(C.SeriesText? st)
    {
        if (st == null) return null;
        var strRef = st.GetFirstChild<C.StringReference>();
        if (strRef != null)
        {
            var cache = strRef.GetFirstChild<C.StringCache>();
            var s = cache?.Elements<C.StringPoint>().FirstOrDefault()?.NumericValue?.Text;
            if (!string.IsNullOrEmpty(s)) return s;
        }
        var lit = st.GetFirstChild<C.NumericValue>()?.Text;
        return string.IsNullOrEmpty(lit) ? null : lit;
    }

    private static IEnumerable<double> ReadValues(C.Values? v) => ReadDoubleSource(v);

    /// <summary>从 c:val / c:xVal / c:yVal 中读 numCache 或 numLit。</summary>
    private static IEnumerable<double> ReadDoubleSource(OpenXmlElement? container)
    {
        if (container == null) yield break;
        OpenXmlElement? src = container.GetFirstChild<C.NumberReference>()?.GetFirstChild<C.NumberingCache>();
        src ??= container.GetFirstChild<C.NumberLiteral>();
        if (src == null) yield break;
        foreach (var p in src.Elements<C.NumericPoint>().OrderBy(p => p.Index?.Value ?? uint.MaxValue))
        {
            var t = p.NumericValue?.Text;
            if (string.IsNullOrEmpty(t)) { yield return double.NaN; continue; }
            if (double.TryParse(t, System.Globalization.NumberStyles.Any,
                                System.Globalization.CultureInfo.InvariantCulture, out var d))
                yield return d;
            else yield return double.NaN;
        }
    }

    private static IEnumerable<string> ReadCategories(C.CategoryAxisData? cat, int valuesCount)
    {
        if (cat == null) yield break;
        // 字符串引用
        var strCache = cat.GetFirstChild<C.StringReference>()?.GetFirstChild<C.StringCache>();
        if (strCache != null)
        {
            foreach (var s in strCache.Elements<C.StringPoint>().OrderBy(p => p.Index?.Value ?? uint.MaxValue))
                yield return s.NumericValue?.Text ?? string.Empty;
            yield break;
        }
        // 字符串字面量
        var strLit = cat.GetFirstChild<C.StringLiteral>();
        if (strLit != null)
        {
            foreach (var s in strLit.Elements<C.StringPoint>().OrderBy(p => p.Index?.Value ?? uint.MaxValue))
                yield return s.NumericValue?.Text ?? string.Empty;
            yield break;
        }
        // 数值类别
        var numCache = cat.GetFirstChild<C.NumberReference>()?.GetFirstChild<C.NumberingCache>();
        if (numCache != null)
        {
            foreach (var p in numCache.Elements<C.NumericPoint>().OrderBy(p => p.Index?.Value ?? uint.MaxValue))
                yield return p.NumericValue?.Text ?? string.Empty;
            yield break;
        }
        var numLit = cat.GetFirstChild<C.NumberLiteral>();
        if (numLit != null)
        {
            foreach (var p in numLit.Elements<C.NumericPoint>().OrderBy(p => p.Index?.Value ?? uint.MaxValue))
                yield return p.NumericValue?.Text ?? string.Empty;
            yield break;
        }
        for (int i = 0; i < valuesCount; i++)
            yield return (i + 1).ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    private static bool HasDataLabels(C.DataLabels? dls)
    {
        if (dls == null) return false;
        return (dls.GetFirstChild<C.ShowValue>()?.Val?.Value ?? false)
            || (dls.GetFirstChild<C.ShowCategoryName>()?.Val?.Value ?? false)
            || (dls.GetFirstChild<C.ShowSeriesName>()?.Val?.Value ?? false)
            || (dls.GetFirstChild<C.ShowPercent>()?.Val?.Value ?? false);
    }
}
