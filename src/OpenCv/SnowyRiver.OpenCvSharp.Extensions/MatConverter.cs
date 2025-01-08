using System;
using OpenCvSharp;

namespace SnowyRiver.OpenCvSharp.Extensions;
public static class MatConverter
{
    public static Mat ConvertTo(this Mat sourceImage, MatType targetType)
    {
        var result = sourceImage.Clone();
        if (sourceImage.Type() != targetType)
        {
            var alpha = 1d;
            if ((targetType == MatType.CV_16UC1 || targetType == MatType.CV_16UC3)
                && (sourceImage.Type() == MatType.CV_8UC1 || sourceImage.Type() == MatType.CV_8UC3))
            {
                alpha = Math.Pow(2, 8) - 1;
            }
            else if ((targetType == MatType.CV_8UC1 || targetType == MatType.CV_8UC3)
                     && (sourceImage.Type() == MatType.CV_16UC1 || sourceImage.Type() == MatType.CV_16UC3))
            {
                alpha = 1d / (Math.Pow(2, 8) - 1);
            }
            sourceImage.ConvertTo(result, targetType, alpha);
        }

        return result;
    }
}
