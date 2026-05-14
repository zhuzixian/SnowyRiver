using System;
using System.IO;
using SnowyRiver.Documents.Converter.Abstractions;
using SnowyRiver.Documents.Converter.Engines;
using SnowyRiver.Documents.Converter.Model;

namespace SnowyRiver.Documents.Converter;

/// <summary>
/// 默认文档转换器实现。当前支持：
/// Word → PDF、Word → XPS、Excel → PDF、Excel → XPS。
/// </summary>
public sealed class DocumentConverter : IDocumentConverter
{
    public void Convert(string sourcePath, string targetPath, ConversionOptions? options = null)
    {
        if (string.IsNullOrEmpty(sourcePath)) throw new ArgumentNullException(nameof(sourcePath));
        if (string.IsNullOrEmpty(targetPath)) throw new ArgumentNullException(nameof(targetPath));

        var srcFmt = InferFormat(sourcePath);
        var dstFmt = InferFormat(targetPath);

        using var source = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var target = new FileStream(targetPath, FileMode.Create, FileAccess.Write);
        Convert(source, srcFmt, target, dstFmt, options);
    }

    public void Convert(
        Stream source,
        DocFormat sourceFormat,
        Stream target,
        DocFormat targetFormat,
        ConversionOptions? options = null)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        if (target is null) throw new ArgumentNullException(nameof(target));
        var opt = options ?? new ConversionOptions();

        // 1) 源 → IR
        IrDocument ir = sourceFormat switch
        {
            DocFormat.Word => WordReader.Read(source, opt),
            DocFormat.Excel => ExcelReader.Read(source, opt),
            _ => throw new NotSupportedException($"暂不支持从 {sourceFormat} 读取，仅支持 Word / Excel 作为源。"),
        };

        // 2) IR → 目标
        switch (targetFormat)
        {
            case DocFormat.Pdf:
                PdfRenderer.Render(ir, target, opt);
                break;
            case DocFormat.Xps:
                {
                    using var pdfMem = new MemoryStream();
                    PdfRenderer.Render(ir, pdfMem, opt);
                    pdfMem.Position = 0;
                    XpsRenderer.RenderPdfToXps(pdfMem, target, opt);
                    break;
                }
            default:
                throw new NotSupportedException($"暂不支持目标格式 {targetFormat}，仅支持 PDF / XPS。");
        }
    }

    private static DocFormat InferFormat(string path)
    {
        var ext = Path.GetExtension(path)?.ToLowerInvariant();
        return ext switch
        {
            ".docx" => DocFormat.Word,
            ".xlsx" => DocFormat.Excel,
            ".pdf" => DocFormat.Pdf,
            ".xps" => DocFormat.Xps,
            _ => throw new NotSupportedException($"无法根据扩展名推断格式：{ext}"),
        };
    }
}
