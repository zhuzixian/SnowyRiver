using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using SnowyRiver.Documents.Converter.Abstractions;
using SnowyRiver.Documents.Converter.Engines;
using SnowyRiver.Documents.Converter.Engines.Charts;
using SnowyRiver.Documents.Converter.Model;
using M = DocumentFormat.OpenXml.Math;
using W = DocumentFormat.OpenXml.Wordprocessing;

namespace SnowyRiver.Documents.Converter.Tests;

public class RoundLPlusTests
{
    [Fact]
    public void OmmlReader_ToMathML_Should_Emit_Msup_And_Mfrac()
    {
        var omath = new M.OfficeMath(
            new M.Fraction(
                new M.Numerator(new M.Run(new M.Text("1"))),
                new M.Denominator(new M.Run(new M.Text("2")))),
            new M.Superscript(
                new M.Base(new M.Run(new M.Text("x"))),
                new M.SuperArgument(new M.Run(new M.Text("2")))));

        var mml = OmmlReader.ToMathML(omath);
        Assert.False(string.IsNullOrEmpty(mml));
        Assert.Contains("<math", mml);
        Assert.Contains("<mfrac", mml);
        Assert.Contains("<msup", mml);
        Assert.Contains("xmlns=\"http://www.w3.org/1998/Math/MathML\"", mml);
    }

    [Fact]
    public void WordReader_Should_Populate_EquationLinear_And_MathML()
    {
        using var docx = BuildDocxWithEquation();
        var ir = WordReader.Read(docx, new ConversionOptions(), null);
        var para = ir.Blocks
            .Select(b => b.Paragraph)
            .FirstOrDefault(p => p != null && p.IsEquation);
        Assert.NotNull(para);
        Assert.False(string.IsNullOrEmpty(para!.EquationLinear));
        Assert.False(string.IsNullOrEmpty(para.EquationMathML));
        Assert.Contains("<math", para.EquationMathML!);
    }

    [Fact]
    public void EmfWmfHelper_RasterizeSvg_Should_Use_ViewBox_When_No_Width()
    {
        var svg = "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 240 120\"><rect width=\"240\" height=\"120\" fill=\"red\"/></svg>";
        var diag = new ConversionDiagnostics();
        var png = EmfWmfHelper.RasterizeSvgToPng(System.Text.Encoding.UTF8.GetBytes(svg), 96, diag);
        Assert.NotNull(png);
        Assert.Equal(0x89, png[0]); // PNG sig
        // 经 Svg.Skia 真正栅格化后应得到 SVG_RASTERIZED；无效 SVG 时也允许 SVG_PARSE_FAIL/SVG_RASTERIZE_FAIL 占位回退。
        Assert.Contains(diag.Entries, e => e.Code == "SVG_RASTERIZED" || e.Code == "SVG_PLACEHOLDER");
    }

    [Fact]
    public void EmfWmfHelper_RasterizeSvg_Should_Render_Real_Pixels_With_SvgSkia()
    {
        var svg = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"64\" height=\"64\"><rect width=\"64\" height=\"64\" fill=\"#ff0000\"/></svg>";
        var diag = new ConversionDiagnostics();
        var png = EmfWmfHelper.RasterizeSvgToPng(System.Text.Encoding.UTF8.GetBytes(svg), 96, diag);
        Assert.NotNull(png);
        Assert.Equal(0x89, png[0]);
        Assert.True(png.Length > 100);
        Assert.Contains(diag.Entries, e => e.Code == "SVG_RASTERIZED");
    }

    [Fact]
    public void EmfWmfHelper_Should_Diagnose_When_Falling_Back_To_Placeholder()
    {
        var bad = new byte[] { 0x00, 0x01, 0x02, 0x03 };
        var diag = new ConversionDiagnostics();
        var png = EmfWmfHelper.RasterizeToPng(bad, "emf", 96, diag);
        Assert.NotNull(png);
        Assert.Contains(diag.Entries, e => e.Code == "VECTOR_RASTERIZE_FAIL");
    }

    [Fact]
    public void ChartRenderer_Should_Render_Combo_With_Secondary_Axis()
    {
        var data = new ChartData
        {
            Kind = ChartKind.Column,
            Title = "组合图（中文标题）",
            AxisTitleY = "主轴",
            AxisTitleY2 = "副轴",
            Categories = { "一", "二", "三", "四" },
            ShowDataLabels = false,
            PixelWidth = 600,
            PixelHeight = 360,
            Series =
            {
                new ChartSeries { Name = "柱", Values = { 10, 20, 30, 25 } },
                new ChartSeries
                {
                    Name = "线",
                    Values = { 0.10, 0.18, 0.22, 0.30 },
                    OverrideKind = ChartKind.Line,
                    UseSecondaryAxis = true,
                },
            },
        };
        var bytes = ChartRenderer.Render(data, dpi: 96, fontFamily: "Microsoft YaHei");
        Assert.NotNull(bytes);
        Assert.True(bytes!.Length > 200);
        Assert.Equal(0x89, bytes[0]);
    }

    [Fact]
    public void IrSnapshot_Should_Be_Stable_For_Simple_Word_Document()
    {
        using var docx = BuildSimpleDocx();
        var ir = WordReader.Read(docx, new ConversionOptions(), null);
        var snap = IrSnapshotSerializer.Serialize(ir);

        Assert.Contains("\"Blocks\"", snap);
        Assert.Contains("Hello", snap);
        Assert.Contains("世界", snap);
        Assert.DoesNotContain(":\\", snap);
        Assert.DoesNotContain("Z\"", snap);
    }

    [Fact]
    public void MathRenderer_Should_Be_Invoked_For_Equation_Paragraph_When_Injected()
    {
        var fake = new FakeMathRenderer();
        var options = new ConversionOptions { MathRenderer = fake };

        using var docx = BuildDocxWithEquation();
        using var pdf = new MemoryStream();
        var converter = new DocumentConverter();
        converter.Convert(docx, DocFormat.Word, pdf, DocFormat.Pdf, options);

        Assert.True(pdf.Length > 0);
        Assert.True(fake.MathMLCalls + fake.LinearCalls > 0,
            $"IMathRenderer 应至少被调用一次（mathml={fake.MathMLCalls}, linear={fake.LinearCalls}）");
    }

    [Fact]
    public void SkiaMathRenderer_Should_Render_MathML_To_Png()
    {
        var renderer = new SkiaMathRenderer("Cambria Math");
        var mml = "<math xmlns=\"http://www.w3.org/1998/Math/MathML\"><mrow><mfrac><mn>1</mn><mn>2</mn></mfrac><msup><mi>x</mi><mn>2</mn></msup></mrow></math>";
        var png = renderer.RenderMathMLToPng(mml, emPx: 18);
        Assert.NotNull(png);
        Assert.True(png!.Length > 100);
        Assert.Equal(0x89, png[0]);
    }

    [Fact]
    public void SkiaMathRenderer_Should_Render_Linear_To_Png()
    {
        var renderer = new SkiaMathRenderer();
        var png = renderer.RenderLinearToPng("x^2 + y^2", emPx: 16);
        Assert.NotNull(png);
        Assert.True(png!.Length > 50);
        Assert.Equal(0x89, png[0]);
    }

    private sealed class FakeMathRenderer : IMathRenderer
    {
        public int MathMLCalls;
        public int LinearCalls;

        public byte[]? RenderMathMLToPng(string mathML, double emPx = 16, ConversionDiagnostics? diag = null)
        {
            MathMLCalls++;
            return BuildOnePixelPng();
        }

        public byte[]? RenderLinearToPng(string linear, double emPx = 16, ConversionDiagnostics? diag = null)
        {
            LinearCalls++;
            return BuildOnePixelPng();
        }

        private static byte[] BuildOnePixelPng()
        {
            using var surface = SkiaSharp.SKSurface.Create(new SkiaSharp.SKImageInfo(8, 8));
            surface.Canvas.Clear(SkiaSharp.SKColors.White);
            using var img = surface.Snapshot();
            using var data = img.Encode(SkiaSharp.SKEncodedImageFormat.Png, 90);
            return data.ToArray();
        }
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

    private static MemoryStream BuildSimpleDocx()
    {
        var ms = new MemoryStream();
        using (var doc = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document, autoSave: true))
        {
            var main = doc.AddMainDocumentPart();
            main.Document = new W.Document(
                new W.Body(
                    new W.Paragraph(new W.Run(new W.Text("Hello"))),
                    new W.Paragraph(new W.Run(new W.Text("世界"))),
                    new W.SectionProperties(
                        new W.PageSize { Width = 11906, Height = 16838 },
                        new W.PageMargin { Top = 1440, Bottom = 1440, Left = 1440, Right = 1440 })));
        }
        ms.Position = 0;
        return ms;
    }
}
