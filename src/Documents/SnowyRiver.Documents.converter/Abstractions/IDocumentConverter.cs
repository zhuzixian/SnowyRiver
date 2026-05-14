using System.IO;

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

    /// <summary>
    /// 按文件扩展名推断格式并转换。
    /// </summary>
    void Convert(string sourcePath, string targetPath, ConversionOptions? options = null);
}
