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
        double factor = 1.0;
        if (props?.Scale is { } pct && pct > 0 && pct != 100) factor *= pct / 100.0;
        // FitToPagesWide：将列总宽缩到 N 页内（仅当当前总宽 > N×可用宽时缩小）。
        // 注意：早期实现要求所有列宽都 HasValue 才进行缩放，但实际表格末尾常出现未设宽度的列，
        // 一旦存在 null 列就会整张表都不缩，进而触发 QuestPDF 横向 Wrap。
        // 这里改为按"已知列宽之和"判断是否需要缩放：只要已知列总宽超出可用宽度，就按比例缩放
        // （未知列宽保持为 null，由 RenderTable 兜底）。
        bool hasAnyKnown = src.ColumnWidthsPt.Any(w => w.HasValue);
        double knownSum = hasAnyKnown ? src.ColumnWidthsPt.Where(w => w.HasValue).Sum(w => w!.Value) : 0;
        double scaledKnownSum = knownSum * factor;
        if (hasAnyKnown && props?.FitToPagesWide is { } pages && pages > 0 && contentWidthPt > 0)
        {
            double maxAllowed = pages * contentWidthPt;
            if (scaledKnownSum > maxAllowed && scaledKnownSum > 0)
                factor *= maxAllowed / scaledKnownSum;
        }

        // FitToPagesTall：按设置的总行高与可用页高估算需要纵向缩小的比例。
        // 这里只记录纵向缩放比 vFactor，最后通过缩小 IrRow.HeightPt（最小 0.5×）应用，
        // 不影响列宽因子 factor。这样可避免“缩列宽反而让行更高”的副作用。
        double vFactor = 1.0;
        if (props?.FitToPagesTall is { } pagesTall && pagesTall > 0)
        {
            double rowHeightSum = src.Rows.Where(r => r.HeightPt.HasValue).Sum(r => r.HeightPt!.Value);
            // 简化估算：扣除常见上下边距（约 72pt）后，A4 每页可用高度约 770pt；
            // 若实际页高/边距已由节属性给出，则按节属性更精确计算。
            double pageHt = props.PageHeightPt ?? 842;
            double mt = props.MarginTopPt ?? 36;
            double mb = props.MarginBottomPt ?? 36;
            double pageContentHt = Math.Max(100, pageHt - mt - mb);
            double maxAllowedHt = pagesTall * pageContentHt;
            if (rowHeightSum > maxAllowedHt && maxAllowedHt > 0)
                vFactor = Math.Max(0.5, maxAllowedHt / rowHeightSum);
        }

        // 兜底：无论 props 是否提供 Scale/FitToPagesWide，只要已知列宽之和超出单页可用宽度，
        // 就按比例进一步缩小，避免 QuestPDF 因横向放不下而抛 DocumentLayoutException(Wrap)。
        if (hasAnyKnown && contentWidthPt > 0)
        {
            double currentSum = knownSum * factor;
            if (currentSum > contentWidthPt && currentSum > 0)
                factor *= contentWidthPt / currentSum;
        }

        // 与 RenderTable 中的 MinTableColumnWidthPt 协同：渲染时每列宽度会被抬高到不小于该下限，
        // 抬高后的总宽若仍超出 contentWidthPt，则会触发 RelativeColumn 回退，列宽比例随之被破坏。
        // 这里在缩放阶段就基于"抬高后的列宽之和"再做一次收敛迭代，使最终 ConstantColumn 的总宽
        // 即便扣除最小列宽下限也能放进可用宽度内，从而尽量保留 Excel 原始列宽比例。
        if (hasAnyKnown && contentWidthPt > 0)
        {
            double minPt = PdfRenderer.MinTableColumnWidthPt;
            var widths = src.ColumnWidthsPt.Where(w => w.HasValue).Select(w => w!.Value).ToList();
            for (int iter = 0; iter < 8; iter++)
            {
                double clamped = widths.Sum(w => Math.Max(minPt, w * factor));
                if (clamped <= contentWidthPt || clamped <= 0) break;
                // 仅 max 项（即 w*factor > minPt 的列）能通过缩放来减小总宽；
                // 已经被抬高到 minPt 的列再缩小 factor 也不会变化。
                double scalable = widths.Where(w => w * factor > minPt).Sum(w => w * factor);
                double pinned = widths.Count(w => w * factor <= minPt) * minPt;
                double room = contentWidthPt - pinned;
                if (scalable <= 0 || room <= 0) break;
                double shrink = room / scalable;
                if (shrink >= 1.0 || double.IsNaN(shrink) || double.IsInfinity(shrink)) break;
                factor *= shrink;
            }
        }

        if (Math.Abs(factor - 1.0) < 1e-6 && Math.Abs(vFactor - 1.0) < 1e-6) return src;

        var dst = new IrTable
        {
            HeaderRowCount = src.HeaderRowCount,
            PrintGridlines = src.PrintGridlines,
            PrintTitleRowCount = src.PrintTitleRowCount,
            PrintTitleColCount = src.PrintTitleColCount,
        };
        foreach (var w in src.ColumnWidthsPt) dst.ColumnWidthsPt.Add(w * factor);
        if (Math.Abs(vFactor - 1.0) < 1e-6)
        {
            foreach (var r in src.Rows) dst.Rows.Add(r);
        }
        else
        {
            foreach (var r in src.Rows)
            {
                var nr = new IrRow
                {
                    HeightPt = r.HeightPt.HasValue ? r.HeightPt.Value * vFactor : r.HeightPt,
                    CantSplit = r.CantSplit,
                };
                foreach (var c in r.Cells) nr.Cells.Add(c);
                dst.Rows.Add(nr);
            }
        }
        foreach (var i in src.HorizontalPageBreakRowIndices) dst.HorizontalPageBreakRowIndices.Add(i);
        foreach (var i in src.VerticalPageBreakColIndices) dst.VerticalPageBreakColIndices.Add(i);
        return dst;
    }
}
