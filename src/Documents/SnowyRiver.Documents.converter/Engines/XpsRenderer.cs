#if SR_WINDOWS
using System.IO;
using System.IO.Packaging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Xps.Packaging;
using PDFtoImage;
using SkiaSharp;
using SnowyRiver.Documents.Converter.Abstractions;
using SnowyRiver.Documents.Converter.Model;

namespace SnowyRiver.Documents.Converter.Engines;

/// <summary>
/// 把 PDF 流栅格化为图像并封装到 XPS。Windows-only。
/// </summary>
internal static class XpsRenderer
{
    [System.ThreadStatic]
    private static IrDocument? _currentIr;

    public static void RenderPdfToXps(Stream pdfSource, Stream xpsTarget, ConversionOptions options)
    {
        // PDFtoImage.Conversion 需要 Stream 可读且支持 Seek。
        var pdfBytes = ReadAllBytes(pdfSource);

        // XpsDocument 必须基于 Package；使用临时文件作为目标，再拷贝到 xpsTarget。
        var tempXpsPath = Path.Combine(Path.GetTempPath(), $"snowyriver_xps_{Guid.NewGuid():N}.xps");
        try
        {
            using (var package = Package.Open(tempXpsPath, FileMode.Create, FileAccess.ReadWrite))
            {
                using var xpsDoc = new XpsDocument(package, CompressionOption.Maximum);
                var fixedDoc = new FixedDocument();

                int dpi = Math.Max(72, options.XpsRasterDpi);

                // 逐页栅格化以避免一次性占用大量内存
                int pageIndex = 0;
                using (var pdfStream = new MemoryStream(pdfBytes, writable: false))
                {
                    foreach (var bitmap in Conversion.ToImages(pdfStream, options: new PDFtoImage.RenderOptions(Dpi: dpi)))
                    {
                        using (bitmap)
                        {
                            var fp = BuildFixedPage(bitmap, dpi);
                            var pageContent = new PageContent { Child = fp };
                            fixedDoc.Pages.Add(pageContent);
                        }
                        pageIndex++;
                    }
                }

                if (pageIndex == 0)
                {
                    // 至少写一页空白以保持 XPS 合法
                    var emptyPage = new FixedPage { Width = 595, Height = 842 };
                    var pageContent = new PageContent { Child = emptyPage };
                    fixedDoc.Pages.Add(pageContent);
                }

                var writer = XpsDocument.CreateXpsDocumentWriter(xpsDoc);
                writer.Write(fixedDoc);
            }

            // 把临时 XPS 文件流拷贝到目标流
            using var fs = new FileStream(tempXpsPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            fs.CopyTo(xpsTarget);
        }
        finally
        {
            try { if (File.Exists(tempXpsPath)) File.Delete(tempXpsPath); } catch { /* 忽略 */ }
        }
    }

    private static FixedPage BuildFixedPage(SKBitmap bitmap, int dpi)
    {
        // 把像素尺寸换算为 1/96 英寸（WPF 设备无关单位）
        double widthDip = bitmap.Width * 96.0 / dpi;
        double heightDip = bitmap.Height * 96.0 / dpi;

        var bitmapSource = SkBitmapToBitmapSource(bitmap, dpi);

        var image = new Image
        {
            Source = bitmapSource,
            Width = widthDip,
            Height = heightDip,
            Stretch = Stretch.Fill,
        };
        FixedPage.SetLeft(image, 0);
        FixedPage.SetTop(image, 0);

        var fp = new FixedPage
        {
            Width = widthDip,
            Height = heightDip,
            Background = Brushes.White,
        };
        fp.Children.Add(image);

        // 触发布局
        var size = new Size(widthDip, heightDip);
        fp.Measure(size);
        fp.Arrange(new Rect(size));
        fp.UpdateLayout();
        return fp;
    }

    private static BitmapSource SkBitmapToBitmapSource(SKBitmap bitmap, int dpi)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 95);
        using var ms = new MemoryStream();
        data.SaveTo(ms);
        ms.Position = 0;

        var bmp = new BitmapImage();
        bmp.BeginInit();
        bmp.CacheOption = BitmapCacheOption.OnLoad;
        bmp.StreamSource = ms;
        bmp.EndInit();
        bmp.Freeze();
        return bmp;
    }

    private static byte[] ReadAllBytes(Stream s)
    {
        if (s is MemoryStream ms) return ms.ToArray();
        using var mem = new MemoryStream();
        s.CopyTo(mem);
        return mem.ToArray();
    }

    /// <summary>
    /// 直接把 IR 渲染为基于 FlowDocument 的 XPS（不经过 PDF 栅格化），保持矢量与文本可选。
    /// 仅支持段落与表格的简化渲染；图片与复杂分节会回退到“PDF→栅格→XPS”路径。
    /// </summary>
    public static void RenderIrToXps(IrDocument ir, Stream xpsTarget, ConversionOptions options)
    {
        _currentIr = ir;
        var tempXpsPath = Path.Combine(Path.GetTempPath(), $"snowyriver_xps_{Guid.NewGuid():N}.xps");
        try
        {
            using (var package = Package.Open(tempXpsPath, FileMode.Create, FileAccess.ReadWrite))
            {
                using var xpsDoc = new XpsDocument(package, CompressionOption.Maximum);

                var flowDoc = new FlowDocument
                {
                    PageWidth = ir.PageWidthPt * 96.0 / 72.0,
                    PageHeight = ir.PageHeightPt * 96.0 / 72.0,
                    PagePadding = new Thickness(ir.MarginPt * 96.0 / 72.0),
                    ColumnWidth = double.PositiveInfinity,
                    FontFamily = new FontFamily(options.DefaultFontFamily),
                    FontSize = 11 * 96.0 / 72.0,
                };

                // 应用首个含多列的节属性到 FlowDocument（FlowDocument 仅有全局列宽/列间距）。
                var firstMultiCol = ir.Blocks
                    .Where(b => b.Section != null && b.Section.ColumnCount > 1)
                    .Select(b => b.Section!)
                    .FirstOrDefault();
                if (firstMultiCol != null)
                {
                    double pageWidthPx = (firstMultiCol.PageWidthPt ?? ir.PageWidthPt) * 96.0 / 72.0;
                    double padPx = ((firstMultiCol.MarginLeftPt ?? ir.MarginPt) + (firstMultiCol.MarginRightPt ?? ir.MarginPt)) * 96.0 / 72.0;
                    double gapPx = (firstMultiCol.ColumnSpacingPt ?? 12) * 96.0 / 72.0;
                    int n = firstMultiCol.ColumnCount;
                    double colW = Math.Max(48, (pageWidthPx - padPx - gapPx * (n - 1)) / n);
                    flowDoc.ColumnWidth = colW;
                    flowDoc.ColumnGap = gapPx;
                    flowDoc.IsColumnWidthFlexible = false;
                }

                IrSectionProperties? currentSection = null;
                double currentContentWidthPt = Math.Max(1, ir.PageWidthPt - 2 * ir.MarginPt);
                foreach (var block in ir.Blocks)
                {
                    if (block.Section != null)
                    {
                        currentSection = block.Section;
                        double pw = block.Section.PageWidthPt ?? ir.PageWidthPt;
                        double ml = block.Section.MarginLeftPt ?? ir.MarginPt;
                        double mr = block.Section.MarginRightPt ?? ir.MarginPt;
                        currentContentWidthPt = Math.Max(1, pw - ml - mr);
                        continue;
                    }
                    if (block.Paragraph != null)
                    {
                        flowDoc.Blocks.Add(BuildFlowParagraph(block.Paragraph, options));
                    }
                    else if (block.Table != null)
                    {
                        // 第九轮：XPS 端同等消费 Excel 打印语义。
                        var scaled = TableLayoutHelper.ApplyExcelScaling(block.Table, currentSection, currentContentWidthPt);
                        AppendFlowTableSliced(flowDoc, scaled, options);
                    }
                    else if (block.PageBreak != null)
                    {
                        var p = new System.Windows.Documents.Paragraph { BreakPageBefore = true };
                        flowDoc.Blocks.Add(p);
                    }
                    else if (block.Shape != null)
                    {
                        // 文本框/形状：以左侧 4pt 缩进与浅灰边框近似呈现
                        foreach (var sp in block.Shape.Paragraphs)
                        {
                            var fp = BuildFlowParagraph(sp, options);
                            fp.BorderBrush = Brushes.LightGray;
                            fp.BorderThickness = new Thickness(0.5);
                            fp.Padding = new Thickness(4);
                            flowDoc.Blocks.Add(fp);
                        }
                    }
                    else if (block.Image != null)
                    {
                        var imgPara = BuildFlowImage(block.Image);
                        if (imgPara != null) flowDoc.Blocks.Add(imgPara);
                    }
                    else if (block.TocField != null)
                    {
                        AppendTocToFlow(flowDoc, ir, block.TocField, options);
                    }
                    // 图片暂跳过：FlowDocument 直出路径侧重文本保真。
                }

                AppendNotesToFlow(flowDoc, ir, options);
                AppendCommentsToFlow(flowDoc, ir, options);

                IDocumentPaginatorSource paginatorSource = flowDoc;
                var writer = XpsDocument.CreateXpsDocumentWriter(xpsDoc);
                if (!string.IsNullOrWhiteSpace(options.WatermarkText))
                {
                    var fixedDoc = BuildFixedDocumentWithWatermark(paginatorSource.DocumentPaginator, options);
                    writer.Write(fixedDoc);
                }
                else
                {
                    writer.Write(paginatorSource.DocumentPaginator);
                }
            }

            using var fs = new FileStream(tempXpsPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            fs.CopyTo(xpsTarget);
        }
        finally
        {
            try { if (File.Exists(tempXpsPath)) File.Delete(tempXpsPath); } catch { /* ignore */ }
            _currentIr = null;
        }
    }

    private static void AppendNotesToFlow(FlowDocument flowDoc, IrDocument ir, ConversionOptions options)
    {
        AppendNotesGroup(flowDoc, ir.Footnotes, "脚注", options);
        AppendNotesGroup(flowDoc, ir.Endnotes, "尾注", options);
    }

    private static void AppendNotesGroup(FlowDocument flowDoc, Dictionary<string, Model.IrFootnote> notes, string title, ConversionOptions options)
    {
        if (notes == null || notes.Count == 0) return;
        var header = new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run(title))
        {
            FontWeight = FontWeights.Bold,
            FontSize = 14 * 96.0 / 72.0,
            BreakPageBefore = true,
        };
        flowDoc.Blocks.Add(header);
        foreach (var n in notes.Values.OrderBy(x => x.Number))
        {
            bool first = true;
            foreach (var p in n.Paragraphs)
            {
                var fp = BuildFlowParagraph(p, options);
                if (first)
                {
                    var marker = new System.Windows.Documents.Run($"{n.Number}. ") { FontWeight = FontWeights.Bold };
                    if (fp.Inlines.FirstInline != null)
                        fp.Inlines.InsertBefore(fp.Inlines.FirstInline, marker);
                    else
                        fp.Inlines.Add(marker);
                    first = false;
                }
                flowDoc.Blocks.Add(fp);
            }
        }
    }

    private static FixedDocument BuildFixedDocumentWithWatermark(System.Windows.Documents.DocumentPaginator paginator, ConversionOptions options)
    {
        var fixedDoc = new FixedDocument();
        Color color;
        try { color = (Color)ColorConverter.ConvertFromString(options.WatermarkColorHex); }
        catch { color = Color.FromRgb(0xD3, 0xD3, 0xD3); }
        var brush = new SolidColorBrush(color) { Opacity = 0.45 };

        for (int i = 0; i < paginator.PageCount; i++)
        {
            var docPage = paginator.GetPage(i);
            var size = docPage.Size;

            var fp = new FixedPage { Width = size.Width, Height = size.Height, Background = Brushes.White };

            // 原始页面内容
            var visualHost = new System.Windows.Controls.ContentControl
            {
                Width = size.Width,
                Height = size.Height,
                Content = docPage.Visual,
            };
            FixedPage.SetLeft(visualHost, 0);
            FixedPage.SetTop(visualHost, 0);
            fp.Children.Add(visualHost);

            // 水印层
            var watermark = new System.Windows.Controls.TextBlock
            {
                Text = options.WatermarkText,
                Foreground = brush,
                FontSize = options.WatermarkFontSize * 96.0 / 72.0,
                FontWeight = FontWeights.Bold,
                IsHitTestVisible = false,
                RenderTransformOrigin = new System.Windows.Point(0.5, 0.5),
                RenderTransform = new RotateTransform(options.WatermarkRotationDegrees),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
            };
            watermark.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
            var ds = watermark.DesiredSize;
            FixedPage.SetLeft(watermark, (size.Width - ds.Width) / 2);
            FixedPage.SetTop(watermark, (size.Height - ds.Height) / 2);
            fp.Children.Add(watermark);

            fp.Measure(size);
            fp.Arrange(new Rect(size));
            fp.UpdateLayout();

            var pc = new PageContent { Child = fp };
            fixedDoc.Pages.Add(pc);
        }
        return fixedDoc;
    }

    private static void AppendTocToFlow(FlowDocument flowDoc, IrDocument ir, IrTocField toc, ConversionOptions options)
    {
        var headings = ir.Blocks
            .Where(b => b.Paragraph != null && b.Paragraph.IsHeading
                && !string.IsNullOrEmpty(b.Paragraph.AnchorId)
                && b.Paragraph.HeadingLevel <= Math.Max(1, toc.MaxLevel))
            .Select(b => b.Paragraph!)
            .ToList();
        if (headings.Count == 0) return;

        var title = new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run(toc.Title ?? options.TocTitle ?? "目录"))
        {
            FontWeight = FontWeights.Bold,
            FontSize = 18 * 96.0 / 72.0,
        };
        flowDoc.Blocks.Add(title);

        foreach (var h in headings)
        {
            var item = new System.Windows.Documents.Paragraph
            {
                Margin = new Thickness(Math.Max(0, h.HeadingLevel - 1) * 12, 0, 0, 0),
            };
            try
            {
                var hyp = new System.Windows.Documents.Hyperlink(new System.Windows.Documents.Run(h.PlainText));
                hyp.NavigateUri = new Uri("#" + h.AnchorId, UriKind.Relative);
                hyp.Foreground = Brushes.Blue;
                hyp.TextDecorations = TextDecorations.Underline;
                item.Inlines.Add(hyp);
            }
            catch
            {
                item.Inlines.Add(new System.Windows.Documents.Run(h.PlainText));
            }
            flowDoc.Blocks.Add(item);
        }

        var sep = new System.Windows.Documents.Paragraph { BreakPageBefore = true };
        flowDoc.Blocks.Add(sep);
    }

    private static void AppendCommentsToFlow(FlowDocument flowDoc, IrDocument ir, ConversionOptions options)
    {
        if (ir.Comments == null || ir.Comments.Count == 0) return;
        var header = new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run("批注"))
        {
            FontWeight = FontWeights.Bold,
            FontSize = 14 * 96.0 / 72.0,
            BreakPageBefore = true,
        };
        flowDoc.Blocks.Add(header);
        foreach (var ic in ir.Comments.Values.OrderBy(c => c.Number))
        {
            var meta = ic.Author ?? string.Empty;
            if (ic.Date.HasValue) meta += (string.IsNullOrEmpty(meta) ? string.Empty : "  ") + ic.Date.Value.ToString("yyyy-MM-dd HH:mm");
            var metaPara = new System.Windows.Documents.Paragraph();
            var marker = new System.Windows.Documents.Run($"[{ic.Number}] ")
            {
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(0xA0, 0x52, 0x2D)),
            };
            metaPara.Inlines.Add(marker);
            if (!string.IsNullOrEmpty(meta))
            {
                metaPara.Inlines.Add(new System.Windows.Documents.Run(meta)
                {
                    FontStyle = FontStyles.Italic,
                    FontSize = 8 * 96.0 / 72.0,
                    Foreground = Brushes.Gray,
                });
            }
            flowDoc.Blocks.Add(metaPara);
            foreach (var p in ic.Paragraphs)
                flowDoc.Blocks.Add(BuildFlowParagraph(p, options));
        }
    }

    private static System.Windows.Documents.Paragraph? BuildFlowImage(IrImage img)
    {
        if (img.Data == null || img.Data.Length == 0) return null;
        try
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.StreamSource = new MemoryStream(img.Data);
            bmp.EndInit();
            bmp.Freeze();
            var image = new System.Windows.Controls.Image { Source = bmp };
            if (img.WidthPx is { } w && w > 0) image.Width = w;
            if (img.HeightPx is { } h && h > 0) image.Height = h;
            var container = new InlineUIContainer(image);
            var fp = new System.Windows.Documents.Paragraph(container);
            fp.TextAlignment = img.Float switch
            {
                ImageFloatMode.Left => TextAlignment.Left,
                ImageFloatMode.Right => TextAlignment.Right,
                ImageFloatMode.Center => TextAlignment.Center,
                _ => TextAlignment.Left,
            };
            return fp;
        }
        catch
        {
            return new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run("[图像]")
            {
                Foreground = Brushes.Gray,
                FontStyle = FontStyles.Italic,
            });
        }
    }

    private static System.Windows.Documents.Paragraph BuildFlowParagraph(IrParagraph p, ConversionOptions options)
    {
        var fp = new System.Windows.Documents.Paragraph
        {
            TextAlignment = p.Alignment switch
            {
                HorizontalAlign.Center => TextAlignment.Center,
                HorizontalAlign.Right => TextAlignment.Right,
                HorizontalAlign.Justify => TextAlignment.Justify,
                _ => TextAlignment.Left,
            },
        };
        if (p.SpaceBeforePt is { } sb && sb > 0) fp.Margin = new Thickness(fp.Margin.Left, sb * 96.0 / 72.0, fp.Margin.Right, fp.Margin.Bottom);
        if (p.SpaceAfterPt is { } sa && sa > 0) fp.Margin = new Thickness(fp.Margin.Left, fp.Margin.Top, fp.Margin.Right, sa * 96.0 / 72.0);
        if (p.FirstLineIndentPt is { } fl && fl > 0) fp.TextIndent = fl * 96.0 / 72.0;
        if (p.IsHeading)
        {
            fp.FontWeight = FontWeights.Bold;
            fp.FontSize = (p.HeadingLevel switch { 1 => 18, 2 => 16, 3 => 14, _ => 12 }) * 96.0 / 72.0;
        }
        if (p.Border != null)
        {
            fp.BorderThickness = new Thickness(
                p.Border.Left?.Thickness ?? 0,
                p.Border.Top?.Thickness ?? 0,
                p.Border.Right?.Thickness ?? 0,
                p.Border.Bottom?.Thickness ?? 0);
            var col = p.Border.Top?.ColorHex ?? p.Border.Left?.ColorHex ?? p.Border.Right?.ColorHex ?? p.Border.Bottom?.ColorHex;
            if (!string.IsNullOrEmpty(col))
            {
                try { fp.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(col)); } catch { fp.BorderBrush = Brushes.Gray; }
            }
            else fp.BorderBrush = Brushes.Gray;
            fp.Padding = new Thickness(2);
        }
        if (!string.IsNullOrEmpty(p.BackgroundHex))
        {
            try { fp.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(p.BackgroundHex)); } catch { }
        }
        // 内部锚点：把首要 AnchorId 作为 Block.Name；其余 BookmarkNames 通过 0 宽 Run.Name 挂载
        if (!string.IsNullOrEmpty(p.AnchorId))
        {
            try { fp.Name = SafeXamlName(p.AnchorId!); } catch { }
        }
        foreach (var bm in p.BookmarkNames)
        {
            if (string.IsNullOrEmpty(bm)) continue;
            if (!string.IsNullOrEmpty(p.AnchorId) && string.Equals(bm, p.AnchorId, StringComparison.Ordinal)) continue;
            try
            {
                var anchorRun = new System.Windows.Documents.Run(string.Empty) { Name = SafeXamlName(bm) };
                fp.Inlines.Add(anchorRun);
            }
            catch { /* 名称非法时跳过 */ }
        }
        // 列表项符号：优先 ListLabel，否则按 ListType 兜底
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
                    _ => string.Empty,
                };
            }
            if (!string.IsNullOrEmpty(marker)) fp.Inlines.Add(new System.Windows.Documents.Run(marker));
        }
        // TabStop 展开：仅当段落含 TabStops 且有 \t 时启用
        bool needTabExpand = p.TabStops.Count > 0 && p.Runs.Any(r => (r.Text ?? string.Empty).IndexOf('\t') >= 0);
        IEnumerable<(string padding, IrRun r)> seq = needTabExpand
            ? TabStopExpander.Expand(p).Select(s => (s.Padding, s.Run))
            : p.Runs.Select(r => (string.Empty, r));
        foreach (var (padding, r) in seq)
        {
            if (!string.IsNullOrEmpty(padding))
            {
                fp.Inlines.Add(new System.Windows.Documents.Run(padding));
            }
            // 字段替换：FlowDocument 直出无法精确获得当前页号，使用占位/已知值。
            string text = r.Text ?? string.Empty;
            if (r.FieldKind == Model.RunFieldKind.Page || r.IsPageNumberField) text = "#";
            else if (r.FieldKind == Model.RunFieldKind.NumPages || r.IsPageCountField) text = "#";
            else if (r.FieldKind == Model.RunFieldKind.Date) text = DateTime.Now.ToString(string.IsNullOrEmpty(r.FieldFormat) ? "yyyy-MM-dd" : r.FieldFormat!);
            else if (r.FieldKind == Model.RunFieldKind.Time) text = DateTime.Now.ToString(string.IsNullOrEmpty(r.FieldFormat) ? "HH:mm" : r.FieldFormat!);
            else if (r.FieldKind == Model.RunFieldKind.Section) text = "1";

            // 脚注/尾注引用：渲染为上标编号
            if (!string.IsNullOrEmpty(r.FootnoteRef) || !string.IsNullOrEmpty(r.EndnoteRef))
            {
                int? num = null;
                if (!string.IsNullOrEmpty(r.FootnoteRef) && _currentIr != null && _currentIr.Footnotes.TryGetValue(r.FootnoteRef!, out var fn)) num = fn.Number;
                else if (!string.IsNullOrEmpty(r.EndnoteRef) && _currentIr != null && _currentIr.Endnotes.TryGetValue(r.EndnoteRef!, out var en)) num = en.Number;
                var supRun = new System.Windows.Documents.Run(num?.ToString() ?? "*")
                {
                    BaselineAlignment = BaselineAlignment.Superscript,
                    FontSize = 8 * 96.0 / 72.0,
                };
                fp.Inlines.Add(supRun);
                continue;
            }
            // 批注引用：上标 [n]
            if (!string.IsNullOrEmpty(r.CommentRef))
            {
                if (string.IsNullOrEmpty(text)) continue;
                int? cnum = null;
                if (_currentIr != null && _currentIr.Comments.TryGetValue(r.CommentRef!, out var ic)) cnum = ic.Number;
                var supRun = new System.Windows.Documents.Run(cnum != null ? $"[{cnum}]" : "[*]")
                {
                    BaselineAlignment = BaselineAlignment.Superscript,
                    FontSize = 8 * 96.0 / 72.0,
                    Foreground = new SolidColorBrush(Color.FromRgb(0xA0, 0x52, 0x2D)),
                };
                fp.Inlines.Add(supRun);
                continue;
            }

            var run = new System.Windows.Documents.Run(text);
            if (!string.IsNullOrEmpty(r.FontFamily)) run.FontFamily = new FontFamily(r.FontFamily);
            if (r.FontSize.HasValue) run.FontSize = r.FontSize.Value * 96.0 / 72.0;
            if (r.Bold) run.FontWeight = FontWeights.Bold;
            if (r.Italic) run.FontStyle = FontStyles.Italic;
            if (r.Underline) run.TextDecorations = TextDecorations.Underline;
            if (r.IsDeletion) run.TextDecorations = TextDecorations.Strikethrough;
            if (r.IsInsertion)
            {
                run.TextDecorations = TextDecorations.Underline;
                if (string.IsNullOrEmpty(r.ColorHex))
                    run.Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0x80, 0x00));
            }
            if (!string.IsNullOrEmpty(r.HighlightHex))
            {
                try { run.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(r.HighlightHex)); } catch { }
            }
            if (!string.IsNullOrEmpty(r.ColorHex))
            {
                try { run.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(r.ColorHex)); } catch { }
            }
            // 超链接 / PAGEREF：以 Hyperlink 包装 + 默认蓝色下划线
            if (!string.IsNullOrEmpty(r.HyperlinkUrl) || !string.IsNullOrEmpty(r.AnchorRef) || !string.IsNullOrEmpty(r.PageRefAnchor))
            {
                var link = new System.Windows.Documents.Hyperlink(run)
                {
                    Foreground = Brushes.Blue,
                    TextDecorations = TextDecorations.Underline,
                };
                if (!string.IsNullOrEmpty(r.HyperlinkUrl))
                {
                    try { link.NavigateUri = new Uri(r.HyperlinkUrl, UriKind.RelativeOrAbsolute); } catch { }
                }
                fp.Inlines.Add(link);
            }
            else
            {
                fp.Inlines.Add(run);
            }
        }
        return fp;
    }

    private static void AppendFlowTableSliced(FlowDocument flowDoc, IrTable table, ConversionOptions options)
    {
        var hBreaks = table.HorizontalPageBreakRowIndices
            .Where(i => i > 0 && i < table.Rows.Count)
            .Distinct().OrderBy(i => i).ToList();
        var vBreaks = table.VerticalPageBreakColIndices
            .Where(i => i > 0).Distinct().OrderBy(i => i).ToList();

        if (hBreaks.Count == 0 && vBreaks.Count == 0)
        {
            flowDoc.Blocks.Add(BuildFlowTable(table, options));
            return;
        }

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
                if (!first)
                    flowDoc.Blocks.Add(new System.Windows.Documents.Paragraph { BreakPageBefore = true });
                first = false;
                var sliced = TableLayoutHelper.SliceTable(table,
                    rowSlices[rs].start, rowSlices[rs].end,
                    colSlices[cs].start, colSlices[cs].end,
                    titleRows, titleCols);
                flowDoc.Blocks.Add(BuildFlowTable(sliced, options));
            }
        }
    }

    private static System.Windows.Documents.Table BuildFlowTable(IrTable table, ConversionOptions options)
    {
        var t = new System.Windows.Documents.Table { CellSpacing = 0 };
        bool gridlines = table.PrintGridlines;
        int colCount = Math.Max(1, table.Rows.Count == 0 ? table.ColumnWidthsPt.Count : table.Rows.Max(r => r.Cells.Count));
        for (int i = 0; i < colCount; i++)
        {
            var col = new TableColumn();
            if (i < table.ColumnWidthsPt.Count && table.ColumnWidthsPt[i].HasValue)
                col.Width = new GridLength(table.ColumnWidthsPt[i]!.Value * 96.0 / 72.0);
            t.Columns.Add(col);
        }
        var group = new TableRowGroup();
        t.RowGroups.Add(group);
        int headerCount = Math.Min(Math.Max(0, table.HeaderRowCount), table.Rows.Count);
        int rowIdx = 0;
        foreach (var row in table.Rows)
        {
            var tr = new TableRow();
            bool isHeader = rowIdx < headerCount;
            foreach (var cell in row.Cells)
            {
                if (cell.Suppressed) continue;
                var tc = new TableCell
                {
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(0.5),
                    Padding = new Thickness(3),
                };
                _ = gridlines; // 第九轮：保留 PrintGridlines 信息以便后续按需切换；当前默认始终绘制薄边框以兼容已有 Word 表格。
                if (cell.ColSpan > 1) tc.ColumnSpan = cell.ColSpan;
                if (cell.RowSpan > 1) tc.RowSpan = cell.RowSpan;
                if (isHeader) tc.FontWeight = FontWeights.Bold;
                if (!string.IsNullOrEmpty(cell.Style.BackgroundHex))
                {
                    try { tc.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(cell.Style.BackgroundHex)); } catch { }
                }
                if (cell.Paragraphs.Count > 0)
                {
                    foreach (var sp in cell.Paragraphs)
                        tc.Blocks.Add(BuildFlowParagraph(sp, options));
                }
                else
                {
                    var displayText = !string.IsNullOrEmpty(cell.FormattedText) ? cell.FormattedText! : (cell.Text ?? string.Empty);
                    if (!string.IsNullOrEmpty(cell.Style.IconPrefix))
                        displayText = cell.Style.IconPrefix + " " + displayText;
                    var runText = new System.Windows.Documents.Run(displayText);
                    System.Windows.Documents.Inline inline = runText;
                    if (!string.IsNullOrEmpty(cell.Hyperlink) && options.PreserveHyperlinks)
                    {
                        var link = new System.Windows.Documents.Hyperlink(runText)
                        {
                            Foreground = Brushes.Blue,
                            TextDecorations = System.Windows.TextDecorations.Underline,
                        };
                        try { link.NavigateUri = new Uri(cell.Hyperlink!, UriKind.RelativeOrAbsolute); } catch { }
                        inline = link;
                    }
                    var para = new System.Windows.Documents.Paragraph(inline)
                    {
                        TextAlignment = cell.Style.HAlign switch
                        {
                            HorizontalAlign.Center => TextAlignment.Center,
                            HorizontalAlign.Right => TextAlignment.Right,
                            _ => TextAlignment.Left,
                        },
                    };
                    if (!string.IsNullOrEmpty(cell.Comment))
                    {
                        para.Inlines.Add(new System.Windows.Documents.Run(" *")
                        {
                            Foreground = Brushes.Red,
                            FontWeight = FontWeights.Bold,
                            ToolTip = cell.Comment,
                        });
                    }
                    tc.Blocks.Add(para);
                }
                tr.Cells.Add(tc);
            }
            group.Rows.Add(tr);
            rowIdx++;
        }
        return t;
    }

    /// <summary>把任意字符串转换为合法的 XAML x:Name（必须以字母/下划线开头，仅含字母数字与下划线）。</summary>
    private static string SafeXamlName(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return "_";
        var sb = new System.Text.StringBuilder(raw.Length + 1);
        char first = raw[0];
        if (!(char.IsLetter(first) || first == '_')) sb.Append('_');
        foreach (var ch in raw)
            sb.Append(char.IsLetterOrDigit(ch) || ch == '_' ? ch : '_');
        return sb.ToString();
    }
}
#endif
