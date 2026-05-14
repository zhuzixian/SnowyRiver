using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using SkiaSharp;
using SnowyRiver.Documents.Converter.Abstractions;

namespace SnowyRiver.Documents.Converter.Engines;

/// <summary>
/// 基于 SkiaSharp 的轻量级 <see cref="IMathRenderer"/> 默认实现。
/// 支持 MathML 的常见子集：math/mrow/mi/mn/mo/mtext/mfrac/msup/msub/msubsup/msqrt/mfenced。
/// 不支持的节点会按子节点的内联文本退化渲染。线性化路径直接按等宽样式绘制。
/// </summary>
public sealed class SkiaMathRenderer : IMathRenderer
{
    private readonly string _fontFamily;

    public SkiaMathRenderer(string? fontFamily = null)
    {
        _fontFamily = string.IsNullOrEmpty(fontFamily) ? "Cambria Math" : fontFamily!;
    }

    public byte[]? RenderLinearToPng(string linear, double emPx = 16, ConversionDiagnostics? diag = null)
    {
        if (string.IsNullOrWhiteSpace(linear)) return null;
        try
        {
            var typeface = ResolveTypeface();
            var font = new SKFont(typeface, (float)emPx);
            var paint = new SKPaint { Color = SKColors.Black, IsAntialias = true };
            float width = font.MeasureText(linear) + 4;
            float height = (float)emPx * 1.6f;
            using var surface = SKSurface.Create(new SKImageInfo((int)Math.Ceiling(width), (int)Math.Ceiling(height)));
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.Transparent);
            canvas.DrawText(linear, 2, height * 0.75f, SKTextAlign.Left, font, paint);
            using var snap = surface.Snapshot();
            using var enc = snap.Encode(SKEncodedImageFormat.Png, 90);
            diag?.Info("MATH_LINEAR_RENDERED", $"线性公式渲染为 {width:F0}x{height:F0} PNG。");
            return enc.ToArray();
        }
        catch (Exception ex)
        {
            diag?.Warn("MATH_LINEAR_FAIL", $"线性公式渲染失败：{ex.Message}");
            return null;
        }
    }

    public byte[]? RenderMathMLToPng(string mathML, double emPx = 16, ConversionDiagnostics? diag = null)
    {
        if (string.IsNullOrWhiteSpace(mathML)) return null;
        XElement root;
        try
        {
            using var sr = new StringReader(mathML);
            var doc = XDocument.Load(sr);
            root = doc.Root!;
        }
        catch (Exception ex)
        {
            diag?.Warn("MATH_MML_PARSE_FAIL", $"MathML 解析失败：{ex.Message}");
            return null;
        }

        try
        {
            var typeface = ResolveTypeface();
            var font = new SKFont(typeface, (float)emPx);
            var paint = new SKPaint { Color = SKColors.Black, IsAntialias = true };
            var box = LayoutNode(root, font, paint);

            int W = Math.Max(1, (int)Math.Ceiling(box.Width + 4));
            int H = Math.Max(1, (int)Math.Ceiling(box.Height + 4));
            using var surface = SKSurface.Create(new SKImageInfo(W, H));
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.Transparent);
            box.Draw(canvas, 2, 2);
            using var snap = surface.Snapshot();
            using var enc = snap.Encode(SKEncodedImageFormat.Png, 90);
            diag?.Info("MATH_MML_RENDERED", $"MathML 渲染为 {W}x{H} PNG。");
            return enc.ToArray();
        }
        catch (Exception ex)
        {
            diag?.Warn("MATH_MML_FAIL", $"MathML 渲染失败：{ex.Message}");
            return null;
        }
    }

    private SKTypeface ResolveTypeface()
    {
        var t = SKTypeface.FromFamilyName(_fontFamily);
        if (t != null) return t;
        return SKTypeface.Default;
    }

    // ----------------- 简单盒模型布局 -----------------

    private abstract class Box
    {
        public float Width;
        public float Height;
        /// <summary>从盒顶到基线的距离。</summary>
        public float Baseline;
        public abstract void Draw(SKCanvas canvas, float x, float y);
    }

    private sealed class TextBox : Box
    {
        public string Text = string.Empty;
        public SKFont Font = null!;
        public SKPaint Paint = null!;
        public override void Draw(SKCanvas canvas, float x, float y)
        {
            canvas.DrawText(Text, x, y + Baseline, SKTextAlign.Left, Font, Paint);
        }
    }

    private sealed class RowBox : Box
    {
        public Box[] Children = Array.Empty<Box>();
        public float Spacing;
        public override void Draw(SKCanvas canvas, float x, float y)
        {
            float cx = x;
            foreach (var c in Children)
            {
                float dy = Baseline - c.Baseline;
                c.Draw(canvas, cx, y + dy);
                cx += c.Width + Spacing;
            }
        }
    }

    private sealed class FractionBox : Box
    {
        public Box Numerator = null!;
        public Box Denominator = null!;
        public SKPaint Rule = null!;
        public override void Draw(SKCanvas canvas, float x, float y)
        {
            float w = Width;
            float nx = x + (w - Numerator.Width) / 2f;
            float dx = x + (w - Denominator.Width) / 2f;
            Numerator.Draw(canvas, nx, y);
            float ruleY = y + Numerator.Height + 2;
            canvas.DrawLine(x, ruleY, x + w, ruleY, Rule);
            Denominator.Draw(canvas, dx, ruleY + 2);
        }
    }

    private sealed class ScriptBox : Box
    {
        public Box Base = null!;
        public Box? Sup;
        public Box? Sub;
        public override void Draw(SKCanvas canvas, float x, float y)
        {
            float baseTopOffset = (Sup != null) ? Sup.Height * 0.6f : 0;
            Base.Draw(canvas, x, y + baseTopOffset);
            if (Sup != null) Sup.Draw(canvas, x + Base.Width + 1, y);
            if (Sub != null) Sub.Draw(canvas, x + Base.Width + 1, y + baseTopOffset + Base.Height * 0.5f);
        }
    }

    private sealed class SqrtBox : Box
    {
        public Box Inner = null!;
        public SKPaint Stroke = null!;
        public override void Draw(SKCanvas canvas, float x, float y)
        {
            float pad = 4;
            float top = y + 1;
            float bottom = y + Height - 1;
            canvas.DrawLine(x, top + (bottom - top) * 0.5f, x + pad * 0.5f, bottom, Stroke);
            canvas.DrawLine(x + pad * 0.5f, bottom, x + pad, top, Stroke);
            canvas.DrawLine(x + pad, top, x + Width, top, Stroke);
            Inner.Draw(canvas, x + pad + 2, y + 2);
        }
    }

    private Box LayoutNode(XElement el, SKFont font, SKPaint paint)
    {
        string name = el.Name.LocalName;
        switch (name)
        {
            case "math":
            case "mrow":
            case "mstyle":
                return LayoutRow(el.Elements().ToArray(), font, paint, 1f);
            case "mfrac":
                {
                    var children = el.Elements().ToArray();
                    if (children.Length < 2) return MakeText(InnerText(el), font, paint);
                    var num = LayoutNode(children[0], font, paint);
                    var den = LayoutNode(children[1], font, paint);
                    var rule = new SKPaint { Color = SKColors.Black, StrokeWidth = Math.Max(1f, font.Size / 14f), IsAntialias = true };
                    float w = Math.Max(num.Width, den.Width) + 4;
                    float h = num.Height + den.Height + 4;
                    return new FractionBox { Numerator = num, Denominator = den, Rule = rule, Width = w, Height = h, Baseline = num.Height + 2 };
                }
            case "msup":
            case "msub":
            case "msubsup":
                {
                    var children = el.Elements().ToArray();
                    if (children.Length == 0) return MakeText(InnerText(el), font, paint);
                    var baseBox = LayoutNode(children[0], font, paint);
                    var smallFont = new SKFont(font.Typeface, font.Size * 0.7f);
                    Box? sup = null, sub = null;
                    if (name == "msup" && children.Length >= 2) sup = LayoutNode(children[1], smallFont, paint);
                    else if (name == "msub" && children.Length >= 2) sub = LayoutNode(children[1], smallFont, paint);
                    else if (name == "msubsup" && children.Length >= 3) { sub = LayoutNode(children[1], smallFont, paint); sup = LayoutNode(children[2], smallFont, paint); }
                    float scriptW = Math.Max(sup?.Width ?? 0, sub?.Width ?? 0);
                    float w = baseBox.Width + 1 + scriptW;
                    float topPad = sup?.Height * 0.6f ?? 0;
                    float bottomPad = sub?.Height * 0.5f ?? 0;
                    float h = topPad + baseBox.Height + bottomPad;
                    return new ScriptBox { Base = baseBox, Sup = sup, Sub = sub, Width = w, Height = h, Baseline = topPad + baseBox.Baseline };
                }
            case "msqrt":
                {
                    var inner = LayoutRow(el.Elements().ToArray(), font, paint, 1f);
                    var stroke = new SKPaint { Color = SKColors.Black, StrokeWidth = Math.Max(1f, font.Size / 14f), IsAntialias = true };
                    float pad = 6;
                    return new SqrtBox { Inner = inner, Stroke = stroke, Width = inner.Width + pad + 4, Height = inner.Height + 4, Baseline = inner.Baseline + 2 };
                }
            case "mfenced":
                {
                    var open = el.Attribute("open")?.Value ?? "(";
                    var close = el.Attribute("close")?.Value ?? ")";
                    var inner = LayoutRow(el.Elements().ToArray(), font, paint, 1f);
                    var openBox = MakeText(open, font, paint);
                    var closeBox = MakeText(close, font, paint);
                    return Combine(new[] { openBox, inner, closeBox }, 1f);
                }
            case "mi":
            case "mn":
            case "mo":
            case "mtext":
                return MakeText(el.Value, font, paint);
            case "mtable":
            case "mtr":
            case "mtd":
                return LayoutRow(el.Elements().ToArray(), font, paint, 4f);
            default:
                if (el.HasElements) return LayoutRow(el.Elements().ToArray(), font, paint, 1f);
                return MakeText(el.Value, font, paint);
        }
    }

    private Box LayoutRow(XElement[] children, SKFont font, SKPaint paint, float spacing)
    {
        if (children.Length == 0) return MakeText(string.Empty, font, paint);
        var boxes = children.Select(c => LayoutNode(c, font, paint)).ToArray();
        return Combine(boxes, spacing);
    }

    private static Box Combine(Box[] boxes, float spacing)
    {
        if (boxes.Length == 1) return boxes[0];
        float baseline = boxes.Max(b => b.Baseline);
        float belowMax = boxes.Max(b => b.Height - b.Baseline);
        float h = baseline + belowMax;
        float w = boxes.Sum(b => b.Width) + spacing * (boxes.Length - 1);
        return new RowBox { Children = boxes, Spacing = spacing, Width = w, Height = h, Baseline = baseline };
    }

    private static TextBox MakeText(string text, SKFont font, SKPaint paint)
    {
        text ??= string.Empty;
        float w = string.IsNullOrEmpty(text) ? 0 : font.MeasureText(text);
        float h = font.Size * 1.2f;
        var fontMetrics = font.Metrics;
        float baseline = -fontMetrics.Ascent;
        return new TextBox { Text = text, Font = font, Paint = paint, Width = w, Height = h, Baseline = baseline };
    }

    private static string InnerText(XElement el) => string.Concat(el.DescendantNodes().OfType<XText>().Select(t => t.Value));
}
