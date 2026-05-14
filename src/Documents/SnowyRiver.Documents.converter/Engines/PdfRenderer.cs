using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SnowyRiver.Documents.Converter.Abstractions;
using SnowyRiver.Documents.Converter.Model;

namespace SnowyRiver.Documents.Converter.Engines;

/// <summary>
/// 使用 QuestPDF 把 IR 渲染为 PDF，尽量保留段落、表格（合并/对齐/颜色/边框）、图片与图表缓存图。
/// </summary>
internal static class PdfRenderer
{
    static PdfRenderer()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public static void Render(IrDocument ir, Stream target, ConversionOptions options)
    {
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size((float)ir.PageWidthPt, (float)ir.PageHeightPt, Unit.Point);
                page.Margin((float)ir.MarginPt, Unit.Point);
                page.DefaultTextStyle(t => t.FontFamily(options.DefaultFontFamily).FontSize(11));

                if (!string.IsNullOrEmpty(ir.Title))
                {
                    page.Header().PaddingBottom(4).Text(ir.Title!).SemiBold();
                }

                page.Content().Column(col =>
                {
                    col.Spacing(6);
                    foreach (var block in ir.Blocks)
                    {
                        if (block.PageBreak != null)
                        {
                            col.Item().PageBreak();
                        }
                        else if (block.Paragraph != null)
                        {
                            RenderParagraph(col.Item(), block.Paragraph, options);
                        }
                        else if (block.Image != null)
                        {
                            RenderImage(col.Item(), block.Image);
                        }
                        else if (block.Table != null)
                        {
                            col.Item().Element(c => RenderTable(c, block.Table, options));
                        }
                    }
                });

                page.Footer().AlignCenter().Text(t =>
                {
                    t.DefaultTextStyle(s => s.FontSize(9).FontColor(Colors.Grey.Medium));
                    t.CurrentPageNumber();
                    t.Span(" / ");
                    t.TotalPages();
                });
            });
        }).WithMetadata(new DocumentMetadata
        {
            Title = ir.Title ?? string.Empty,
            Author = ir.Author ?? string.Empty,
            Creator = "SnowyRiver.Documents.Converter",
        }).GeneratePdf(target);
    }

    private static void RenderParagraph(IContainer container, IrParagraph p, ConversionOptions options)
    {
        var item = p.Alignment switch
        {
            HorizontalAlign.Center => container.AlignCenter(),
            HorizontalAlign.Right => container.AlignRight(),
            _ => container,
        };

        item.Text(span =>
        {
            if (p.IsHeading)
            {
                span.DefaultTextStyle(s => s.Bold().FontSize(p.HeadingLevel switch
                {
                    1 => 18,
                    2 => 16,
                    3 => 14,
                    _ => 12,
                }));
            }

            if (p.Runs.Count == 0)
            {
                span.Span(string.Empty);
                return;
            }

            foreach (var r in p.Runs)
            {
                var s = span.Span(r.Text)
                    .FontFamily(string.IsNullOrEmpty(r.FontFamily) ? options.DefaultFontFamily : r.FontFamily!);
                if (r.FontSize.HasValue) s = s.FontSize((float)r.FontSize.Value);
                if (r.Bold) s = s.Bold();
                if (r.Italic) s = s.Italic();
                if (r.Underline) s = s.Underline();
                if (!string.IsNullOrEmpty(r.ColorHex)) s = s.FontColor(r.ColorHex!);
            }
        });
    }

    private static void RenderImage(IContainer container, IrImage img)
    {
        if (img.Data is null || img.Data.Length == 0) return;
        try
        {
            var item = container.AlignCenter();
            item.Image(img.Data).FitArea();
        }
        catch
        {
            // QuestPDF 当前不支持的格式（如 EMF/WMF）会抛异常 → 退化为占位
            container.AlignCenter().Text("[图像]").FontColor(Colors.Grey.Medium).Italic();
        }
    }

    private static void RenderTable(IContainer container, IrTable table, ConversionOptions options)
    {
        if (table.Rows.Count == 0) return;
        int colCount = Math.Max(1, table.Rows.Max(r => r.Cells.Count));

        container.Table(t =>
        {
            t.ColumnsDefinition(c =>
            {
                for (int i = 0; i < colCount; i++)
                {
                    if (i < table.ColumnWidthsPt.Count && table.ColumnWidthsPt[i].HasValue)
                    {
                        c.ConstantColumn((float)table.ColumnWidthsPt[i]!.Value);
                    }
                    else
                    {
                        c.RelativeColumn();
                    }
                }
            });

            foreach (var row in table.Rows)
            {
                int colIndex = 0;
                foreach (var cell in row.Cells)
                {
                    if (cell.Suppressed)
                    {
                        colIndex++;
                        continue;
                    }

                    var c = t.Cell();
                    if (cell.RowSpan > 1) c = c.RowSpan((uint)cell.RowSpan);
                    if (cell.ColSpan > 1) c = c.ColumnSpan((uint)cell.ColSpan);

                    var box = c
                        .Border((float)cell.Style.BorderThickness)
                        .BorderColor(cell.Style.BorderHex ?? Colors.Grey.Medium);

                    if (!string.IsNullOrEmpty(cell.Style.BackgroundHex))
                    {
                        box = box.Background(cell.Style.BackgroundHex!);
                    }

                    box = cell.Style.VAlign switch
                    {
                        VerticalAlign.Top => box.AlignTop(),
                        VerticalAlign.Bottom => box.AlignBottom(),
                        _ => box.AlignMiddle(),
                    };
                    box = cell.Style.HAlign switch
                    {
                        HorizontalAlign.Center => box.AlignCenter(),
                        HorizontalAlign.Right => box.AlignRight(),
                        _ => box.AlignLeft(),
                    };

                    box.Padding(3).Text(span =>
                    {
                        var s = span.Span(cell.Text ?? string.Empty)
                            .FontFamily(string.IsNullOrEmpty(cell.Style.FontFamily) ? options.DefaultFontFamily : cell.Style.FontFamily!);
                        if (cell.Style.FontSize.HasValue) s = s.FontSize((float)cell.Style.FontSize.Value);
                        if (cell.Style.Bold) s = s.Bold();
                        if (cell.Style.Italic) s = s.Italic();
                        if (!string.IsNullOrEmpty(cell.Style.FontColorHex)) s = s.FontColor(cell.Style.FontColorHex!);
                    });

                    colIndex += cell.ColSpan;
                }
            }
        });
    }
}
