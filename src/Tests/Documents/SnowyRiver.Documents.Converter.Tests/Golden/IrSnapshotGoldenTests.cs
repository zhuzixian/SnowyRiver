using System;
using System.IO;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using SnowyRiver.Documents.Converter.Abstractions;
using SnowyRiver.Documents.Converter.Engines;
using W = DocumentFormat.OpenXml.Wordprocessing;

namespace SnowyRiver.Documents.Converter.Tests.Golden;

/// <summary>
/// IR JSON 快照黄金回归：固定一个最小 Word 文档，要求 IR 序列化结果与基线 JSON 完全一致。
/// 基线缺失时自动生成（首次跑过即落盘到 Tests\Golden\Baselines\ir-simple-word.json）。
/// 结构变更需要显式更新基线，避免无意中破坏快照协议或字段顺序。
/// </summary>
public class IrSnapshotGoldenTests
{
    private const string BaselineFileName = "ir-simple-word.json";

    [Fact]
    public void Ir_Simple_Word_Should_Match_Golden_Baseline()
    {
        using var docx = BuildDeterministicWordDoc();
        var ir = WordReader.Read(docx, new ConversionOptions(), null);
        var actual = NormalizeNewlines(IrSnapshotSerializer.Serialize(ir));

        var baselinePath = ResolveBaselinePath();
        if (!File.Exists(baselinePath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(baselinePath)!);
            File.WriteAllText(baselinePath, actual);
            return; // 首次落盘，跳过等值断言。
        }

        var expected = NormalizeNewlines(File.ReadAllText(baselinePath));
        Assert.Equal(expected, actual);
    }

    private static string NormalizeNewlines(string s) => s.Replace("\r\n", "\n").Replace("\r", "\n");

    private static string ResolveBaselinePath()
    {
        // 从测试运行目录向上查找包含测试项目文件的源码目录，把基线放在 Golden\Baselines 下，便于纳入仓库。
        var dir = AppContext.BaseDirectory;
        for (int i = 0; i < 8 && !string.IsNullOrEmpty(dir); i++)
        {
            var projFile = Path.Combine(dir, "SnowyRiver.Documents.Converter.Tests.csproj");
            if (File.Exists(projFile))
            {
                return Path.Combine(dir, "Golden", "Baselines", BaselineFileName);
            }
            dir = Path.GetDirectoryName(dir);
        }
        // 退回到二进制目录下，至少能产出基线文件，便于排查。
        return Path.Combine(AppContext.BaseDirectory, "Golden", "Baselines", BaselineFileName);
    }

    private static MemoryStream BuildDeterministicWordDoc()
    {
        var ms = new MemoryStream();
        using (var doc = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document, autoSave: true))
        {
            var main = doc.AddMainDocumentPart();
            main.Document = new W.Document(
                new W.Body(
                    new W.Paragraph(new W.Run(new W.Text("Title") { Space = SpaceProcessingModeValues.Preserve })),
                    new W.Paragraph(new W.Run(new W.Text("Hello world") { Space = SpaceProcessingModeValues.Preserve })),
                    new W.Paragraph(new W.Run(new W.Text("快照基线") { Space = SpaceProcessingModeValues.Preserve })),
                    new W.SectionProperties(
                        new W.PageSize { Width = 11906, Height = 16838 },
                        new W.PageMargin { Top = 1440, Bottom = 1440, Left = 1440, Right = 1440 })));
        }
        ms.Position = 0;
        return ms;
    }
}
