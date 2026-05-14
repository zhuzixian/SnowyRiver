using SkiaSharp;
using SnowyRiver.Documents.Converter.Model;

namespace SnowyRiver.Documents.Converter.Engines;

/// <summary>
/// Excel 列宽 auto-fit：基于 SkiaSharp 的真实文字度量。
/// 仅用于 ExcelReader 已经写入了 Excel 默认列宽（基于字符宽度公式）的列：
/// 当某列存在比默认宽度更宽的内容（按真实像素度量）时，扩大该列宽度，
/// 这样 PDF/XPS 输出与 PrintScale/FitToPagesWide 缩放才能贴近 Excel 自动列宽体验。
/// </summary>
internal static class ExcelAutoFit
{
    private const double DefaultFontSize = 11.0; // 与 PdfRenderer 保持一致
    private const double PaddingPt = 6.0;        // 单元格左右内边距合计的近似值
    private const double MaxColumnPt = 360.0;    // 防止单列过宽（约 5 英寸；超过页面可用宽时仍由 FitToPagesWide/Scale 兜底）

    /// <summary>对表中所有列按可见单元格内容做 auto-fit，原地更新 <see cref="IrTable.ColumnWidthsPt"/>。</summary>
    public static void AutoFit(IrTable table, string defaultFontFamily)
    {
        if (table.Rows.Count == 0 || table.ColumnWidthsPt.Count == 0) return;

        using var typeface = SKTypeface.FromFamilyName(defaultFontFamily) ?? SKTypeface.Default;
        using var font = new SKFont(typeface, (float)DefaultFontSize);

        int colCount = table.ColumnWidthsPt.Count;
        for (int ci = 0; ci < colCount; ci++)
        {
            double measuredPt = 0;
            foreach (var row in table.Rows)
            {
                if (ci >= row.Cells.Count) continue;
                var cell = row.Cells[ci];
                if (cell.Suppressed) continue;
                if (cell.ColSpan > 1) continue; // 合并跨列：不参与单列 auto-fit
                var text = !string.IsNullOrEmpty(cell.FormattedText) ? cell.FormattedText : cell.Text;
                if (string.IsNullOrEmpty(text)) continue;

                double sizeFactor = 1.0;
                if (cell.Style.FontSize is { } fs && fs > 0)
                    sizeFactor = fs / DefaultFontSize;
                bool bold = cell.Style.Bold;

                // 按行内首段（无换行）取最长一段。换行时取各段最大值。
                double cellPt = 0;
                int start = 0;
                while (start <= text.Length)
                {
                    int nl = text.IndexOf('\n', start);
                    int end = nl < 0 ? text.Length : nl;
                    var seg = text.AsSpan(start, end - start);
                    if (!seg.IsEmpty)
                    {
                        float widthPx = font.MeasureText(seg);
                        // 96DPI: 1pt = 96/72 px → pt = px * 72/96
                        double pt = widthPx * 72.0 / 96.0 * sizeFactor;
                        if (bold) pt *= 1.06; // 粗体近似加宽
                        if (pt > cellPt) cellPt = pt;
                    }
                    if (nl < 0) break;
                    start = nl + 1;
                }
                if (cellPt > measuredPt) measuredPt = cellPt;
            }

            double current = table.ColumnWidthsPt[ci] ?? 0;
            double target = measuredPt + PaddingPt;
            if (target > MaxColumnPt) target = MaxColumnPt;
            if (target > current)
                table.ColumnWidthsPt[ci] = target;
        }
    }
}
