using SnowyRiver.Documents.Converter.Model;

namespace SnowyRiver.Documents.Converter.Engines;

/// <summary>
/// 表格分页 / 标题重复 / Excel 打印缩放的共享助手。
/// </summary>
internal static class TableLayoutHelper
{
    /// <summary>把分页索引列表转成 [start, end) 半开区间序列。</summary>
    public static List<(int start, int end)> SliceRanges(IEnumerable<int> breaks, int total)
    {
        var slices = new List<(int start, int end)>();
        int prev = 0;
        foreach (var b in breaks.Where(i => i > 0 && i < total).Distinct().OrderBy(i => i))
        {
            if (b <= prev) continue;
            slices.Add((prev, b));
            prev = b;
        }
        slices.Add((prev, total));
        return slices;
    }

    /// <summary>从原表中切出 [rowStart,rowEnd)×[colStart,colEnd)，并在切片头部重复 PrintTitle 行/列。</summary>
    public static IrTable SliceTable(IrTable src, int rowStart, int rowEnd, int colStart, int colEnd, int titleRows, int titleCols)
    {
        var dst = new IrTable
        {
            HeaderRowCount = src.HeaderRowCount,
            PrintGridlines = src.PrintGridlines,
        };

        for (int i = 0; i < titleCols && i < src.ColumnWidthsPt.Count; i++)
            dst.ColumnWidthsPt.Add(src.ColumnWidthsPt[i]);
        int colSliceStart = Math.Max(colStart, titleCols);
        for (int i = colSliceStart; i < colEnd && i < src.ColumnWidthsPt.Count; i++)
            dst.ColumnWidthsPt.Add(src.ColumnWidthsPt[i]);

        IEnumerable<int> rowIndices = (rowStart > 0 && titleRows > 0)
            ? Enumerable.Range(0, titleRows).Concat(Enumerable.Range(rowStart, rowEnd - rowStart))
            : Enumerable.Range(rowStart, rowEnd - rowStart);

        foreach (var ri in rowIndices)
        {
            if (ri < 0 || ri >= src.Rows.Count) continue;
            var srcRow = src.Rows[ri];
            var newRow = new IrRow { HeightPt = srcRow.HeightPt };
            for (int ci = 0; ci < titleCols && ci < srcRow.Cells.Count; ci++)
                newRow.Cells.Add(srcRow.Cells[ci]);
            for (int ci = colSliceStart; ci < colEnd && ci < srcRow.Cells.Count; ci++)
                newRow.Cells.Add(srcRow.Cells[ci]);
            dst.Rows.Add(newRow);
        }

        int extraTitleRows = (rowStart > 0 ? titleRows : 0);
        if (extraTitleRows > 0 && dst.HeaderRowCount < extraTitleRows)
            dst.HeaderRowCount = extraTitleRows;
        return dst;
    }

    /// <summary>
    /// 按 Excel 节属性的 Scale/FitToPagesWide 缩放表格列宽，必要时返回新副本。
    /// 不修改原表。<paramref name="contentWidthPt"/> 为页面可用宽度（除去左右边距）。
    /// </summary>
    public static IrTable ApplyExcelScaling(IrTable src, IrSectionProperties? props, double contentWidthPt)
    {
        if (props == null) return src;
        double factor = 1.0;
        if (props.Scale is { } pct && pct > 0 && pct != 100) factor *= pct / 100.0;
        // FitToPagesWide：将列总宽缩到 N 页内（仅当当前总宽 > N×可用宽时缩小）。
        double sumPt = 0;
        bool allKnown = src.ColumnWidthsPt.Count > 0 && src.ColumnWidthsPt.All(w => w.HasValue);
        if (allKnown) sumPt = src.ColumnWidthsPt.Sum(w => w!.Value);
        sumPt *= factor;
        if (allKnown && props.FitToPagesWide is { } pages && pages > 0 && contentWidthPt > 0)
        {
            double maxAllowed = pages * contentWidthPt;
            if (sumPt > maxAllowed && sumPt > 0)
                factor *= maxAllowed / sumPt;
        }
        if (Math.Abs(factor - 1.0) < 1e-6) return src;

        var dst = new IrTable
        {
            HeaderRowCount = src.HeaderRowCount,
            PrintGridlines = src.PrintGridlines,
            PrintTitleRowCount = src.PrintTitleRowCount,
            PrintTitleColCount = src.PrintTitleColCount,
        };
        foreach (var w in src.ColumnWidthsPt) dst.ColumnWidthsPt.Add(w * factor);
        foreach (var r in src.Rows) dst.Rows.Add(r);
        foreach (var i in src.HorizontalPageBreakRowIndices) dst.HorizontalPageBreakRowIndices.Add(i);
        foreach (var i in src.VerticalPageBreakColIndices) dst.VerticalPageBreakColIndices.Add(i);
        return dst;
    }
}
