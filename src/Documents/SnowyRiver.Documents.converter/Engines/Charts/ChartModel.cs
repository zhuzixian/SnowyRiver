namespace SnowyRiver.Documents.Converter.Engines.Charts;

/// <summary>
/// 图表渲染所支持的种类。
/// </summary>
internal enum ChartKind
{
    Unknown,
    Column,        // 垂直柱状（簇）
    BarHorizontal, // 水平条形
    Line,
    Pie,
    Doughnut,
    Area,
    Scatter,
}

/// <summary>图例位置。</summary>
internal enum LegendPos
{
    None,
    Right,
    Bottom,
    Top,
    Left,
}

/// <summary>单个数据系列。</summary>
internal sealed class ChartSeries
{
    public string? Name { get; set; }
    /// <summary>显式 ARGB；为 null 时由调色板分配。</summary>
    public uint? ColorArgb { get; set; }
    /// <summary>X/分类（数值或文本）。仅散点图使用 X 数值；其它图表用 Categories。</summary>
    public List<double> XValues { get; } = new();
    public List<double> Values { get; } = new();
}

/// <summary>统一图表模型。</summary>
internal sealed class ChartData
{
    public ChartKind Kind { get; set; } = ChartKind.Unknown;
    public string? Title { get; set; }
    public LegendPos Legend { get; set; } = LegendPos.Right;
    public List<string> Categories { get; } = new();
    public List<ChartSeries> Series { get; } = new();
    public bool ShowDataLabels { get; set; }
    public bool ShowGridLines { get; set; } = true;

    /// <summary>原始锚点像素尺寸（用于按 EMU 换算）。</summary>
    public int PixelWidth { get; set; } = 800;
    public int PixelHeight { get; set; } = 480;
}

/// <summary>Office 默认调色板（Office 2013+ Accent 1-6）。</summary>
internal static class OfficePalette
{
    public static readonly uint[] Colors =
    {
        0xFF4472C4, // Accent 1 蓝
        0xFFED7D31, // Accent 2 橙
        0xFFA5A5A5, // Accent 3 灰
        0xFFFFC000, // Accent 4 金
        0xFF5B9BD5, // Accent 5 浅蓝
        0xFF70AD47, // Accent 6 绿
        0xFF264478,
        0xFF9E480E,
        0xFF636363,
        0xFF997300,
        0xFF255E91,
        0xFF43682B,
    };

    public static uint Get(int index) => Colors[((index % Colors.Length) + Colors.Length) % Colors.Length];
}
