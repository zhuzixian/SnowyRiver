using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using OpenCvSharp;

namespace SnowyRiver.OpenCvSharp.Extensions;
public static class BitmapConverter
{
    public static Mat ToMat(this Bitmap src)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            throw new NotSupportedException("Non-Windows OS are not supported");
        if (src is null)
            throw new ArgumentNullException(nameof(src));

        if (src.PixelFormat == PixelFormat.Format48bppRgb)
        {
            var w = src.Width;
            var h = src.Height;
            var dst = new Mat(h, w, MatType.CV_16UC3);
            ToMat(src, dst);
            return dst;
        }

        return global::OpenCvSharp.Extensions.BitmapConverter.ToMat(src);
    }

#if NET5_0_OR_GREATER
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif
    public static unsafe void ToMat(this Bitmap src, Mat dst)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            throw new NotSupportedException("Non-Windows OS are not supported");
        if (src is null)
            throw new ArgumentNullException(nameof(src));
        if (dst is null)
            throw new ArgumentNullException(nameof(dst));
        if (dst.IsDisposed)
            throw new ArgumentException("The specified dst is disposed.", nameof(dst));
        if (dst.Dims != 2)
            throw new NotSupportedException("Mat dims != 2");
        if (src.Width != dst.Width || src.Height != dst.Height)
            throw new ArgumentException("src.Size != dst.Size");

        if (src.PixelFormat == PixelFormat.Format48bppRgb)
        {
            var w = src.Width;
            var h = src.Height;
            var rect = new Rectangle(0, 0, w, h);
            BitmapData? bd = null;
            try
            {
                bd = src.LockBits(rect, ImageLockMode.ReadOnly, src.PixelFormat);
                Format48bppRgb();
            }
            finally
            {
                if (bd is not null)
                    src.UnlockBits(bd);
            }

            // ReSharper disable once InconsistentNaming
            void Format48bppRgb()
            {
                if (dst.Channels() != 3)
                    throw new ArgumentException("Invalid nChannels");
                if (dst.Depth() != MatType.CV_16U && dst.Depth() != MatType.CV_16S)
                    throw new ArgumentException("Invalid depth of dst Mat");

                var srcStep = bd.Stride;
                var dstStep = dst.Step();
                if (dstStep == srcStep && !dst.IsSubmatrix() && dst.IsContinuous())
                {
                    var dstData = dst.Data;
                    var bytesToCopy = dst.DataEnd.ToInt64() - dstData.ToInt64();
                    Buffer.MemoryCopy(bd.Scan0.ToPointer(), dstData.ToPointer(), bytesToCopy, bytesToCopy);
                }
                else
                {
                    // Copy line bytes from src to dst for each line
                    var sp = (byte*)bd.Scan0;
                    var dp = (byte*)dst.Data;
                    for (var y = 0; y < h; y++)
                    {
                        Buffer.MemoryCopy(sp, dp, dstStep, dstStep);
                        sp += srcStep;
                        dp += dstStep;
                    }
                }
            }
        }
        else
        {
            global::OpenCvSharp.Extensions.BitmapConverter.ToMat(src, dst);
        }
    }

#if NET5_0_OR_GREATER
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif
    public static Bitmap ToBitmap(this Mat src)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            throw new NotSupportedException("Non-Windows OS are not supported");
        if (src is null)
            throw new ArgumentNullException(nameof(src));

        if (src.Type() == MatType.CV_16UC3)
        {
            return ToBitmap(src, PixelFormat.Format48bppRgb);
        }

        return global::OpenCvSharp.Extensions.BitmapConverter.ToBitmap(src);
    }

    /// <summary>
    /// Converts Mat to System.Drawing.Bitmap
    /// </summary>
    /// <param name="src">Mat</param>
    /// <param name="pf">Pixel Depth</param>
    /// <returns></returns>
#if NET5_0_OR_GREATER
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif
    public static Bitmap ToBitmap(this Mat src, PixelFormat pf)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            throw new NotSupportedException("Non-Windows OS are not supported");
        if (src is null)
            throw new ArgumentNullException(nameof(src));
        src.ThrowIfDisposed();

        var bitmap = new Bitmap(src.Width, src.Height, pf);
        ToBitmap(src, bitmap);
        return bitmap;
    }

    /// <summary>
    /// Converts Mat to System.Drawing.Bitmap
    /// </summary>
    /// <param name="src">Mat</param>
    /// <param name="dst">Mat</param>
    /// <remarks>Author: shimat, Gummo (ROI support)</remarks>
#if NET5_0_OR_GREATER
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif
    public static unsafe void ToBitmap(this Mat src, Bitmap dst)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            throw new NotSupportedException("Non-Windows OS are not supported");
        if (src is null)
            throw new ArgumentNullException(nameof(src));
        if (dst is null)
            throw new ArgumentNullException(nameof(dst));
        if (src.IsDisposed)
            throw new ArgumentException("The image is disposed.", nameof(src));
        //if (src.IsSubmatrix())
        //    throw new ArgumentException("Submatrix is not supported");
        if (src.Width != dst.Width || src.Height != dst.Height)
            throw new ArgumentException("");

        if (src.Type() == MatType.CV_16UC3)
        {
            var pf = dst.PixelFormat;

            var w = src.Width;
            var h = src.Height;
            var rect = new Rectangle(0, 0, w, h);
            BitmapData? bd = null;

            var submat = src.IsSubmatrix();
            var continuous = src.IsContinuous();

            try
            {
                bd = dst.LockBits(rect, ImageLockMode.WriteOnly, pf);

                var srcData = src.Data;
                var pSrc = (byte*)(srcData.ToPointer());
                var pDst = (byte*)(bd.Scan0.ToPointer());
                var ch = src.Channels();
                var srcStep = (int)src.Step();
                var dstStep = new Mat(src.Size(), MatType.CV_16UC3).Step();
                if (srcStep == dstStep && !submat && continuous)
                {
                    var bytesToCopy = src.DataEnd.ToInt64() - src.Data.ToInt64();
                    Buffer.MemoryCopy(pSrc, pDst, bytesToCopy, bytesToCopy);
                }
                else
                {
                    for (var y = 0; y < h; y++)
                    {
                        long offsetSrc = (y * srcStep);
                        long offsetDst = (y * dstStep);
                        long bytesToCopy = w * ch;
                        // 一列ごとにコピー
                        Buffer.MemoryCopy(pSrc + offsetSrc, pDst + offsetDst, bytesToCopy, bytesToCopy);
                    }
                }
            }
            finally
            {
                if (bd is not null)
                    dst.UnlockBits(bd);
            }
        }
        else
        {
            global::OpenCvSharp.Extensions.BitmapConverter.ToBitmap(src, dst);
        }
    }
}
