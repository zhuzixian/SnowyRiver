using System;
using System.IO;
using System.Linq;
using System.Text;
using ClosedXML.Excel;
using SnowyRiver.Documents.Converter.Abstractions;

namespace SnowyRiver.Documents.Converter.Tests.Golden;

/// <summary>
/// 黄金样本回归 harness：把已知输入转换为 PDF/XPS，并对结果做轻量结构断言。
/// 不引入新的 PDF 解析依赖；通过扫描 PDF 内置 token（如 "/Type /Page"）做粗粒度页数估计。
/// </summary>
internal static class GoldenHarness
{
    public sealed record PdfStats(int Length, int PageCount, byte[] Bytes);

    public static PdfStats ConvertExcelToPdf(MemoryStream xlsx, ConversionOptions? options = null)
    {
        xlsx.Position = 0;
        using var pdfStream = new MemoryStream();
        var converter = new DocumentConverter();
        converter.Convert(xlsx, DocFormat.Excel, pdfStream, DocFormat.Pdf, options);
        var bytes = pdfStream.ToArray();
        return new PdfStats(bytes.Length, CountPdfPages(bytes), bytes);
    }

    public static int CountPdfPages(byte[] pdfBytes)
    {
        // 粗略：把 PDF 当成 ASCII 文本扫描 "/Type /Page" 标记数；
        // 不区分 /Pages 和 /Page —— 通过排除带尾空白后跟 "s" 的情况避免 Pages 误计。
        var ascii = Encoding.ASCII.GetString(pdfBytes);
        int count = 0;
        int idx = 0;
        while (true)
        {
            int hit = ascii.IndexOf("/Type", idx, StringComparison.Ordinal);
            if (hit < 0) break;
            // 跳过任意空白
            int p = hit + 5;
            while (p < ascii.Length && (ascii[p] == ' ' || ascii[p] == '\t' || ascii[p] == '\r' || ascii[p] == '\n')) p++;
            if (p < ascii.Length && ascii[p] == '/')
            {
                p++;
                if (p + 4 <= ascii.Length && ascii.AsSpan(p, 4).SequenceEqual("Page".AsSpan()))
                {
                    int after = p + 4;
                    char nc = after < ascii.Length ? ascii[after] : ' ';
                    if (nc != 's' && nc != 'S') count++;
                }
            }
            idx = hit + 5;
        }
        return count;
    }

    public static MemoryStream BuildSampleWorkbook(int rows = 50, int cols = 4)
    {
        var ms = new MemoryStream();
        using (var wb = new XLWorkbook())
        {
            var sheet = wb.AddWorksheet("Data");
            for (int c = 1; c <= cols; c++)
                sheet.Cell(1, c).Value = $"列{c}";
            sheet.Range(1, 1, 1, cols).Style.Font.Bold = true;

            for (int r = 2; r <= rows + 1; r++)
            {
                sheet.Cell(r, 1).Value = $"项目-{r - 1:D3}";
                for (int c = 2; c <= cols; c++)
                    sheet.Cell(r, c).Value = (r - 1) * c;
            }
            wb.SaveAs(ms);
        }
        ms.Position = 0;
        return ms;
    }
}
