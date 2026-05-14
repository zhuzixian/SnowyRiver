using System.IO;
using ClosedXML.Excel;
using ClosedXML.Excel.Drawings;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.Extensions.Logging;
using SnowyRiver.Documents.Converter.Abstractions;
using SnowyRiver.Documents.Converter.Engines.Charts;
using SnowyRiver.Documents.Converter.Model;

namespace SnowyRiver.Documents.Converter.Engines;

/// <summary>
/// 使用 ClosedXML（MIT，可商用）读取 .xlsx 为 IR；
/// 图表通过 <see cref="DocumentFormat.OpenXml"/> 直接读 ChartPart，再交给 SkiaSharp 自绘。
/// </summary>
internal static class ExcelReader
{
    public static IrDocument Read(Stream source, ConversionOptions options) => Read(source, options, null, null);

    public static IrDocument Read(Stream source, ConversionOptions options, ConversionDiagnostics? diagnostics)
        => Read(source, options, diagnostics, null);

    public static IrDocument Read(Stream source, ConversionOptions options, ConversionDiagnostics? diagnostics, string? sourceFileName)
    {
        var log = options.LoggerFactory?.CreateLogger("SnowyRiver.Documents.Converter.ExcelReader");
        var ir = new IrDocument
        {
            Title = options.Title,
            Author = options.Author,
            PageWidthPt = 842,
            PageHeightPt = 595,
            MarginPt = 18,
        };

        // ClosedXML 与 SpreadsheetDocument 都需要可寻址流；如不是先复制到 MemoryStream。
        Stream wbStream = source;
        MemoryStream? owned = null;
        if (!source.CanSeek)
        {
            owned = new MemoryStream();
            source.CopyTo(owned);
            owned.Position = 0;
            wbStream = owned;
        }
        long origPos = wbStream.Position;

        try
        {
            using var workbook = new XLWorkbook(wbStream);

            // 公式重算回退：xlsx 缓存值缺失时，调用 ClosedXML 内置计算引擎补齐。
            // 失败时静默忽略，避免影响整体导出。
            if (NeedsFormulaRecalc(workbook))
            {
                try { workbook.RecalculateAllFormulas(); }
                catch (Exception ex) { log?.LogDebug(ex, "Recalculate formulas failed; continuing with cached values."); }
            }

            // 复位流后，提前用 OpenXML 抽取所有图表 -> PNG，按工作表名分组。
            var chartImages = options.RenderExcelCharts
                ? ExtractChartsByOpenXml(wbStream, origPos, options)
                : new Dictionary<string, List<byte[]>>(StringComparer.OrdinalIgnoreCase);

            bool first = true;
            int sheetIndex = 0;
            foreach (var ws in workbook.Worksheets)
            {
                if (ws.Visibility != XLWorksheetVisibility.Visible) continue;
                sheetIndex++;
                if (!first) ir.Blocks.Add(IrBlock.NewPage());
                first = false;

                // 为每个工作表生成一个节属性：页面方向、页边距、适配页数、缩放。
                var sec = BuildSectionFromPageSetup(ws, sourceFileName);
                if (sec != null)
                {
                    // 打印标题行/列（Print_Titles）
                    try
                    {
                        var pt = ws.PageSetup?.PrintAreas;
                    }
                    catch { }
                    try
                    {
                        var rng = ws.PageSetup?.FirstRowToRepeatAtTop;
                        var rngE = ws.PageSetup?.LastRowToRepeatAtTop;
                        if (rng.HasValue && rngE.HasValue && rng.Value > 0 && rngE.Value >= rng.Value)
                            sec.PrintTitleRows = (rng.Value, rngE.Value);
                        var cf = ws.PageSetup?.FirstColumnToRepeatAtLeft;
                        var cl = ws.PageSetup?.LastColumnToRepeatAtLeft;
                        if (cf.HasValue && cl.HasValue && cf.Value > 0 && cl.Value >= cf.Value)
                            sec.PrintTitleCols = (cf.Value, cl.Value);
                    }
                    catch (Exception ex) { log?.LogDebug(ex, "Read print titles failed for sheet '{Sheet}'.", ws.Name); }

                    // 水平分页符
                    try
                    {
                        foreach (var rb in ws.PageSetup?.RowBreaks ?? Enumerable.Empty<int>())
                            if (rb > 0) sec.HorizontalPageBreaks.Add(rb);
                    }
                    catch { }

                    // 垂直分页符
                    try
                    {
                        foreach (var cb in ws.PageSetup?.ColumnBreaks ?? Enumerable.Empty<int>())
                            if (cb > 0) sec.VerticalPageBreaks.Add(cb);
                    }
                    catch { }

                    // 网格线打印
                    try { sec.PrintGridlines = ws.PageSetup?.ShowGridlines; } catch { }

                    ir.Blocks.Add(IrBlock.Of(sec));
                }

                ir.Blocks.Add(IrBlock.Of(new IrParagraph
                {
                    IsHeading = true,
                    HeadingLevel = 1,
                    AnchorId = MakeSheetAnchor(sheetIndex, ws.Name),
                    Runs = { new IrRun { Text = ws.Name, Bold = true, FontSize = 14 } },
                }));

                var used = ws.RangeUsed();
                if (used != null)
                {
                    var firstAddr = used.RangeAddress.FirstAddress;
                    var lastAddr = used.RangeAddress.LastAddress;
                    int fromRow = firstAddr.RowNumber;
                    int toRow = lastAddr.RowNumber;
                    int fromCol = firstAddr.ColumnNumber;
                    int toCol = lastAddr.ColumnNumber;

                    var table = new IrTable();
                    // 表头重复改在 used 区扫描完毕后由 PrintTitleRows 同步逻辑统一处理。

                    // 列宽精度：Excel 列宽单位 = 默认字体下 '0' 字符在 96DPI 下的像素宽度。
                    // 公式：pixels = Truncate(((256*width + Truncate(128/MDW))/256) * MDW)
                    // 然后 px -> pt：pt = px * 72 / 96。
                    double mdw = MeasureMaxDigitWidthPx(workbook);
                    var visibleCols = new List<int>();
                    for (int c = fromCol; c <= toCol; c++)
                    {
                        if (options.SkipHiddenExcelRowsAndColumns && ws.Column(c).IsHidden) continue;
                        visibleCols.Add(c);
                        var w = ws.Column(c).Width;
                        if (w <= 0) w = workbook.ColumnWidth;
                        double px = Math.Truncate(((256 * w + Math.Truncate(128.0 / mdw)) / 256.0) * mdw);
                        table.ColumnWidthsPt.Add(px * 72.0 / 96.0);
                    }

                    var anchors = new Dictionary<(int r, int c), (int rs, int cs)>();
                    var suppressed = new HashSet<(int r, int c)>();
                    var visibleRows = new List<int>();
                    foreach (var range in ws.MergedRanges)
                    {
                        var fr = range.RangeAddress.FirstAddress.RowNumber;
                        var fc = range.RangeAddress.FirstAddress.ColumnNumber;
                        var lr = range.RangeAddress.LastAddress.RowNumber;
                        var lc = range.RangeAddress.LastAddress.ColumnNumber;
                        anchors[(fr, fc)] = (lr - fr + 1, lc - fc + 1);
                        for (int rr = fr; rr <= lr; rr++)
                            for (int cc = fc; cc <= lc; cc++)
                                if (rr != fr || cc != fc) suppressed.Add((rr, cc));
                    }

                    for (int r = fromRow; r <= toRow; r++)
                    {
                        if (options.SkipHiddenExcelRowsAndColumns && ws.Row(r).IsHidden) continue;
                        visibleRows.Add(r);
                        var row = new IrRow();
                        var rh = ws.Row(r).Height;
                        if (rh > 0) row.HeightPt = rh;

                        foreach (var c in visibleCols)
                        {
                            var xc = ws.Cell(r, c);
                            var formatted = xc.GetFormattedString() ?? string.Empty;
                            var cell = new IrCell { Text = formatted };
                            if (options.EnableExcelNumberFormat) cell.FormattedText = formatted;
                            if (suppressed.Contains((r, c))) cell.Suppressed = true;
                            if (anchors.TryGetValue((r, c), out var span))
                            {
                                cell.RowSpan = span.rs;
                                cell.ColSpan = span.cs;
                            }

                            var st = xc.Style;
                            cell.Style = new IrCellStyle
                            {
                                BackgroundHex = ColorToHex(st.Fill.BackgroundColor),
                                BorderHex = ExtractBorderColor(st.Border),
                                BorderThickness = HasAnyBorder(st.Border) ? 0.5 : 0,
                                HAlign = MapH(st.Alignment.Horizontal),
                                VAlign = MapV(st.Alignment.Vertical),
                                FontFamily = string.IsNullOrEmpty(st.Font.FontName) ? null : st.Font.FontName,
                                FontSize = st.Font.FontSize > 0 ? st.Font.FontSize : null,
                                Bold = st.Font.Bold,
                                Italic = st.Font.Italic,
                                FontColorHex = ColorToHex(st.Font.FontColor),
                                WrapText = st.Alignment.WrapText,
                            };
                            ApplyConditionalFormats(ws, xc, cell.Style);

                            // 单元格超链接（外部 URL）
                            try
                            {
                                if (xc.HasHyperlink)
                                {
                                    var hl = xc.GetHyperlink();
                                    if (hl.IsExternal && hl.ExternalAddress != null)
                                        cell.Hyperlink = hl.ExternalAddress.ToString();
                                    else if (!hl.IsExternal && !string.IsNullOrEmpty(hl.InternalAddress))
                                        cell.Hyperlink = "#" + hl.InternalAddress;
                                }
                            }
                            catch { /* ignore malformed hyperlink */ }

                            // 单元格批注 / 注释
                            try
                            {
                                if (xc.HasComment)
                                {
                                    var ct = xc.GetComment()?.Text;
                                    if (!string.IsNullOrWhiteSpace(ct)) cell.Comment = ct;
                                }
                            }
                            catch { /* ignore */ }

                            row.Cells.Add(cell);
                        }
                        table.Rows.Add(row);
                    }

                    // 把节级 PrintTitles / 分页符 / 网格线同步到表（按可见行/列索引转换）。
                    if (sec != null)
                    {
                        if (sec.PrintTitleRows is { } ptr2)
                        {
                            int firstVisibleIdx = visibleRows.FindIndex(rn => rn >= ptr2.FromRow);
                            int lastVisibleIdx = visibleRows.FindLastIndex(rn => rn <= ptr2.ToRow);
                            if (firstVisibleIdx == 0 && lastVisibleIdx >= 0)
                            {
                                int count = lastVisibleIdx - firstVisibleIdx + 1;
                                table.PrintTitleRowCount = count;
                                if (options.RepeatExcelPrintTitles && table.HeaderRowCount == 0)
                                    table.HeaderRowCount = count;
                            }
                        }
                        if (sec.PrintTitleCols is { } ptc2)
                        {
                            int firstVisibleColIdx = visibleCols.FindIndex(cn => cn >= ptc2.FromCol);
                            int lastVisibleColIdx = visibleCols.FindLastIndex(cn => cn <= ptc2.ToCol);
                            if (firstVisibleColIdx == 0 && lastVisibleColIdx >= 0)
                                table.PrintTitleColCount = lastVisibleColIdx - firstVisibleColIdx + 1;
                        }
                        if (sec.PrintGridlines == true) table.PrintGridlines = true;

                        foreach (var rn in sec.HorizontalPageBreaks)
                        {
                            int idx = visibleRows.FindIndex(v => v == rn + 1);
                            if (idx < 0) idx = visibleRows.FindIndex(v => v > rn);
                            if (idx > 0) table.HorizontalPageBreakRowIndices.Add(idx);
                        }
                        foreach (var cn in sec.VerticalPageBreaks)
                        {
                            int idx = visibleCols.FindIndex(v => v == cn + 1);
                            if (idx < 0) idx = visibleCols.FindIndex(v => v > cn);
                            if (idx > 0) table.VerticalPageBreakColIndices.Add(idx);
                        }
                    }

                    ir.Blocks.Add(IrBlock.Of(table));
                    if (options.EnableExcelAutoFitColumns)
                    {
                        try { ExcelAutoFit.AutoFit(table, options.DefaultFontFamily); }
                        catch (Exception ex) { log?.LogDebug(ex, "Excel auto-fit failed for sheet '{Sheet}'.", ws.Name); }
                    }
                }

                foreach (var pic in ws.Pictures)
                {
                    try
                    {
                        using var ms = new MemoryStream();
                        pic.ImageStream.Position = 0;
                        pic.ImageStream.CopyTo(ms);
                        ir.Blocks.Add(IrBlock.Of(new IrImage
                        {
                            Data = ms.ToArray(),
                            Format = pic.Format.ToString().ToLowerInvariant(),
                        }));
                    }
                    catch (Exception ex) { log?.LogDebug(ex, "Failed to extract embedded picture in worksheet '{Sheet}'.", ws.Name); }
                }

                if (chartImages.TryGetValue(ws.Name, out var imgs))
                {
                    foreach (var png in imgs)
                        ir.Blocks.Add(IrBlock.Of(new IrImage { Data = png, Format = "png" }));
                }
            }
        }
        finally
        {
            owned?.Dispose();
        }

        return ir;
    }

    private static bool NeedsFormulaRecalc(XLWorkbook wb)
    {
        try
        {
            foreach (var ws in wb.Worksheets)
            {
                if (ws.Visibility != XLWorksheetVisibility.Visible) continue;
                foreach (var c in ws.CellsUsed(XLCellsUsedOptions.AllContents, x => x.HasFormula))
                {
                    var v = c.CachedValue;
                    if (v.IsBlank) return true;
                }
            }
        }
        catch { return false; }
        return false;
    }

    /// <summary>
    /// 应用工作表上覆盖该单元格的条件格式规则。仅实现常用且稳定子集：
    /// CellIs（数值/字符串比较）、ColorScale（双色/三色渐变）。
    /// 不支持的规则类型静默跳过，不影响导出。
    /// </summary>
    private static void ApplyConditionalFormats(IXLWorksheet ws, IXLCell cell, IrCellStyle style)
    {
        IEnumerable<IXLConditionalFormat>? formats;
        try { formats = ws.ConditionalFormats; }
        catch { return; }
        if (formats == null) return;

        foreach (var cf in formats)
        {
            try
            {
                if (!RangeContains(cf.Range, cell)) continue;
                ApplyConditionalFormatRule(cf, cell, style);
            }
            catch { /* 单条规则失败不阻断 */ }
        }
    }

    private static bool RangeContains(IXLRange range, IXLCell cell)
    {
        if (range == null) return false;
        var f = range.RangeAddress.FirstAddress;
        var l = range.RangeAddress.LastAddress;
        var r = cell.Address.RowNumber;
        var c = cell.Address.ColumnNumber;
        return r >= f.RowNumber && r <= l.RowNumber && c >= f.ColumnNumber && c <= l.ColumnNumber;
    }

    private static void ApplyConditionalFormatRule(IXLConditionalFormat cf, IXLCell cell, IrCellStyle style)
    {
        var type = cf.ConditionalFormatType;
        var v = cell.CachedValue;
        bool match = false;

        switch (type)
        {
            case XLConditionalFormatType.CellIs:
                match = EvaluateCellIs(cf, v);
                break;
            case XLConditionalFormatType.IsBlank:
                match = v.IsBlank;
                break;
            case XLConditionalFormatType.NotBlank:
                match = !v.IsBlank;
                break;
            case XLConditionalFormatType.IsError:
                match = v.IsError;
                break;
            case XLConditionalFormatType.NotError:
                match = !v.IsError;
                break;
            case XLConditionalFormatType.ContainsText:
                if (v.IsText && cf.Values is { Count: > 0 } cv && cv.TryGetValue(1, out var tv))
                    match = v.GetText().Contains(tv.Value ?? string.Empty, StringComparison.OrdinalIgnoreCase);
                break;
            case XLConditionalFormatType.ColorScale:
                ApplyColorScale(cf, cell, style);
                return;
            case XLConditionalFormatType.DataBar:
                ApplyDataBar(cf, cell, style);
                return;
            case XLConditionalFormatType.IconSet:
                ApplyIconSet(cf, cell, style);
                return;
            default:
                return;
        }

        if (!match) return;

        // 命中规则：用规则定义的样式覆盖
        var s = cf.Style;
        var bg = ColorToHex(s.Fill.BackgroundColor);
        if (!string.IsNullOrEmpty(bg)) style.BackgroundHex = bg;
        var fc = ColorToHex(s.Font.FontColor);
        if (!string.IsNullOrEmpty(fc)) style.FontColorHex = fc;
        if (s.Font.Bold) style.Bold = true;
        if (s.Font.Italic) style.Italic = true;
    }

    private static bool EvaluateCellIs(IXLConditionalFormat cf, XLCellValue v)
    {
        if (!v.IsNumber && !v.IsText) return false;

        double? a = TryNumber(cf, 1);
        double? b = TryNumber(cf, 2);
        string? sa = TryText(cf, 1);

        if (v.IsNumber)
        {
            double n = v.GetNumber();
            return cf.Operator switch
            {
                XLCFOperator.Equal => a.HasValue && n == a.Value,
                XLCFOperator.NotEqual => a.HasValue && n != a.Value,
                XLCFOperator.GreaterThan => a.HasValue && n > a.Value,
                XLCFOperator.LessThan => a.HasValue && n < a.Value,
                XLCFOperator.EqualOrGreaterThan => a.HasValue && n >= a.Value,
                XLCFOperator.EqualOrLessThan => a.HasValue && n <= a.Value,
                XLCFOperator.Between => a.HasValue && b.HasValue && n >= Math.Min(a.Value, b.Value) && n <= Math.Max(a.Value, b.Value),
                XLCFOperator.NotBetween => a.HasValue && b.HasValue && (n < Math.Min(a.Value, b.Value) || n > Math.Max(a.Value, b.Value)),
                _ => false,
            };
        }
        if (v.IsText && !string.IsNullOrEmpty(sa))
        {
            string s = v.GetText();
            return cf.Operator switch
            {
                XLCFOperator.Equal => string.Equals(s, sa, StringComparison.Ordinal),
                XLCFOperator.NotEqual => !string.Equals(s, sa, StringComparison.Ordinal),
                _ => false,
            };
        }
        return false;
    }

    private static double? TryNumber(IXLConditionalFormat cf, int key)
    {
        if (!cf.Values.TryGetValue(key, out var fv) || fv?.Value == null) return null;
        if (double.TryParse(fv.Value, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var d))
            return d;
        return null;
    }

    private static string? TryText(IXLConditionalFormat cf, int key)
        => cf.Values.TryGetValue(key, out var fv) ? fv?.Value : null;

    private static void ApplyColorScale(IXLConditionalFormat cf, IXLCell cell, IrCellStyle style)
    {
        var v = cell.CachedValue;
        if (!v.IsNumber) return;
        double n = v.GetNumber();

        // 收集色阶上的关键节点的数值与颜色
        var stops = new List<(double pos, uint color)>();

        // 用范围内有效数据生成最小/最大
        var range = cf.Range;
        double rMin = double.PositiveInfinity, rMax = double.NegativeInfinity;
        foreach (var c in range.CellsUsed())
        {
            var cv = c.CachedValue;
            if (!cv.IsNumber) continue;
            var d = cv.GetNumber();
            if (d < rMin) rMin = d;
            if (d > rMax) rMax = d;
        }
        if (double.IsInfinity(rMin) || rMin == rMax) return;

        // 简化：取规则颜色 1..3
        for (int i = 1; i <= 3; i++)
        {
            if (!cf.Colors.TryGetValue(i, out var col)) continue;
            var hex = ColorToHex(col);
            if (string.IsNullOrEmpty(hex)) continue;
            uint argb = HexToArgb(hex);
            double pos = (i - 1) / (double)Math.Max(1, cf.Colors.Count - 1);
            stops.Add((pos, argb));
        }
        if (stops.Count < 2) return;

        double t = (n - rMin) / (rMax - rMin);
        t = Math.Clamp(t, 0, 1);
        // 找两个相邻 stop 进行线性插值
        for (int i = 0; i < stops.Count - 1; i++)
        {
            if (t >= stops[i].pos && t <= stops[i + 1].pos)
            {
                double seg = stops[i + 1].pos - stops[i].pos;
                double k = seg <= 0 ? 0 : (t - stops[i].pos) / seg;
                uint mixed = LerpArgb(stops[i].color, stops[i + 1].color, k);
                style.BackgroundHex = $"#{(mixed & 0x00FFFFFF):X6}";
                return;
            }
        }
    }

    /// <summary>DataBar 占位渲染：根据值在范围 min..max 的位置，用规则颜色按比例淡化为单元格背景。</summary>
    private static void ApplyDataBar(IXLConditionalFormat cf, IXLCell cell, IrCellStyle style)
    {
        if (!cell.CachedValue.IsNumber) return;
        double v = cell.CachedValue.GetNumber();
        double min = double.MaxValue, max = double.MinValue;
        try
        {
            foreach (var c in cf.Range.CellsUsed())
            {
                if (!c.CachedValue.IsNumber) continue;
                var n = c.CachedValue.GetNumber();
                if (n < min) min = n;
                if (n > max) max = n;
            }
        }
        catch { return; }
        if (min == double.MaxValue || max <= min) return;

        var hex = ColorToHex(cf.Colors?.Count > 0 ? cf.Colors[1] : null) ?? "#638EC6";
        uint argb = HexToArgb(hex);
        double t = Math.Clamp((v - min) / (max - min), 0.0, 1.0);
        // 用白色按 (1-t) 比例稀释；t 越大颜色越深
        uint mixed = LerpArgb(0xFFFFFFFF, argb, t * 0.85);
        style.BackgroundHex = $"#{(mixed & 0x00FFFFFF):X6}";
    }

    /// <summary>IconSet 占位渲染：根据值在范围中的分位插入 ▲▶▼ 等占位字符到 FormattedText 前缀。</summary>
    private static void ApplyIconSet(IXLConditionalFormat cf, IXLCell cell, IrCellStyle style)
    {
        if (!cell.CachedValue.IsNumber) return;
        double v = cell.CachedValue.GetNumber();
        double min = double.MaxValue, max = double.MinValue;
        try
        {
            foreach (var c in cf.Range.CellsUsed())
            {
                if (!c.CachedValue.IsNumber) continue;
                var n = c.CachedValue.GetNumber();
                if (n < min) min = n;
                if (n > max) max = n;
            }
        }
        catch { return; }
        if (min == double.MaxValue || max <= min) return;

        double t = (v - min) / (max - min);
        string icon = t >= 2.0 / 3 ? "▲" : t >= 1.0 / 3 ? "▶" : "▼";
        string color = t >= 2.0 / 3 ? "#2E7D32" : t >= 1.0 / 3 ? "#F9A825" : "#C62828";
        // 通过字体颜色提示；图标作为前缀写入 FontFamily 不合适，这里只调字体颜色 + 在样式上记一个标识。
        style.FontColorHex = color;
        style.IconPrefix = icon;
    }

    private static uint HexToArgb(string hex)
    {
        var s = hex.TrimStart('#');
        if (s.Length == 6) s = "FF" + s;
        return uint.Parse(s, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
    }

    private static uint LerpArgb(uint a, uint b, double t)
    {
        byte ar = (byte)((a >> 16) & 0xFF), ag = (byte)((a >> 8) & 0xFF), ab = (byte)(a & 0xFF);
        byte br = (byte)((b >> 16) & 0xFF), bg = (byte)((b >> 8) & 0xFF), bb = (byte)(b & 0xFF);
        byte r = (byte)(ar + (br - ar) * t);
        byte g = (byte)(ag + (bg - ag) * t);
        byte bl = (byte)(ab + (bb - ab) * t);
        return 0xFF000000u | ((uint)r << 16) | ((uint)g << 8) | bl;
    }

    private static Dictionary<string, List<byte[]>> ExtractChartsByOpenXml(
        Stream stream, long origPos, ConversionOptions options)
    {
        var map = new Dictionary<string, List<byte[]>>(StringComparer.OrdinalIgnoreCase);
        try
        {
            stream.Position = origPos;
            using var doc = SpreadsheetDocument.Open(stream, isEditable: false);
            var wbPart = doc.WorkbookPart;
            if (wbPart?.Workbook?.Sheets == null) return map;

            foreach (var sheet in wbPart.Workbook.Sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>())
            {
                var rid = sheet.Id?.Value;
                var name = sheet.Name?.Value;
                if (string.IsNullOrEmpty(rid) || string.IsNullOrEmpty(name)) continue;
                if (wbPart.GetPartById(rid) is not WorksheetPart wsPart) continue;

                var list = new List<byte[]>();
                if (wsPart.DrawingsPart != null)
                {
                    foreach (var dp in wsPart.DrawingsPart.ChartParts)
                    {
                        try
                        {
                            var data = ChartDataExtractor.Extract(dp);
                            if (data == null) continue;
                            var png = ChartRenderer.Render(data, options.ChartRenderDpi, options.ChartFontFamily);
                            if (png != null && png.Length > 0) list.Add(png);
                        }
                        catch { }
                    }
                }
                if (list.Count > 0) map[name!] = list;
            }
        }
        catch
        {
            // 静默：图表抽取失败时跳过，不影响其余内容
        }
        finally
        {
            try { stream.Position = origPos; } catch { }
        }
        return map;
    }

    private static string? ColorToHex(XLColor? color)
    {
        if (color == null) return null;
        try
        {
            var c = color.Color;
            if (c.A == 0) return null;
            return $"#{c.R:X2}{c.G:X2}{c.B:X2}";
        }
        catch { return null; }
    }

    private static string? ExtractBorderColor(IXLBorder b)
    {
        if (b == null) return null;
        return ColorToHex(b.TopBorderColor)
            ?? ColorToHex(b.BottomBorderColor)
            ?? ColorToHex(b.LeftBorderColor)
            ?? ColorToHex(b.RightBorderColor)
            ?? "#666666";
    }

    private static bool HasAnyBorder(IXLBorder b)
    {
        if (b == null) return false;
        return b.TopBorder != XLBorderStyleValues.None
            || b.BottomBorder != XLBorderStyleValues.None
            || b.LeftBorder != XLBorderStyleValues.None
            || b.RightBorder != XLBorderStyleValues.None;
    }

    private static HorizontalAlign MapH(XLAlignmentHorizontalValues h) => h switch
    {
        XLAlignmentHorizontalValues.Center or XLAlignmentHorizontalValues.CenterContinuous => HorizontalAlign.Center,
        XLAlignmentHorizontalValues.Right => HorizontalAlign.Right,
        XLAlignmentHorizontalValues.Justify or XLAlignmentHorizontalValues.Distributed => HorizontalAlign.Justify,
        _ => HorizontalAlign.Left,
    };

    private static VerticalAlign MapV(XLAlignmentVerticalValues v) => v switch
    {
        XLAlignmentVerticalValues.Center => VerticalAlign.Middle,
        XLAlignmentVerticalValues.Bottom => VerticalAlign.Bottom,
        _ => VerticalAlign.Top,
    };

    private static string MakeSheetAnchor(int index, string name)
    {
        var safe = string.Concat((name ?? string.Empty).Where(c => char.IsLetterOrDigit(c)));
        if (safe.Length > 24) safe = safe.Substring(0, 24);
        return $"sheet_{index:D3}_{safe}";
    }

    /// <summary>
    /// 从 ClosedXML 工作表的 PageSetup 构建 IR 节属性，覆盖方向、纸张大小、页边距、缩放与适配页数。
    /// </summary>
    private static IrSectionProperties? BuildSectionFromPageSetup(IXLWorksheet ws, string? sourceFileName)
    {
        try
        {
            var ps = ws.PageSetup;
            if (ps == null) return null;
            var sec = new IrSectionProperties
            {
                Orientation = ps.PageOrientation == XLPageOrientation.Landscape
                    ? PageOrientation.Landscape
                    : PageOrientation.Portrait,
                // ClosedXML 的页边距单位是英寸；1 in = 72 pt。
                MarginTopPt = ps.Margins?.Top * 72,
                MarginBottomPt = ps.Margins?.Bottom * 72,
                MarginLeftPt = ps.Margins?.Left * 72,
                MarginRightPt = ps.Margins?.Right * 72,
                Scale = ps.Scale > 0 ? ps.Scale : (int?)null,
                FitToPagesWide = ps.PagesWide > 0 ? ps.PagesWide : (int?)null,
                FitToPagesTall = ps.PagesTall > 0 ? ps.PagesTall : (int?)null,
            };

            // 简化的常见纸张尺寸映射（pt）。未识别时不设，沿用 IR 默认。
            (double w, double h)? size = ps.PaperSize switch
            {
                XLPaperSize.A4Paper => (595, 842),
                XLPaperSize.A3Paper => (842, 1191),
                XLPaperSize.A5Paper => (420, 595),
                XLPaperSize.LetterPaper => (612, 792),
                XLPaperSize.LegalPaper => (612, 1008),
                _ => null,
            };
            if (size.HasValue)
            {
                sec.PageWidthPt = size.Value.w;
                sec.PageHeightPt = size.Value.h;
            }

            // 页眉/页脚：把 ClosedXML 的左/中/右 拼接为 "left\tcenter\tright" 文本，
            // 然后展开 &P/&N/&D/&T/&A/&F 等占位为 IrRun（FieldKind / 静态文本）。
            try
            {
                AddHfParagraph(sec.HeaderParagraphs, ps.Header, ws.Name, isHeader: true, sourceFileName);
                AddHfParagraph(sec.FooterParagraphs, ps.Footer, ws.Name, isHeader: false, sourceFileName);
            }
            catch { }

            return sec;
        }
        catch
        {
            return null;
        }
    }

    private static void AddHfParagraph(List<IrParagraph> sink, IXLHeaderFooter? hf, string sheetName, bool isHeader, string? sourceFileName)
    {
        if (hf == null) return;
        // 取 odd（默认）三段
        string left = SafeHfText(() => hf.Left.GetText(XLHFOccurrence.OddPages));
        string center = SafeHfText(() => hf.Center.GetText(XLHFOccurrence.OddPages));
        string right = SafeHfText(() => hf.Right.GetText(XLHFOccurrence.OddPages));
        if (string.IsNullOrEmpty(left) && string.IsNullOrEmpty(center) && string.IsNullOrEmpty(right)) return;

        // 用三个段落表示左/中/右对齐
        if (!string.IsNullOrEmpty(left))
        {
            var p = new IrParagraph { Alignment = HorizontalAlign.Left };
            ExpandHfText(p, left, sheetName, sourceFileName);
            if (p.Runs.Count > 0) sink.Add(p);
        }
        if (!string.IsNullOrEmpty(center))
        {
            var p = new IrParagraph { Alignment = HorizontalAlign.Center };
            ExpandHfText(p, center, sheetName, sourceFileName);
            if (p.Runs.Count > 0) sink.Add(p);
        }
        if (!string.IsNullOrEmpty(right))
        {
            var p = new IrParagraph { Alignment = HorizontalAlign.Right };
            ExpandHfText(p, right, sheetName, sourceFileName);
            if (p.Runs.Count > 0) sink.Add(p);
        }
    }

    private static string SafeHfText(Func<string?> get)
    {
        try { return get() ?? string.Empty; } catch { return string.Empty; }
    }

    /// <summary>展开 Excel 页眉页脚 &amp; 占位符为 IR Run 集合。</summary>
    private static void ExpandHfText(IrParagraph p, string raw, string sheetName, string? sourceFileName)
    {
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < raw.Length; i++)
        {
            char ch = raw[i];
            if (ch == '&' && i + 1 < raw.Length)
            {
                char code = raw[++i];
                // 跳过字体/样式格式开关：&"...,..."、&数字（字号）、&B/&I/&U/&S/&E/&X/&Y/&K... 等
                if (code == '"')
                {
                    // 跳到下一个引号
                    int end = raw.IndexOf('"', i + 1);
                    i = end < 0 ? raw.Length - 1 : end;
                    continue;
                }
                if (char.IsDigit(code))
                {
                    while (i + 1 < raw.Length && char.IsDigit(raw[i + 1])) i++;
                    continue;
                }
                if ("BIUSEXYK".IndexOf(char.ToUpperInvariant(code)) >= 0)
                {
                    // 颜色 &K 后跟 6 位十六进制
                    if (char.ToUpperInvariant(code) == 'K')
                    {
                        int take = Math.Min(6, raw.Length - i - 1);
                        i += take;
                    }
                    continue;
                }
                // 字段类
                FlushHfStatic(p, sb);
                switch (char.ToUpperInvariant(code))
                {
                    case 'P':
                        p.Runs.Add(new IrRun { Text = "#", FieldKind = RunFieldKind.Page, IsPageNumberField = true });
                        break;
                    case 'N':
                        p.Runs.Add(new IrRun { Text = "#", FieldKind = RunFieldKind.NumPages, IsPageCountField = true });
                        break;
                    case 'D':
                        p.Runs.Add(new IrRun { Text = "#", FieldKind = RunFieldKind.Date });
                        break;
                    case 'T':
                        p.Runs.Add(new IrRun { Text = "#", FieldKind = RunFieldKind.Time });
                        break;
                    case 'A':
                        p.Runs.Add(new IrRun { Text = sheetName, FieldKind = RunFieldKind.SheetName });
                        break;
                    case 'F':
                        p.Runs.Add(new IrRun { Text = string.IsNullOrEmpty(sourceFileName) ? string.Empty : Path.GetFileName(sourceFileName), FieldKind = RunFieldKind.FileName });
                        break;
                    case 'Z':
                        p.Runs.Add(new IrRun { Text = string.IsNullOrEmpty(sourceFileName) ? string.Empty : (Path.GetDirectoryName(sourceFileName) ?? string.Empty), FieldKind = RunFieldKind.FilePath });
                        break;
                    case '&':
                        sb.Append('&');
                        break;
                    case 'L':
                    case 'C':
                    case 'R':
                        // 对齐分隔符已展开为 3 段，这里忽略
                        break;
                    default:
                        // 未识别的代码：忽略
                        break;
                }
                continue;
            }
            sb.Append(ch);
        }
        FlushHfStatic(p, sb);
    }

    private static void FlushHfStatic(IrParagraph p, System.Text.StringBuilder sb)
    {
        if (sb.Length == 0) return;
        p.Runs.Add(new IrRun { Text = sb.ToString() });
        sb.Clear();
    }

    /// <summary>
    /// 估算工作簿默认字体下数字 '0' 的最大像素宽度（96 DPI）。
    /// 由于运行时不一定可用 GDI/SkiaSharp 字体度量，这里采用与 Excel 行为接近的经验值：
    /// Calibri 11pt 约 7px；其他常见字体回退到 7px。
    /// 用户可通过 ConversionOptions（未来扩展）自定义。
    /// </summary>
    private static double MeasureMaxDigitWidthPx(XLWorkbook wb)
    {
        try
        {
            var font = wb.Style?.Font;
            double size = font?.FontSize > 0 ? font.FontSize : 11.0;
            string name = string.IsNullOrEmpty(font?.FontName) ? "Calibri" : font!.FontName;

            // 常见字体的 '0' 字符在 96DPI 下约为：fontSize * factor 像素。
            double factor = name switch
            {
                "Calibri" => 0.55,
                "Arial" => 0.56,
                "Times New Roman" => 0.50,
                "Microsoft YaHei" => 0.55,
                "宋体" or "SimSun" => 0.50,
                _ => 0.55,
            };
            // pt -> px @ 96DPI: px = pt * 96/72
            double px = size * 96.0 / 72.0 * factor;
            // Excel 把 MDW 向下取整为整数像素
            return Math.Max(1.0, Math.Floor(px));
        }
        catch
        {
            return 7.0;
        }
    }
}
