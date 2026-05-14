using System.IO;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using SnowyRiver.Documents.Converter.Abstractions;
using SnowyRiver.Documents.Converter.Engines;
using SnowyRiver.Documents.Converter.Engines.Charts;
using SnowyRiver.Documents.Converter.Model;
using M = DocumentFormat.OpenXml.Math;
using W = DocumentFormat.OpenXml.Wordprocessing;

namespace SnowyRiver.Documents.Converter.Tests;

public class VectorChartOmmlTests
{
    [Fact]
    public void EmfWmfHelper_Should_Return_Placeholder_When_Bytes_Invalid()
    {
        var data = new byte[] { 0x00, 0x01, 0x02, 0x03 };
        var png = EmfWmfHelper.RasterizeToPng(data, "x-emf");
        Assert.NotNull(png);
        Assert.True(png.Length > 8);
        Assert.Equal(0x89, png[0]);
        Assert.Equal(0x50, png[1]);
        Assert.Equal(0x4E, png[2]);
        Assert.Equal(0x47, png[3]);
    }

    [Fact]
    public void ChartRenderer_Should_Render_Pie_With_DataLabels_To_Png()
    {
        var data = new ChartData
        {
            Kind = ChartKind.Pie,
            Title = "Pie",
            Categories = { "A", "B", "C" },
            Series = { new ChartSeries { Name = "S", Values = { 10, 20, 70 } } },
            ShowDataLabels = true,
            PixelWidth = 400,
            PixelHeight = 300,
        };
        var bytes = ChartRenderer.Render(data, dpi: 96, fontFamily: "Arial");
        Assert.NotNull(bytes);
        Assert.True(bytes!.Length > 100);
        Assert.Equal(0x89, bytes[0]);
        Assert.Equal(0x50, bytes[1]);
    }

    [Fact]
    public void OmmlReader_Should_Linearize_Fraction_And_Power()
    {
        var omath = new M.OfficeMath();
        var frac = new M.Fraction(
            new M.Numerator(new M.Run(new M.Text("1"))),
            new M.Denominator(new M.Run(new M.Text("2"))));
        omath.Append(frac);
        var sup = new M.Superscript(
            new M.Base(new M.Run(new M.Text("x"))),
            new M.SuperArgument(new M.Run(new M.Text("2"))));
        omath.Append(sup);

        var s = OmmlReader.Linearize(omath);
        Assert.Contains("(1)/(2)", s);
        Assert.Contains("x^(2)", s);
    }

    [Fact]
    public void WordReader_Should_Mark_IsEquation_For_OMML_Paragraph()
    {
        using var docx = BuildDocxWithEquation();
        var ir = WordReader.Read(docx, new ConversionOptions(), null);
        var para = ir.Blocks
            .Where(b => b.Paragraph != null && b.Paragraph!.IsEquation)
            .Select(b => b.Paragraph!)
            .FirstOrDefault();
        Assert.NotNull(para);
        Assert.Contains(para!.Runs, r => r.IsMathPlaceholder);
    }

    private static MemoryStream BuildDocxWithEquation()
    {
        var ms = new MemoryStream();
        using (var doc = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document, autoSave: true))
        {
            var main = doc.AddMainDocumentPart();
            var omath = new M.OfficeMath(
                new M.Run(new M.Text("a")),
                new M.Superscript(
                    new M.Base(new M.Run(new M.Text("x"))),
                    new M.SuperArgument(new M.Run(new M.Text("2")))));
            main.Document = new W.Document(
                new W.Body(
                    new W.Paragraph(omath),
                    new W.SectionProperties(
                        new W.PageSize { Width = 11906, Height = 16838 },
                        new W.PageMargin { Top = 1440, Bottom = 1440, Left = 1440, Right = 1440 })));
        }
        ms.Position = 0;
        return ms;
    }
}
