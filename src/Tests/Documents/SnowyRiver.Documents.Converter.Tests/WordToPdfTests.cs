using System.IO;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using SnowyRiver.Documents.Converter;
using SnowyRiver.Documents.Converter.Abstractions;

namespace SnowyRiver.Documents.Converter.Tests;

public class WordToPdfTests
{
    [Fact]
    public void Convert_SimpleDocx_To_Pdf_Should_Produce_NonEmpty_Stream()
    {
        using var docxStream = BuildSimpleDocx();
        using var pdfStream = new MemoryStream();

        var converter = new DocumentConverter();
        converter.Convert(docxStream, DocFormat.Word, pdfStream, DocFormat.Pdf);

        Assert.True(pdfStream.Length > 0, "PDF 输出应非空");

        // PDF 文件以 %PDF- 开头
        var bytes = pdfStream.ToArray();
        Assert.Equal((byte)'%', bytes[0]);
        Assert.Equal((byte)'P', bytes[1]);
        Assert.Equal((byte)'D', bytes[2]);
        Assert.Equal((byte)'F', bytes[3]);
    }

    private static MemoryStream BuildSimpleDocx()
    {
        var ms = new MemoryStream();
        using (var doc = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document, autoSave: true))
        {
            var main = doc.AddMainDocumentPart();
            main.Document = new Document(
                new Body(
                    new Paragraph(
                        new ParagraphProperties(
                            new ParagraphStyleId { Val = "Heading1" }),
                        new Run(new Text("测试标题"))),
                    new Paragraph(
                        new ParagraphProperties(
                            new Indentation { FirstLine = "420" },
                            new SpacingBetweenLines { Before = "120", After = "120" }),
                        new Run(new Text("第一段正文，包含中文与 English 混排。"))),
                    new Paragraph(
                        new Run(new Text("第二段，"),
                                new RunProperties(new Bold())),
                        new Run(new RunProperties(new Bold()), new Text("加粗部分"))),
                    new SectionProperties(
                        new PageSize { Width = 11906, Height = 16838 },
                        new PageMargin { Top = 1440, Bottom = 1440, Left = 1440, Right = 1440 })
                ));
        }
        ms.Position = 0;
        return ms;
    }
}
