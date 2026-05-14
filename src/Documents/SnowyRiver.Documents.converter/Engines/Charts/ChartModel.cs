namespace SnowyRiver.Documents.Converter.Engines.Charts;

/// <summary>
/// 图表渲染所支持的种类。
/// </summary>
internal enum ChartKind
{
    Unknown,
    Column,        // 垂直柱状（簇/堆叠/百分比堆叠）
    BarHorizontal, // 水平条形（簇/堆叠/百分比堆叠）
    Line,
    Pie,
    Doughnut,
    Area,          // 面积（簇/堆叠/百分比堆叠）
    Scatter,
    Radar,         // 雷达（线 / 填充）
}

/// <summary>柱/条/面积的堆叠模式。</summary>
internal enum StackMode
{
    Clustered,
    Stacked,
    PercentStacked,
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

/// <summary>填充样式。</summary>
internal enum FillKind
{
    Solid,
    LinearGradient,
    RadialGradient,
}

/// <summary>渐变色 stop（位置 0..1，颜色 ARGB）。</summary>
internal readonly record struct GradientStop(double Position, uint ColorArgb);

/// <summary>趋势线类型。</summary>
internal enum TrendlineKind
{
    None,
    Linear,
    Polynomial,
    Logarithmic,
    Exponential,
    Power,
    MovingAverage,
}

/// <summary>趋势线参数。</summary>
internal sealed class Trendline
{
    public TrendlineKind Kind { get; set; } = TrendlineKind.None;
    /// <summary>多项式阶数 / 移动平均周期。</summary>
    public int Order { get; set; } = 2;
    /// <summary>趋势线显示颜色（ARGB），为 null 时复用所属系列颜色。</summary>
    public uint? ColorArgb { get; set; }
}

/// <summary>单个数据系列。</summary>
internal sealed class ChartSeries
{
    public string? Name { get; set; }
    /// <summary>主色 ARGB；为 null 时由调色板分配。</summary>
    public uint? ColorArgb { get; set; }
    public FillKind Fill { get; set; } = FillKind.Solid;
    /// <summary>渐变填充时的色标（>=2 个）。</summary>
    public List<GradientStop> GradientStops { get; } = new();
    /// <summary>X/分类（数值或文本）。仅散点图使用 X 数值；其它图表用 Categories。</summary>
    public List<double> XValues { get; } = new();
    public List<double> Values { get; } = new();
    /// <summary>按数据点索引覆盖的颜色（c:dPt），缺省值表示沿用系列色。</summary>
    public Dictionary<int, uint> PointColors { get; } = new();
    /// <summary>趋势线（最多一条，常见用法）。</summary>
    public Trendline? Trend { get; set; }
    /// <summary>组合图：覆盖该系列的渲染图形（用于在主 Column 上叠加 Line 等）。null 表示沿用主图类型。</summary>
    public ChartKind? OverrideKind { get; set; }
    /// <summary>是否使用副坐标轴（Y2）。仅在 axis-based 主图（Column/BarHorizontal/Line/Area）下生效。</summary>
    public bool UseSecondaryAxis { get; set; }
}

/// <summary>统一图表模型。</summary>
internal sealed class ChartData
{
    public ChartKind Kind { get; set; } = ChartKind.Unknown;
    public StackMode Stack { get; set; } = StackMode.Clustered;
    /// <summary>雷达图是否填充（c:radarStyle = filled）。</summary>
    public bool RadarFilled { get; set; }
    public string? Title { get; set; }
    public LegendPos Legend { get; set; } = LegendPos.Right;
    public List<string> Categories { get; } = new();
    public List<ChartSeries> Series { get; } = new();
    public bool ShowDataLabels { get; set; }
    public bool ShowGridLines { get; set; } = true;
    /// <summary>X 轴标题（类别/数值轴），可空。</summary>
    public string? AxisTitleX { get; set; }
    /// <summary>Y 轴标题（数值轴），可空。</summary>
    public string? AxisTitleY { get; set; }
    /// <summary>副 Y 轴标题（数值轴），可空。</summary>
    public string? AxisTitleY2 { get; set; }

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

