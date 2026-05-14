using System.IO;
using ClosedXML.Excel;
using SnowyRiver.Documents.Converter.Abstractions;
using SnowyRiver.Documents.Converter.Tests.Golden;

namespace SnowyRiver.Documents.Converter.Tests;

/// <summary>
/// 黄金样本回归：聚焦 Excel→PDF 转换在多次运行下的结构稳定性，以及第十轮新功能开关
/// （Excel auto-fit 列宽、系统字体扫描）开启后的健壮性。
/// </summary>
public class GoldenRegressionTests
{
    [Fact]
    public void Excel_To_Pdf_Should_Be_Structurally_Stable_Across_Runs()
    {
        using var xlsx = GoldenHarness.BuildSampleWorkbook(rows: 30, cols: 4);

        var first = GoldenHarness.ConvertExcelToPdf(xlsx);
        var second = GoldenHarness.ConvertExcelToPdf(xlsx);

        Assert.True(first.Length > 0, "first PDF should be non-empty");
        Assert.True(second.Length > 0, "second PDF should be non-empty");
        Assert.True(first.PageCount >= 1, $"expected at least 1 page, got {first.PageCount}");

        // 页数稳定（最重要的回归断言）。
        Assert.Equal(first.PageCount, second.PageCount);

        // 长度允许小幅波动（PDF 时间戳/ID 等元数据），但不应出现量级差异。
        var ratio = (double)first.Length / second.Length;
        Assert.InRange(ratio, 0.95, 1.05);
    }

    [Fact]
    public void Excel_AutoFit_Disabled_Should_Still_Produce_Valid_Pdf()
    {
        using var xlsx = GoldenHarness.BuildSampleWorkbook(rows: 10, cols: 3);
        var options = new ConversionOptions
        {
            EnableExcelAutoFitColumns = false,
            EnableSystemFontScan = false,
        };

        var stats = GoldenHarness.ConvertExcelToPdf(xlsx, options);
        Assert.True(stats.Length > 0);
        Assert.True(stats.PageCount >= 1);
        Assert.Equal((byte)'%', stats.Bytes[0]);
        Assert.Equal((byte)'P', stats.Bytes[1]);
        Assert.Equal((byte)'D', stats.Bytes[2]);
        Assert.Equal((byte)'F', stats.Bytes[3]);
    }

    [Fact]
    public void Excel_AutoFit_Enabled_Should_Not_Shrink_PageCount_For_LongText()
    {
        // 中等长度中文文本 → 列宽 auto-fit 应至少不会让页数变得比关闭时更小（极端时一致）。
        var text = new string('字', 12);
        using var xlsxOff = BuildLongTextWorkbook(text);
        using var xlsxOn = BuildLongTextWorkbook(text);

        var off = GoldenHarness.ConvertExcelToPdf(xlsxOff, new ConversionOptions { EnableExcelAutoFitColumns = false });
        var on = GoldenHarness.ConvertExcelToPdf(xlsxOn, new ConversionOptions { EnableExcelAutoFitColumns = true });

        Assert.True(off.PageCount >= 1);
        Assert.True(on.PageCount >= off.PageCount,
            $"auto-fit enabled produced fewer pages ({on.PageCount}) than disabled ({off.PageCount})");
    }

    private static MemoryStream BuildLongTextWorkbook(string text)
    {
        var ms = new MemoryStream();
        using (var wb = new XLWorkbook())
        {
            var sheet = wb.AddWorksheet("S");
            for (int r = 1; r <= 8; r++)
            {
                sheet.Cell(r, 1).Value = text;
                sheet.Cell(r, 2).Value = text;
            }
            wb.SaveAs(ms);
        }
        ms.Position = 0;
        return ms;
    }
}
