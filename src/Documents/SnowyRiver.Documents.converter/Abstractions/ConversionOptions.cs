namespace SnowyRiver.Documents.Converter.Abstractions;

/// <summary>
/// 文档转换的可选参数。
/// </summary>
public sealed class ConversionOptions
{
    /// <summary>文档元数据：标题。</summary>
    public string? Title { get; set; }

    /// <summary>文档元数据：作者。</summary>
    public string? Author { get; set; }

    /// <summary>
    /// PDF 渲染时使用的默认字体族（用于无法解析字体的回退）。
    /// 中文环境推荐 "Microsoft YaHei"。
    /// </summary>
    public string DefaultFontFamily { get; set; } = "Microsoft YaHei";

    /// <summary>
    /// 当目标为 XPS 时，对每页 PDF 栅格化的 DPI。
    /// 数值越高 XPS 越清晰但文件越大。默认 200 DPI。
    /// </summary>
    public int XpsRasterDpi { get; set; } = 200;

    /// <summary>
    /// Excel 图表自绘时使用的目标 DPI（影响 PNG 像素尺寸）。
    /// 默认 192 DPI（约 2 倍清晰度）。
    /// </summary>
    public int ChartRenderDpi { get; set; } = 192;

    /// <summary>
    /// Excel 图表自绘时使用的字体族。中文环境推荐 "Microsoft YaHei"。
    /// </summary>
    public string ChartFontFamily { get; set; } = "Microsoft YaHei";

    /// <summary>
    /// 是否启用 Excel 图表的 SkiaSharp 自绘。
    /// 关闭后会跳过图表（与旧版行为一致）。默认 true。
    /// </summary>
    public bool RenderExcelCharts { get; set; } = true;

    /// <summary>默认实例。</summary>
    public static ConversionOptions Default { get; } = new();
}
