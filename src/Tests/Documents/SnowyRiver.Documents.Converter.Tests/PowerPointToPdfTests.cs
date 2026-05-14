using System.IO;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using SnowyRiver.Documents.Converter;
using SnowyRiver.Documents.Converter.Abstractions;
using D = DocumentFormat.OpenXml.Drawing;

namespace SnowyRiver.Documents.Converter.Tests;

public class PowerPointToPdfTests
{
    [Fact]
    public void Convert_SimplePptx_To_Pdf_Should_Produce_NonEmpty_Pdf()
    {
        using var pptx = BuildMinimalPptx();
        using var pdf = new MemoryStream();
        var converter = new DocumentConverter();
        converter.Convert(pptx, DocFormat.PowerPoint, pdf, DocFormat.Pdf);
        Assert.True(pdf.Length > 0, "PDF 输出应非空");
        var bytes = pdf.ToArray();
        Assert.Equal((byte)'%', bytes[0]);
        Assert.Equal((byte)'P', bytes[1]);
        Assert.Equal((byte)'D', bytes[2]);
        Assert.Equal((byte)'F', bytes[3]);
    }

    private static MemoryStream BuildMinimalPptx()
    {
        var ms = new MemoryStream();
        using (var doc = PresentationDocument.Create(ms, PresentationDocumentType.Presentation, autoSave: true))
        {
            var presPart = doc.AddPresentationPart();
            presPart.Presentation = new Presentation();

            // SlideMaster
            var smPart = presPart.AddNewPart<SlideMasterPart>("smId1");
            smPart.SlideMaster = new SlideMaster(
                new CommonSlideData(new ShapeTree(
                    new NonVisualGroupShapeProperties(
                        new NonVisualDrawingProperties { Id = 1U, Name = "" },
                        new NonVisualGroupShapeDrawingProperties(),
                        new ApplicationNonVisualDrawingProperties()),
                    new GroupShapeProperties())),
                new ColorMap
                {
                    Background1 = D.ColorSchemeIndexValues.Light1,
                    Text1 = D.ColorSchemeIndexValues.Dark1,
                    Background2 = D.ColorSchemeIndexValues.Light2,
                    Text2 = D.ColorSchemeIndexValues.Dark2,
                    Accent1 = D.ColorSchemeIndexValues.Accent1,
                    Accent2 = D.ColorSchemeIndexValues.Accent2,
                    Accent3 = D.ColorSchemeIndexValues.Accent3,
                    Accent4 = D.ColorSchemeIndexValues.Accent4,
                    Accent5 = D.ColorSchemeIndexValues.Accent5,
                    Accent6 = D.ColorSchemeIndexValues.Accent6,
                    Hyperlink = D.ColorSchemeIndexValues.Hyperlink,
                    FollowedHyperlink = D.ColorSchemeIndexValues.FollowedHyperlink,
                });

            // SlideLayout
            var slPart = smPart.AddNewPart<SlideLayoutPart>("slId1");
            slPart.SlideLayout = new SlideLayout(
                new CommonSlideData(new ShapeTree(
                    new NonVisualGroupShapeProperties(
                        new NonVisualDrawingProperties { Id = 1U, Name = "" },
                        new NonVisualGroupShapeDrawingProperties(),
                        new ApplicationNonVisualDrawingProperties()),
                    new GroupShapeProperties())),
                new ColorMapOverride(new D.MasterColorMapping()));

            // Slide 1
            var sp1 = presPart.AddNewPart<SlidePart>("sId1");
            sp1.AddPart(slPart, "rIdLayout");
            sp1.Slide = BuildSlide("第一张幻灯片", "项目一\n项目二");

            // Slide 2
            var sp2 = presPart.AddNewPart<SlidePart>("sId2");
            sp2.AddPart(slPart, "rIdLayout");
            sp2.Slide = BuildSlide("第二张幻灯片", "正文内容");

            // Slide id list & sizes
            presPart.Presentation.SlideIdList = new SlideIdList(
                new SlideId { Id = 256U, RelationshipId = "sId1" },
                new SlideId { Id = 257U, RelationshipId = "sId2" });
            presPart.Presentation.SlideMasterIdList = new SlideMasterIdList(
                new SlideMasterId { Id = 2147483648U, RelationshipId = "smId1" });
            presPart.Presentation.SlideSize = new SlideSize { Cx = 9144000, Cy = 6858000 };
            presPart.Presentation.NotesSize = new NotesSize { Cx = 6858000, Cy = 9144000 };
        }
        ms.Position = 0;
        return ms;
    }

    private static Slide BuildSlide(string title, string body)
    {
        return new Slide(
            new CommonSlideData(new ShapeTree(
                new NonVisualGroupShapeProperties(
                    new NonVisualDrawingProperties { Id = 1U, Name = "" },
                    new NonVisualGroupShapeDrawingProperties(),
                    new ApplicationNonVisualDrawingProperties()),
                new GroupShapeProperties(),
                // Title
                new Shape(
                    new NonVisualShapeProperties(
                        new NonVisualDrawingProperties { Id = 2U, Name = "Title" },
                        new NonVisualShapeDrawingProperties(new D.ShapeLocks { NoGrouping = true }),
                        new ApplicationNonVisualDrawingProperties(new PlaceholderShape { Type = PlaceholderValues.Title })),
                    new ShapeProperties(),
                    new TextBody(new D.BodyProperties(), new D.ListStyle(),
                        new D.Paragraph(new D.Run(new D.RunProperties { Language = "zh-CN" }, new D.Text(title))))),
                // Body
                new Shape(
                    new NonVisualShapeProperties(
                        new NonVisualDrawingProperties { Id = 3U, Name = "Body" },
                        new NonVisualShapeDrawingProperties(new D.ShapeLocks { NoGrouping = true }),
                        new ApplicationNonVisualDrawingProperties(new PlaceholderShape { Index = 1U })),
                    new ShapeProperties(),
                    new TextBody(new D.BodyProperties(), new D.ListStyle(),
                        new D.Paragraph(new D.Run(new D.RunProperties { Language = "zh-CN" }, new D.Text(body))))))));
    }
}
