namespace SnowyRiver.Documents.Converter.Abstractions;

/// <summary>
/// 受支持的文档格式。
/// </summary>
public enum DocFormat
{
    /// <summary>未知/未指定。</summary>
    Unknown = 0,
    /// <summary>Microsoft Word 文档（.docx）。</summary>
    Word,
    /// <summary>Microsoft Excel 工作簿（.xlsx）。</summary>
    Excel,
    /// <summary>便携式文档格式（.pdf）。</summary>
    Pdf,
    /// <summary>XML 纸张规格文档（.xps）。</summary>
    Xps,
}
