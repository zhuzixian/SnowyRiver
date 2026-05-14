using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using SkiaSharp;

namespace SnowyRiver.Documents.Converter.Engines.Charts;

/// <summary>
/// SkiaSharp 字体回退助手：优先使用主字体，否则在 CJK 候选族中按字符匹配回退。
/// 用于图表渲染时正确显示中文/日文/韩文标签。
/// </summary>
internal static class SkiaFontFallback
{
    private static readonly ConcurrentDictionary<string, SKTypeface?> _cache = new(StringComparer.OrdinalIgnoreCase);

    public static SKTypeface ResolvePrimary(string? fontFamily)
    {
        if (!string.IsNullOrEmpty(fontFamily))
        {
            var tf = _cache.GetOrAdd(fontFamily!, n => SKTypeface.FromFamilyName(n));
            if (tf != null) return tf;
        }
        return SKTypeface.Default;
    }

    /// <summary>
    /// 给定一段文本与主字体，返回适合该文本的字体。如果主字体不能渲染其中的某个非 ASCII 字符，
    /// 则在 CJK 候选族中找到第一个能渲染的字体；找不到时回退到 SkiaSharp 内置匹配。
    /// </summary>
    public static SKTypeface ResolveForText(string? fontFamily, string text)
    {
        var primary = ResolvePrimary(fontFamily);
        if (string.IsNullOrEmpty(text)) return primary;

        // 快速路径：纯 ASCII 直接用主字体
        bool needFallback = false;
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] > 0x7F) { needFallback = true; break; }
        }
        if (!needFallback) return primary;

        // 找到第一个非 ASCII 字符，针对它做匹配
        int rune = 0;
        foreach (var r in EnumerateRunes(text))
        {
            if (r > 0x7F) { rune = r; break; }
        }
        if (rune == 0) return primary;

        if (CanRender(primary, rune)) return primary;

        foreach (var fam in FontRegistry.CjkFallbackCandidates)
        {
            var tf = _cache.GetOrAdd(fam, n => SKTypeface.FromFamilyName(n));
            if (tf != null && CanRender(tf, rune)) return tf;
        }

        // 最后兜底：让 Skia 自动匹配
        try
        {
            var fm = SKFontManager.Default;
            var match = fm.MatchCharacter(rune);
            if (match != null) return match;
        }
        catch { }
        return primary;
    }

    private static bool CanRender(SKTypeface tf, int codepoint)
    {
        try { return tf.GetGlyph(codepoint) != 0; }
        catch { return false; }
    }

    private static IEnumerable<int> EnumerateRunes(string s)
    {
        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            if (char.IsHighSurrogate(c) && i + 1 < s.Length && char.IsLowSurrogate(s[i + 1]))
            {
                yield return char.ConvertToUtf32(c, s[i + 1]);
                i++;
            }
            else
            {
                yield return c;
            }
        }
    }
}
