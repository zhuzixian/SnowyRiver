namespace SnowyRiver.Documents.Converter.Abstractions;

/// <summary>转换进度阶段。</summary>
public enum ConversionStage
{
    Reading,
    Rendering,
    Finalizing,
}

/// <summary>转换进度报告。</summary>
public sealed class ConversionProgress
{
    public ConversionStage Stage { get; init; }
    /// <summary>0..1 的整体百分比；未知时为 null。</summary>
    public double? Percent { get; init; }
    /// <summary>当前正在处理的对象描述（节/页/工作表）。</summary>
    public string? Message { get; init; }
}
