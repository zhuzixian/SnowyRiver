namespace SnowyRiver.Documents.Converter.Abstractions;

/// <summary>PDF 合规等级。</summary>
public enum PdfStandard
{
    /// <summary>普通 PDF（默认）。</summary>
    None = 0,
    /// <summary>PDF/A-2B（长期归档，常用）。</summary>
    PdfA2B,
    /// <summary>PDF/A-3B。</summary>
    PdfA3B,
}

/// <summary>
/// 文档转换的可选参数。
/// </summary>
public sealed class ConversionOptions
{
    /// <summary>文档元数据：标题。</summary>
    public string? Title { get; set; }

    /// <summary>文档元数据：作者。</summary>
    public string? Author { get; set; }

    /// <summary>文档元数据：主题。</summary>
    public string? Subject { get; set; }

    /// <summary>文档元数据：关键字（逗号分隔）。</summary>
    public string? Keywords { get; set; }

    /// <summary>文档元数据：生成器（PDF Producer）。默认 SnowyRiver.Documents.Converter。</summary>
    public string Producer { get; set; } = "SnowyRiver.Documents.Converter";

    /// <summary>
    /// PDF 渲染时使用的默认字体族（用于无法解析字体的回退）。
    /// 中文环境推荐 "Microsoft YaHei"。
    /// </summary>
    public string DefaultFontFamily { get; set; } = "Microsoft YaHei";

    /// <summary>
    /// 字体回退链：当某个字符在主字体中无字形时，依次尝试这些字体族。
    /// 跨平台/容器环境推荐附加 "Noto Sans CJK SC", "Segoe UI Symbol" 等。
    /// </summary>
    public IList<string> FallbackFontFamilies { get; } = new List<string>
    {
        "Microsoft YaHei",
        "Segoe UI",
        "Arial",
    };

    /// <summary>PDF 合规等级。默认 <see cref="PdfStandard.None"/>。</summary>
    public PdfStandard PdfStandard { get; set; } = PdfStandard.None;

    /// <summary>是否为 Word 标题段落与 Excel 工作表生成 PDF 大纲（书签）。默认 true。</summary>
    public bool EnablePdfBookmarks { get; set; } = true;

    /// <summary>是否在 PDF 中保留超链接（Word 中的 hyperlink）。默认 true。</summary>
    public bool PreserveHyperlinks { get; set; } = true;

    /// <summary>是否在 PDF 文档开头自动生成目录（基于标题段落的 AnchorId）。默认 false。</summary>
    public bool GenerateToc { get; set; }

    /// <summary>目录页标题文本。默认 "目录"。</summary>
    public string TocTitle { get; set; } = "目录";

    /// <summary>目录中包含的最大标题层级。默认 3。</summary>
    public int TocMaxLevel { get; set; } = 3;

    /// <summary>当目标为 XPS 时，是否启用基于 WPF FlowDocument 的直出路径（保留矢量、可选中文本）。默认 true；失败时回退到 PDF 栅格化。</summary>
    public bool UseDirectXps { get; set; } = true;

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

    /// <summary>可选日志工厂；为 null 时不输出日志。</summary>
    public Microsoft.Extensions.Logging.ILoggerFactory? LoggerFactory { get; set; }

    /// <summary>转换过程中要预先注册到 QuestPDF 的字体文件路径（TTF/OTF）。容器化部署用于注入中文字体。</summary>
    public IList<string> EmbeddedFontPaths { get; } = new List<string>();

    /// <summary>转换过程中要预先注册到 QuestPDF 的字体字节流。优先级高于 <see cref="EmbeddedFontPaths"/>。</summary>
    public IList<byte[]> EmbeddedFontStreams { get; } = new List<byte[]>();

    /// <summary>是否启用 Word 字段求值（TOC/REF/PAGEREF/DATE/TIME/HYPERLINK）。默认 true。</summary>
    public bool EnableWordFields { get; set; } = true;

    /// <summary>是否启用 Excel 数字格式（按 numFmt 格式化数值/日期）。默认 true。</summary>
    public bool EnableExcelNumberFormat { get; set; } = true;

    /// <summary>是否启用 Excel 条件格式渲染（颜色 / 字体颜色快照）。默认 true。</summary>
    public bool EnableExcelConditionalFormat { get; set; } = true;

    /// <summary>是否在 Excel 渲染时跳过隐藏行/列。默认 true。</summary>
    public bool SkipHiddenExcelRowsAndColumns { get; set; } = true;

    /// <summary>是否在 Excel 渲染时按 Print_Titles 重复行/列。默认 true。</summary>
    public bool RepeatExcelPrintTitles { get; set; } = true;

    /// <summary>
    /// 是否对 Excel 中未显式指定列宽的列进行 auto-fit 真实文本度量（基于 SkiaSharp）。
    /// 关闭后保持 ClosedXML 默认列宽。默认 true。
    /// </summary>
    public bool EnableExcelAutoFitColumns { get; set; } = true;

    /// <summary>
    /// 是否扫描宿主操作系统字体目录并注册到 QuestPDF（Windows: %WINDIR%\Fonts；Linux: /usr/share/fonts 等）。
    /// 关闭后仅使用 <see cref="EmbeddedFontPaths"/>/<see cref="EmbeddedFontStreams"/> 显式注入的字体。默认 true。
    /// </summary>
    public bool EnableSystemFontScan { get; set; } = true;

    /// <summary>页面水印文本；为 null/空表示不绘制水印。</summary>
    public string? WatermarkText { get; set; }

    /// <summary>水印颜色 #RRGGBB；默认浅灰。</summary>
    public string WatermarkColorHex { get; set; } = "#D3D3D3";

    /// <summary>水印字体大小（pt）。默认 60。</summary>
    public double WatermarkFontSize { get; set; } = 60;

    /// <summary>水印旋转角度（度）。默认 -45。</summary>
    public double WatermarkRotationDegrees { get; set; } = -45;

    /// <summary>
    /// 可选数学公式渲染器。注入后渲染管线会优先调用 <see cref="IMathRenderer"/> 把段落级公式
    /// 渲染为图片插入；为 null 时保持线性化文本回退行为，且不影响 IR 中的 MathML 字段。
    /// </summary>
    public IMathRenderer? MathRenderer { get; set; }

    /// <summary>是否启用 PDF/UA（无障碍）输出。默认 false。</summary>
    public bool PdfUaEnabled { get; set; }

    /// <summary>PDF 所有者密码（设置后用于加密 PDF 输出，需配合 <see cref="PdfEncryptionStrength"/>）。null 表示不加密。</summary>
    public string? PdfOwnerPassword { get; set; }

    /// <summary>PDF 用户密码（设置后打开 PDF 需要输入；可与所有者密码不同）。null 表示打开时不需要密码。</summary>
    public string? PdfUserPassword { get; set; }

    /// <summary>PDF 加密强度。仅当设置了 <see cref="PdfOwnerPassword"/> 或 <see cref="PdfUserPassword"/> 时生效。默认 Aes256。</summary>
    public PdfEncryptionStrength PdfEncryptionStrength { get; set; } = PdfEncryptionStrength.Aes256;

    /// <summary>PDF 加密时授予的权限位组合。默认允许打印+复制+辅助设备访问。</summary>
    public PdfPermissions PdfPermissions { get; set; } =
        PdfPermissions.AllowPrinting | PdfPermissions.AllowContentCopying | PdfPermissions.AllowContentCopyingForAccessibility;

    /// <summary>PDF 内嵌图像的目标光栅 DPI 上限（QuestPDF ImageRasterDpi）。默认 288。</summary>
    public int PdfImageRasterDpi { get; set; } = 288;

    /// <summary>PDF 内嵌图像的压缩质量（QuestPDF ImageCompressionQuality）。默认 High。</summary>
    public PdfImageCompression PdfImageCompression { get; set; } = PdfImageCompression.High;

    /// <summary>是否启用按 Unicode 段落对 run 进行字体切片（CJK/Latin/Symbol 分别选最佳字体），减少回退乱码。默认 true。</summary>
    public bool EnableCjkFontSlicing { get; set; } = true;

    /// <summary>是否使用 QuestPDF 原生 MultiColumn 实现真实多列回流（替代旧版按段落桶式分配）。默认 true。</summary>
    public bool UseNativeMultiColumn { get; set; } = true;

    /// <summary>默认实例。</summary>
    public static ConversionOptions Default { get; } = new();
}

/// <summary>PDF 加密强度。</summary>
public enum PdfEncryptionStrength
{
    /// <summary>RC4-40 位（兼容性最好，强度最低）。</summary>
    Rc4_40,
    /// <summary>RC4-128 位。</summary>
    Rc4_128,
    /// <summary>AES-256 位（默认推荐）。</summary>
    Aes256,
}

/// <summary>PDF 内嵌图像的压缩质量。映射到 QuestPDF ImageCompressionQuality。</summary>
public enum PdfImageCompression
{
    /// <summary>最佳质量（最大文件）。</summary>
    Best,
    /// <summary>高质量（默认）。</summary>
    High,
    /// <summary>中等。</summary>
    Medium,
    /// <summary>低质量（更小文件）。</summary>
    Low,
    /// <summary>极低质量。</summary>
    VeryLow,
}

/// <summary>PDF 加密时授予用户的权限位（可按位组合）。</summary>
[System.Flags]
public enum PdfPermissions
{
    /// <summary>无任何额外权限。</summary>
    None = 0,
    /// <summary>允许打印。</summary>
    AllowPrinting = 1 << 0,
    /// <summary>允许修改内容。</summary>
    AllowContentModification = 1 << 1,
    /// <summary>允许复制内容。</summary>
    AllowContentCopying = 1 << 2,
    /// <summary>允许添加注释。</summary>
    AllowAnnotations = 1 << 3,
    /// <summary>允许填写表单。</summary>
    AllowFillingForms = 1 << 4,
    /// <summary>允许辅助设备读取（屏幕阅读器等）。</summary>
    AllowContentCopyingForAccessibility = 1 << 5,
    /// <summary>允许文档组装（提取/插入页面）。</summary>
    AllowDocumentAssembly = 1 << 6,
    /// <summary>允许高质量打印。</summary>
    AllowHighQualityPrinting = 1 << 7,
}

