using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using SnowyRiver.Documents.Converter.Model;

namespace SnowyRiver.Documents.Converter.Engines;

/// <summary>
/// 把 IrDocument 序列化为确定性的、跨平台稳定的 JSON 快照，用于回归测试。
/// 仅保留结构性、内容性字段，剥离绝对路径、时间戳、随机数等易变信息。
/// </summary>
public static class IrSnapshotSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    public static string Serialize(IrDocument ir)
    {
        if (ir == null) return "{}";
        var dto = new
        {
            Blocks = ir.Blocks.Select(MapBlock).ToList(),
        };
        return JsonSerializer.Serialize(dto, Options);
    }

    private static object MapBlock(IrBlock b)
    {
        if (b.Paragraph != null) return new { Kind = "Paragraph", Paragraph = MapParagraph(b.Paragraph) };
        if (b.Table != null) return new
        {
            Kind = "Table",
            Rows = b.Table.Rows.Count,
            Cols = b.Table.Rows.Count > 0 ? b.Table.Rows[0].Cells.Count : 0,
        };
        if (b.Image != null) return new
        {
            Kind = "Image",
            Bytes = b.Image.Data?.Length ?? 0,
            Format = b.Image.Format,
        };
        return new { Kind = "Other" };
    }

    private static object MapParagraph(IrParagraph p) => new
    {
        IsHeading = p.IsHeading ? true : (bool?)null,
        HeadingLevel = p.HeadingLevel > 0 ? p.HeadingLevel : (int?)null,
        IsEquation = p.IsEquation ? true : (bool?)null,
        ListLabel = p.ListLabel,
        AnchorId = p.AnchorId,
        Bookmarks = p.BookmarkNames.Count > 0 ? p.BookmarkNames : null,
        EquationLinear = p.EquationLinear,
        HasMathML = string.IsNullOrEmpty(p.EquationMathML) ? (bool?)null : true,
        Runs = p.Runs.Select(r => new
        {
            r.Text,
            Bold = r.Bold ? true : (bool?)null,
            Italic = r.Italic ? true : (bool?)null,
            Math = r.IsMathPlaceholder ? true : (bool?)null,
        }).ToList(),
    };
}
