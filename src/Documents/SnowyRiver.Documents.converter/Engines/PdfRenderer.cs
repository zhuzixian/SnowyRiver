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
    [System.ThreadStatic]
    private static IrDocument? _currentIr;

    static PdfRenderer()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    private static IContainer ApplySemanticHeader(IContainer c, int level) => level switch
    {
        1 => c.SemanticHeader1(),
        2 => c.SemanticHeader2(),
        3 => c.SemanticHeader3(),
        4 => c.SemanticHeader4(),
        5 => c.SemanticHeader5(),
        _ => c.SemanticHeader6(),
    };

    private static int? ResolveNoteNumber(IrRun r)
    {
        var ir = _currentIr;
        if (ir == null) return null;
        if (!string.IsNullOrEmpty(r.FootnoteRef) && ir.Footnotes.TryGetValue(r.FootnoteRef!, out var fn)) return fn.Number;
        if (!string.IsNullOrEmpty(r.EndnoteRef) && ir.Endnotes.TryGetValue(r.EndnoteRef!, out var en)) return en.Number;
        return null;
    }

    public static void Render(IrDocument ir, Stream target, ConversionOptions options) => Render(ir, target, options, null);

    public static void Render(IrDocument ir, Stream target, ConversionOptions options, ConversionDiagnostics? diagnostics)
    {
        _currentIr = ir;
        try
        {
        // 把块按 Section 分组：每遇到一个 IrBlock.Section 就开始新一节（QuestPDF 多 Page）。
        var sections = SplitSections(ir);

        // 收集标题以便生成目录
        var headings = options.GenerateToc
            ? ir.Blocks
                .Where(b => b.Paragraph != null && b.Paragraph.IsHeading
                            && !string.IsNullOrEmpty(b.Paragraph.AnchorId)
                            && b.Paragraph.HeadingLevel <= options.TocMaxLevel)
                .Select(b => b.Paragraph!)
                .ToList()
            : new List<IrParagraph>();

        Document.Create(container =>
        {
            if (headings.Count > 0)
            {
                container.Page(page =>
                {
                    page.Size((float)ir.PageWidthPt, (float)ir.PageHeightPt, Unit.Point);
                    page.Margin((float)ir.MarginPt, Unit.Point);
                    page.DefaultTextStyle(t => t.FontFamily(options.DefaultFontFamily).FontSize(11));
                    page.Content().Column(col =>
                    {
                        col.Spacing(6);
                        col.Item().Text(options.TocTitle ?? "目录").FontSize(18).Bold();
                        foreach (var h in headings)
                        {
                            int indentLevel = Math.Max(0, h.HeadingLevel - 1);
                            col.Item().PaddingLeft(indentLevel * 12).Text(t =>
                            {
                                t.SectionLink(h.PlainText, h.AnchorId!);
                                t.Span(" ");
                                t.BeginPageNumberOfSection(h.AnchorId!);
                            });
                        }
                    });
                });
            }
            foreach (var sec in sections)
            {
                container.Page(page =>
                {
                    var props = sec.Properties;
                    var pw = props?.PageWidthPt ?? ir.PageWidthPt;
                    var ph = props?.PageHeightPt ?? ir.PageHeightPt;
                    if (props != null && props.Orientation == PageOrientation.Landscape && pw < ph)
                    {
                        (pw, ph) = (ph, pw);
                    }
                    page.Size((float)pw, (float)ph, Unit.Point);

                    var mt = (float)(props?.MarginTopPt ?? ir.MarginPt);
                    var mb = (float)(props?.MarginBottomPt ?? ir.MarginPt);
                    var ml = (float)(props?.MarginLeftPt ?? ir.MarginPt);
                    var mr = (float)(props?.MarginRightPt ?? ir.MarginPt);
                    page.MarginTop(mt, Unit.Point);
                    page.MarginBottom(mb, Unit.Point);
                    page.MarginLeft(ml, Unit.Point);
                    page.MarginRight(mr, Unit.Point);

                    page.DefaultTextStyle(t =>
                    {
                        // QuestPDF 2024.3+ 推荐：把主字体与回退字体一并传入 FontFamily(params string[])，
                        // 由内置自动回退机制处理缺字。
                        var families = new List<string> { options.DefaultFontFamily };
                        if (options.FallbackFontFamilies != null)
                        {
                            foreach (var fam in options.FallbackFontFamilies)
                                if (!string.IsNullOrWhiteSpace(fam) && !families.Contains(fam))
                                    families.Add(fam);
                        }
                        return t.FontFamily(families.ToArray()).FontSize(11);
                    });

                    if (!string.IsNullOrWhiteSpace(options.WatermarkText))
                    {
                        page.Background()
                            .AlignCenter().AlignMiddle()
                            .Rotate((float)options.WatermarkRotationDegrees)
                            .Text(options.WatermarkText!)
                            .FontSize((float)options.WatermarkFontSize)
                            .FontColor(options.WatermarkColorHex)
                            .Bold();
                    }

                    bool hasFirstHeader = props != null && props.DifferentFirstPage && props.HeaderFirstParagraphs.Count > 0;
                    bool hasDefaultHeader = props != null && props.HeaderParagraphs.Count > 0;
                    bool hasEvenHeader = props != null && props.DifferentOddEven && props.HeaderEvenParagraphs.Count > 0;
                    if (hasFirstHeader || hasDefaultHeader || hasEvenHeader)
                    {
                        page.Header().PaddingBottom(4).Column(hc =>
                        {
                            hc.Spacing(2);
                            if (hasFirstHeader)
                            {
                                hc.Item().ShowIf(ctx => ctx.PageNumber == 1).Column(c =>
                                {
                                    c.Spacing(2);
                                    foreach (var hp in props!.HeaderFirstParagraphs)
                                        RenderParagraph(c.Item(), hp, options);
                                });
                            }
                            if (hasDefaultHeader)
                            {
                                hc.Item().ShowIf(ctx =>
                                        (!hasFirstHeader || ctx.PageNumber > 1) &&
                                        (!hasEvenHeader || ctx.PageNumber % 2 == 1))
                                    .Column(c =>
                                    {
                                        c.Spacing(2);
                                        foreach (var hp in props!.HeaderParagraphs)
                                            RenderParagraph(c.Item(), hp, options);
                                    });
                            }
                            if (hasEvenHeader)
                            {
                                hc.Item().ShowIf(ctx => ctx.PageNumber % 2 == 0).Column(c =>
                                {
                                    c.Spacing(2);
                                    foreach (var hp in props!.HeaderEvenParagraphs)
                                        RenderParagraph(c.Item(), hp, options);
                                });
                            }
                        });
                    }
                    else
                    {
                        var headerText = props?.HeaderText ?? ir.Title;
                        if (!string.IsNullOrEmpty(headerText))
                        {
                            page.Header().PaddingBottom(4).Text(headerText!).SemiBold();
                        }
                    }

                    page.Content().Column(col =>
                    {
                        col.Spacing(6);
                        int colCount = Math.Max(1, props?.ColumnCount ?? 1);
                        double contentWidthPt = Math.Max(1, pw - ml - mr);
                        if (colCount <= 1)
                        {
                            foreach (var block in sec.Blocks)
                                RenderBlock(col, block, ir, options, props, contentWidthPt);
                        }
                        else
                        {
                            // 近似多列：将 sec.Blocks 按顺序均分到 N 桶，再用 Row 等宽并排。
                            // 注意：单列内仍是流式，但桶之间无法跨列回流。
                            float gap = (float)(props?.ColumnSpacingPt ?? 12);
                            double perColWidthPt = Math.Max(1, (contentWidthPt - gap * (colCount - 1)) / colCount);
                            var buckets = new List<List<IrBlock>>();
                            var weights = new double[colCount];
                            for (int i = 0; i < colCount; i++) buckets.Add(new List<IrBlock>());
                            // 贪心：按文档顺序遍历 block，将其放到当前累计权重最小的桶中。
                            // 权重启发式：段落 ≈ ceil(charCount / 80)，表格 ≈ rows * 1.5，其他 1.0。
                            foreach (var b in sec.Blocks)
                            {
                                int target = 0;
                                for (int i = 1; i < colCount; i++)
                                    if (weights[i] < weights[target]) target = i;
                                buckets[target].Add(b);
                                weights[target] += EstimateBlockWeight(b);
                            }
                            col.Item().Row(row =>
                            {
                                row.Spacing(gap);
                                foreach (var bucket in buckets)
                                {
                                    row.RelativeItem().Column(bc =>
                                    {
                                        bc.Spacing(6);
                                        foreach (var block in bucket)
                                            RenderBlock(bc, block, ir, options, props, perColWidthPt);
                                    });
                                }
                            });
                        }
                    });

                    bool hasFirstFooter = props != null && props.DifferentFirstPage && props.FooterFirstParagraphs.Count > 0;
                    bool hasDefaultFooter = props != null && props.FooterParagraphs.Count > 0;
                    bool hasEvenFooter = props != null && props.DifferentOddEven && props.FooterEvenParagraphs.Count > 0;
                    if (hasFirstFooter || hasDefaultFooter || hasEvenFooter)
                    {
                        page.Footer().PaddingTop(4).Column(fc =>
                        {
                            fc.Spacing(2);
                            if (hasFirstFooter)
                            {
                                fc.Item().ShowIf(ctx => ctx.PageNumber == 1).Column(c =>
                                {
                                    c.Spacing(2);
                                    foreach (var fp in props!.FooterFirstParagraphs)
                                        RenderParagraph(c.Item(), fp, options);
                                });
                            }
                            if (hasDefaultFooter)
                            {
                                fc.Item().ShowIf(ctx =>
                                        (!hasFirstFooter || ctx.PageNumber > 1) &&
                                        (!hasEvenFooter || ctx.PageNumber % 2 == 1))
                                    .Column(c =>
                                    {
                                        c.Spacing(2);
                                        foreach (var fp in props!.FooterParagraphs)
                                            RenderParagraph(c.Item(), fp, options);
                                    });
                            }
                            if (hasEvenFooter)
                            {
                                fc.Item().ShowIf(ctx => ctx.PageNumber % 2 == 0).Column(c =>
                                {
                                    c.Spacing(2);
                                    foreach (var fp in props!.FooterEvenParagraphs)
                                        RenderParagraph(c.Item(), fp, options);
                                });
                            }
                        });
                    }
                    else
                    {
                        page.Footer().AlignCenter().Text(t =>
                        {
                            t.DefaultTextStyle(s => s.FontSize(9).FontColor(Colors.Grey.Medium));
                            var footerText = props?.FooterText;
                            if (!string.IsNullOrEmpty(footerText))
                            {
                                t.Span(footerText! + "  ");
                            }
                            var pnFmt = props?.PageNumberFormat ?? PageNumberFormat.Decimal;
                            int pnStart = props?.PageNumberStart ?? 1;
                            if (pnFmt == PageNumberFormat.Decimal && pnStart == 1)
                            {
                                t.CurrentPageNumber();
                                t.Span(" / ");
                                t.TotalPages();
                            }
                            else
                            {
                                t.CurrentPageNumber().Format(x => PageNumberFormatter.Format((x ?? 1) + pnStart - 1, pnFmt));
                                t.Span(" / ");
                                t.TotalPages().Format(x => PageNumberFormatter.Format(x ?? 1, pnFmt));
                            }
                        });
                    }
                });
            }
            // 末尾追加脚注/尾注页（如果有）
            AppendNotesPage(container, ir, options, ir.Footnotes, "脚注");
            AppendNotesPage(container, ir, options, ir.Endnotes, "尾注");
            AppendCommentsPage(container, ir, options);
        }).WithMetadata(new DocumentMetadata
        {
            Title = ir.Title ?? string.Empty,
            Author = ir.Author ?? string.Empty,
            Subject = options.Subject ?? string.Empty,
            Keywords = options.Keywords ?? string.Empty,
            Producer = options.Producer ?? "SnowyRiver.Documents.Converter",
            Creator = "SnowyRiver.Documents.Converter",
            CreationDate = DateTimeOffset.Now,
        }).WithSettings(new DocumentSettings
        {
            PDFA_Conformance = options.PdfStandard switch
            {
                PdfStandard.PdfA2B => PDFA_Conformance.PDFA_2B,
                PdfStandard.PdfA3B => PDFA_Conformance.PDFA_3B,
                _ => PDFA_Conformance.None,
            },
            PDFUA_Conformance = options.PdfUaEnabled ? PDFUA_Conformance.PDFUA_1 : PDFUA_Conformance.None,
            ImageRasterDpi = Math.Max(72, options.PdfImageRasterDpi),
            ImageCompressionQuality = options.PdfImageCompression switch
            {
                PdfImageCompression.Best => ImageCompressionQuality.Best,
                PdfImageCompression.High => ImageCompressionQuality.High,
                PdfImageCompression.Medium => ImageCompressionQuality.Medium,
                PdfImageCompression.Low => ImageCompressionQuality.Low,
                PdfImageCompression.VeryLow => ImageCompressionQuality.VeryLow,
                _ => ImageCompressionQuality.High,
            },
        }).GeneratePdf(target);

        // 可选：加密后处理。QuestPDF 加密 API 仅暴露文件路径形式，
        // 因此把生成的 PDF 暂存到临时文件、加密后再回写到 target。
        bool needEncrypt = !string.IsNullOrEmpty(options.PdfOwnerPassword)
                            || !string.IsNullOrEmpty(options.PdfUserPassword);
        if (needEncrypt && target.CanSeek)
        {
            try
            {
                ApplyPdfEncryption(target, options, diagnostics);
            }
            catch (Exception ex)
            {
                diagnostics?.Warn(DiagCodes.PDF_ENCRYPT_FAIL, $"PDF 加密失败：{ex.Message}", stage: DiagnosticStage.Rendering);
            }
        }
        else if (needEncrypt)
        {
            diagnostics?.Warn(DiagCodes.PDF_ENCRYPT_FAIL, "目标流不支持随机访问 (CanSeek=false)，已跳过 PDF 加密。", stage: DiagnosticStage.Rendering);
        }
        }
        finally
        {
            _currentIr = null;
        }
    }

    private static void ApplyPdfEncryption(Stream target, ConversionOptions options, ConversionDiagnostics? diagnostics)
    {
        // 把刚刚写入的 PDF 读回来，落到临时文件，加密后再回写。
        long generatedLength = target.Position;
        target.Position = 0;
        var pdfBytes = new byte[generatedLength];
        int total = 0;
        while (total < generatedLength)
        {
            int n = target.Read(pdfBytes, total, (int)(generatedLength - total));
            if (n <= 0) break;
            total += n;
        }

        string tempIn = Path.Combine(Path.GetTempPath(), $"snowyriver_pdf_in_{Guid.NewGuid():N}.pdf");
        string tempOut = Path.Combine(Path.GetTempPath(), $"snowyriver_pdf_out_{Guid.NewGuid():N}.pdf");
        try
        {
            File.WriteAllBytes(tempIn, pdfBytes);

            DocumentOperation op = DocumentOperation.LoadFile(tempIn, password: string.Empty);

            string owner = options.PdfOwnerPassword ?? options.PdfUserPassword ?? string.Empty;
            string user = options.PdfUserPassword ?? string.Empty;
            var perm = options.PdfPermissions;
            switch (options.PdfEncryptionStrength)
            {
                case PdfEncryptionStrength.Rc4_40:
                    op = op.Encrypt(new DocumentOperation.Encryption40Bit
                    {
                        OwnerPassword = owner,
                        UserPassword = user,
                        AllowPrinting = perm.HasFlag(PdfPermissions.AllowPrinting),
                        AllowModification = perm.HasFlag(PdfPermissions.AllowContentModification),
                        AllowContentExtraction = perm.HasFlag(PdfPermissions.AllowContentCopying)
                                                  || perm.HasFlag(PdfPermissions.AllowContentCopyingForAccessibility),
                        AllowAnnotation = perm.HasFlag(PdfPermissions.AllowAnnotations),
                    });
                    break;
                case PdfEncryptionStrength.Rc4_128:
                    op = op.Encrypt(new DocumentOperation.Encryption128Bit
                    {
                        OwnerPassword = owner,
                        UserPassword = user,
                        AllowPrinting = perm.HasFlag(PdfPermissions.AllowPrinting),
                        AllowAssembly = perm.HasFlag(PdfPermissions.AllowDocumentAssembly),
                        AllowContentExtraction = perm.HasFlag(PdfPermissions.AllowContentCopying)
                                                  || perm.HasFlag(PdfPermissions.AllowContentCopyingForAccessibility),
                        AllowFillingForms = perm.HasFlag(PdfPermissions.AllowFillingForms),
                        AllowAnnotation = perm.HasFlag(PdfPermissions.AllowAnnotations),
                        EncryptMetadata = true,
                    });
                    break;
                default:
                    op = op.Encrypt(new DocumentOperation.Encryption256Bit
                    {
                        OwnerPassword = owner,
                        UserPassword = user,
                        AllowPrinting = perm.HasFlag(PdfPermissions.AllowPrinting),
                        AllowAssembly = perm.HasFlag(PdfPermissions.AllowDocumentAssembly),
                        AllowContentExtraction = perm.HasFlag(PdfPermissions.AllowContentCopying)
                                                  || perm.HasFlag(PdfPermissions.AllowContentCopyingForAccessibility),
                        AllowFillingForms = perm.HasFlag(PdfPermissions.AllowFillingForms),
                        AllowAnnotation = perm.HasFlag(PdfPermissions.AllowAnnotations),
                        EncryptMetadata = true,
                    });
                    break;
            }
            op.Save(tempOut);

            var encrypted = File.ReadAllBytes(tempOut);
            target.SetLength(0);
            target.Position = 0;
            target.Write(encrypted, 0, encrypted.Length);
        }
        finally
        {
            try { if (File.Exists(tempIn)) File.Delete(tempIn); } catch { /* best effort */ }
            try { if (File.Exists(tempOut)) File.Delete(tempOut); } catch { /* best effort */ }
        }
    }

    private static void AppendCommentsPage(IDocumentContainer container, IrDocument ir, ConversionOptions options)
    {
        if (ir.Comments == null || ir.Comments.Count == 0) return;
        container.Page(page =>
        {
            page.Size((float)ir.PageWidthPt, (float)ir.PageHeightPt, Unit.Point);
            page.Margin((float)ir.MarginPt, Unit.Point);
            page.DefaultTextStyle(t => t.FontFamily(options.DefaultFontFamily).FontSize(10));
            page.Content().Column(col =>
            {
                col.Spacing(4);
                col.Item().Text("批注").FontSize(14).Bold();
                foreach (var ic in ir.Comments.Values.OrderBy(c => c.Number))
                {
                    col.Item().Row(row =>
                    {
                        row.ConstantItem(28).Text($"[{ic.Number}]").Bold().FontColor("#A0522D");
                        row.RelativeItem().Column(c =>
                        {
                            var meta = ic.Author ?? string.Empty;
                            if (ic.Date.HasValue) meta += (string.IsNullOrEmpty(meta) ? string.Empty : "  ") + ic.Date.Value.ToString("yyyy-MM-dd HH:mm");
                            if (!string.IsNullOrEmpty(meta))
                                c.Item().Text(meta).FontSize(8).FontColor(Colors.Grey.Darken1).Italic();
                            foreach (var p in ic.Paragraphs)
                                RenderParagraph(c.Item(), p, options);
                        });
                    });
                }
            });
        });
    }

    private static void AppendNotesPage(IDocumentContainer container, IrDocument ir, ConversionOptions options, Dictionary<string, IrFootnote> notes, string title)
    {
        if (notes == null || notes.Count == 0) return;
        container.Page(page =>
        {
            page.Size((float)ir.PageWidthPt, (float)ir.PageHeightPt, Unit.Point);
            page.Margin((float)ir.MarginPt, Unit.Point);
            page.DefaultTextStyle(t => t.FontFamily(options.DefaultFontFamily).FontSize(10));
            page.Content().Column(col =>
            {
                col.Spacing(4);
                col.Item().Text(title).FontSize(14).Bold();
                foreach (var kv in notes.Values.OrderBy(n => n.Number))
                {
                    col.Item().Row(row =>
                    {
                        row.ConstantItem(20).Text(kv.Number.ToString()).Bold();
                        row.RelativeItem().Column(c =>
                        {
                            foreach (var p in kv.Paragraphs)
                                RenderParagraph(c.Item(), p, options);
                        });
                    });
                }
            });
        });
    }

    private sealed class SectionGroup
    {
        public IrSectionProperties? Properties { get; set; }
        public List<IrBlock> Blocks { get; } = new();
    }

    private static void RenderBlock(ColumnDescriptor col, IrBlock block, IrDocument ir, ConversionOptions options, IrSectionProperties? sectionProps = null, double contentWidthPt = 0)
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
            // 第九轮：按 Excel Scale / FitToPagesWide 缩放表格列宽以贴近打印效果。
            var table = TableLayoutHelper.ApplyExcelScaling(block.Table, sectionProps, contentWidthPt);

            var hBreaks = table.HorizontalPageBreakRowIndices
                .Where(i => i > 0 && i < table.Rows.Count)
                .Distinct().OrderBy(i => i).ToList();
            var vBreaks = table.VerticalPageBreakColIndices
                .Where(i => i > 0).Distinct().OrderBy(i => i).ToList();

            if (hBreaks.Count == 0 && vBreaks.Count == 0)
            {
                col.Item().Element(c => RenderTable(c, table, options));
            }
            else
            {
                int totalCols = table.Rows.Max(r => r.Cells.Sum(cc => Math.Max(1, cc.ColSpan)));
                var colSlices = TableLayoutHelper.SliceRanges(vBreaks, totalCols);
                var rowSlices = TableLayoutHelper.SliceRanges(hBreaks, table.Rows.Count);
                int titleCols = Math.Max(0, table.PrintTitleColCount);
                int titleRows = Math.Max(0, table.PrintTitleRowCount);
                bool first = true;
                for (int rs = 0; rs < rowSlices.Count; rs++)
                {
                    for (int cs = 0; cs < colSlices.Count; cs++)
                    {
                        if (!first) col.Item().PageBreak();
                        first = false;
                        var sliced = TableLayoutHelper.SliceTable(table,
                            rowSlices[rs].start, rowSlices[rs].end,
                            colSlices[cs].start, colSlices[cs].end,
                            titleRows, titleCols);
                        col.Item().Element(c => RenderTable(c, sliced, options));
                    }
                }
            }
        }
        else if (block.Shape != null)
        {
            col.Item().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Column(sc =>
            {
                foreach (var sp in block.Shape.Paragraphs)
                    RenderParagraph(sc.Item(), sp, options);
            });
        }
        else if (block.TocField != null)
        {
            int max = Math.Max(1, block.TocField.MaxLevel);
            var tocHeadings = ir.Blocks
                .Where(b => b.Paragraph != null && b.Paragraph.IsHeading
                            && !string.IsNullOrEmpty(b.Paragraph.AnchorId)
                            && b.Paragraph.HeadingLevel <= max)
                .Select(b => b.Paragraph!)
                .ToList();
            if (tocHeadings.Count > 0)
            {
                if (!string.IsNullOrEmpty(block.TocField.Title))
                    col.Item().Text(block.TocField.Title!).FontSize(16).Bold();
                foreach (var h in tocHeadings)
                {
                    int indent = Math.Max(0, h.HeadingLevel - 1);
                    col.Item().PaddingLeft(indent * 12).Text(t =>
                    {
                        t.SectionLink(h.PlainText, h.AnchorId!);
                        t.Span(" ");
                        t.BeginPageNumberOfSection(h.AnchorId!);
                    });
                }
            }
        }
    }

    private static List<SectionGroup> SplitSections(IrDocument ir)
    {
        var list = new List<SectionGroup>();
        SectionGroup current = new();
        list.Add(current);

        foreach (var block in ir.Blocks)
        {
            if (block.Section != null)
            {
                // 节属性：若当前节已有内容，则该属性应用到当前节（视为该节的尾部 sectPr）；
                // 否则作为下一节属性。简化处理：如果当前节已经有内容，开新节并把属性指给新节。
                if (current.Blocks.Count == 0 && current.Properties == null)
                {
                    current.Properties = block.Section;
                }
                else
                {
                    current.Properties ??= block.Section; // 当前未指定 → 用作当前
                    var next = new SectionGroup { Properties = block.Section };
                    list.Add(next);
                    current = next;
                }
                continue;
            }
            current.Blocks.Add(block);
        }

        // 移除尾部空节
        if (list.Count > 1 && list[^1].Blocks.Count == 0)
        {
            list.RemoveAt(list.Count - 1);
        }
        return list;
    }

    private static void RenderParagraph(IContainer container, IrParagraph p, ConversionOptions options)
    {
        // 段前/段后 + 左/首行缩进
        var outer = container;
        if (p.SpaceBeforePt is { } sb && sb > 0) outer = outer.PaddingTop((float)sb);
        if (p.SpaceAfterPt is { } sa && sa > 0) outer = outer.PaddingBottom((float)sa);
        // 列表层级附加缩进：每层 18pt
        double leftIndent = p.LeftIndentPt ?? 0;
        if (p.ListType != ListType.None) leftIndent += (p.ListLevel + 1) * 18;
        if (leftIndent > 0) outer = outer.PaddingLeft((float)leftIndent);

        // 标题段落：作为 PDF 大纲（书签）锚点 + PDF/UA 标签化结构（SemanticHeader1..6）。
        if (options.EnablePdfBookmarks && p.IsHeading && !string.IsNullOrEmpty(p.AnchorId))
        {
            outer = outer.Section(p.AnchorId!);
            outer = ApplySemanticHeader(outer, p.HeadingLevel);
        }
        // 非标题段落锚点：仍生成 Section 以支持内部链接跳转
        else if (!string.IsNullOrEmpty(p.AnchorId))
        {
            outer = outer.Section(p.AnchorId!);
        }
        // 段落上挂载的 Word 自定义书签：每个名称都生成一个 Section（命名锚点），用于 PAGEREF/Hyperlink 跳转。
        foreach (var bm in p.BookmarkNames)
        {
            if (string.IsNullOrEmpty(bm)) continue;
            if (!string.IsNullOrEmpty(p.AnchorId) && string.Equals(bm, p.AnchorId, StringComparison.Ordinal)) continue;
            outer = outer.Section(bm);
        }

        // 段落边框
        if (p.Border != null)
        {
            if (p.Border.Top is { } pbt) outer = outer.BorderTop((float)pbt.Thickness).BorderColor(pbt.ColorHex ?? Colors.Grey.Medium);
            if (p.Border.Right is { } pbr) outer = outer.BorderRight((float)pbr.Thickness).BorderColor(pbr.ColorHex ?? Colors.Grey.Medium);
            if (p.Border.Bottom is { } pbb) outer = outer.BorderBottom((float)pbb.Thickness).BorderColor(pbb.ColorHex ?? Colors.Grey.Medium);
            if (p.Border.Left is { } pbl) outer = outer.BorderLeft((float)pbl.Thickness).BorderColor(pbl.ColorHex ?? Colors.Grey.Medium);
            outer = outer.Padding(2);
        }
        // 段落背景
        if (!string.IsNullOrEmpty(p.BackgroundHex))
        {
            outer = outer.Background(p.BackgroundHex!);
        }

        var item = p.Alignment switch
        {
            HorizontalAlign.Center => outer.AlignCenter(),
            HorizontalAlign.Right => outer.AlignRight(),
            _ => outer,
        };

        // 公式段落：若注入了 IMathRenderer 且能渲染成功，则用图像代替线性化文本。
        if (p.IsEquation && options.MathRenderer is { } mathRenderer)
        {
            byte[]? mathPng = null;
            try
            {
                if (!string.IsNullOrEmpty(p.EquationMathML))
                {
                    mathPng = mathRenderer.RenderMathMLToPng(p.EquationMathML!, emPx: 16);
                }
                if (mathPng == null && !string.IsNullOrEmpty(p.EquationLinear))
                {
                    mathPng = mathRenderer.RenderLinearToPng(p.EquationLinear!, emPx: 16);
                }
            }
            catch
            {
                mathPng = null;
            }
            if (mathPng != null && mathPng.Length > 0)
            {
                item.Image(mathPng).FitWidth();
                return;
            }
        }

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

            // 行高：优先 ratio（Word auto），否则按 LineHeightPt 折算近似
            if (p.LineHeightRatio is { } lhr && lhr > 0)
            {
                span.DefaultTextStyle(s => s.LineHeight((float)lhr));
            }
            else if (p.LineHeightPt is { } lh && lh > 0)
            {
                span.DefaultTextStyle(s => s.LineHeight((float)Math.Max(1.0, lh / 12.0)));
            }

            // 真正的首行缩进（QuestPDF 2026.5 起原生支持）
            if (p.FirstLineIndentPt is { } fl && fl > 0)
            {
                span.ParagraphFirstLineIndentation((float)fl, Unit.Point);
            }

            // 列表项符号
            if (p.ListType != ListType.None)
            {
                string marker;
                if (!string.IsNullOrEmpty(p.ListLabel))
                {
                    marker = p.ListLabel!.EndsWith(' ') ? p.ListLabel! : p.ListLabel + " ";
                }
                else
                {
                    marker = p.ListType switch
                    {
                        ListType.Bullet => "• ",
                        ListType.Decimal => $"{p.ListNumber ?? 1}. ",
                        ListType.LowerLetter => $"{ToLetter(p.ListNumber ?? 1, true)}. ",
                        ListType.UpperLetter => $"{ToLetter(p.ListNumber ?? 1, false)}. ",
                        ListType.LowerRoman => $"{ToRoman(p.ListNumber ?? 1).ToLowerInvariant()}. ",
                        ListType.UpperRoman => $"{ToRoman(p.ListNumber ?? 1)}. ",
                        _ => string.Empty,
                    };
                }
                if (!string.IsNullOrEmpty(marker)) span.Span(marker);
            }

            if (p.Runs.Count == 0)
            {
                span.Span(string.Empty);
                return;
            }

            // 制表位展开：仅当段落含 TabStops 且 Runs 内含 \t 时启用，避免无谓拷贝
            bool needTabExpand = p.TabStops.Count > 0 && p.Runs.Any(r => (r.Text ?? string.Empty).IndexOf('\t') >= 0);
            IEnumerable<(string padding, IrRun r)> seq = needTabExpand
                ? TabStopExpander.Expand(p).Select(s => (s.Padding, s.Run))
                : p.Runs.Select(r => (string.Empty, r));

            foreach (var (padding, r) in seq)
            {
                if (!string.IsNullOrEmpty(padding)) span.Span(padding);
                if (r.IsPageNumberField || r.FieldKind == RunFieldKind.Page) { span.CurrentPageNumber(); continue; }
                if (r.IsPageCountField || r.FieldKind == RunFieldKind.NumPages) { span.TotalPages(); continue; }
                if (r.FieldKind == RunFieldKind.Date) { span.Span(DateTime.Now.ToString(string.IsNullOrEmpty(r.FieldFormat) ? "yyyy-MM-dd" : r.FieldFormat!)); continue; }
                if (r.FieldKind == RunFieldKind.Time) { span.Span(DateTime.Now.ToString(string.IsNullOrEmpty(r.FieldFormat) ? "HH:mm" : r.FieldFormat!)); continue; }
                if (r.FieldKind == RunFieldKind.Section) { span.Span("1"); continue; }
                if (r.FieldKind == RunFieldKind.SheetName || r.FieldKind == RunFieldKind.FileName || r.FieldKind == RunFieldKind.FilePath)
                {
                    span.Span(r.Text ?? string.Empty);
                    continue;
                }
                if (!string.IsNullOrEmpty(r.FootnoteRef) || !string.IsNullOrEmpty(r.EndnoteRef))
                {
                    // 上标编号占位（实际编号已在 IrFootnote.Number 上；这里查 ContextNotes）
                    var num = ResolveNoteNumber(r);
                    span.Span(num != null ? num.ToString()! : "*").FontSize(8).Superscript();
                    continue;
                }
                if (!string.IsNullOrEmpty(r.CommentRef))
                {
                    if (string.IsNullOrEmpty(r.Text)) continue; // CommentRangeStart 占位 → 跳过
                    int? cnum = null;
                    if (_currentIr != null && _currentIr.Comments.TryGetValue(r.CommentRef!, out var ic)) cnum = ic.Number;
                    span.Span(cnum != null ? $"[{cnum}]" : "[*]").FontSize(8).Superscript().FontColor("#A0522D");
                    continue;
                }

                TextSpanDescriptor sd;
                if (!string.IsNullOrEmpty(r.PageRefAnchor))
                {
                    sd = span.SectionLink(string.IsNullOrEmpty(r.Text) ? "#" : r.Text, r.PageRefAnchor!);
                }
                else if (!string.IsNullOrEmpty(r.AnchorRef))
                {
                    sd = span.SectionLink(r.Text, r.AnchorRef!);
                }
                else if (!string.IsNullOrEmpty(r.HyperlinkUrl) && options.PreserveHyperlinks)
                {
                    sd = span.Hyperlink(r.Text, r.HyperlinkUrl!);
                }
                else
                {
                    sd = span.Span(r.Text);
                }
                sd = sd.FontFamily(BuildFontFamilyChain(r.FontFamily, options));
                if (r.FontSize.HasValue) sd = sd.FontSize((float)r.FontSize.Value);
                if (r.Bold) sd = sd.Bold();
                if (r.Italic) sd = sd.Italic();
                if (r.Underline) sd = sd.Underline();
                if (r.IsDeletion) sd = sd.Strikethrough();
                if (r.IsInsertion)
                {
                    sd = sd.Underline();
                    if (string.IsNullOrEmpty(r.ColorHex)) sd = sd.FontColor("#008000");
                }
                if (!string.IsNullOrEmpty(r.HighlightHex)) sd = sd.BackgroundColor(r.HighlightHex!);
                if (!string.IsNullOrEmpty(r.ColorHex)) sd = sd.FontColor(r.ColorHex!);
            }
        });
    }

    private static string ToLetter(int n, bool lower)
    {
        if (n <= 0) return string.Empty;
        var sb = new System.Text.StringBuilder();
        while (n > 0) { n--; sb.Insert(0, (char)((lower ? 'a' : 'A') + (n % 26))); n /= 26; }
        return sb.ToString();
    }

    private static string ToRoman(int n)
    {
        if (n <= 0) return n.ToString();
        var map = new (int v, string s)[]
        {
            (1000,"M"),(900,"CM"),(500,"D"),(400,"CD"),(100,"C"),(90,"XC"),
            (50,"L"),(40,"XL"),(10,"X"),(9,"IX"),(5,"V"),(4,"IV"),(1,"I"),
        };
        var sb = new System.Text.StringBuilder();
        foreach (var (v, s) in map) while (n >= v) { sb.Append(s); n -= v; }
        return sb.ToString();
    }

    private static void RenderImage(IContainer container, IrImage img)
    {
        if (img.Data is null || img.Data.Length == 0) return;
        try
        {
            var aligned = img.Float switch
            {
                ImageFloatMode.Left => container.AlignLeft(),
                ImageFloatMode.Right => container.AlignRight(),
                _ => container.AlignCenter(),
            };
            // px → pt：96 DPI 假设
            if (img.WidthPx is { } wpx && wpx > 0)
            {
                var widthPt = (float)(wpx * 72.0 / 96.0);
                var item = aligned.Width(widthPt, Unit.Point);
                if (img.HeightPx is { } hpx2 && hpx2 > 0)
                {
                    item = item.Height((float)(hpx2 * 72.0 / 96.0), Unit.Point);
                }
                item.Image(img.Data).FitArea();
            }
            else
            {
                aligned.Image(img.Data).FitArea();
            }
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

            int headerCount = Math.Min(Math.Max(0, table.HeaderRowCount), table.Rows.Count);

            if (headerCount > 0)
            {
                t.Header(h =>
                {
                    for (int ri = 0; ri < headerCount; ri++)
                        RenderTableRow(h.Cell, table.Rows[ri], options, table.PrintGridlines);
                });
            }

            for (int ri = headerCount; ri < table.Rows.Count; ri++)
                RenderTableRow(t.Cell, table.Rows[ri], options, table.PrintGridlines);
        });
    }

    private static void RenderTableRow(System.Func<QuestPDF.Elements.Table.ITableCellContainer> cellFactory, IrRow row, ConversionOptions options, bool gridlines = false)
    {
        foreach (var cell in row.Cells)
        {
            if (cell.Suppressed) continue;

            var c = cellFactory();
            if (cell.RowSpan > 1) c = c.RowSpan((uint)cell.RowSpan);
            if (cell.ColSpan > 1) c = c.ColumnSpan((uint)cell.ColSpan);

            IContainer box = c;
            var bs = cell.Style.Borders;
            bool hasAny = (bs != null && (bs.Top?.Thickness > 0 || bs.Right?.Thickness > 0 || bs.Bottom?.Thickness > 0 || bs.Left?.Thickness > 0))
                          || cell.Style.BorderThickness > 0;
            if (bs != null)
            {
                if (bs.Top is { } bt && bt.Thickness > 0)
                    box = box.BorderTop((float)bt.Thickness).BorderColor(bt.ColorHex ?? Colors.Grey.Medium);
                if (bs.Right is { } br && br.Thickness > 0)
                    box = box.BorderRight((float)br.Thickness).BorderColor(br.ColorHex ?? Colors.Grey.Medium);
                if (bs.Bottom is { } bb && bb.Thickness > 0)
                    box = box.BorderBottom((float)bb.Thickness).BorderColor(bb.ColorHex ?? Colors.Grey.Medium);
                if (bs.Left is { } bl && bl.Thickness > 0)
                    box = box.BorderLeft((float)bl.Thickness).BorderColor(bl.ColorHex ?? Colors.Grey.Medium);
            }
            else if (cell.Style.BorderThickness > 0)
            {
                box = box
                    .Border((float)cell.Style.BorderThickness)
                    .BorderColor(cell.Style.BorderHex ?? Colors.Grey.Medium);
            }
            if (gridlines && !hasAny)
            {
                box = box.Border(0.25f).BorderColor(Colors.Grey.Lighten1);
            }

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

            var content = box.Padding(3);
            if (!string.IsNullOrEmpty(cell.Hyperlink) && options.PreserveHyperlinks)
            {
                content = content.Hyperlink(cell.Hyperlink!);
            }

            if (cell.Paragraphs.Count > 0)
            {
                content.Column(col =>
                {
                    foreach (var p in cell.Paragraphs)
                        col.Item().Element(e => RenderParagraph(e, p, options));
                });
            }
            else
            {
                var displayText = !string.IsNullOrEmpty(cell.FormattedText) ? cell.FormattedText! : (cell.Text ?? string.Empty);
                if (!string.IsNullOrEmpty(cell.Style.IconPrefix))
                    displayText = cell.Style.IconPrefix + " " + displayText;
                content.Text(span =>
                {
                    var s = span.Span(displayText)
                        .FontFamily(BuildFontFamilyChain(cell.Style.FontFamily, options));
                    if (cell.Style.FontSize.HasValue) s = s.FontSize((float)cell.Style.FontSize.Value);
                    if (cell.Style.Bold) s = s.Bold();
                    if (cell.Style.Italic) s = s.Italic();
                    if (!string.IsNullOrEmpty(cell.Style.FontColorHex)) s = s.FontColor(cell.Style.FontColorHex!);
                    if (!string.IsNullOrEmpty(cell.Hyperlink) && options.PreserveHyperlinks && string.IsNullOrEmpty(cell.Style.FontColorHex))
                        s.FontColor(Colors.Blue.Medium).Underline();
                    if (!string.IsNullOrEmpty(cell.Comment))
                    {
                        span.Span(" *").FontColor(Colors.Red.Medium).Bold();
                    }
                });
            }
        }
    }

    private static string[] BuildFontFamilyChain(string? primary, ConversionOptions options)
    {
        var families = new List<string>(4);
        if (!string.IsNullOrWhiteSpace(primary)) families.Add(primary!);
        if (!string.IsNullOrWhiteSpace(options.DefaultFontFamily) &&
            !families.Contains(options.DefaultFontFamily, StringComparer.OrdinalIgnoreCase))
        {
            families.Add(options.DefaultFontFamily);
        }
        if (options.EnableCjkFontSlicing && options.FallbackFontFamilies != null)
        {
            foreach (var fam in options.FallbackFontFamilies)
            {
                if (string.IsNullOrWhiteSpace(fam)) continue;
                if (!families.Contains(fam, StringComparer.OrdinalIgnoreCase))
                    families.Add(fam);
            }
        }
        if (families.Count == 0) families.Add(options.DefaultFontFamily);
        return families.ToArray();
    }

    private static double EstimateBlockWeight(IrBlock block)
    {
        if (block.Paragraph is { } p)
        {
            int chars = 0;
            foreach (var r in p.Runs) chars += r.Text?.Length ?? 0;
            if (p.HeadingLevel is > 0) chars = (int)(chars * 1.5);
            return Math.Max(1.0, Math.Ceiling(chars / 80.0));
        }
        if (block.Table is { } t)
            return Math.Max(1.0, t.Rows.Count * 1.5);
        return 1.0;
    }
}
