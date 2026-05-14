using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using SnowyRiver.Documents.Converter.Abstractions;

namespace SnowyRiver.Documents.Converter.Engines;

/// <summary>
/// 系统字体扫描与按需注册到 QuestPDF。
/// 进程内只扫描一次，重复调用幂等。
/// </summary>
internal static class FontRegistry
{
    private static readonly object _lock = new();
    private static bool _systemScanned;
    private static readonly HashSet<string> _registered = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>常见 CJK / Symbol 回退家族（按优先级），用于补全 <see cref="ConversionOptions.FallbackFontFamilies"/>。</summary>
    internal static IReadOnlyList<string> CjkFallbackCandidates => CjkCandidates;

    private static readonly string[] CjkCandidates =
    {
        "Microsoft YaHei", "Microsoft YaHei UI",
        "SimSun", "SimHei", "NSimSun", "FangSong", "KaiTi",
        "PingFang SC", "PingFang TC",
        "Noto Sans CJK SC", "Noto Sans CJK TC", "Noto Sans CJK JP", "Noto Sans CJK KR",
        "Source Han Sans SC", "Source Han Sans TC",
        "WenQuanYi Micro Hei", "WenQuanYi Zen Hei",
    };

    /// <summary>
    /// 确保系统字体（如启用）以及 CJK 回退族已注册到 QuestPDF。
    /// </summary>
    public static void EnsureRegistered(ConversionOptions options, ILogger? log = null, ConversionDiagnostics? diag = null)
    {
        if (!options.EnableSystemFontScan) return;
        lock (_lock)
        {
            if (_systemScanned) return;
            _systemScanned = true;
            try
            {
                foreach (var dir in EnumerateFontDirs())
                {
                    if (!Directory.Exists(dir)) continue;
                    foreach (var path in EnumerateFontFiles(dir))
                    {
                        TryRegisterFile(path, log);
                    }
                }
                EnsureCjkFallbacks(options);
            }
            catch (Exception ex)
            {
                log?.LogWarning(ex, "System font scan failed.");
                diag?.Warn("FONT_SCAN_FAIL", $"系统字体扫描失败：{ex.Message}");
            }
        }
    }

    private static IEnumerable<string> EnumerateFontDirs()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var win = Environment.GetEnvironmentVariable("WINDIR");
            if (!string.IsNullOrEmpty(win)) yield return Path.Combine(win, "Fonts");
            var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (!string.IsNullOrEmpty(local)) yield return Path.Combine(local, "Microsoft", "Windows", "Fonts");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            yield return "/System/Library/Fonts";
            yield return "/Library/Fonts";
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (!string.IsNullOrEmpty(home)) yield return Path.Combine(home, "Library", "Fonts");
        }
        else
        {
            yield return "/usr/share/fonts";
            yield return "/usr/local/share/fonts";
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (!string.IsNullOrEmpty(home)) yield return Path.Combine(home, ".fonts");
            if (!string.IsNullOrEmpty(home)) yield return Path.Combine(home, ".local", "share", "fonts");
        }
    }

    private static IEnumerable<string> EnumerateFontFiles(string dir)
    {
        IEnumerable<string> Safe(string pattern)
        {
            try { return Directory.EnumerateFiles(dir, pattern, SearchOption.AllDirectories); }
            catch { return Array.Empty<string>(); }
        }
        return Safe("*.ttf").Concat(Safe("*.otf")).Concat(Safe("*.ttc"));
    }

    private static void TryRegisterFile(string path, ILogger? log)
    {
        if (!_registered.Add(path)) return;
        try
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            QuestPDF.Drawing.FontManager.RegisterFont(fs);
        }
        catch (Exception ex)
        {
            log?.LogTrace(ex, "Skip font {Path}.", path);
            // 个别字体失败不影响整体流程。
        }
    }

    private static void EnsureCjkFallbacks(ConversionOptions options)
    {
        // 把当前操作系统较常见的 CJK 候选追加进回退链，去重后保持原优先级。
        var existing = new HashSet<string>(options.FallbackFontFamilies, StringComparer.OrdinalIgnoreCase);
        foreach (var fam in CjkCandidates)
        {
            if (existing.Add(fam))
                options.FallbackFontFamilies.Add(fam);
        }
    }

    /// <summary>仅供测试：重置内部缓存，下一次调用会重新扫描。</summary>
    internal static void ResetForTests()
    {
        lock (_lock)
        {
            _systemScanned = false;
            _registered.Clear();
        }
    }
}
