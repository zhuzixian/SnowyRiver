using System.IO;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using SnowyRiver.Documents.Converter.Abstractions;
using SnowyRiver.Documents.Converter.Engines;
using SnowyRiver.Documents.Converter.Model;

namespace SnowyRiver.Documents.Converter.Tests;

public class WordListLabelAndBookmarkTests
{
    [Fact]
    public void Decimal_List_Should_Populate_ListLabel_And_BookmarkNames()
    {
        using var docx = BuildDocxWithListAndBookmark();
        var ir = WordReader.Read(docx, new ConversionOptions(), null);

        var listParas = ir.Blocks
            .Where(b => b.Paragraph != null && b.Paragraph!.ListType != ListType.None)
            .Select(b => b.Paragraph!)
            .ToList();
        Assert.NotEmpty(listParas);
        Assert.All(listParas, p => Assert.False(string.IsNullOrEmpty(p.ListLabel),
            $"ListLabel 应被填充，实际为空。ListNumber={p.ListNumber} Type={p.ListType}"));

        var bmPara = ir.Blocks
            .Where(b => b.Paragraph != null && b.Paragraph!.BookmarkNames.Count > 0)
            .Select(b => b.Paragraph!)
            .FirstOrDefault();
        Assert.NotNull(bmPara);
        Assert.Contains(bmPara!.BookmarkNames, n => n.Contains("MyBookmark"));
    }

    private static MemoryStream BuildDocxWithListAndBookmark()
    {
        var ms = new MemoryStream();
        using (var doc = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document, autoSave: true))
        {
            var main = doc.AddMainDocumentPart();

            // numbering.xml: 简单 decimal 列表
            var numPart = main.AddNewPart<NumberingDefinitionsPart>();
            numPart.Numbering = new Numbering(
                new AbstractNum(
                    new Level(
                        new StartNumberingValue { Val = 1 },
                        new NumberingFormat { Val = NumberFormatValues.Decimal },
                        new LevelText { Val = "%1." },
                        new LevelJustification { Val = LevelJustificationValues.Left })
                    { LevelIndex = 0 })
                { AbstractNumberId = 0 },
                new NumberingInstance(new AbstractNumId { Val = 0 }) { NumberID = 1 });

            main.Document = new Document(
                new Body(
                    new Paragraph(
                        new ParagraphProperties(
                            new NumberingProperties(new NumberingLevelReference { Val = 0 }, new NumberingId { Val = 1 })),
                        new Run(new Text("第一项"))),
                    new Paragraph(
                        new ParagraphProperties(
                            new NumberingProperties(new NumberingLevelReference { Val = 0 }, new NumberingId { Val = 1 })),
                        new Run(new Text("第二项"))),
                    new Paragraph(
                        new BookmarkStart { Id = "1", Name = "MyBookmark" },
                        new Run(new Text("被书签覆盖的段落")),
                        new BookmarkEnd { Id = "1" }),
                    new SectionProperties(
                        new PageSize { Width = 11906, Height = 16838 },
                        new PageMargin { Top = 1440, Bottom = 1440, Left = 1440, Right = 1440 })
                ));
        }
        ms.Position = 0;
        return ms;
    }
}
