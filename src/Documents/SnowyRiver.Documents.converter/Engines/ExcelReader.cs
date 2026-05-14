using System.IO;
using ClosedXML.Excel;
using ClosedXML.Excel.Drawings;
using DocumentFormat.OpenXml.Packaging;
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
    public static IrDocument Read(Stream source, ConversionOptions options)
    {
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

            // 复位流后，提前用 OpenXML 抽取所有图表 -> PNG，按工作表名分组。
            var chartImages = options.RenderExcelCharts
                ? ExtractChartsByOpenXml(wbStream, origPos, options)
                : new Dictionary<string, List<byte[]>>(StringComparer.OrdinalIgnoreCase);

            bool first = true;
            foreach (var ws in workbook.Worksheets)
            {
                if (ws.Visibility != XLWorksheetVisibility.Visible) continue;
                if (!first) ir.Blocks.Add(IrBlock.NewPage());
                first = false;

                ir.Blocks.Add(IrBlock.Of(new IrParagraph
                {
                    IsHeading = true,
                    HeadingLevel = 1,
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

                    for (int c = fromCol; c <= toCol; c++)
                    {
                        var w = ws.Column(c).Width;
                        if (w <= 0) w = workbook.ColumnWidth;
                        table.ColumnWidthsPt.Add(w * 5.25);
                    }

                    var anchors = new Dictionary<(int r, int c), (int rs, int cs)>();
                    var suppressed = new HashSet<(int r, int c)>();
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
                        var row = new IrRow();
                        var rh = ws.Row(r).Height;
                        if (rh > 0) row.HeightPt = rh;

                        for (int c = fromCol; c <= toCol; c++)
                        {
                            var xc = ws.Cell(r, c);
                            var cell = new IrCell { Text = xc.GetFormattedString() ?? string.Empty };
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
                            row.Cells.Add(cell);
                        }
                        table.Rows.Add(row);
                    }

                    ir.Blocks.Add(IrBlock.Of(table));
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
                    catch { }
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
}
