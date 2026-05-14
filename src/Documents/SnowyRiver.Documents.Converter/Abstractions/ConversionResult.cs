namespace SnowyRiver.Documents.Converter.Abstractions;

/// <summary>转换结果（包含诊断与统计）。</summary>
public sealed class ConversionResult
{
    public ConversionDiagnostics Diagnostics { get; } = new();
    /// <summary>渲染输出的页数（如可统计）。</summary>
    public int? PageCount { get; set; }
    /// <summary>读取得到的源文档块数。</summary>
    public int? SourceBlockCount { get; set; }
    /// <summary>是否成功完成（无 Error 级诊断）。</summary>
    public bool Success
    {
        get
        {
            foreach (var e in Diagnostics.Entries)
                if (e.Severity == DiagnosticSeverity.Error) return false;
            return true;
        }
    }
}
