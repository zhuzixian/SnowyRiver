using System;

namespace SnowyRiver.Documents.Converter.Abstractions;

/// <summary>
/// 数学公式渲染抽象：将 OMML 线性化文本或 MathML 字符串渲染为 PNG 图片。
/// 默认管线不内置实现；调用方可注入第三方实现（如 MathJax/JEB Math/自研引擎）以提升公式保真度。
/// </summary>
public interface IMathRenderer
{
    /// <summary>
    /// 将一段 MathML 字符串渲染为 PNG。
    /// </summary>
    /// <param name="mathML">MathML 3 兼容字符串，必须以 <c>&lt;math ...&gt;</c> 为根。</param>
    /// <param name="emPx">目标 em 高度像素值；用于推断字号/分辨率。</param>
    /// <param name="diag">可选诊断；实现可在失败/降级时记录。</param>
    /// <returns>PNG 字节流；返回 null 表示渲染失败，由调用方决定回退策略。</returns>
    byte[]? RenderMathMLToPng(string mathML, double emPx = 16, ConversionDiagnostics? diag = null);

    /// <summary>
    /// 将一段线性化公式文本（OMML 线性化结果，如 <c>a/b</c>、<c>x^2</c>）渲染为 PNG。
    /// 当实现仅支持 MathML 时可在内部转换或返回 null。
    /// </summary>
    byte[]? RenderLinearToPng(string linear, double emPx = 16, ConversionDiagnostics? diag = null);
}
