using System.Globalization;
using System.Text;
using SnowyRiver.Documents.Converter.Model;

namespace SnowyRiver.Documents.Converter.Engines;

/// <summary>页码编号格式化（支持 decimal / lower|upperLetter / lower|upperRoman）。</summary>
internal static class PageNumberFormatter
{
    public static string Format(int number, PageNumberFormat format)
    {
        if (number <= 0) number = 1;
        return format switch
        {
            PageNumberFormat.LowerLetter => ToAlpha(number, lower: true),
            PageNumberFormat.UpperLetter => ToAlpha(number, lower: false),
            PageNumberFormat.LowerRoman => ToRoman(number).ToLower(CultureInfo.InvariantCulture),
            PageNumberFormat.UpperRoman => ToRoman(number),
            _ => number.ToString(CultureInfo.InvariantCulture),
        };
    }

    private static string ToAlpha(int n, bool lower)
    {
        // Word 行为：1->a, 26->z, 27->aa, 28->bb（同字符重复，长度=⌈n/26⌉）
        int len = (n - 1) / 26 + 1;
        char ch = (char)((lower ? 'a' : 'A') + (n - 1) % 26);
        return new string(ch, len);
    }

    private static string ToRoman(int n)
    {
        if (n <= 0 || n >= 4000) return n.ToString(CultureInfo.InvariantCulture);
        int[] vals = { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };
        string[] syms = { "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" };
        var sb = new StringBuilder();
        for (int i = 0; i < vals.Length; i++)
        {
            while (n >= vals[i]) { sb.Append(syms[i]); n -= vals[i]; }
        }
        return sb.ToString();
    }
}
