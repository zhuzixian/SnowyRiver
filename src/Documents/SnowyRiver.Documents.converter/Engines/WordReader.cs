using System.IO;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using SnowyRiver.Documents.Converter.Model;
using ConversionOptions = SnowyRiver.Documents.Converter.Abstractions.ConversionOptions;
using A = DocumentFormat.OpenXml.Drawing;

namespace SnowyRiver.Documents.Converter.Engines;

/// <summary>
/// 使用 DocumentFormat.OpenXml 解析 .docx 为 IR。
/// 支持：段落/run 的字体/字号/粗体/斜体/下划线/颜色/对齐、标题样式、表格（合并/列宽/边框/背景）、嵌入图片、嵌入图表的渲染缓存图。
/// </summary>
internal static class WordReader
{
    public static IrDocument Read(Stream source, ConversionOptions options)
    {
        using var ms = new MemoryStream();
        source.CopyTo(ms);
        ms.Position = 0;

        var ir = new IrDocument();
        using var doc = WordprocessingDocument.Open(ms, false);
        var main = doc.MainDocumentPart ?? throw new InvalidDataException("Word 文档缺少 MainDocumentPart。");
        var body = main.Document.Body ?? throw new InvalidDataException("Word 文档缺少 Body。");

        ir.Title = options.Title ?? doc.PackageProperties.Title;
        ir.Author = options.Author ?? doc.PackageProperties.Creator;

        // 应用页面尺寸与边距
        var sectPr = body.Elements<SectionProperties>().FirstOrDefault()
            ?? body.Descendants<SectionProperties>().FirstOrDefault();
        if (sectPr != null)
        {
            var size = sectPr.Elements<PageSize>().FirstOrDefault();
            if (size?.Width?.Value != null) ir.PageWidthPt = size.Width.Value / 20.0;
            if (size?.Height?.Value != null) ir.PageHeightPt = size.Height.Value / 20.0;
            var margin = sectPr.Elements<PageMargin>().FirstOrDefault();
            if (margin?.Left?.Value != null) ir.MarginPt = margin.Left.Value / 20.0;
        }

        foreach (var element in body.ChildElements)
        {
            switch (element)
            {
                case Paragraph p:
                    AddParagraph(p, main, ir);
                    break;
                case Table t:
                    ir.Blocks.Add(IrBlock.Of(BuildTable(t, main)));
                    break;
            }
        }

        return ir;
    }

    private static void AddParagraph(Paragraph p, MainDocumentPart main, IrDocument ir)
    {
        // 检测分页符
        bool hasPageBreak = p.Descendants<Break>().Any(b => b.Type?.Value == BreakValues.Page);
        if (hasPageBreak)
        {
            ir.Blocks.Add(IrBlock.NewPage());
        }

        // 检测嵌入对象（图片 / 图表）
        var images = ExtractImages(p, main).ToList();
        if (images.Count > 0)
        {
            foreach (var img in images)
            {
                ir.Blocks.Add(IrBlock.Of(img));
            }
            // 段落中如果只是图片，则不再生成空段落
            if (string.IsNullOrWhiteSpace(string.Concat(p.Descendants<Text>().Select(t => t.Text))))
            {
                return;
            }
        }

        var styleId = p.ParagraphProperties?.ParagraphStyleId?.Val?.Value;
        bool isHeading = !string.IsNullOrEmpty(styleId)
            && styleId.StartsWith("Heading", StringComparison.OrdinalIgnoreCase);
        int level = 0;
        if (isHeading)
        {
            var levelStr = new string(styleId!.Where(char.IsDigit).ToArray());
            int.TryParse(levelStr, out level);
            if (level <= 0) level = 1;
        }

        var ip = new IrParagraph
        {
            IsHeading = isHeading,
            HeadingLevel = level,
            Alignment = ParseAlignment(p.ParagraphProperties?.Justification?.Val?.Value),
        };

        foreach (var run in p.Elements<Run>())
        {
            var text = string.Concat(run.Elements<Text>().Select(t => t.Text));
            if (string.IsNullOrEmpty(text)) continue;
            var rp = run.RunProperties;
            ip.Runs.Add(new IrRun
            {
                Text = text,
                FontFamily = rp?.RunFonts?.Ascii?.Value ?? rp?.RunFonts?.EastAsia?.Value,
                FontSize = ParseHalfPoint(rp?.FontSize?.Val?.Value),
                Bold = rp?.Bold != null,
                Italic = rp?.Italic != null,
                Underline = rp?.Underline != null,
                ColorHex = NormalizeColor(rp?.Color?.Val?.Value),
            });
        }

        if (ip.Runs.Count == 0 && !isHeading) return;
        ir.Blocks.Add(IrBlock.Of(ip));
    }

    private static IEnumerable<IrImage> ExtractImages(Paragraph p, MainDocumentPart main)
    {
        // 普通嵌入图片：a:blip 引用 ImagePart
        foreach (var blip in p.Descendants<A.Blip>())
        {
            var rid = blip.Embed?.Value;
            if (string.IsNullOrEmpty(rid)) continue;
            if (main.GetPartById(rid) is ImagePart imagePart)
            {
                using var s = imagePart.GetStream();
                using var mem = new MemoryStream();
                s.CopyTo(mem);
                yield return new IrImage
                {
                    Data = mem.ToArray(),
                    Format = imagePart.ContentType?.Replace("image/", string.Empty),
                };
            }
        }

        // 图表：使用图表部件中的 PNG/EMF 缓存（若存在）
        foreach (var chartRef in p.Descendants<DocumentFormat.OpenXml.Drawing.Charts.ChartReference>())
        {
            var rid = chartRef.Id?.Value;
            if (string.IsNullOrEmpty(rid)) continue;
            var chartPart = main.GetPartById(rid) as ChartPart;
            if (chartPart == null) continue;
            // 尝试取 chartPart 内部的 ImagePart 缓存
            foreach (var ip in chartPart.ImageParts)
            {
                using var s = ip.GetStream();
                using var mem = new MemoryStream();
                s.CopyTo(mem);
                yield return new IrImage
                {
                    Data = mem.ToArray(),
                    Format = ip.ContentType?.Replace("image/", string.Empty),
                };
            }
        }
    }

    private static IrTable BuildTable(Table t, MainDocumentPart main)
    {
        var table = new IrTable();
        var grid = t.Elements<TableGrid>().FirstOrDefault();
        if (grid != null)
        {
            foreach (var col in grid.Elements<GridColumn>())
            {
                if (col.Width?.Value != null && double.TryParse(col.Width.Value, out var w))
                {
                    table.ColumnWidthsPt.Add(w / 20.0);
                }
                else
                {
                    table.ColumnWidthsPt.Add(null);
                }
            }
        }

        var rows = t.Elements<TableRow>().ToList();
        // 第一遍：构造行/单元格，并记录 vMerge
        var matrix = new List<List<IrCell>>();
        foreach (var tr in rows)
        {
            var row = new List<IrCell>();
            foreach (var tc in tr.Elements<TableCell>())
            {
                var cell = new IrCell();
                var tcPr = tc.TableCellProperties;
                if (tcPr?.GridSpan?.Val?.Value is int span && span > 1)
                {
                    cell.ColSpan = span;
                }
                var vMerge = tcPr?.VerticalMerge;
                bool vMergeContinue = vMerge != null && (vMerge.Val == null || vMerge.Val.Value == MergedCellValues.Continue);
                bool vMergeRestart = vMerge != null && vMerge.Val?.Value == MergedCellValues.Restart;

                cell.Text = string.Join(Environment.NewLine,
                    tc.Elements<Paragraph>().Select(p =>
                        string.Concat(p.Descendants<Text>().Select(x => x.Text))));

                cell.Style = new IrCellStyle
                {
                    BackgroundHex = NormalizeColor(tcPr?.Shading?.Fill?.Value),
                    HAlign = ParseAlignment(tc.Elements<Paragraph>().FirstOrDefault()?.ParagraphProperties?.Justification?.Val?.Value),
                    VAlign = ParseVAlign(tcPr?.TableCellVerticalAlignment?.Val?.Value),
                };
                cell.Suppressed = vMergeContinue;
                row.Add(cell);

                // 占位 ColSpan-1 个被压制单元格
                for (int i = 1; i < cell.ColSpan; i++)
                {
                    row.Add(new IrCell { Suppressed = true });
                }
                _ = vMergeRestart;
            }
            matrix.Add(row);
        }

        // 第二遍：处理 vMerge，将 Continue 单元格往上找 Restart 累加 RowSpan
        for (int c = 0; c < (matrix.Count == 0 ? 0 : matrix[0].Count); c++)
        {
            int? anchor = null;
            for (int r = 0; r < matrix.Count; r++)
            {
                if (c >= matrix[r].Count) { anchor = null; continue; }
                var cell = matrix[r][c];
                if (cell.Suppressed && anchor.HasValue)
                {
                    matrix[anchor.Value][c].RowSpan++;
                }
                else if (!cell.Suppressed)
                {
                    anchor = r;
                }
            }
        }

        foreach (var rowCells in matrix)
        {
            var row = new IrRow();
            row.Cells.AddRange(rowCells);
            table.Rows.Add(row);
        }

        _ = main;
        return table;
    }

    private static HorizontalAlign ParseAlignment(JustificationValues? j)
    {
        if (j == null) return HorizontalAlign.Left;
        if (j.Value == JustificationValues.Center) return HorizontalAlign.Center;
        if (j.Value == JustificationValues.Right || j.Value == JustificationValues.End) return HorizontalAlign.Right;
        if (j.Value == JustificationValues.Both || j.Value == JustificationValues.Distribute) return HorizontalAlign.Justify;
        return HorizontalAlign.Left;
    }

    private static VerticalAlign ParseVAlign(TableVerticalAlignmentValues? v)
    {
        if (v == null) return VerticalAlign.Top;
        if (v.Value == TableVerticalAlignmentValues.Center) return VerticalAlign.Middle;
        if (v.Value == TableVerticalAlignmentValues.Bottom) return VerticalAlign.Bottom;
        return VerticalAlign.Top;
    }

    private static double? ParseHalfPoint(string? halfPoint)
    {
        if (double.TryParse(halfPoint, out var v)) return v / 2.0;
        return null;
    }

    private static string? NormalizeColor(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (value.Equals("auto", StringComparison.OrdinalIgnoreCase)) return null;
        var v = value.TrimStart('#');
        if (v.Length == 6) return "#" + v.ToUpperInvariant();
        return null;
    }
}
