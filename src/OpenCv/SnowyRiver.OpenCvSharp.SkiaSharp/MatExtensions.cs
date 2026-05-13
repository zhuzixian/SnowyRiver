using OpenCvSharp;
using SkiaSharp;

namespace SnowyRiver.OpenCvSharp.SkiaSharp;

public static class MatExtensions
{
    public static SKBitmap ToSkBitmap(this Mat mat)
    {
        if (mat.Empty())
            throw new ArgumentException("Mat is empty", nameof(mat));

        var colorType = mat.Channels() switch
        {
            1 => SKColorType.Gray8,
            3 => SKColorType.Rgb888x,
            4 => SKColorType.Bgra8888,
            _ => throw new NotSupportedException($"Unsupported channel count: {mat.Channels()}")
        };

        var bitmap = new SKBitmap(mat.Width, mat.Height, colorType, SKAlphaType.Unpremul);

        // 转换颜色空间（如果需要）
        using var convertedMat = mat.Channels() == 3
            ? mat.CvtColor(ColorConversionCodes.BGR2RGBA)
            : mat;

        // 复制像素数据
        var bytes = new byte[convertedMat.Total() * convertedMat.ElemSize()];
        System.Runtime.InteropServices.Marshal.Copy(convertedMat.Data, bytes, 0, bytes.Length);
        System.Runtime.InteropServices.Marshal.Copy(bytes, 0, bitmap.GetPixels(), bytes.Length);

        return bitmap;
    }
}
