using System.IO;
using ClosedXML.Excel;
using SnowyRiver.Documents.Converter;
using SnowyRiver.Documents.Converter.Abstractions;

namespace SnowyRiver.Documents.Converter.Tests;

public class ExcelToPdfTests
{
    [Fact]
    public void Convert_SimpleXlsx_To_Pdf_Should_Produce_NonEmpty_Stream()
    {
        using var xlsxStream = BuildSimpleXlsx();
        using var pdfStream = new MemoryStream();

        var converter = new DocumentConverter();
        converter.Convert(xlsxStream, DocFormat.Excel, pdfStream, DocFormat.Pdf);

        Assert.True(pdfStream.Length > 0);
        var bytes = pdfStream.ToArray();
        Assert.Equal((byte)'%', bytes[0]);
        Assert.Equal((byte)'P', bytes[1]);
        Assert.Equal((byte)'D', bytes[2]);
        Assert.Equal((byte)'F', bytes[3]);
    }

    private static MemoryStream BuildSimpleXlsx()
    {
        var ms = new MemoryStream();
        using (var wb = new XLWorkbook())
        {
            var sheet = wb.AddWorksheet("Sheet1");
            sheet.Cell(1, 1).Value = "项目";
            sheet.Cell(1, 2).Value = "数量";
            sheet.Cell(1, 3).Value = "金额";
            sheet.Cell(2, 1).Value = "苹果";
            sheet.Cell(2, 2).Value = 10;
            sheet.Cell(2, 3).Value = 25.5;
            sheet.Cell(3, 1).Value = "橙子";
            sheet.Cell(3, 2).Value = 7;
            sheet.Cell(3, 3).Value = 14.7;
            sheet.Cell(4, 1).Value = "合计";
            sheet.Cell(4, 3).FormulaA1 = "SUM(C2:C3)";

            sheet.Range("A1:C1").Style.Font.Bold = true;
            sheet.Range("A1:C1").Style.Fill.BackgroundColor = XLColor.LightGray;
            wb.SaveAs(ms);
        }
        ms.Position = 0;
        return ms;
    }
}
