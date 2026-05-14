using System.IO;
using System.IO.Packaging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Xps.Packaging;
using PDFtoImage;
using SkiaSharp;
using SnowyRiver.Documents.Converter.Abstractions;

namespace SnowyRiver.Documents.Converter.Engines;

/// <summary>
/// 把 PDF 流栅格化为图像并封装到 XPS。Windows-only。
/// </summary>
internal static class XpsRenderer
{
    public static void RenderPdfToXps(Stream pdfSource, Stream xpsTarget, ConversionOptions options)
    {
        // PDFtoImage.Conversion 需要 Stream 可读且支持 Seek。
        var pdfBytes = ReadAllBytes(pdfSource);

        // XpsDocument 必须基于 Package；使用临时文件作为目标，再拷贝到 xpsTarget。
        var tempXpsPath = Path.Combine(Path.GetTempPath(), $"snowyriver_xps_{Guid.NewGuid():N}.xps");
        try
        {
            using (var package = Package.Open(tempXpsPath, FileMode.Create, FileAccess.ReadWrite))
            {
                using var xpsDoc = new XpsDocument(package, CompressionOption.Maximum);
                var fixedDoc = new FixedDocument();

                int dpi = Math.Max(72, options.XpsRasterDpi);

                // 逐页栅格化以避免一次性占用大量内存
                int pageIndex = 0;
                using (var pdfStream = new MemoryStream(pdfBytes, writable: false))
                {
                    foreach (var bitmap in Conversion.ToImages(pdfStream, options: new PDFtoImage.RenderOptions(Dpi: dpi)))
                    {
                        using (bitmap)
                        {
                            var fp = BuildFixedPage(bitmap, dpi);
                            var pageContent = new PageContent { Child = fp };
                            fixedDoc.Pages.Add(pageContent);
                        }
                        pageIndex++;
                    }
                }

                if (pageIndex == 0)
                {
                    // 至少写一页空白以保持 XPS 合法
                    var emptyPage = new FixedPage { Width = 595, Height = 842 };
                    var pageContent = new PageContent { Child = emptyPage };
                    fixedDoc.Pages.Add(pageContent);
                }

                var writer = XpsDocument.CreateXpsDocumentWriter(xpsDoc);
                writer.Write(fixedDoc);
            }

            // 把临时 XPS 文件流拷贝到目标流
            using var fs = new FileStream(tempXpsPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            fs.CopyTo(xpsTarget);
        }
        finally
        {
            try { if (File.Exists(tempXpsPath)) File.Delete(tempXpsPath); } catch { /* 忽略 */ }
        }
    }

    private static FixedPage BuildFixedPage(SKBitmap bitmap, int dpi)
    {
        // 把像素尺寸换算为 1/96 英寸（WPF 设备无关单位）
        double widthDip = bitmap.Width * 96.0 / dpi;
        double heightDip = bitmap.Height * 96.0 / dpi;

        var bitmapSource = SkBitmapToBitmapSource(bitmap, dpi);

        var image = new Image
        {
            Source = bitmapSource,
            Width = widthDip,
            Height = heightDip,
            Stretch = Stretch.Fill,
        };
        FixedPage.SetLeft(image, 0);
        FixedPage.SetTop(image, 0);

        var fp = new FixedPage
        {
            Width = widthDip,
            Height = heightDip,
            Background = Brushes.White,
        };
        fp.Children.Add(image);

        // 触发布局
        var size = new Size(widthDip, heightDip);
        fp.Measure(size);
        fp.Arrange(new Rect(size));
        fp.UpdateLayout();
        return fp;
    }

    private static BitmapSource SkBitmapToBitmapSource(SKBitmap bitmap, int dpi)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 95);
        using var ms = new MemoryStream();
        data.SaveTo(ms);
        ms.Position = 0;

        var bmp = new BitmapImage();
        bmp.BeginInit();
        bmp.CacheOption = BitmapCacheOption.OnLoad;
        bmp.StreamSource = ms;
        bmp.EndInit();
        bmp.Freeze();
        return bmp;
    }

    private static byte[] ReadAllBytes(Stream s)
    {
        if (s is MemoryStream ms) return ms.ToArray();
        using var mem = new MemoryStream();
        s.CopyTo(mem);
        return mem.ToArray();
    }
}
