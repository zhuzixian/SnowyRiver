using System.Text;
using System.Xml.Linq;
using DocumentFormat.OpenXml;

namespace SnowyRiver.Documents.Converter.Engines;

/// <summary>
/// Office Math Markup Language（OMML，命名空间 http://schemas.openxmlformats.org/officeDocument/2006/math，前缀 m:）
/// 简易线性化器：将 m:oMath / m:oMathPara 转为可读纯文本（如 "x^2 + sqrt(y)"）。
/// 不追求完整数学排版，仅保证文字管线（PDF/XPS）能输出可读公式占位。
/// </summary>
internal static class OmmlReader
{
    private static readonly XNamespace M = "http://schemas.openxmlformats.org/officeDocument/2006/math";

    /// <summary>
    /// 把 OpenXmlElement（应为 m:oMath 或 m:oMathPara）线性化为纯文本。
    /// </summary>
    public static string Linearize(OpenXmlElement element)
    {
        if (element == null) return string.Empty;
        try
        {
            var x = XElement.Parse(element.OuterXml);
            var sb = new StringBuilder();
            Render(x, sb);
            return sb.ToString().Trim();
        }
        catch
        {
            return element.InnerText ?? string.Empty;
        }
    }

    private static void Render(XElement el, StringBuilder sb)
    {
        var name = el.Name.LocalName;
        switch (name)
        {
            case "oMathPara":
            case "oMath":
                foreach (var c in el.Elements()) Render(c, sb);
                break;
            case "r": // run: 文本与符号
                foreach (var t in el.Elements(M + "t")) sb.Append(t.Value);
                break;
            case "t":
                sb.Append(el.Value);
                break;
            case "f": // 分式: f/num/den => (num)/(den)
                {
                    var num = el.Element(M + "num");
                    var den = el.Element(M + "den");
                    sb.Append('(');
                    if (num != null) foreach (var c in num.Elements()) Render(c, sb);
                    sb.Append(")/(");
                    if (den != null) foreach (var c in den.Elements()) Render(c, sb);
                    sb.Append(')');
                    break;
                }
            case "sup": // 上标：sSup/e + sup
            case "sSup":
                {
                    var e = el.Element(M + "e");
                    var sup = el.Element(M + "sup");
                    if (e != null) foreach (var c in e.Elements()) Render(c, sb);
                    sb.Append('^');
                    sb.Append('(');
                    if (sup != null) foreach (var c in sup.Elements()) Render(c, sb);
                    sb.Append(')');
                    break;
                }
            case "sSub":
                {
                    var e = el.Element(M + "e");
                    var sub = el.Element(M + "sub");
                    if (e != null) foreach (var c in e.Elements()) Render(c, sb);
                    sb.Append('_');
                    sb.Append('(');
                    if (sub != null) foreach (var c in sub.Elements()) Render(c, sb);
                    sb.Append(')');
                    break;
                }
            case "sSubSup":
                {
                    var e = el.Element(M + "e");
                    var sub = el.Element(M + "sub");
                    var sup = el.Element(M + "sup");
                    if (e != null) foreach (var c in e.Elements()) Render(c, sb);
                    sb.Append('_');
                    sb.Append('(');
                    if (sub != null) foreach (var c in sub.Elements()) Render(c, sb);
                    sb.Append(")^(");
                    if (sup != null) foreach (var c in sup.Elements()) Render(c, sb);
                    sb.Append(')');
                    break;
                }
            case "rad": // 根式: deg + e => sqrt(e) 或 root(deg, e)
                {
                    var deg = el.Element(M + "deg");
                    var e = el.Element(M + "e");
                    var degText = new StringBuilder();
                    if (deg != null) foreach (var c in deg.Elements()) Render(c, degText);
                    var degStr = degText.ToString().Trim();
                    if (string.IsNullOrEmpty(degStr))
                    {
                        sb.Append("sqrt(");
                        if (e != null) foreach (var c in e.Elements()) Render(c, sb);
                        sb.Append(')');
                    }
                    else
                    {
                        sb.Append("root(");
                        sb.Append(degStr);
                        sb.Append(", ");
                        if (e != null) foreach (var c in e.Elements()) Render(c, sb);
                        sb.Append(')');
                    }
                    break;
                }
            case "nary": // 大型运算符: ∑/∏/∫
                {
                    var chr = el.Element(M + "naryPr")?.Element(M + "chr")?.Attribute(M + "val")?.Value ?? "∑";
                    var sub = el.Element(M + "sub");
                    var sup = el.Element(M + "sup");
                    var e = el.Element(M + "e");
                    sb.Append(chr);
                    if (sub != null)
                    {
                        sb.Append("_(");
                        foreach (var c in sub.Elements()) Render(c, sb);
                        sb.Append(')');
                    }
                    if (sup != null)
                    {
                        sb.Append("^(");
                        foreach (var c in sup.Elements()) Render(c, sb);
                        sb.Append(')');
                    }
                    sb.Append(' ');
                    if (e != null) foreach (var c in e.Elements()) Render(c, sb);
                    break;
                }
            case "d": // 定界符
                {
                    var pr = el.Element(M + "dPr");
                    var beg = pr?.Element(M + "begChr")?.Attribute(M + "val")?.Value ?? "(";
                    var end = pr?.Element(M + "endChr")?.Attribute(M + "val")?.Value ?? ")";
                    sb.Append(beg);
                    foreach (var e in el.Elements(M + "e"))
                    {
                        foreach (var c in e.Elements()) Render(c, sb);
                    }
                    sb.Append(end);
                    break;
                }
            case "func": // 函数: fName + e
                {
                    var fName = el.Element(M + "fName");
                    var e = el.Element(M + "e");
                    if (fName != null) foreach (var c in fName.Elements()) Render(c, sb);
                    sb.Append('(');
                    if (e != null) foreach (var c in e.Elements()) Render(c, sb);
                    sb.Append(')');
                    break;
                }
            case "limLow":
            case "limUpp":
                {
                    var e = el.Element(M + "e");
                    var lim = el.Element(M + "lim");
                    if (e != null) foreach (var c in e.Elements()) Render(c, sb);
                    sb.Append(name == "limLow" ? "_(" : "^(");
                    if (lim != null) foreach (var c in lim.Elements()) Render(c, sb);
                    sb.Append(')');
                    break;
                }
            case "m": // 矩阵
                {
                    sb.Append('[');
                    foreach (var mr in el.Elements(M + "mr"))
                    {
                        var first = true;
                        foreach (var e in mr.Elements(M + "e"))
                        {
                            if (!first) sb.Append(", ");
                            foreach (var c in e.Elements()) Render(c, sb);
                            first = false;
                        }
                        sb.Append("; ");
                    }
                    if (sb.Length >= 2 && sb[sb.Length - 2] == ';') sb.Length -= 2;
                    sb.Append(']');
                    break;
                }
            case "bar":
            case "acc":
                {
                    var e = el.Element(M + "e");
                    if (e != null) foreach (var c in e.Elements()) Render(c, sb);
                    break;
                }
            default:
                // 未识别：递归子节点
                foreach (var c in el.Elements()) Render(c, sb);
                break;
        }
    }

    // ============== ToMathML ==============

    private static readonly XNamespace MathMlNs = "http://www.w3.org/1998/Math/MathML";

    /// <summary>
    /// 把 OpenXmlElement（应为 m:oMath 或 m:oMathPara）转换为 MathML 字符串。
    /// 输出符合 MathML 3 的核心子集：math/mrow/mi/mn/mo/mfrac/msup/msub/msubsup/msqrt/mroot/munderover/mfenced/mtable/mtr/mtd。
    /// </summary>
    public static string ToMathML(OpenXmlElement element)
    {
        if (element == null) return string.Empty;
        try
        {
            var x = XElement.Parse(element.OuterXml);
            var math = new XElement(MathMlNs + "math",
                new XAttribute("xmlns", MathMlNs.NamespaceName),
                new XAttribute("display", "block"));
            foreach (var c in EnumerateRoots(x))
            {
                foreach (var node in MapMath(c)) math.Add(node);
            }
            return math.ToString(SaveOptions.DisableFormatting);
        }
        catch
        {
            return string.Empty;
        }
    }

    private static IEnumerable<XElement> EnumerateRoots(XElement el)
    {
        var name = el.Name.LocalName;
        if (name == "oMathPara")
        {
            foreach (var m in el.Elements(M + "oMath")) yield return m;
        }
        else
        {
            yield return el;
        }
    }

    private static IEnumerable<XElement> MapMath(XElement el)
    {
        var name = el.Name.LocalName;
        switch (name)
        {
            case "oMath":
                foreach (var c in el.Elements())
                    foreach (var n in MapMath(c)) yield return n;
                yield break;
            case "r":
                foreach (var t in el.Elements(M + "t"))
                    foreach (var n in EmitToken(t.Value)) yield return n;
                yield break;
            case "t":
                foreach (var n in EmitToken(el.Value)) yield return n;
                yield break;
            case "f":
                {
                    var num = new XElement(MathMlNs + "mrow");
                    var den = new XElement(MathMlNs + "mrow");
                    var ne = el.Element(M + "num");
                    var de = el.Element(M + "den");
                    if (ne != null) foreach (var c in ne.Elements()) foreach (var n in MapMath(c)) num.Add(n);
                    if (de != null) foreach (var c in de.Elements()) foreach (var n in MapMath(c)) den.Add(n);
                    yield return new XElement(MathMlNs + "mfrac", num, den);
                    yield break;
                }
            case "sup":
            case "sSup":
                {
                    var baseEl = new XElement(MathMlNs + "mrow");
                    var supEl = new XElement(MathMlNs + "mrow");
                    var be = el.Element(M + "e"); var se = el.Element(M + "sup");
                    if (be != null) foreach (var c in be.Elements()) foreach (var n in MapMath(c)) baseEl.Add(n);
                    if (se != null) foreach (var c in se.Elements()) foreach (var n in MapMath(c)) supEl.Add(n);
                    yield return new XElement(MathMlNs + "msup", baseEl, supEl);
                    yield break;
                }
            case "sSub":
                {
                    var baseEl = new XElement(MathMlNs + "mrow");
                    var subEl = new XElement(MathMlNs + "mrow");
                    var be = el.Element(M + "e"); var se = el.Element(M + "sub");
                    if (be != null) foreach (var c in be.Elements()) foreach (var n in MapMath(c)) baseEl.Add(n);
                    if (se != null) foreach (var c in se.Elements()) foreach (var n in MapMath(c)) subEl.Add(n);
                    yield return new XElement(MathMlNs + "msub", baseEl, subEl);
                    yield break;
                }
            case "sSubSup":
                {
                    var baseEl = new XElement(MathMlNs + "mrow");
                    var subEl = new XElement(MathMlNs + "mrow");
                    var supEl = new XElement(MathMlNs + "mrow");
                    var be = el.Element(M + "e"); var sub = el.Element(M + "sub"); var sup = el.Element(M + "sup");
                    if (be != null) foreach (var c in be.Elements()) foreach (var n in MapMath(c)) baseEl.Add(n);
                    if (sub != null) foreach (var c in sub.Elements()) foreach (var n in MapMath(c)) subEl.Add(n);
                    if (sup != null) foreach (var c in sup.Elements()) foreach (var n in MapMath(c)) supEl.Add(n);
                    yield return new XElement(MathMlNs + "msubsup", baseEl, subEl, supEl);
                    yield break;
                }
            case "rad":
                {
                    var deg = el.Element(M + "deg");
                    var be = el.Element(M + "e");
                    var degRow = new XElement(MathMlNs + "mrow");
                    var baseRow = new XElement(MathMlNs + "mrow");
                    if (deg != null) foreach (var c in deg.Elements()) foreach (var n in MapMath(c)) degRow.Add(n);
                    if (be != null) foreach (var c in be.Elements()) foreach (var n in MapMath(c)) baseRow.Add(n);
                    if (degRow.HasElements)
                        yield return new XElement(MathMlNs + "mroot", baseRow, degRow);
                    else
                        yield return new XElement(MathMlNs + "msqrt", baseRow);
                    yield break;
                }
            case "nary":
                {
                    var chr = el.Element(M + "naryPr")?.Element(M + "chr")?.Attribute(M + "val")?.Value ?? "∑";
                    var sub = el.Element(M + "sub"); var sup = el.Element(M + "sup"); var be = el.Element(M + "e");
                    var subRow = new XElement(MathMlNs + "mrow");
                    var supRow = new XElement(MathMlNs + "mrow");
                    if (sub != null) foreach (var c in sub.Elements()) foreach (var n in MapMath(c)) subRow.Add(n);
                    if (sup != null) foreach (var c in sup.Elements()) foreach (var n in MapMath(c)) supRow.Add(n);
                    var op = new XElement(MathMlNs + "mo", chr);
                    yield return new XElement(MathMlNs + "munderover", op, subRow, supRow);
                    if (be != null) foreach (var c in be.Elements()) foreach (var n in MapMath(c)) yield return n;
                    yield break;
                }
            case "d":
                {
                    var pr = el.Element(M + "dPr");
                    var beg = pr?.Element(M + "begChr")?.Attribute(M + "val")?.Value ?? "(";
                    var end = pr?.Element(M + "endChr")?.Attribute(M + "val")?.Value ?? ")";
                    var inner = new XElement(MathMlNs + "mrow");
                    foreach (var e in el.Elements(M + "e"))
                        foreach (var c in e.Elements())
                            foreach (var n in MapMath(c)) inner.Add(n);
                    yield return new XElement(MathMlNs + "mfenced",
                        new XAttribute("open", beg),
                        new XAttribute("close", end),
                        inner);
                    yield break;
                }
            case "func":
                {
                    var fName = el.Element(M + "fName"); var be = el.Element(M + "e");
                    var nameRow = new XElement(MathMlNs + "mrow");
                    if (fName != null) foreach (var c in fName.Elements()) foreach (var n in MapMath(c)) nameRow.Add(n);
                    foreach (var n in nameRow.Elements()) yield return n;
                    var arg = new XElement(MathMlNs + "mrow");
                    if (be != null) foreach (var c in be.Elements()) foreach (var n in MapMath(c)) arg.Add(n);
                    yield return new XElement(MathMlNs + "mfenced", new XAttribute("open", "("), new XAttribute("close", ")"), arg);
                    yield break;
                }
            case "limLow":
            case "limUpp":
                {
                    var be = el.Element(M + "e"); var lim = el.Element(M + "lim");
                    var baseRow = new XElement(MathMlNs + "mrow");
                    var limRow = new XElement(MathMlNs + "mrow");
                    if (be != null) foreach (var c in be.Elements()) foreach (var n in MapMath(c)) baseRow.Add(n);
                    if (lim != null) foreach (var c in lim.Elements()) foreach (var n in MapMath(c)) limRow.Add(n);
                    yield return new XElement(MathMlNs + (name == "limLow" ? "munder" : "mover"), baseRow, limRow);
                    yield break;
                }
            case "m":
                {
                    var table = new XElement(MathMlNs + "mtable");
                    foreach (var mr in el.Elements(M + "mr"))
                    {
                        var tr = new XElement(MathMlNs + "mtr");
                        foreach (var e in mr.Elements(M + "e"))
                        {
                            var td = new XElement(MathMlNs + "mtd");
                            foreach (var c in e.Elements()) foreach (var n in MapMath(c)) td.Add(n);
                            tr.Add(td);
                        }
                        table.Add(tr);
                    }
                    yield return new XElement(MathMlNs + "mfenced",
                        new XAttribute("open", "["),
                        new XAttribute("close", "]"),
                        table);
                    yield break;
                }
            case "bar":
            case "acc":
                {
                    var be = el.Element(M + "e");
                    var baseRow = new XElement(MathMlNs + "mrow");
                    if (be != null) foreach (var c in be.Elements()) foreach (var n in MapMath(c)) baseRow.Add(n);
                    yield return baseRow;
                    yield break;
                }
            default:
                foreach (var c in el.Elements())
                    foreach (var n in MapMath(c)) yield return n;
                yield break;
        }
    }

    private static IEnumerable<XElement> EmitToken(string text)
    {
        if (string.IsNullOrEmpty(text)) yield break;
        var sb = new StringBuilder();
        TokenKind kind = TokenKind.None;
        foreach (var ch in text)
        {
            var k = Classify(ch);
            if (k == TokenKind.Space)
            {
                if (sb.Length > 0) { yield return EmitOne(kind, sb.ToString()); sb.Clear(); kind = TokenKind.None; }
                continue;
            }
            if (kind != TokenKind.None && k != kind)
            {
                yield return EmitOne(kind, sb.ToString());
                sb.Clear();
            }
            sb.Append(ch);
            kind = k;
        }
        if (sb.Length > 0) yield return EmitOne(kind, sb.ToString());
    }

    private enum TokenKind { None, Number, Ident, Op, Space }

    private static TokenKind Classify(char ch)
    {
        if (char.IsWhiteSpace(ch)) return TokenKind.Space;
        if (char.IsDigit(ch) || ch == '.') return TokenKind.Number;
        if (char.IsLetter(ch)) return TokenKind.Ident;
        return TokenKind.Op;
    }

    private static XElement EmitOne(TokenKind kind, string text) => kind switch
    {
        TokenKind.Number => new XElement(MathMlNs + "mn", text),
        TokenKind.Ident => new XElement(MathMlNs + "mi", text),
        _ => new XElement(MathMlNs + "mo", text),
    };
}
