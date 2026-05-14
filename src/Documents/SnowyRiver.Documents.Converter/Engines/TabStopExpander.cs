using System.Collections.Generic;
using SnowyRiver.Documents.Converter.Model;

namespace SnowyRiver.Documents.Converter.Engines;

/// <summary>
/// 把段落内含 \t 的 Run 序列按 TabStop 展开为一组 (前置填充, IrRun) 片段。
/// 渲染器据此将填充字符串与原 Run 拼接，近似实现 Word TabStop 对齐与 Leader。
/// </summary>
internal static class TabStopExpander
{
    public readonly struct Segment
    {
        public Segment(string padding, IrRun run) { Padding = padding; Run = run; }
        /// <summary>插入到 Run 前的填充串（Leader 字符或空格）。</summary>
        public string Padding { get; }
        public IrRun Run { get; }
    }

    /// <summary>近似平均字符宽度（pt），按字号比例缩放。</summary>
    private const double BaseCharWidthPt = 5.5; // 11pt 半角字符宽

    public static IEnumerable<Segment> Expand(IrParagraph paragraph, double defaultFontSizePt = 11.0)
    {
        if (paragraph.TabStops.Count == 0)
        {
            foreach (var run in paragraph.Runs) yield return new Segment(string.Empty, run);
            yield break;
        }

        // 累计当前已经渲染的近似列偏移（pt），用于决定下一个 \t 跳到哪个 TabStop。
        double cursorPt = 0;
        for (int i = 0; i < paragraph.Runs.Count; i++)
        {
            var run = paragraph.Runs[i];
            string text = run.Text ?? string.Empty;
            double size = run.FontSize ?? defaultFontSizePt;
            double charWidth = BaseCharWidthPt * (size / 11.0);

            // 把文本以 \t 切片，每段前可能要插入 Tab 填充
            var parts = text.Split('\t');
            for (int p = 0; p < parts.Length; p++)
            {
                string padding = string.Empty;
                if (p > 0)
                {
                    // 命中：找到 cursor 之后的下一个 TabStop
                    var stop = NextStop(paragraph.TabStops, cursorPt);
                    if (stop != null)
                    {
                        double gap = stop.PositionPt - cursorPt;
                        if (gap < charWidth) gap = charWidth; // 至少一字符
                        int n = System.Math.Max(1, (int)(gap / charWidth));
                        char fill = stop.Leader ?? ' ';
                        padding = new string(fill, n);
                        cursorPt = stop.PositionPt;
                    }
                    else
                    {
                        padding = "    ";
                        cursorPt += charWidth * 4;
                    }
                }

                var sliced = CloneRun(run, parts[p]);
                yield return new Segment(padding, sliced);
                cursorPt += parts[p].Length * charWidth;
            }
        }
    }

    private static IrTabStop? NextStop(IList<IrTabStop> stops, double cursor)
    {
        IrTabStop? best = null;
        foreach (var s in stops)
        {
            if (s.PositionPt > cursor + 0.01)
            {
                if (best == null || s.PositionPt < best.PositionPt) best = s;
            }
        }
        return best;
    }

    private static IrRun CloneRun(IrRun src, string text) => new IrRun
    {
        Text = text,
        FontFamily = src.FontFamily,
        FontSize = src.FontSize,
        Bold = src.Bold,
        Italic = src.Italic,
        Underline = src.Underline,
        ColorHex = src.ColorHex,
        HighlightHex = src.HighlightHex,
        HyperlinkUrl = src.HyperlinkUrl,
        AnchorRef = src.AnchorRef,
        PageRefAnchor = src.PageRefAnchor,
        IsPageNumberField = src.IsPageNumberField,
        IsPageCountField = src.IsPageCountField,
        FieldKind = src.FieldKind,
        CommentRef = src.CommentRef,
        IsInsertion = src.IsInsertion,
        IsDeletion = src.IsDeletion,
    };
}
