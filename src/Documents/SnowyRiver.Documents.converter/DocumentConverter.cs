using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
        Convert(source, srcFmt, target, dstFmt, options, sourcePath);
    }

    public void Convert(
        Stream source,
        DocFormat sourceFormat,
        Stream target,
        DocFormat targetFormat,
        ConversionOptions? options = null)
        => ConvertCore(source, sourceFormat, target, targetFormat, options, CancellationToken.None);

    public void Convert(
        Stream source,
        DocFormat sourceFormat,
        Stream target,
        DocFormat targetFormat,
        ConversionOptions? options,
        CancellationToken cancellationToken)
        => ConvertCore(source, sourceFormat, target, targetFormat, options, cancellationToken);

    internal void Convert(
        Stream source,
        DocFormat sourceFormat,
        Stream target,
        DocFormat targetFormat,
        ConversionOptions? options,
        string? sourceFileName)
        => ConvertCore(source, sourceFormat, target, targetFormat, options, CancellationToken.None, sourceFileName: sourceFileName);

    public Task ConvertAsync(
        Stream source,
        DocFormat sourceFormat,
        Stream target,
        DocFormat targetFormat,
        ConversionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        if (target is null) throw new ArgumentNullException(nameof(target));
        return Task.Run(() => ConvertCore(source, sourceFormat, target, targetFormat, options, cancellationToken),
            cancellationToken);
    }

    public Task ConvertAsync(
        string sourcePath,
        string targetPath,
        ConversionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(sourcePath)) throw new ArgumentNullException(nameof(sourcePath));
        if (string.IsNullOrEmpty(targetPath)) throw new ArgumentNullException(nameof(targetPath));
        return Task.Run(() =>
        {
            var srcFmt = InferFormat(sourcePath);
            var dstFmt = InferFormat(targetPath);
            using var source = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var target = new FileStream(targetPath, FileMode.Create, FileAccess.Write);
            ConvertCore(source, srcFmt, target, dstFmt, options, cancellationToken, sourceFileName: sourcePath);
        }, cancellationToken);
    }

    public Task<ConversionResult> ConvertWithDiagnosticsAsync(
        Stream source,
        DocFormat sourceFormat,
        Stream target,
        DocFormat targetFormat,
        ConversionOptions? options = null,
        IProgress<ConversionProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        if (target is null) throw new ArgumentNullException(nameof(target));
        return Task.Run(() =>
        {
            var result = new ConversionResult();
            ConvertCore(source, sourceFormat, target, targetFormat, options, cancellationToken, result, progress);
            return result;
        }, cancellationToken);
    }

    public byte[] Convert(
        byte[] source,
        DocFormat sourceFormat,
        DocFormat targetFormat,
        ConversionOptions? options = null)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        using var sourceStream = new MemoryStream(source, writable: false);
        using var targetStream = new MemoryStream();
        Convert(sourceStream, sourceFormat, targetStream, targetFormat, options);
        return targetStream.ToArray();
    }

    public byte[] Convert(
        byte[] source,
        DocFormat sourceFormat,
        DocFormat targetFormat,
        ConversionOptions? options,
        CancellationToken cancellationToken)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        using var sourceStream = new MemoryStream(source, writable: false);
        using var targetStream = new MemoryStream();
        Convert(sourceStream, sourceFormat, targetStream, targetFormat, options, cancellationToken);
        return targetStream.ToArray();
    }

    public Task<byte[]> ConvertAsync(
        byte[] source,
        DocFormat sourceFormat,
        DocFormat targetFormat,
        ConversionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        return Task.Run(() =>
        {
            using var sourceStream = new MemoryStream(source, writable: false);
            using var targetStream = new MemoryStream();
            ConvertCore(sourceStream, sourceFormat, targetStream, targetFormat, options, cancellationToken);
            return targetStream.ToArray();
        }, cancellationToken);
    }

    public Task<(byte[] data, ConversionResult diagnostics)> ConvertWithDiagnosticsAsync(
        byte[] source,
        DocFormat sourceFormat,
        DocFormat targetFormat,
        ConversionOptions? options = null,
        IProgress<ConversionProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        return Task.Run(() =>
        {
            using var sourceStream = new MemoryStream(source, writable: false);
            using var targetStream = new MemoryStream();
            var result = new ConversionResult();
            ConvertCore(sourceStream, sourceFormat, targetStream, targetFormat, options, cancellationToken, result, progress);
            return (targetStream.ToArray(), result);
        }, cancellationToken);
    }

    private static void ConvertCore(
        Stream source,
        DocFormat sourceFormat,
        Stream target,
        DocFormat targetFormat,
        ConversionOptions? options,
        CancellationToken cancellationToken,
        ConversionResult? result = null,
        IProgress<ConversionProgress>? progress = null,
        string? sourceFileName = null)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        if (target is null) throw new ArgumentNullException(nameof(target));
        var opt = options ?? new ConversionOptions();
        var log = opt.LoggerFactory?.CreateLogger("SnowyRiver.Documents.Converter.DocumentConverter");
        var diag = result?.Diagnostics;

        // 集中字体注册（QuestPDF 全局注册即可，多次调用幂等：同名字体只注册一次。）
        TryRegisterFonts(opt, log, diag);

        cancellationToken.ThrowIfCancellationRequested();
        progress?.Report(new ConversionProgress { Stage = ConversionStage.Reading, Percent = 0.0, Message = sourceFormat.ToString() });

        // 1) 源 → IR
        log?.LogDebug("Reading source as {SourceFormat}.", sourceFormat);
        IrDocument ir = sourceFormat switch
        {
            DocFormat.Word => WordReader.Read(source, opt, diag),
            DocFormat.Excel => ExcelReader.Read(source, opt, diag, sourceFileName),
            DocFormat.PowerPoint => PowerPointReader.Read(source, opt, diag),
            _ => throw new NotSupportedException($"暂不支持从 {sourceFormat} 读取，仅支持 Word / Excel / PowerPoint 作为源。"),
        };
        if (result != null) result.SourceBlockCount = ir.Blocks.Count;

        cancellationToken.ThrowIfCancellationRequested();
        progress?.Report(new ConversionProgress { Stage = ConversionStage.Rendering, Percent = 0.5, Message = targetFormat.ToString() });

        // 2) IR → 目标
        log?.LogDebug("Rendering IR as {TargetFormat}.", targetFormat);
        switch (targetFormat)
        {
            case DocFormat.Pdf:
                PdfRenderer.Render(ir, target, opt, diag);
                break;
            case DocFormat.Xps:
                {
#if SR_WINDOWS
                    if (opt.UseDirectXps)
                    {
                        XpsRenderer.RenderIrToXps(ir, target, opt);
                    }
                    else
                    {
                        using var pdfMem = new MemoryStream();
                        PdfRenderer.Render(ir, pdfMem, opt, diag);
                        pdfMem.Position = 0;
                        cancellationToken.ThrowIfCancellationRequested();
                        XpsRenderer.RenderPdfToXps(pdfMem, target, opt);
                    }
                    break;
#else
                    throw new PlatformNotSupportedException(
                        "XPS 输出当前仅在 Windows TFM (net10.0-windows) 下可用，请改用 net10.0-windows 目标或选择 PDF 输出。");
#endif
                }
            default:
                throw new NotSupportedException($"暂不支持目标格式 {targetFormat}，仅支持 PDF / XPS。");
        }

        progress?.Report(new ConversionProgress { Stage = ConversionStage.Finalizing, Percent = 1.0 });
    }

    private static void TryRegisterFonts(ConversionOptions opt, ILogger? log, ConversionDiagnostics? diag)
    {
        // 第十轮：先扫描宿主系统字体并补全 CJK 回退链；再注册显式注入的字体。
        Engines.FontRegistry.EnsureRegistered(opt, log, diag);
        try
        {
            foreach (var bytes in opt.EmbeddedFontStreams)
            {
                if (bytes is null || bytes.Length == 0) continue;
                using var ms = new MemoryStream(bytes, writable: false);
                QuestPDF.Drawing.FontManager.RegisterFont(ms);
            }
            foreach (var path in opt.EmbeddedFontPaths)
            {
                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) continue;
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                QuestPDF.Drawing.FontManager.RegisterFont(fs);
            }
        }
        catch (Exception ex)
        {
            log?.LogWarning(ex, "Failed to register embedded fonts.");
            diag?.Warn("FONT_REGISTER_FAIL", $"嵌入字体注册失败：{ex.Message}");
        }
    }

    private static DocFormat InferFormat(string path)
    {
        var ext = Path.GetExtension(path)?.ToLowerInvariant();
        return ext switch
        {
            ".docx" => DocFormat.Word,
            ".xlsx" => DocFormat.Excel,
            ".pptx" => DocFormat.PowerPoint,
            ".pdf" => DocFormat.Pdf,
            ".xps" => DocFormat.Xps,
            _ => throw new NotSupportedException($"无法根据扩展名推断格式：{ext}"),
        };
    }
}
