using System.Collections.Generic;

namespace SnowyRiver.Documents.Converter.Abstractions;

/// <summary>转换诊断信息严重性。</summary>
public enum DiagnosticSeverity
{
    Info,
    Warning,
    Error,
}

/// <summary>诊断所属的转换阶段（结构化定位用，可选）。</summary>
public enum DiagnosticStage
{
    Unknown = 0,
    Reading,
    Layout,
    Rendering,
    Finalizing,
    FontRegistration,
    Vector,
    Math,
    Chart,
}

/// <summary>
/// 强类型诊断代码常量。所有 <see cref="ConversionDiagnostics"/> 调用方应优先使用此处常量，
/// 避免字符串字面量散落带来的拼写漂移；保留 string 类型以兼容历史 API。
/// </summary>
public static class DiagCodes
{
    // 字体
    public const string FONT_REGISTER_FAIL = nameof(FONT_REGISTER_FAIL);
    public const string FONT_SCAN_FAIL = nameof(FONT_SCAN_FAIL);

    // 矢量 / EMF / WMF / SVG
    public const string VECTOR_EMPTY = nameof(VECTOR_EMPTY);
    public const string VECTOR_RASTERIZE_FAIL = nameof(VECTOR_RASTERIZE_FAIL);
    public const string VECTOR_PLATFORM_UNSUPPORTED = nameof(VECTOR_PLATFORM_UNSUPPORTED);
    public const string EMFPLUS_DETECTED = nameof(EMFPLUS_DETECTED);
    public const string SVG_EMPTY = nameof(SVG_EMPTY);
    public const string SVG_PARSE_FAIL = nameof(SVG_PARSE_FAIL);
    public const string SVG_LOAD_NULL = nameof(SVG_LOAD_NULL);
    public const string SVG_RASTERIZE_FAIL = nameof(SVG_RASTERIZE_FAIL);
    public const string SVG_RASTERIZED = nameof(SVG_RASTERIZED);

    // 公式
    public const string MATH_LINEAR_RENDERED = nameof(MATH_LINEAR_RENDERED);
    public const string MATH_LINEAR_FAIL = nameof(MATH_LINEAR_FAIL);
    public const string MATH_MML_RENDERED = nameof(MATH_MML_RENDERED);
    public const string MATH_MML_FAIL = nameof(MATH_MML_FAIL);
    public const string MATH_MML_PARSE_FAIL = nameof(MATH_MML_PARSE_FAIL);

    // PDF 后处理
    public const string PDF_ENCRYPT_FAIL = nameof(PDF_ENCRYPT_FAIL);
}

/// <summary>转换诊断条目。</summary>
public sealed class DiagnosticEntry
{
    public DiagnosticSeverity Severity { get; init; }
    /// <summary>简短代码（例如 "OMML_NOT_RENDERED"），便于宿主分类，建议使用 <see cref="DiagCodes"/>。</summary>
    public string Code { get; init; } = string.Empty;
    /// <summary>面向用户的可读消息。</summary>
    public string Message { get; init; } = string.Empty;
    /// <summary>可选定位信息（如 "section=1, paragraph=12" 或 "sheet=Sheet1, cell=B3"）。</summary>
    public string? Location { get; init; }
    /// <summary>所属阶段（可选）。</summary>
    public DiagnosticStage Stage { get; init; } = DiagnosticStage.Unknown;
    /// <summary>所属 IR Block 索引（可选）。</summary>
    public int? BlockIndex { get; init; }

    public override string ToString()
    {
        var stagePart = Stage == DiagnosticStage.Unknown ? string.Empty : $" {Stage}";
        var locPart = string.IsNullOrEmpty(Location) ? string.Empty : $" ({Location})";
        return $"[{Severity}{stagePart}] {Code}{locPart}: {Message}";
    }
}

/// <summary>转换过程中收集的诊断列表（线程不安全：仅在 Convert 同步段写入）。</summary>
public sealed class ConversionDiagnostics
{
    private readonly List<DiagnosticEntry> _entries = new();

    public IReadOnlyList<DiagnosticEntry> Entries => _entries;

    public void Add(DiagnosticSeverity severity, string code, string message, string? location = null,
        DiagnosticStage stage = DiagnosticStage.Unknown, int? blockIndex = null)
        => _entries.Add(new DiagnosticEntry
        {
            Severity = severity,
            Code = code,
            Message = message,
            Location = location,
            Stage = stage,
            BlockIndex = blockIndex,
        });

    public void Info(string code, string message, string? location = null,
        DiagnosticStage stage = DiagnosticStage.Unknown, int? blockIndex = null)
        => Add(DiagnosticSeverity.Info, code, message, location, stage, blockIndex);

    public void Warn(string code, string message, string? location = null,
        DiagnosticStage stage = DiagnosticStage.Unknown, int? blockIndex = null)
        => Add(DiagnosticSeverity.Warning, code, message, location, stage, blockIndex);

    public void Error(string code, string message, string? location = null,
        DiagnosticStage stage = DiagnosticStage.Unknown, int? blockIndex = null)
        => Add(DiagnosticSeverity.Error, code, message, location, stage, blockIndex);

    /// <summary>按严重性筛选。</summary>
    public IEnumerable<DiagnosticEntry> Where(DiagnosticSeverity severity)
    {
        foreach (var e in _entries) if (e.Severity == severity) yield return e;
    }

    /// <summary>按代码筛选（精确匹配）。</summary>
    public IEnumerable<DiagnosticEntry> Where(string code)
    {
        foreach (var e in _entries) if (e.Code == code) yield return e;
    }
}

