using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Style;
using SnowyRiver.Documents.Converter.Abstractions;
using SnowyRiver.Documents.Converter.Engines.Charts;
using SnowyRiver.Documents.Converter.Model;

namespace SnowyRiver.Documents.Converter.Engines;

/// <summary>
/// 使用 EPPlus 读取 .xlsx 为 IR。
/// 支持：每个工作表为一段以 PageBreak 隔开的内容；保留单元格字体/字号/颜色/背景/边框/对齐/合并；保留列宽与行高；
/// 提取嵌入图片，图表使用 EPPlus 的 GetAsByteArray 渲染缓存。
/// </summary>
internal static class ExcelReader
{
    static ExcelReader()
    {
        // EPPlus 8 必须显式声明许可。本项目按非商业组织使用许可。
        ExcelPackage.License.SetNonCommercialOrganization("SnowyRiver");
    }

    public static IrDocument Read(Stream source, ConversionOptions options)
    {
        var ir = new IrDocument
        {
            Title = options.Title,
            Author = options.Author,
            // Excel 默认横向 A4
            PageWidthPt = 842,
            PageHeightPt = 595,
            MarginPt = 18,
        };

        using var package = new ExcelPackage(source);
        bool first = true;
        foreach (var ws in package.Workbook.Worksheets)
        {
            if (ws.Hidden != eWorkSheetHidden.Visible) continue;
            if (!first) ir.Blocks.Add(IrBlock.NewPage());
            first = false;

            // 工作表名作为标题
            ir.Blocks.Add(IrBlock.Of(new IrParagraph
            {
                IsHeading = true,
                HeadingLevel = 1,
                Runs = { new IrRun { Text = ws.Name, Bold = true, FontSize = 14 } },
            }));

            if (ws.Dimension == null) continue;

            int fromRow = ws.Dimension.Start.Row;
            int toRow = ws.Dimension.End.Row;
            int fromCol = ws.Dimension.Start.Column;
            int toCol = ws.Dimension.End.Column;
            int colCount = toCol - fromCol + 1;

            var table = new IrTable();

            // 列宽（EPPlus 列宽单位 ≈ 字符数，1 字符 ≈ 7px ≈ 5.25pt）
            for (int c = fromCol; c <= toCol; c++)
            {
                var w = ws.Column(c).Width;
                if (w <= 0) w = ws.DefaultColWidth;
                table.ColumnWidthsPt.Add(w * 5.25);
            }

            // 预解析合并区域：建立 (row,col) -> (anchorRow, anchorCol, rowSpan, colSpan)
            var anchors = new Dictionary<(int r, int c), (int rs, int cs)>();
            var suppressedSet = new HashSet<(int r, int c)>();
            foreach (var addr in ws.MergedCells)
            {
                if (addr == null) continue;
                var range = new ExcelAddress(addr);
                anchors[(range.Start.Row, range.Start.Column)] = (range.End.Row - range.Start.Row + 1, range.End.Column - range.Start.Column + 1);
                for (int r = range.Start.Row; r <= range.End.Row; r++)
                {
                    for (int c = range.Start.Column; c <= range.End.Column; c++)
                    {
                        if (r == range.Start.Row && c == range.Start.Column) continue;
                        suppressedSet.Add((r, c));
                    }
                }
            }

            for (int r = fromRow; r <= toRow; r++)
            {
                var row = new IrRow();
                var rowObj = ws.Row(r);
                if (rowObj.Height > 0) row.HeightPt = rowObj.Height;

                for (int c = fromCol; c <= toCol; c++)
                {
                    var cellRange = ws.Cells[r, c];
                    var cell = new IrCell { Text = cellRange.Text ?? string.Empty };
                    if (suppressedSet.Contains((r, c)))
                    {
                        cell.Suppressed = true;
                    }
                    if (anchors.TryGetValue((r, c), out var span))
                    {
                        cell.RowSpan = span.rs;
                        cell.ColSpan = span.cs;
                    }

                    var st = cellRange.Style;
                    cell.Style = new IrCellStyle
                    {
                        BackgroundHex = ExtractFillColor(st.Fill),
                        BorderHex = ExtractBorderColor(st.Border),
                        BorderThickness = HasAnyBorder(st.Border) ? 0.5 : 0,
                        HAlign = MapH(st.HorizontalAlignment),
                        VAlign = MapV(st.VerticalAlignment),
                        FontFamily = string.IsNullOrEmpty(st.Font.Name) ? null : st.Font.Name,
                        FontSize = st.Font.Size > 0 ? st.Font.Size : null,
                        Bold = st.Font.Bold,
                        Italic = st.Font.Italic,
                        FontColorHex = ExtractFontColor(st.Font.Color),
                        WrapText = st.WrapText,
                    };
                    row.Cells.Add(cell);
                }
                table.Rows.Add(row);
            }

            ir.Blocks.Add(IrBlock.Of(table));

            // 嵌入图片 / 图表
            if (ws.Drawings != null)
            {
                foreach (var drawing in ws.Drawings)
                {
                    var bytes = TryGetDrawingBytes(drawing, options, out var format);
                    if (bytes != null && bytes.Length > 0)
                    {
                        ir.Blocks.Add(IrBlock.Of(new IrImage { Data = bytes, Format = format }));
                    }
                }
            }
        }

        return ir;
    }

    private static byte[]? TryGetDrawingBytes(ExcelDrawing drawing, ConversionOptions options, out string? format)
    {
        format = null;
        try
        {
            if (drawing is ExcelPicture pic)
            {
                format = pic.Image?.Type?.ToString().ToLowerInvariant();
                return pic.Image?.ImageBytes;
            }
            if (drawing is ExcelChart chart)
            {
                if (!options.RenderExcelCharts) return null;
                var data = ChartDataExtractor.Extract(chart);
                if (data == null) return null;
                var png = ChartRenderer.Render(data, options.ChartRenderDpi, options.ChartFontFamily);
                if (png != null && png.Length > 0)
                {
                    format = "png";
                    return png;
                }
            }
        }
        catch
        {
            // 部分图表类型 EPPlus 可能不支持，忽略
        }
        return null;
    }

    private static string? ExtractFillColor(ExcelFill fill)
    {
        if (fill == null || fill.PatternType == ExcelFillStyle.None) return null;
        var rgb = fill.BackgroundColor?.Rgb;
        if (string.IsNullOrEmpty(rgb)) return null;
        return NormalizeRgb(rgb);
    }

    private static string? ExtractFontColor(ExcelColor color)
    {
        if (color == null) return null;
        var rgb = color.Rgb;
        if (string.IsNullOrEmpty(rgb)) return null;
        return NormalizeRgb(rgb);
    }

    private static string? ExtractBorderColor(Border border)
    {
        if (border == null) return null;
        var rgb = border.Top?.Color?.Rgb
                  ?? border.Bottom?.Color?.Rgb
                  ?? border.Left?.Color?.Rgb
                  ?? border.Right?.Color?.Rgb;
        if (string.IsNullOrEmpty(rgb)) return "#666666";
        return NormalizeRgb(rgb);
    }

    private static bool HasAnyBorder(Border b)
    {
        if (b == null) return false;
        return b.Top?.Style != ExcelBorderStyle.None
               || b.Bottom?.Style != ExcelBorderStyle.None
               || b.Left?.Style != ExcelBorderStyle.None
               || b.Right?.Style != ExcelBorderStyle.None;
    }

    private static string? NormalizeRgb(string? rgb)
    {
        if (string.IsNullOrWhiteSpace(rgb)) return null;
        var s = rgb.TrimStart('#');
        if (s.Length == 8) s = s.Substring(2); // 去掉 alpha
        if (s.Length != 6) return null;
        return "#" + s.ToUpperInvariant();
    }

    private static HorizontalAlign MapH(ExcelHorizontalAlignment h) => h switch
    {
        ExcelHorizontalAlignment.Center or ExcelHorizontalAlignment.CenterContinuous => HorizontalAlign.Center,
        ExcelHorizontalAlignment.Right => HorizontalAlign.Right,
        ExcelHorizontalAlignment.Justify or ExcelHorizontalAlignment.Distributed => HorizontalAlign.Justify,
        _ => HorizontalAlign.Left,
    };

    private static VerticalAlign MapV(ExcelVerticalAlignment v) => v switch
    {
        ExcelVerticalAlignment.Center => VerticalAlign.Middle,
        ExcelVerticalAlignment.Bottom => VerticalAlign.Bottom,
        _ => VerticalAlign.Top,
    };
}
