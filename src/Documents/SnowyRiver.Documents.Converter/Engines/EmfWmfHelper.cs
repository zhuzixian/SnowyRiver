using System;
using System.IO;
using System.Xml.Linq;
using SkiaSharp;
using SnowyRiver.Documents.Converter.Abstractions;

namespace SnowyRiver.Documents.Converter.Engines;

/// <summary>
/// EMF/WMF 矢量图栅格化工具：尝试用 GDI+（System.Drawing）把 EMF/WMF 渲染为 PNG，
/// 失败时返回一个简单占位 PNG（带文字提示），保证渲染管线不中断。
/// 仅在 Windows 桌面环境（UseWPF=true → 包含 WindowsDesktop SDK）下可用。
/// </summary>
internal static class EmfWmfHelper
{
    /// <summary>
    /// 将 EMF/WMF 字节流转换为 PNG 字节流。
    /// </summary>
    /// <param name="data">EMF/WMF 原始字节。</param>
    /// <param name="format">提示格式（"emf"/"wmf"），仅用于占位文字。</param>
    /// <param name="targetDpi">目标 DPI（默认 96）。</param>
    /// <param name="diag">可选诊断；当回退到占位时记录警告。</param>
    /// <returns>PNG 字节；若 EMF/WMF 解析失败，返回占位 PNG。</returns>
    public static byte[] RasterizeToPng(byte[] data, string? format, int targetDpi = 96, ConversionDiagnostics? diag = null)
    {
        if (data == null || data.Length == 0)
        {
            diag?.Warn("VECTOR_EMPTY", $"[{format ?? "vector"}] 数据为空，已用占位图替代。");
            return CreatePlaceholderPng(format);
        }

        // EMF+ 检测：若包含 EMF+ comment 记录，GDI+ 可绘制但效果可能不完整。
        bool isEmfPlus = (string.Equals(format, "emf", StringComparison.OrdinalIgnoreCase) || format is null) && ContainsEmfPlusSignature(data);

#if SR_WINDOWS
        try
        {
#pragma warning disable CA1416 // Validate platform compatibility (Windows-only)
            using var ms = new MemoryStream(data, writable: false);
            using var img = System.Drawing.Image.FromStream(ms);
            int w = Math.Max(1, img.Width);
            int h = Math.Max(1, img.Height);
            // 部分 EMF 的 PixelWidth 可能为 0，按 HorizontalResolution 校准
            if (img.HorizontalResolution > 0 && img.VerticalResolution > 0)
            {
                w = Math.Max(1, (int)Math.Round(img.Width * targetDpi / img.HorizontalResolution));
                h = Math.Max(1, (int)Math.Round(img.Height * targetDpi / img.VerticalResolution));
            }
            using var bmp = new System.Drawing.Bitmap(w, h);
            bmp.SetResolution(targetDpi, targetDpi);
            using (var g = System.Drawing.Graphics.FromImage(bmp))
            {
                g.Clear(System.Drawing.Color.White);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                g.DrawImage(img, new System.Drawing.Rectangle(0, 0, w, h));
            }
            using var outMs = new MemoryStream();
            bmp.Save(outMs, System.Drawing.Imaging.ImageFormat.Png);
            if (isEmfPlus)
            {
                diag?.Info("EMFPLUS_DETECTED", "检测到 EMF+ 记录，已通过 GDI+ 栅格化，复杂效果可能略有差异。");
            }
            return outMs.ToArray();
#pragma warning restore CA1416
        }
        catch (Exception ex)
        {
            diag?.Warn("VECTOR_RASTERIZE_FAIL", $"[{format ?? "vector"}] 栅格化失败：{ex.Message}，已用占位图替代。");
            return CreatePlaceholderPng(format);
        }
#else
        // 非 Windows TFM 下没有 GDI+，EMF/WMF 走占位以保证管线不中断。
        diag?.Warn("VECTOR_PLATFORM_UNSUPPORTED",
            $"[{format ?? "vector"}] 当前 TFM 不支持 GDI+ EMF/WMF 解码（仅 net10.0-windows 可用），已用占位图替代。" +
            (isEmfPlus ? " 检测到 EMF+ 记录。" : string.Empty));
        return CreatePlaceholderPng(format);
#endif
    }

    /// <summary>
    /// 将 SVG 字节流转换为 PNG。优先使用 Svg.Skia 真正栅格化为 PNG；当 Svg.Skia 失败或得不到有效 picture 时，
    /// 退回到根据 svg 的 width/height/viewBox 生成尺寸感知的占位图，并通过诊断记录降级原因。
    /// </summary>
    public static byte[] RasterizeSvgToPng(byte[] data, int targetDpi = 96, ConversionDiagnostics? diag = null)
    {
        if (data == null || data.Length == 0)
        {
            diag?.Warn("SVG_EMPTY", "SVG 数据为空，已用占位图替代。");
            return CreatePlaceholderPng("svg");
        }

        // 解析显式尺寸 / viewBox 作为降级或缩放参考。
        int w = 320, h = 180;
        bool hasExplicitSize = false;
        try
        {
            using var sniff = new MemoryStream(data, writable: false);
            var doc = XDocument.Load(sniff);
            var root = doc.Root;
            if (root != null && string.Equals(root.Name.LocalName, "svg", StringComparison.OrdinalIgnoreCase))
            {
                int? pw = ParseLength(root.Attribute("width")?.Value);
                int? ph = ParseLength(root.Attribute("height")?.Value);
                if (pw.HasValue && ph.HasValue) { w = pw.Value; h = ph.Value; hasExplicitSize = true; }
                else
                {
                    var vb = root.Attribute("viewBox")?.Value;
                    if (!string.IsNullOrEmpty(vb))
                    {
                        var parts = vb!.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length == 4 &&
                            double.TryParse(parts[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var vw) &&
                            double.TryParse(parts[3], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var vh))
                        {
                            w = Math.Max(1, (int)Math.Round(vw));
                            h = Math.Max(1, (int)Math.Round(vh));
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            diag?.Warn("SVG_PARSE_FAIL", $"SVG 解析失败：{ex.Message}，已用占位图替代。");
            return CreatePlaceholderPng("svg", w, h);
        }

        // 真正的 Svg.Skia 栅格化。
        try
        {
            using var svg = new Svg.Skia.SKSvg();
            using var ms = new MemoryStream(data, writable: false);
            var picture = svg.Load(ms);
            if (picture == null)
            {
                diag?.Warn("SVG_LOAD_NULL", "Svg.Skia 未能加载 SVG，已退回占位图。");
                return CreatePlaceholderPng("svg", w, h);
            }

            var bounds = picture.CullRect;
            float pw = bounds.Width  > 0 ? bounds.Width  : w;
            float ph = bounds.Height > 0 ? bounds.Height : h;

            // 若显式提供尺寸，按显式尺寸缩放；否则按 picture 自身尺寸。
            int outW = hasExplicitSize ? Math.Max(1, w) : Math.Max(1, (int)Math.Round(pw));
            int outH = hasExplicitSize ? Math.Max(1, h) : Math.Max(1, (int)Math.Round(ph));
            // DPI 缩放（默认 96 时 scale=1）。
            float scale = targetDpi / 96f;
            outW = Math.Min(8192, Math.Max(1, (int)Math.Round(outW * scale)));
            outH = Math.Min(8192, Math.Max(1, (int)Math.Round(outH * scale)));

            using var surface = SKSurface.Create(new SKImageInfo(outW, outH, SKColorType.Bgra8888, SKAlphaType.Premul));
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.Transparent);
            float sx = outW / pw;
            float sy = outH / ph;
            canvas.Save();
            canvas.Scale(sx, sy);
            if (bounds.Left != 0 || bounds.Top != 0)
            {
                canvas.Translate(-bounds.Left, -bounds.Top);
            }
            canvas.DrawPicture(picture);
            canvas.Restore();
            using var snap = surface.Snapshot();
            using var enc = snap.Encode(SKEncodedImageFormat.Png, 90);
            diag?.Info("SVG_RASTERIZED", $"SVG 已通过 Svg.Skia 栅格化为 {outW}x{outH} PNG。");
            return enc.ToArray();
        }
        catch (Exception ex)
        {
            diag?.Warn("SVG_RASTERIZE_FAIL", $"Svg.Skia 栅格化失败：{ex.Message}，已用占位图替代。");
            return CreatePlaceholderPng("svg", w, h);
        }
    }

    private static int? ParseLength(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var s = raw!.Trim();
        int end = 0;
        while (end < s.Length && (char.IsDigit(s[end]) || s[end] == '.' || s[end] == '-' || s[end] == '+')) end++;
        if (end == 0) return null;
        if (!double.TryParse(s.AsSpan(0, end), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var v)) return null;
        // 忽略单位（px/pt/...）
        return Math.Max(1, (int)Math.Round(v));
    }

    private static bool ContainsEmfPlusSignature(byte[] data)
    {
        // EMF+ 通过 EMR_COMMENT_EMFPLUS 嵌入：在文件中搜索 ASCII "EMF+" 标记是一种常见的轻量启发式检测。
        ReadOnlySpan<byte> sig = stackalloc byte[] { (byte)'E', (byte)'M', (byte)'F', (byte)'+' };
        int max = Math.Min(data.Length - sig.Length, 64 * 1024);
        for (int i = 0; i < max; i++)
        {
            if (data[i] == sig[0] && data[i + 1] == sig[1] && data[i + 2] == sig[2] && data[i + 3] == sig[3])
                return true;
        }
        return false;
    }

    /// <summary>生成一个灰白占位 PNG（标注 "[FORMAT] rendering unavailable"）。</summary>
    private static byte[] CreatePlaceholderPng(string? format) => CreatePlaceholderPng(format, 320, 180);

    private static byte[] CreatePlaceholderPng(string? format, int width, int height)
    {
        int W = Math.Max(40, Math.Min(2048, width));
        int H = Math.Max(40, Math.Min(2048, height));
        using var surface = SKSurface.Create(new SKImageInfo(W, H, SKColorType.Bgra8888, SKAlphaType.Premul));
        var canvas = surface.Canvas;
        canvas.Clear(new SKColor(0xF5, 0xF5, 0xF5));
        using (var border = new SKPaint { Color = new SKColor(0xCC, 0xCC, 0xCC), Style = SKPaintStyle.Stroke, StrokeWidth = 1 })
            canvas.DrawRect(0.5f, 0.5f, W - 1, H - 1, border);
        using var paint = new SKPaint
        {
            Color = new SKColor(0x66, 0x66, 0x66),
            IsAntialias = true,
        };
        using var font1 = new SKFont { Size = 18 };
        using var font2 = new SKFont { Size = 12 };
        canvas.DrawText($"[{(format ?? "vector").ToUpperInvariant()}]", W / 2f, H / 2f - 8, SKTextAlign.Center, font1, paint);
        canvas.DrawText("rendering unavailable", W / 2f, H / 2f + 14, SKTextAlign.Center, font2, paint);
        using var snap = surface.Snapshot();
        using var enc = snap.Encode(SKEncodedImageFormat.Png, 90);
        return enc.ToArray();
    }
}
