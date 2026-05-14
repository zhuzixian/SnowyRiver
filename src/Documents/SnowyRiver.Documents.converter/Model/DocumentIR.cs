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
    /// <summary>该 Run 表示当前页码字段（PAGE）；渲染器应替换为动态页码。</summary>
    public bool IsPageNumberField { get; set; }
    /// <summary>该 Run 表示总页数字段（NUMPAGES）；渲染器应替换为动态总页数。</summary>
    public bool IsPageCountField { get; set; }
    /// <summary>外部超链接 URL（http/https/mailto/...）；非空时渲染为可点击链接。</summary>
    public string? HyperlinkUrl { get; set; }
    /// <summary>内部锚点引用 ID；非空时渲染为可点击的本文档跳转（指向某段落的 <see cref="IrParagraph.AnchorId"/>）。</summary>
    public string? AnchorRef { get; set; }
    /// <summary>该 Run 是某个 SectionLink 引用的目标页码（PAGEREF 字段）；渲染器替换为目标页码。</summary>
    public string? PageRefAnchor { get; set; }
    /// <summary>OMML 数学公式的纯文占位（已退化）。</summary>
    public bool IsMathPlaceholder { get; set; }
    /// <summary>结构化字段类型（PAGE/NUMPAGES/DATE/TIME/SECTION）；非 None 时优先于 IsPageNumberField/IsPageCountField。</summary>
    public RunFieldKind FieldKind { get; set; } = RunFieldKind.None;
    /// <summary>字段附加格式串（如 DATE 的格式字符串 "yyyy-MM-dd"）。</summary>
    public string? FieldFormat { get; set; }
    /// <summary>脚注引用 ID（指向 <see cref="IrDocument.Footnotes"/> 的键）。</summary>
    public string? FootnoteRef { get; set; }
    /// <summary>尾注引用 ID（指向 <see cref="IrDocument.Endnotes"/> 的键）。</summary>
    public string? EndnoteRef { get; set; }
    /// <summary>批注引用 ID（指向 <see cref="IrDocument.Comments"/> 的键）。</summary>
    public string? CommentRef { get; set; }
    /// <summary>修订：插入。</summary>
    public bool IsInsertion { get; set; }
    /// <summary>修订：删除。</summary>
    public bool IsDeletion { get; set; }
    /// <summary>文本高亮（背景色 #RRGGBB；对应 Word w:highlight 或 w:shd）。</summary>
    public string? HighlightHex { get; set; }
}

/// <summary>结构化字段类型。</summary>
public enum RunFieldKind
{
    None,
    Page,
    NumPages,
    Date,
    Time,
    Section,
    SheetName,
    FilePath,
    FileName,
}

/// <summary>列表类型。</summary>
public enum ListType { None, Bullet, Decimal, LowerLetter, UpperLetter, LowerRoman, UpperRoman }

/// <summary>段落（多个 Run 加格式属性）。</summary>
public sealed class IrParagraph
{
    public List<IrRun> Runs { get; } = new();
    public HorizontalAlign Alignment { get; set; } = HorizontalAlign.Left;
    public bool IsHeading { get; set; }
    public int HeadingLevel { get; set; }
    /// <summary>首行缩进（点 pt，可空）。</summary>
    public double? FirstLineIndentPt { get; set; }
    /// <summary>左缩进（点 pt，可空）。</summary>
    public double? LeftIndentPt { get; set; }
    /// <summary>段前距（点 pt，可空）。</summary>
    public double? SpaceBeforePt { get; set; }
    /// <summary>段后距（点 pt，可空）。</summary>
    public double? SpaceAfterPt { get; set; }
    /// <summary>行高（点 pt，可空；为 null 表示按字号默认）。</summary>
    public double? LineHeightPt { get; set; }
    /// <summary>行高倍数（如 1.0、1.5、2.0）；当 Word 行间距 rule=auto 时设置。优先于 <see cref="LineHeightPt"/>。</summary>
    public double? LineHeightRatio { get; set; }
    /// <summary>稳定的目标锚点 ID（用于 PDF 书签 / 内部链接），通常对标题段落或工作表生成。</summary>
    public string? AnchorId { get; set; }
    /// <summary>列表类型；<see cref="ListType.None"/> 表示非列表项。</summary>
    public ListType ListType { get; set; } = ListType.None;
    /// <summary>列表层级（0 起；越大越内层）。</summary>
    public int ListLevel { get; set; }
    /// <summary>编号列表的当前序号（1 起；项目符号忽略）。</summary>
    public int? ListNumber { get; set; }
    /// <summary>已格式化的完整列表标签文本（如 "1.", "1.1.", "(a)", "•"）；非空时渲染器优先使用，避免按级别再次格式化。</summary>
    public string? ListLabel { get; set; }
    /// <summary>该段落上挂载的 Word 自定义书签名（w:bookmarkStart/@w:name），可同时持有多个；用于 PDF 命名锚点 / XPS Block.Name。</summary>
    public List<string> BookmarkNames { get; } = new();
    /// <summary>段落四边边框（对应 Word w:pBdr）；为 null 时不绘制。</summary>
    public IrBorders? Border { get; set; }
    /// <summary>段落底纹背景色 #RRGGBB（w:pPr/w:shd）。</summary>
    public string? BackgroundHex { get; set; }
    /// <summary>制表位列表（点 pt 升序）；非空时渲染器据此处理 \t。</summary>
    public List<IrTabStop> TabStops { get; } = new();
    /// <summary>该段落是否为 OMML 公式段落（由 m:oMath / m:oMathPara 转换而来）；渲染器可选用等宽字体。</summary>
    public bool IsEquation { get; set; }
    /// <summary>方程的线性化纯文本（OmmlReader.Linearize 结果）；当 <see cref="IsEquation"/> 为真时通常非空。</summary>
    public string? EquationLinear { get; set; }
    /// <summary>方程的 MathML 表达式（OmmlReader.ToMathML 结果）；当 <see cref="IsEquation"/> 为真时通常非空。可由 <c>IMathRenderer</c> 渲染为图像。</summary>
    public string? EquationMathML { get; set; }
    public string PlainText => string.Concat(Runs.Select(r => r.Text));
}

/// <summary>制表位对齐方式。</summary>
public enum TabAlignment { Left, Center, Right, Decimal }

/// <summary>段落制表位（w:tabs/w:tab）。</summary>
public sealed class IrTabStop
{
    public double PositionPt { get; set; }
    public TabAlignment Alignment { get; set; } = TabAlignment.Left;
    /// <summary>引导符（'.' / '_' / '-'），为 null 表示不绘制。</summary>
    public char? Leader { get; set; }
}

/// <summary>图片浮动模式。</summary>
public enum ImageFloatMode { Inline, Left, Right, Center }

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
    /// <summary>浮动模式（Word wp:anchor 的简化映射）。</summary>
    public ImageFloatMode Float { get; set; } = ImageFloatMode.Inline;
    /// <summary>是否为矢量图（EMF/WMF/SVG 等）；渲染层可据此选择不同路径。</summary>
    public bool IsVector { get; set; }
    /// <summary>矢量原始格式提示（如 "emf"、"wmf"、"svg"）；仅诊断使用。</summary>
    public string? VectorFormat { get; set; }
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
    /// <summary>条件格式 IconSet 推断出的图标占位字符（如 ▲▶▼）；渲染器在文本前拼接。</summary>
    public string? IconPrefix { get; set; }
    /// <summary>四边边框，独立设置；为 null 时回退到 <see cref="BorderThickness"/>+<see cref="BorderHex"/>。</summary>
    public IrBorders? Borders { get; set; }
}

/// <summary>单边边框定义。</summary>
public sealed class IrBorder
{
    public double Thickness { get; set; }
    public string? ColorHex { get; set; }
}

/// <summary>四边边框集合。</summary>
public sealed class IrBorders
{
    public IrBorder? Top { get; set; }
    public IrBorder? Right { get; set; }
    public IrBorder? Bottom { get; set; }
    public IrBorder? Left { get; set; }
}

/// <summary>表格单元格。</summary>
public sealed class IrCell
{
    public string Text { get; set; } = string.Empty;
    /// <summary>富文本段落（多段落 + 多 Run）；非空时优先于 <see cref="Text"/>。</summary>
    public List<IrParagraph> Paragraphs { get; } = new();
    /// <summary>横向跨越的列数（合并单元格），默认为 1。</summary>
    public int ColSpan { get; set; } = 1;
    /// <summary>纵向跨越的行数（合并单元格），默认为 1。</summary>
    public int RowSpan { get; set; } = 1;
    /// <summary>本单元格是否为合并块的从属（不渲染）。</summary>
    public bool Suppressed { get; set; }
    public IrCellStyle Style { get; set; } = new();
    /// <summary>Excel 数字格式化后的显示文本快照（已应用 numFmt + 条件格式颜色）。</summary>
    public string? FormattedText { get; set; }
    /// <summary>单元格超链接 URL（外部链接，非空时整个单元格作为可点击链接）。</summary>
    public string? Hyperlink { get; set; }
    /// <summary>单元格批注文本（Excel Comment / Note）。</summary>
    public string? Comment { get; set; }
}

/// <summary>表格行。</summary>
public sealed class IrRow
{
    public List<IrCell> Cells { get; } = new();
    /// <summary>行高（点 pt，可空）。</summary>
    public double? HeightPt { get; set; }
    /// <summary>是否禁止行内分页（Word w:cantSplit）。</summary>
    public bool CantSplit { get; set; }
}

/// <summary>表格。</summary>
public sealed class IrTable
{
    public List<IrRow> Rows { get; } = new();
    /// <summary>每列宽度（点 pt）；为 null 或空表示均分。</summary>
    public List<double?> ColumnWidthsPt { get; } = new();
    public int ColumnCount => Rows.Count == 0 ? ColumnWidthsPt.Count : Rows.Max(r => r.Cells.Sum(c => c.ColSpan));
    /// <summary>表头行数（QuestPDF Table.Header / Excel Print_Titles 重复）。</summary>
    public int HeaderRowCount { get; set; }
    /// <summary>Excel: 重复打印的标题行数（仅头部连续行；当 Print_Titles 行不与 used 区起始对齐时会用作"虚拟头"重复）。</summary>
    public int PrintTitleRowCount { get; set; }
    /// <summary>Excel: 重复打印的标题列数（仅头部连续列）。</summary>
    public int PrintTitleColCount { get; set; }
    /// <summary>Excel: 是否打印网格线。</summary>
    public bool PrintGridlines { get; set; }
    /// <summary>Excel: 表内人工水平分页符（按表行索引，0 起；该索引行将开启新页）。</summary>
    public List<int> HorizontalPageBreakRowIndices { get; } = new();
    /// <summary>Excel: 表内人工垂直分页符（按表列索引，0 起；该索引列将开启新页）。</summary>
    public List<int> VerticalPageBreakColIndices { get; } = new();
}

/// <summary>分页符。</summary>
public sealed class IrPageBreak { }

/// <summary>浮动形状/文本框（Word w:txbxContent / wps:txbx）。简化为含子段落与可选边框。</summary>
public sealed class IrFloatingShape
{
    public List<IrParagraph> Paragraphs { get; } = new();
    public ImageFloatMode Float { get; set; } = ImageFloatMode.Center;
    public double? WidthPx { get; set; }
    public double? HeightPx { get; set; }
    public string? BorderHex { get; set; }
    public double BorderThickness { get; set; }
    public string? BackgroundHex { get; set; }
}

/// <summary>TOC 字段占位（Word "TOC \\o" 字段）；渲染器应在此处生成目录。</summary>
public sealed class IrTocField
{
    public int MaxLevel { get; set; } = 3;
    public string? Title { get; set; }
}

/// <summary>页面方向。</summary>
public enum PageOrientation { Portrait, Landscape }

/// <summary>节级页面属性（对应 Word w:sectPr）。</summary>
public sealed class IrSectionProperties
{
    /// <summary>页宽（点 pt）。</summary>
    public double? PageWidthPt { get; set; }
    /// <summary>页高（点 pt）。</summary>
    public double? PageHeightPt { get; set; }
    public PageOrientation Orientation { get; set; } = PageOrientation.Portrait;
    public double? MarginTopPt { get; set; }
    public double? MarginBottomPt { get; set; }
    public double? MarginLeftPt { get; set; }
    public double? MarginRightPt { get; set; }
    /// <summary>页眉段落列表（富文本，可包含 PAGE/NUMPAGES 占位 Run）。</summary>
    public List<IrParagraph> HeaderParagraphs { get; } = new();
    /// <summary>页脚段落列表（富文本，可包含 PAGE/NUMPAGES 占位 Run）。</summary>
    public List<IrParagraph> FooterParagraphs { get; } = new();
    /// <summary>偶数页页眉（启用奇偶页头时；为空回退到默认）。</summary>
    public List<IrParagraph> HeaderEvenParagraphs { get; } = new();
    /// <summary>偶数页页脚。</summary>
    public List<IrParagraph> FooterEvenParagraphs { get; } = new();
    /// <summary>首页页眉（启用首页不同页头时；为空回退到默认）。</summary>
    public List<IrParagraph> HeaderFirstParagraphs { get; } = new();
    /// <summary>首页页脚。</summary>
    public List<IrParagraph> FooterFirstParagraphs { get; } = new();
    /// <summary>是否启用奇偶页不同页眉页脚。</summary>
    public bool DifferentOddEven { get; set; }
    /// <summary>是否启用首页不同页眉页脚。</summary>
    public bool DifferentFirstPage { get; set; }
    /// <summary>页眉文本（兼容旧字段；优先使用 HeaderParagraphs）。</summary>
    public string? HeaderText { get; set; }
    /// <summary>页脚文本（兼容旧字段；优先使用 FooterParagraphs）。</summary>
    public string? FooterText { get; set; }
    /// <summary>是否在节首额外起新页。</summary>
    public bool BreakBefore { get; set; }
    /// <summary>Excel: 适配宽度的页数（<c>FitToPagesWide</c>）。</summary>
    public int? FitToPagesWide { get; set; }
    /// <summary>Excel: 适配高度的页数（<c>FitToPagesTall</c>）。</summary>
    public int? FitToPagesTall { get; set; }
    /// <summary>Excel: 打印缩放比（百分比；100 = 原大小）。</summary>
    public int? Scale { get; set; }
    /// <summary>Excel: 重复打印的标题行（1 起，闭区间）。</summary>
    public (int FromRow, int ToRow)? PrintTitleRows { get; set; }
    /// <summary>Excel: 重复打印的标题列（1 起，闭区间）。</summary>
    public (int FromCol, int ToCol)? PrintTitleCols { get; set; }
    /// <summary>Excel: 人工水平分页符所在的行号（1 起）。</summary>
    public List<int> HorizontalPageBreaks { get; } = new();
    /// <summary>Excel: 人工垂直分页符所在的列号（1 起）。</summary>
    public List<int> VerticalPageBreaks { get; } = new();
    /// <summary>Excel: 是否打印网格线。</summary>
    public bool? PrintGridlines { get; set; }
    /// <summary>分栏数（Word w:cols/@num）；&lt;=1 表示单列。</summary>
    public int ColumnCount { get; set; } = 1;
    /// <summary>分栏间距（点 pt）。</summary>
    public double? ColumnSpacingPt { get; set; }
    /// <summary>页码编号格式（w:pgNumType/@fmt）：decimal / lowerLetter / upperLetter / lowerRoman / upperRoman。</summary>
    public PageNumberFormat PageNumberFormat { get; set; } = PageNumberFormat.Decimal;
    /// <summary>页码起始值（w:pgNumType/@start）。</summary>
    public int? PageNumberStart { get; set; }
}

/// <summary>页码编号格式。</summary>
public enum PageNumberFormat { Decimal, LowerLetter, UpperLetter, LowerRoman, UpperRoman }

/// <summary>文档块（多态：段落/图片/表格/分页/节）。</summary>
public sealed class IrBlock
{
    public IrParagraph? Paragraph { get; set; }
    public IrImage? Image { get; set; }
    public IrTable? Table { get; set; }
    public IrPageBreak? PageBreak { get; set; }
    public IrSectionProperties? Section { get; set; }
    public IrFloatingShape? Shape { get; set; }
    public IrTocField? TocField { get; set; }

    public static IrBlock Of(IrParagraph p) => new() { Paragraph = p };
    public static IrBlock Of(IrImage i) => new() { Image = i };
    public static IrBlock Of(IrTable t) => new() { Table = t };
    public static IrBlock Of(IrSectionProperties s) => new() { Section = s };
    public static IrBlock Of(IrFloatingShape s) => new() { Shape = s };
    public static IrBlock Of(IrTocField tf) => new() { TocField = tf };
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
    /// <summary>脚注表（按字段引用 ID 索引）。</summary>
    public Dictionary<string, IrFootnote> Footnotes { get; } = new();
    /// <summary>尾注表（按字段引用 ID 索引）。</summary>
    public Dictionary<string, IrFootnote> Endnotes { get; } = new();
    /// <summary>批注表（按 Word commentId 索引）。</summary>
    public Dictionary<string, IrComment> Comments { get; } = new();
}

/// <summary>脚注/尾注（按段落集合表示）。</summary>
public sealed class IrFootnote
{
    public string Id { get; set; } = string.Empty;
    /// <summary>显示编号（1 起；渲染器为引用上标 + 列表项使用）。</summary>
    public int Number { get; set; }
    public List<IrParagraph> Paragraphs { get; } = new();
}

/// <summary>批注（comments.xml）。</summary>
public sealed class IrComment
{
    public string Id { get; set; } = string.Empty;
    public int Number { get; set; }
    public string? Author { get; set; }
    public string? Initials { get; set; }
    public DateTime? Date { get; set; }
    public List<IrParagraph> Paragraphs { get; } = new();
}
