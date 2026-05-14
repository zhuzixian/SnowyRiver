using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
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
        };

        foreach (var element in plotArea.ChildElements)
        {
            switch (element)
            {
                case C.BarChart bar:
                    data.Kind = (bar.GetFirstChild<C.BarDirection>()?.Val?.Value == C.BarDirectionValues.Bar)
                        ? ChartKind.BarHorizontal : ChartKind.Column;
                    FillCategorySeries(data, bar.Elements<C.BarChartSeries>());
                    data.ShowDataLabels |= HasDataLabels(bar.GetFirstChild<C.DataLabels>());
                    return Done(data);

                case C.Bar3DChart bar3D:
                    data.Kind = (bar3D.GetFirstChild<C.BarDirection>()?.Val?.Value == C.BarDirectionValues.Bar)
                        ? ChartKind.BarHorizontal : ChartKind.Column;
                    FillCategorySeries(data, bar3D.Elements<C.BarChartSeries>());
                    data.ShowDataLabels |= HasDataLabels(bar3D.GetFirstChild<C.DataLabels>());
                    return Done(data);

                case C.LineChart line:
                    data.Kind = ChartKind.Line;
                    FillCategorySeries(data, line.Elements<C.LineChartSeries>());
                    data.ShowDataLabels |= HasDataLabels(line.GetFirstChild<C.DataLabels>());
                    return Done(data);

                case C.Line3DChart line3D:
                    data.Kind = ChartKind.Line;
                    FillCategorySeries(data, line3D.Elements<C.LineChartSeries>());
                    data.ShowDataLabels |= HasDataLabels(line3D.GetFirstChild<C.DataLabels>());
                    return Done(data);

                case C.AreaChart area:
                    data.Kind = ChartKind.Area;
                    FillCategorySeries(data, area.Elements<C.AreaChartSeries>());
                    data.ShowDataLabels |= HasDataLabels(area.GetFirstChild<C.DataLabels>());
                    return Done(data);

                case C.Area3DChart area3D:
                    data.Kind = ChartKind.Area;
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
            }
        }
        return null;
    }

    private static ChartData? Done(ChartData d) => d.Series.Count == 0 ? null : d;

    private static string? ExtractTitle(C.Chart chart)
    {
        var t = chart.GetFirstChild<C.Title>();
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
            var ser = new ChartSeries
            {
                Name = ReadSeriesName(s.GetFirstChild<C.SeriesText>()),
                ColorArgb = OfficePalette.Get(ReadIndex(s, autoIdx)),
            };
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
        ser.Values.AddRange(ReadValues(first.GetFirstChild<C.Values>()));
        data.Categories.AddRange(ReadCategories(first.GetFirstChild<C.CategoryAxisData>(), ser.Values.Count));
        if (ser.Values.Count > 0) data.Series.Add(ser);
    }

    private static void FillScatter(ChartData data, IEnumerable<C.ScatterChartSeries> series)
    {
        int autoIdx = 0;
        foreach (var s in series)
        {
            var ser = new ChartSeries
            {
                Name = ReadSeriesName(s.GetFirstChild<C.SeriesText>()),
                ColorArgb = OfficePalette.Get(ReadIndex(s, autoIdx)),
            };
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
