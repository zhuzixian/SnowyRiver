using System.IO;
using DocumentFormat.OpenXml.Packaging;
using SnowyRiver.Documents.Converter.Model;
using ConversionOptions = SnowyRiver.Documents.Converter.Abstractions.ConversionOptions;
using P = DocumentFormat.OpenXml.Presentation;
using A = DocumentFormat.OpenXml.Drawing;

namespace SnowyRiver.Documents.Converter.Engines;

/// <summary>
/// 使用 DocumentFormat.OpenXml 解析 .pptx 为 IR。
/// 当前实现：按 sldIdLst 顺序遍历幻灯片；每张幻灯片之间插入分页符；
/// 形状（shape）的文本框以段落输出，标题占位符段落 IsHeading=true 并生成 AnchorId（可作为 PDF 大纲）；
/// 表格转换为 <see cref="IrTable"/>；图片导出为 <see cref="IrImage"/>。
/// </summary>
internal static class PowerPointReader
{
    public static IrDocument Read(Stream source, ConversionOptions options) => Read(source, options, null);

    public static IrDocument Read(Stream source, ConversionOptions options, SnowyRiver.Documents.Converter.Abstractions.ConversionDiagnostics? diagnostics)
    {
        using var ms = new MemoryStream();
        source.CopyTo(ms);
        ms.Position = 0;

        var ir = new IrDocument();
        using var doc = PresentationDocument.Open(ms, false);
        var pres = doc.PresentationPart ?? throw new InvalidDataException("PowerPoint 文档缺少 PresentationPart。");

        ir.Title = options.Title ?? doc.PackageProperties.Title;
        ir.Author = options.Author ?? doc.PackageProperties.Creator;

        // 页面尺寸（EMU → pt；1 pt = 12700 EMU）
        var sldSize = pres.Presentation?.SlideSize;
        if (sldSize?.Cx?.Value is int cx) ir.PageWidthPt = cx / 12700.0;
        if (sldSize?.Cy?.Value is int cy) ir.PageHeightPt = cy / 12700.0;
        ir.MarginPt = 24;

        var sldIdList = pres.Presentation?.SlideIdList?.Elements<P.SlideId>().ToList() ?? new List<P.SlideId>();
        int slideIndex = 0;
        foreach (var sldId in sldIdList)
        {
            var rid = sldId.RelationshipId?.Value;
            if (string.IsNullOrEmpty(rid)) continue;
            if (pres.GetPartById(rid!) is not SlidePart slidePart) continue;

            if (slideIndex > 0) ir.Blocks.Add(IrBlock.NewPage());
            slideIndex++;
            BuildSlide(slidePart, ir, slideIndex, diagnostics);
        }

        diagnostics?.Info("PPTX_SLIDES", $"共解析幻灯片 {slideIndex} 张。");
        return ir;
    }

    private static void BuildSlide(SlidePart part, IrDocument ir, int slideIndex, SnowyRiver.Documents.Converter.Abstractions.ConversionDiagnostics? diag)
    {
        var tree = part.Slide?.CommonSlideData?.ShapeTree;
        if (tree == null) return;

        foreach (var child in tree.ChildElements)
        {
            switch (child)
            {
                case P.Shape sh:
                    BuildShape(sh, ir, slideIndex);
                    break;
                case P.GraphicFrame gf:
                    BuildGraphicFrame(gf, ir);
                    break;
                case P.Picture pic:
                    BuildPicture(pic, part, ir);
                    break;
                case P.GroupShape gs:
                    foreach (var inner in gs.ChildElements)
                    {
                        if (inner is P.Shape s2) BuildShape(s2, ir, slideIndex);
                        else if (inner is P.Picture p2) BuildPicture(p2, part, ir);
                        else if (inner is P.GraphicFrame g2) BuildGraphicFrame(g2, ir);
                    }
                    break;
            }
        }
    }

    private static void BuildShape(P.Shape sh, IrDocument ir, int slideIndex)
    {
        var txBody = sh.TextBody;
        if (txBody == null) return;
        bool isTitle = IsTitlePlaceholder(sh);
        bool first = true;
        foreach (var para in txBody.Elements<A.Paragraph>())
        {
            var ip = BuildParagraph(para, isTitle);
            if (ip == null) continue;
            if (isTitle && first)
            {
                ip.IsHeading = true;
                ip.HeadingLevel = 1;
                ip.AnchorId = MakeSlideAnchor(slideIndex, ip.PlainText);
                first = false;
            }
            ir.Blocks.Add(IrBlock.Of(ip));
        }
    }

    private static bool IsTitlePlaceholder(P.Shape sh)
    {
        var ph = sh.NonVisualShapeProperties?.ApplicationNonVisualDrawingProperties?.PlaceholderShape;
        if (ph == null) return false;
        var t = ph.Type?.Value;
        return t == P.PlaceholderValues.Title || t == P.PlaceholderValues.CenteredTitle;
    }

    private static IrParagraph? BuildParagraph(A.Paragraph para, bool inTitle)
    {
        var ip = new IrParagraph();
        var pPr = para.ParagraphProperties;
        var algn = pPr?.Alignment?.Value;
        if (algn == A.TextAlignmentTypeValues.Center) ip.Alignment = HorizontalAlign.Center;
        else if (algn == A.TextAlignmentTypeValues.Right) ip.Alignment = HorizontalAlign.Right;
        else if (algn == A.TextAlignmentTypeValues.Justified) ip.Alignment = HorizontalAlign.Justify;

        // 列表层级（PPTX 用 buChar/buAutoNum/lvl 表示；这里仅做层级缩进与 None/Bullet 推断）
        int lvl = pPr?.Level?.Value ?? 0;
        if (lvl > 0) ip.ListLevel = lvl;
        var buAuto = pPr?.GetFirstChild<A.AutoNumberedBullet>();
        var buChar = pPr?.GetFirstChild<A.CharacterBullet>();
        var buNone = pPr?.GetFirstChild<A.NoBullet>();
        if (buNone == null && (buAuto != null || buChar != null || (lvl > 0 && !inTitle)))
        {
            ip.ListType = buAuto != null ? ListType.Decimal : ListType.Bullet;
            if (buAuto == null) ip.ListLabel = buChar?.Char?.Value ?? "•";
        }

        foreach (var run in para.Elements<A.Run>())
        {
            var text = run.Text?.Text ?? string.Empty;
            if (string.IsNullOrEmpty(text)) continue;
            var rPr = run.RunProperties;
            var ir0 = new IrRun
            {
                Text = text,
                Bold = rPr?.Bold?.Value ?? false,
                Italic = rPr?.Italic?.Value ?? false,
                Underline = (rPr?.Underline?.Value ?? A.TextUnderlineValues.None) != A.TextUnderlineValues.None,
            };
            if (rPr?.FontSize?.Value is int sz) ir0.FontSize = sz / 100.0;
            var fill = rPr?.GetFirstChild<A.SolidFill>();
            var rgb = fill?.RgbColorModelHex?.Val?.Value;
            if (!string.IsNullOrEmpty(rgb) && rgb!.Length == 6)
                ir0.ColorHex = "#" + rgb.ToUpperInvariant();
            var latin = rPr?.GetFirstChild<A.LatinFont>();
            var ea = rPr?.GetFirstChild<A.EastAsianFont>();
            ir0.FontFamily = latin?.Typeface?.Value ?? ea?.Typeface?.Value;
            ip.Runs.Add(ir0);
        }

        // 处理纯换行
        if (ip.Runs.Count == 0)
        {
            foreach (var br in para.Elements<A.Break>())
                ip.Runs.Add(new IrRun { Text = string.Empty });
        }
        if (ip.Runs.Count == 0) return null;
        return ip;
    }

    private static void BuildGraphicFrame(P.GraphicFrame gf, IrDocument ir)
    {
        // 仅支持 a:tbl
        var tbl = gf.Descendants<A.Table>().FirstOrDefault();
        if (tbl == null) return;

        var t = new IrTable();
        var grid = tbl.TableGrid;
        if (grid != null)
        {
            foreach (var col in grid.Elements<A.GridColumn>())
            {
                if (col.Width?.Value is long wEmu)
                    t.ColumnWidthsPt.Add(wEmu / 12700.0);
                else
                    t.ColumnWidthsPt.Add(null);
            }
        }

        foreach (var row in tbl.Elements<A.TableRow>())
        {
            var ir0 = new IrRow();
            if (row.Height?.Value is long hEmu) ir0.HeightPt = hEmu / 12700.0;
            foreach (var cell in row.Elements<A.TableCell>())
            {
                var c = new IrCell();
                if (cell.GridSpan?.Value is int gs) c.ColSpan = gs;
                if (cell.RowSpan?.Value is int rs) c.RowSpan = rs;
                if ((cell.HorizontalMerge?.Value ?? false) || (cell.VerticalMerge?.Value ?? false))
                    c.Suppressed = true;
                var body = cell.TextBody;
                if (body != null)
                {
                    foreach (var para in body.Elements<A.Paragraph>())
                    {
                        var ip = BuildParagraph(para, inTitle: false);
                        if (ip != null) c.Paragraphs.Add(ip);
                    }
                    c.Text = string.Concat(c.Paragraphs.SelectMany(p => p.Runs).Select(r => r.Text));
                }
                ir0.Cells.Add(c);
            }
            t.Rows.Add(ir0);
        }
        ir.Blocks.Add(IrBlock.Of(t));
    }

    private static void BuildPicture(P.Picture pic, SlidePart part, IrDocument ir)
    {
        var blip = pic.Descendants<A.Blip>().FirstOrDefault();
        var rid = blip?.Embed?.Value;
        if (string.IsNullOrEmpty(rid)) return;
        if (part.GetPartById(rid!) is not ImagePart imgPart) return;

        try
        {
            using var s = imgPart.GetStream();
            using var copy = new MemoryStream();
            s.CopyTo(copy);
            var ctype = imgPart.ContentType?.Split('/').LastOrDefault();
            var rawBytes = copy.ToArray();
            bool isVector = !string.IsNullOrEmpty(ctype) &&
                (ctype!.IndexOf("emf", StringComparison.OrdinalIgnoreCase) >= 0
                || ctype.IndexOf("wmf", StringComparison.OrdinalIgnoreCase) >= 0
                || ctype.IndexOf("svg", StringComparison.OrdinalIgnoreCase) >= 0);
            byte[] dataBytes = isVector ? EmfWmfHelper.RasterizeToPng(rawBytes, ctype) : rawBytes;
            var img = new IrImage
            {
                Data = dataBytes,
                Format = isVector ? "png" : ctype,
                IsVector = isVector,
                VectorFormat = isVector ? ctype : null,
            };
            // 形状框尺寸（EMU → px 近似按 96 DPI：1 px = 9525 EMU）
            var ext = pic.ShapeProperties?.Transform2D?.Extents;
            if (ext?.Cx?.Value is long cx) img.WidthPx = cx / 9525.0;
            if (ext?.Cy?.Value is long cy) img.HeightPx = cy / 9525.0;
            ir.Blocks.Add(IrBlock.Of(img));
        }
        catch { /* swallow */ }
    }

    private static string MakeSlideAnchor(int index, string title)
    {
        var safe = string.Concat((title ?? string.Empty).Where(c => char.IsLetterOrDigit(c) || c == '_'));
        if (safe.Length > 32) safe = safe.Substring(0, 32);
        return "slide_" + index + (string.IsNullOrEmpty(safe) ? string.Empty : "_" + safe);
    }
}
