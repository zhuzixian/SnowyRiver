using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SnowyRiver.Documents.Converter.Abstractions;

/// <summary>
/// 文档转换公共门面。当前支持 Word/Excel → PDF/XPS。
/// </summary>
public interface IDocumentConverter
{
    /// <summary>
    /// 把 <paramref name="source"/> 中由 <paramref name="sourceFormat"/> 指定格式的文档，
    /// 转换为 <paramref name="targetFormat"/> 并写入 <paramref name="target"/>。
    /// </summary>
    void Convert(
        Stream source,
        DocFormat sourceFormat,
        Stream target,
        DocFormat targetFormat,
        ConversionOptions? options = null);

    /// <summary>同步转换，支持 <paramref name="cancellationToken"/> 协作取消（在阶段切换处检查）。</summary>
    void Convert(
        Stream source,
        DocFormat sourceFormat,
        Stream target,
        DocFormat targetFormat,
        ConversionOptions? options,
        CancellationToken cancellationToken);

    /// <summary>
    /// 按文件扩展名推断格式并转换。
    /// </summary>
    void Convert(string sourcePath, string targetPath, ConversionOptions? options = null);

    /// <summary>异步执行转换；后台线程执行，并在阶段间检查 <paramref name="cancellationToken"/>。</summary>
    Task ConvertAsync(
        Stream source,
        DocFormat sourceFormat,
        Stream target,
        DocFormat targetFormat,
        ConversionOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>异步执行转换；按文件扩展名推断格式。</summary>
    Task ConvertAsync(
        string sourcePath,
        string targetPath,
        ConversionOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>异步执行转换并返回详细诊断结果；可上报进度。</summary>
    Task<ConversionResult> ConvertWithDiagnosticsAsync(
        Stream source,
        DocFormat sourceFormat,
        Stream target,
        DocFormat targetFormat,
        ConversionOptions? options = null,
        IProgress<ConversionProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 把 <paramref name="source"/> 中由 <paramref name="sourceFormat"/> 指定格式的文档，
    /// 转换为 <paramref name="targetFormat"/> 格式并返回字节数组。
    /// </summary>
    byte[] Convert(
        byte[] source,
        DocFormat sourceFormat,
        DocFormat targetFormat,
        ConversionOptions? options = null);

    /// <summary>同步转换，支持 <paramref name="cancellationToken"/> 协作取消（在阶段切换处检查）。</summary>
    byte[] Convert(
        byte[] source,
        DocFormat sourceFormat,
        DocFormat targetFormat,
        ConversionOptions? options,
        CancellationToken cancellationToken);

    /// <summary>异步执行转换；后台线程执行，并在阶段间检查 <paramref name="cancellationToken"/>。</summary>
    Task<byte[]> ConvertAsync(
        byte[] source,
        DocFormat sourceFormat,
        DocFormat targetFormat,
        ConversionOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>异步执行转换并返回详细诊断结果；可上报进度。</summary>
    Task<(byte[] data, ConversionResult diagnostics)> ConvertWithDiagnosticsAsync(
        byte[] source,
        DocFormat sourceFormat,
        DocFormat targetFormat,
        ConversionOptions? options = null,
        IProgress<ConversionProgress>? progress = null,
        CancellationToken cancellationToken = default);
}

