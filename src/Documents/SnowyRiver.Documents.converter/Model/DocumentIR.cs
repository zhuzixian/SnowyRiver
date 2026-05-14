namespace SnowyRiver.Documents.Converter.Model;

/// <summary>水平对齐方式。</summary>
public enum HorizontalAlign { Left, Center, Right, Justify }

/// <summary>垂直对齐方式。</summary>
public enum VerticalAlign { Top, Middle, Bottom }

/// <summary>文本运行（同一段落中具有一致格式的文字片段）。</summary>
public sealed class IrRun
{
    public string Text { get; set; } = string.Empty;
    public string? FontFamily { get; set; }
    public double? FontSize { get; set; }
    public bool Bold { get; set; }
    public bool Italic { get; set; }
    public bool Underline { get; set; }
    public string? ColorHex { get; set; }
}

/// <summary>段落（多个 Run 加格式属性）。</summary>
public sealed class IrParagraph
{
    public List<IrRun> Runs { get; } = new();
    public HorizontalAlign Alignment { get; set; } = HorizontalAlign.Left;
    public bool IsHeading { get; set; }
    public int HeadingLevel { get; set; }
    public string PlainText => string.Concat(Runs.Select(r => r.Text));
}

/// <summary>嵌入图片块。</summary>
public sealed class IrImage
{
    /// <summary>图片二进制内容（PNG / JPEG / EMF 等原始字节）。</summary>
    public byte[] Data { get; set; } = Array.Empty<byte>();
    /// <summary>图片格式提示（如 "png"、"jpeg"、"emf"），仅用于诊断。</summary>
    public string? Format { get; set; }
    /// <summary>渲染时使用的宽度（像素，可空表示自适应）。</summary>
    public double? WidthPx { get; set; }
    /// <summary>渲染时使用的高度（像素，可空表示自适应）。</summary>
    public double? HeightPx { get; set; }
}

/// <summary>表格单元格样式。</summary>
public sealed class IrCellStyle
{
    public string? BackgroundHex { get; set; }
    public string? BorderHex { get; set; }
    public double BorderThickness { get; set; } = 0.5;
    public HorizontalAlign HAlign { get; set; } = HorizontalAlign.Left;
    public VerticalAlign VAlign { get; set; } = VerticalAlign.Middle;
    public string? FontFamily { get; set; }
    public double? FontSize { get; set; }
    public bool Bold { get; set; }
    public bool Italic { get; set; }
    public string? FontColorHex { get; set; }
    public bool WrapText { get; set; } = true;
}

/// <summary>表格单元格。</summary>
public sealed class IrCell
{
    public string Text { get; set; } = string.Empty;
    /// <summary>横向跨越的列数（合并单元格），默认为 1。</summary>
    public int ColSpan { get; set; } = 1;
    /// <summary>纵向跨越的行数（合并单元格），默认为 1。</summary>
    public int RowSpan { get; set; } = 1;
    /// <summary>本单元格是否为合并块的从属（不渲染）。</summary>
    public bool Suppressed { get; set; }
    public IrCellStyle Style { get; set; } = new();
}

/// <summary>表格行。</summary>
public sealed class IrRow
{
    public List<IrCell> Cells { get; } = new();
    /// <summary>行高（点 pt，可空）。</summary>
    public double? HeightPt { get; set; }
}

/// <summary>表格。</summary>
public sealed class IrTable
{
    public List<IrRow> Rows { get; } = new();
    /// <summary>每列宽度（点 pt）；为 null 或空表示均分。</summary>
    public List<double?> ColumnWidthsPt { get; } = new();
    public int ColumnCount => Rows.Count == 0 ? ColumnWidthsPt.Count : Rows.Max(r => r.Cells.Sum(c => c.ColSpan));
}

/// <summary>分页符。</summary>
public sealed class IrPageBreak { }

/// <summary>文档块（多态：段落/图片/表格/分页）。</summary>
public sealed class IrBlock
{
    public IrParagraph? Paragraph { get; set; }
    public IrImage? Image { get; set; }
    public IrTable? Table { get; set; }
    public IrPageBreak? PageBreak { get; set; }

    public static IrBlock Of(IrParagraph p) => new() { Paragraph = p };
    public static IrBlock Of(IrImage i) => new() { Image = i };
    public static IrBlock Of(IrTable t) => new() { Table = t };
    public static IrBlock NewPage() => new() { PageBreak = new IrPageBreak() };
}

/// <summary>文档中间表示。</summary>
public sealed class IrDocument
{
    public string? Title { get; set; }
    public string? Author { get; set; }
    /// <summary>页面宽度（点 pt），默认 A4。</summary>
    public double PageWidthPt { get; set; } = 595;
    /// <summary>页面高度（点 pt），默认 A4。</summary>
    public double PageHeightPt { get; set; } = 842;
    /// <summary>页面四周边距（点 pt）。</summary>
    public double MarginPt { get; set; } = 36;
    public List<IrBlock> Blocks { get; } = new();
}
