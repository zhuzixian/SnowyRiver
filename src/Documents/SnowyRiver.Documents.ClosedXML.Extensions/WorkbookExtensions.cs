using ClosedXML.Excel;

namespace SnowyRiver.Documents.ClosedXML.Extensions;

/// <summary>
/// 为 ClosedXML 提供扩展方法
/// </summary>
public static class WorkbookExtensions
{
    /// <summary>
    /// IXLWorkbook 的扩展方法
    /// </summary>
    extension(IXLWorkbook workbook)
    {
        /// <summary>
        /// 替换工作簿中所有匹配的单元格值
        /// </summary>
        /// <param name="sourceValue">要查找的源值</param>
        /// <param name="newValue">要替换的新值</param>
        /// <exception cref="ArgumentNullException">当 sourceValue 为 null 时抛出</exception>
        public void ReplaceCellsValue(string sourceValue, XLCellValue newValue)
        {
            ArgumentNullException.ThrowIfNull(sourceValue);

            var cells = workbook.FindCells(x =>
                x.TryGetValue<string>(out var cellValue) && cellValue == sourceValue);

            foreach (var cell in cells)
            {
                cell.SetValue(newValue);
            }
        }

        /// <summary>
        /// 批量替换工作簿中的多个值
        /// </summary>
        /// <param name="replacements">键值对字典，键为要查找的值，值为要替换的值</param>
        /// <exception cref="ArgumentNullException">当 replacements 为 null 时抛出</exception>
        public void ReplaceCellsValue(Dictionary<string, XLCellValue> replacements)
        {
            ArgumentNullException.ThrowIfNull(replacements);

            foreach (var (sourceValue, newValue) in replacements)
            {
                workbook.ReplaceCellsValue(sourceValue, newValue);
            }
        }

        /// <summary>
        /// 获取工作簿中所有包含指定文本的单元格
        /// </summary>
        /// <param name="searchText">要搜索的文本</param>
        /// <param name="ignoreCase">是否忽略大小写，默认为 false</param>
        /// <returns>包含指定文本的单元格集合</returns>
        /// <exception cref="ArgumentNullException">当 searchText 为 null 时抛出</exception>
        public IEnumerable<IXLCell> FindCellsContaining(string searchText, bool ignoreCase = false)
        {
            ArgumentNullException.ThrowIfNull(searchText);

            var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            return workbook.FindCells(x =>
                x.TryGetValue<string>(out var cellValue) && 
                cellValue?.Contains(searchText, comparison) == true);
        }

        /// <summary>
        /// 清除工作簿中所有工作表的内容（保留格式）
        /// </summary>
        public void ClearAllContents()
        {
            foreach (var worksheet in workbook.Worksheets)
            {
                worksheet.Clear(XLClearOptions.Contents);
            }
        }

        /// <summary>
        /// 获取工作簿中所有使用的单元格范围
        /// </summary>
        /// <returns>所有已使用单元格的范围字典，键为工作表名称</returns>
        public Dictionary<string, IXLRange?> GetAllUsedRanges()
        {
            return workbook.Worksheets.ToDictionary(
                ws => ws.Name,
                ws => ws.RangeUsed());
        }

        /// <summary>
        /// 获取工作簿中所有工作表的名称
        /// </summary>
        /// <returns>工作表名称集合</returns>
        public IEnumerable<string> GetWorksheetNames()
        {
            return workbook.Worksheets.Select(ws => ws.Name);
        }

        /// <summary>
        /// 保存工作簿到内存流
        /// </summary>
        /// <returns>包含工作簿数据的内存流</returns>
        public MemoryStream SaveToStream()
        {
            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;
            return stream;
        }

        /// <summary>
        /// 批量添加多个工作表
        /// </summary>
        /// <param name="sheetNames">工作表名称集合</param>
        /// <returns>新创建的工作表集合</returns>
        public IEnumerable<IXLWorksheet> AddWorksheets(IEnumerable<string> sheetNames)
        {
            ArgumentNullException.ThrowIfNull(sheetNames);

            var worksheets = new List<IXLWorksheet>();
            foreach (var name in sheetNames)
            {
                worksheets.Add(workbook.Worksheets.Add(name));
            }
            return worksheets;
        }

        /// <summary>
        /// 删除所有空白工作表
        /// </summary>
        /// <returns>删除的工作表数量</returns>
        public int DeleteEmptyWorksheets()
        {
            var emptySheets = workbook.Worksheets
                .Where(ws => ws.RangeUsed() == null)
                .ToList();

            foreach (var sheet in emptySheets)
            {
                sheet.Delete();
            }

            return emptySheets.Count;
        }

        /// <summary>
        /// 检查工作表名称是否存在
        /// </summary>
        /// <param name="worksheetName">工作表名称</param>
        /// <returns>如果存在返回 true，否则返回 false</returns>
        public bool WorksheetExists(string worksheetName)
        {
            ArgumentNullException.ThrowIfNull(worksheetName);
            return workbook.Worksheets.Any(ws => ws.Name.Equals(worksheetName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 获取或创建工作表（如果不存在则创建）
        /// </summary>
        /// <param name="worksheetName">工作表名称</param>
        /// <returns>工作表对象</returns>
        public IXLWorksheet GetOrAddWorksheet(string worksheetName)
        {
            ArgumentNullException.ThrowIfNull(worksheetName);

            return workbook.Worksheets.TryGetWorksheet(worksheetName, out var worksheet)
                ? worksheet
                : workbook.Worksheets.Add(worksheetName);
        }

        /// <summary>
        /// 设置工作簿属性
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="subject">主题</param>
        /// <param name="author">作者</param>
        /// <param name="company">公司</param>
        public void SetProperties(string? title = null, string? subject = null, string? author = null, string? company = null)
        {
            if (title != null) workbook.Properties.Title = title;
            if (subject != null) workbook.Properties.Subject = subject;
            if (author != null) workbook.Properties.Author = author;
            if (company != null) workbook.Properties.Company = company;
        }
    }

    /// <summary>
    /// IXLDefinedName 的扩展方法
    /// </summary>
    extension(IXLDefinedName definedName)
    {
        /// <summary>
        /// 获取定义名称区域的宽度（以像素为单位）
        /// </summary>
        /// <param name="mdw">最大数字宽度（Maximum Digit Width）</param>
        /// <returns>区域宽度（像素）</returns>
        /// <exception cref="ArgumentOutOfRangeException">当 mdw 小于或等于 0 时抛出</exception>
        public double GetWidthPixels(int mdw)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(mdw);

            var rangeWidth = definedName.Ranges
                .Sum(range => range.Columns()
                    .Sum(column => column.Worksheet
                        .Column(column.ColumnNumber())
                        .Width));

            return WidthToPixels(rangeWidth, mdw);
        }

        /// <summary>
        /// 获取定义名称区域的高度（以像素为单位）
        /// </summary>
        /// <param name="dpi">每英寸点数（Dots Per Inch）</param>
        /// <returns>区域高度（像素）</returns>
        /// <exception cref="ArgumentOutOfRangeException">当 dpi 小于或等于 0 时抛出</exception>
        public double GetHeightPixels(double dpi)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(dpi);

            var rangeHeight = definedName.Ranges
                .Sum(range => range.Rows()
                    .Sum(row => row.Worksheet
                        .Row(row.RowNumber())
                        .Height));

            return PointsToPixels(rangeHeight, dpi);
        }

        /// <summary>
        /// 获取定义名称引用的所有单元格
        /// </summary>
        /// <returns>所有引用的单元格集合</returns>
        public IEnumerable<IXLCell> GetAllCells()
        {
            return definedName.Ranges.SelectMany(range => range.Cells());
        }

        /// <summary>
        /// 判断定义名称是否为空（不包含任何单元格）
        /// </summary>
        /// <returns>如果为空返回 true，否则返回 false</returns>
        public bool IsEmpty()
        {
            return !definedName.Ranges.Any() || 
                   !definedName.Ranges.Any(range => range.CellsUsed().Any());
        }
    }

    /// <summary>
    /// IXLWorksheet 的扩展方法
    /// </summary>
    extension(IXLWorksheet worksheet)
    {
        /// <summary>
        /// 获取工作表的所有非空单元格
        /// </summary>
        /// <returns>非空单元格集合</returns>
        public IEnumerable<IXLCell> GetNonEmptyCells()
        {
            return worksheet.CellsUsed(XLCellsUsedOptions.Contents);
        }

        /// <summary>
        /// 批量设置单元格值
        /// </summary>
        /// <param name="cellValues">单元格地址与值的字典（例如："A1" -> "值"）</param>
        /// <exception cref="ArgumentNullException">当 cellValues 为 null 时抛出</exception>
        public void SetCellValues(Dictionary<string, object> cellValues)
        {
            ArgumentNullException.ThrowIfNull(cellValues);

            foreach (var (address, value) in cellValues)
            {
                var cell = worksheet.Cell(address);
                cell.Value = value switch
                {
                    string strValue => strValue,
                    double doubleValue => doubleValue,
                    int intValue => intValue,
                    DateTime dateValue => dateValue,
                    bool boolValue => boolValue,
                    _ => value?.ToString() ?? string.Empty
                };
            }
        }

        /// <summary>
        /// 自动调整所有列宽以适应内容
        /// </summary>
        /// <param name="minWidth">最小列宽，默认为 8</param>
        /// <param name="maxWidth">最大列宽，默认为 50</param>
        public void AutoFitAllColumns(double minWidth = 8d, double maxWidth = 50d)
        {
            worksheet.Columns().AdjustToContents(minWidth, maxWidth);
        }

        /// <summary>
        /// 冻结首行
        /// </summary>
        public void FreezeTopRow()
        {
            worksheet.SheetView.FreezeRows(1);
        }

        /// <summary>
        /// 冻结首列
        /// </summary>
        public void FreezeFirstColumn()
        {
            worksheet.SheetView.FreezeColumns(1);
        }

        /// <summary>
        /// 冻结窗格（左上角的行列）
        /// </summary>
        /// <param name="rows">要冻结的行数</param>
        /// <param name="columns">要冻结的列数</param>
        public void FreezePanes(int rows, int columns)
        {
            if (rows > 0) worksheet.SheetView.FreezeRows(rows);
            if (columns > 0) worksheet.SheetView.FreezeColumns(columns);
        }

        /// <summary>
        /// 为工作表添加自动筛选
        /// </summary>
        /// <param name="range">要应用筛选的范围，如果为 null 则应用于已使用的范围</param>
        public void AddAutoFilter(IXLRange? range = null)
        {
            var targetRange = range ?? worksheet.RangeUsed();
            targetRange?.SetAutoFilter();
        }

        /// <summary>
        /// 清除工作表中的所有筛选
        /// </summary>
        public void ClearAutoFilter()
        {
            if (worksheet.AutoFilter.IsEnabled)
            {
                worksheet.AutoFilter.Clear();
            }
        }

        /// <summary>
        /// 获取工作表中所有包含公式的单元格
        /// </summary>
        /// <returns>包含公式的单元格集合</returns>
        public IEnumerable<IXLCell> GetCellsWithFormulas()
        {
            return worksheet.CellsUsed().Where(cell => cell.HasFormula);
        }

        /// <summary>
        /// 将范围转换为 Excel 表格
        /// </summary>
        /// <param name="range">要转换的范围</param>
        /// <param name="tableName">表格名称，如果为 null 则自动生成</param>
        /// <returns>创建的表格对象</returns>
        public IXLTable CreateTable(IXLRange range, string? tableName = null)
        {
            ArgumentNullException.ThrowIfNull(range);
            return tableName != null ? range.CreateTable(tableName) : range.CreateTable();
        }

        /// <summary>
        /// 复制当前工作表到同一工作簿
        /// </summary>
        /// <param name="newSheetName">新工作表的名称</param>
        /// <returns>复制的新工作表</returns>
        public IXLWorksheet CopyTo(string newSheetName)
        {
            ArgumentNullException.ThrowIfNull(newSheetName);
            return worksheet.CopyTo(newSheetName);
        }

        /// <summary>
        /// 获取工作表中指定列的所有值
        /// </summary>
        /// <param name="columnLetter">列字母（如 "A", "B"）</param>
        /// <param name="skipEmpty">是否跳过空单元格，默认为 true</param>
        /// <returns>列值集合</returns>
        public IEnumerable<object> GetColumnValues(string columnLetter, bool skipEmpty = true)
        {
            ArgumentNullException.ThrowIfNull(columnLetter);

            var column = worksheet.Column(columnLetter);
            var cells = skipEmpty ? column.CellsUsed() : column.Cells();

            return cells.Select(cell => (object)cell.Value);
        }

        /// <summary>
        /// 查找并替换工作表中的文本
        /// </summary>
        /// <param name="searchText">要查找的文本</param>
        /// <param name="replaceText">替换的文本</param>
        /// <param name="matchCase">是否区分大小写，默认为 false</param>
        /// <returns>替换的单元格数量</returns>
        public int FindAndReplace(string searchText, string replaceText, bool matchCase = false)
        {
            ArgumentNullException.ThrowIfNull(searchText);
            ArgumentNullException.ThrowIfNull(replaceText);

            var comparison = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            var cells = worksheet.CellsUsed().Where(cell =>
                cell.TryGetValue<string>(out var value) && 
                value?.Contains(searchText, comparison) == true);

            int count = 0;
            foreach (var cell in cells)
            {
                if (cell.TryGetValue<string>(out var value))
                {
                    cell.Value = value.Replace(searchText, replaceText, comparison);
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// 保护工作表（防止编辑）
        /// </summary>
        /// <param name="password">密码，如果为 null 则不设置密码</param>
        public void Protect(string? password = null)
        {
            if (password != null)
                worksheet.Protect(password);
            else
                worksheet.Protect();
        }

        /// <summary>
        /// 解除工作表保护
        /// </summary>
        /// <param name="password">密码，如果设置了密码则需要提供</param>
        public void Unprotect(string? password = null)
        {
            if (password != null)
                worksheet.Unprotect(password);
            else
                worksheet.Unprotect();
        }

        /// <summary>
        /// 删除所有空行
        /// </summary>
        /// <returns>删除的行数</returns>
        public int DeleteEmptyRows()
        {
            var emptyRows = worksheet.Rows()
                .Where(row => !row.CellsUsed().Any())
                .ToList();

            foreach (var row in emptyRows)
            {
                row.Delete();
            }

            return emptyRows.Count;
        }

        /// <summary>
        /// 删除所有空列
        /// </summary>
        /// <returns>删除的列数</returns>
        public int DeleteEmptyColumns()
        {
            var emptyColumns = worksheet.Columns()
                .Where(col => !col.CellsUsed().Any())
                .ToList();

            foreach (var col in emptyColumns)
            {
                col.Delete();
            }

            return emptyColumns.Count;
        }

        /// <summary>
        /// 插入新行
        /// </summary>
        /// <param name="rowNumber">要插入的行号</param>
        /// <param name="numberOfRows">插入的行数，默认为 1</param>
        public void InsertRows(int rowNumber, int numberOfRows = 1)
        {
            worksheet.Row(rowNumber).InsertRowsAbove(numberOfRows);
        }

        /// <summary>
        /// 插入新列
        /// </summary>
        /// <param name="columnNumber">要插入的列号</param>
        /// <param name="numberOfColumns">插入的列数，默认为 1</param>
        public void InsertColumns(int columnNumber, int numberOfColumns = 1)
        {
            worksheet.Column(columnNumber).InsertColumnsBefore(numberOfColumns);
        }

        /// <summary>
        /// 对范围进行排序
        /// </summary>
        /// <param name="range">要排序的范围</param>
        /// <param name="columnToSortBy">排序依据的列号</param>
        /// <param name="ascending">是否升序排序，默认为 true</param>
        public void SortRange(IXLRange range, int columnToSortBy, bool ascending = true)
        {
            ArgumentNullException.ThrowIfNull(range);

            if (ascending)
                range.Sort(columnToSortBy);
            else
                range.SortLeftToRight(XLSortOrder.Descending);
        }

        /// <summary>
        /// 设置打印区域
        /// </summary>
        /// <param name="range">打印区域范围</param>
        public void SetPrintArea(IXLRange range)
        {
            ArgumentNullException.ThrowIfNull(range);
            worksheet.PageSetup.PrintAreas.Clear();
            worksheet.PageSetup.PrintAreas.Add(range.RangeAddress.ToString());
        }

        /// <summary>
        /// 设置打印方向
        /// </summary>
        /// <param name="landscape">是否横向打印，默认为 false（纵向）</param>
        public void SetPageOrientation(bool landscape = false)
        {
            worksheet.PageSetup.PageOrientation = landscape 
                ? XLPageOrientation.Landscape 
                : XLPageOrientation.Portrait;
        }

        /// <summary>
        /// 获取工作表的行数（包含数据的最大行号）
        /// </summary>
        /// <returns>行数</returns>
        public int GetRowCount()
        {
            return worksheet.RangeUsed()?.LastRow().RowNumber() ?? 0;
        }

        /// <summary>
        /// 获取工作表的列数（包含数据的最大列号）
        /// </summary>
        /// <returns>列数</returns>
        public int GetColumnCount()
        {
            return worksheet.RangeUsed()?.LastColumn().ColumnNumber() ?? 0;
        }

        /// <summary>
        /// 导出工作表为 CSV 格式的字符串
        /// </summary>
        /// <param name="delimiter">分隔符，默认为逗号</param>
        /// <param name="includeHeaders">是否包含表头，默认为 true</param>
        /// <returns>CSV 格式的字符串</returns>
        public string ExportToCsv(string delimiter = ",", bool includeHeaders = true)
        {
            var range = worksheet.RangeUsed();
            if (range == null) return string.Empty;

            var sb = new System.Text.StringBuilder();
            var startRow = includeHeaders ? range.FirstRow().RowNumber() : range.FirstRow().RowNumber() + 1;

            for (var row = startRow; row <= range.LastRow().RowNumber(); row++)
            {
                var values = new List<string>();
                for (var col = range.FirstColumn().ColumnNumber(); col <= range.LastColumn().ColumnNumber(); col++)
                {
                    var cellValue = worksheet.Cell(row, col).GetStringOrDefault();
                    // 处理包含分隔符或换行的值
                    if (cellValue.Contains(delimiter) || cellValue.Contains('\n') || cellValue.Contains('"'))
                    {
                        cellValue = $"\"{cellValue.Replace("\"", "\"\"")}\"";
                    }
                    values.Add(cellValue);
                }
                sb.AppendLine(string.Join(delimiter, values));
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// IXLRange 的扩展方法
    /// </summary>
    extension(IXLRange range)
    {
        /// <summary>
        /// 应用标准表格样式（带表头）
        /// </summary>
        /// <param name="headerBackgroundColor">表头背景色，默认为深蓝色</param>
        /// <param name="headerFontColor">表头字体色，默认为白色</param>
        public void ApplyTableStyle(XLColor? headerBackgroundColor = null, XLColor? headerFontColor = null)
        {
            var headerBg = headerBackgroundColor ?? XLColor.FromArgb(0x44, 0x72, 0xC4);
            var headerFont = headerFontColor ?? XLColor.White;

            // 设置表头样式
            var firstRow = range.FirstRow();
            firstRow.Style.Fill.BackgroundColor = headerBg;
            firstRow.Style.Font.FontColor = headerFont;
            firstRow.Style.Font.Bold = true;
            firstRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // 设置边框
            range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // 交替行颜色
            bool isEvenRow = false;
            foreach (var row in range.Rows().Skip(1))
            {
                if (isEvenRow)
                {
                    row.Style.Fill.BackgroundColor = XLColor.FromArgb(0xDD, 0xEB, 0xF7);
                }
                isEvenRow = !isEvenRow;
            }
        }

        /// <summary>
        /// 为范围添加数据验证（下拉列表）
        /// </summary>
        /// <param name="listValues">下拉列表的值</param>
        /// <exception cref="ArgumentNullException">当 listValues 为 null 时抛出</exception>
        public void AddDropdownValidation(IEnumerable<string> listValues)
        {
            ArgumentNullException.ThrowIfNull(listValues);

            var valuesList = string.Join(",", listValues.Select(v => $"\"{v}\""));
            range.GetDataValidation().List(valuesList);
        }

        /// <summary>
        /// 获取范围内所有唯一值
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <returns>唯一值集合</returns>
        public IEnumerable<T> GetUniqueValues<T>()
        {
            return range.CellsUsed()
                .Where(cell => cell.TryGetValue<T>(out _))
                .Select(cell => cell.GetValue<T>())
                .Distinct();
        }

        /// <summary>
        /// 批量设置范围内的单元格样式
        /// </summary>
        /// <param name="styleAction">样式配置操作</param>
        public void ApplyStyle(Action<IXLStyle> styleAction)
        {
            ArgumentNullException.ThrowIfNull(styleAction);
            styleAction(range.Style);
        }

        /// <summary>
        /// 将范围导出为二维数组
        /// </summary>
        /// <returns>二维对象数组</returns>
        public object[,] ToArray()
        {
            var rowCount = range.RowCount();
            var colCount = range.ColumnCount();
            var result = new object[rowCount, colCount];

            var rowIndex = 0;
            foreach (var row in range.Rows())
            {
                var colIndex = 0;
                foreach (var cell in row.Cells())
                {
                    result[rowIndex, colIndex] = cell.Value;
                    colIndex++;
                }
                rowIndex++;
            }

            return result;
        }

        /// <summary>
        /// 从二维数组填充范围
        /// </summary>
        /// <param name="data">二维数据数组</param>
        public void FromArray(object[,] data)
        {
            ArgumentNullException.ThrowIfNull(data);

            var rowCount = data.GetLength(0);
            var colCount = data.GetLength(1);

            var firstCell = range.FirstCell();
            var targetRange = firstCell.Worksheet.Range(
                firstCell.Address.RowNumber,
                firstCell.Address.ColumnNumber,
                firstCell.Address.RowNumber + rowCount - 1,
                firstCell.Address.ColumnNumber + colCount - 1);

            var rowIndex = 0;
            foreach (var row in targetRange.Rows())
            {
                var colIndex = 0;
                foreach (var cell in row.Cells())
                {
                    if (rowIndex < rowCount && colIndex < colCount)
                    {
                        var value = data[rowIndex, colIndex];
                        cell.Value = value switch
                        {
                            string strValue => strValue,
                            double doubleValue => doubleValue,
                            int intValue => intValue,
                            DateTime dateValue => dateValue,
                            bool boolValue => boolValue,
                            _ => value?.ToString() ?? string.Empty
                        };
                    }
                    colIndex++;
                }
                rowIndex++;
            }
        }

        /// <summary>
        /// 清除范围内的格式但保留内容
        /// </summary>
        public void ClearFormatting()
        {
            range.Style.Fill.BackgroundColor = XLColor.NoColor;
            range.Style.Font.FontColor = XLColor.Black;
            range.Style.Font.Bold = false;
            range.Style.Font.Italic = false;
            range.Style.Font.Underline = XLFontUnderlineValues.None;
            range.Style.Border.OutsideBorder = XLBorderStyleValues.None;
            range.Style.Border.InsideBorder = XLBorderStyleValues.None;
            range.Style.NumberFormat.Format = "General";
        }

        /// <summary>
        /// 检查范围是否为空（所有单元格都没有值）
        /// </summary>
        /// <returns>如果范围为空返回 true，否则返回 false</returns>
        public bool IsEmpty()
        {
            return !range.CellsUsed(XLCellsUsedOptions.Contents).Any();
        }

        /// <summary>
        /// 对范围按指定列排序
        /// </summary>
        /// <param name="columnIndex">排序依据的列索引（相对于范围的第一列，从 1 开始）</param>
        /// <param name="ascending">是否升序排序，默认为 true</param>
        public void SortByColumn(int columnIndex = 1, bool ascending = true)
        {
            var sortOrder = ascending ? XLSortOrder.Ascending : XLSortOrder.Descending;
            range.Sort(columnIndex, sortOrder);
        }

        /// <summary>
        /// 转置范围（行列互换）
        /// </summary>
        /// <returns>转置后的数据（二维数组）</returns>
        public object[,] Transpose()
        {
            var rowCount = range.RowCount();
            var colCount = range.ColumnCount();
            var result = new object[colCount, rowCount];

            int rowIndex = 0;
            foreach (var row in range.Rows())
            {
                int colIndex = 0;
                foreach (var cell in row.Cells())
                {
                    result[colIndex, rowIndex] = cell.Value;
                    colIndex++;
                }
                rowIndex++;
            }

            return result;
        }

        /// <summary>
        /// 获取范围内的统计信息
        /// </summary>
        /// <returns>包含统计信息的字典</returns>
        public Dictionary<string, double> GetStatistics()
        {
            var numbers = range.CellsUsed()
                .Where(cell => cell.TryGetValue<double>(out _))
                .Select(cell => cell.GetValue<double>())
                .ToList();

            if (!numbers.Any())
                return new Dictionary<string, double>();

            return new Dictionary<string, double>
            {
                ["Count"] = numbers.Count,
                ["Sum"] = numbers.Sum(),
                ["Average"] = numbers.Average(),
                ["Min"] = numbers.Min(),
                ["Max"] = numbers.Max(),
                ["Median"] = GetMedian(numbers)
            };
        }

        /// <summary>
        /// 应用条件格式（高亮大于指定值的单元格）
        /// </summary>
        /// <param name="threshold">阈值</param>
        /// <param name="color">高亮颜色，默认为红色</param>
        public void HighlightGreaterThan(double threshold, XLColor? color = null)
        {
            var highlightColor = color ?? XLColor.Red;

            foreach (var cell in range.CellsUsed())
            {
                if (cell.TryGetValue<double>(out var value) && value > threshold)
                {
                    cell.Style.Fill.BackgroundColor = highlightColor;
                }
            }
        }

        /// <summary>
        /// 应用条件格式（高亮小于指定值的单元格）
        /// </summary>
        /// <param name="threshold">阈值</param>
        /// <param name="color">高亮颜色，默认为蓝色</param>
        public void HighlightLessThan(double threshold, XLColor? color = null)
        {
            var highlightColor = color ?? XLColor.Blue;

            foreach (var cell in range.CellsUsed())
            {
                if (cell.TryGetValue<double>(out var value) && value < threshold)
                {
                    cell.Style.Fill.BackgroundColor = highlightColor;
                }
            }
        }

        /// <summary>
        /// 合并当前范围的所有单元格
        /// </summary>
        public void Merge()
        {
            range.Merge();
        }

        /// <summary>
        /// 取消合并当前范围
        /// </summary>
        public void Unmerge()
        {
            range.Unmerge();
        }

        /// <summary>
        /// 设置范围内所有单元格的文本对齐方式
        /// </summary>
        /// <param name="horizontal">水平对齐方式</param>
        /// <param name="vertical">垂直对齐方式</param>
        public void SetAlignment(XLAlignmentHorizontalValues horizontal, XLAlignmentVerticalValues vertical = XLAlignmentVerticalValues.Center)
        {
            range.Style.Alignment.Horizontal = horizontal;
            range.Style.Alignment.Vertical = vertical;
        }

        /// <summary>
        /// 设置范围内所有单元格的字体
        /// </summary>
        /// <param name="fontName">字体名称</param>
        /// <param name="fontSize">字体大小</param>
        /// <param name="bold">是否粗体，默认为 false</param>
        public void SetFont(string fontName, double fontSize, bool bold = false)
        {
            ArgumentNullException.ThrowIfNull(fontName);

            range.Style.Font.FontName = fontName;
            range.Style.Font.FontSize = fontSize;
            range.Style.Font.Bold = bold;
        }

        /// <summary>
        /// 计算中位数（辅助方法）
        /// </summary>
        private static double GetMedian(List<double> numbers)
        {
            var sorted = numbers.OrderBy(n => n).ToList();
            var count = sorted.Count;

            if (count % 2 == 0)
                return (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;
            else
                return sorted[count / 2];
        }
    }

    /// <summary>
    /// IXLCell 的扩展方法
    /// </summary>
    extension(IXLCell cell)
    {
        /// <summary>
        /// 安全获取单元格的字符串值（如果为空或错误则返回默认值）
        /// </summary>
        /// <param name="defaultValue">默认值，默认为空字符串</param>
        /// <returns>单元格的字符串值或默认值</returns>
        public string GetStringOrDefault(string defaultValue = "")
        {
            return cell.TryGetValue<string>(out var value) ? value : defaultValue;
        }

        /// <summary>
        /// 安全获取单元格的数值（如果为空或错误则返回默认值）
        /// </summary>
        /// <param name="defaultValue">默认值，默认为 0</param>
        /// <returns>单元格的数值或默认值</returns>
        public double GetNumberOrDefault(double defaultValue = 0d)
        {
            return cell.TryGetValue<double>(out var value) ? value : defaultValue;
        }

        /// <summary>
        /// 安全获取单元格的日期时间值（如果为空或错误则返回默认值）
        /// </summary>
        /// <param name="defaultValue">默认值，如果为 null 则返回当前时间</param>
        /// <returns>单元格的日期时间值或默认值</returns>
        public DateTime GetDateTimeOrDefault(DateTime? defaultValue = null)
        {
            return cell.TryGetValue<DateTime>(out var value) 
                ? value 
                : (defaultValue ?? DateTime.Now);
        }

        /// <summary>
        /// 设置单元格为超链接
        /// </summary>
        /// <param name="url">链接地址</param>
        /// <param name="displayText">显示文本，如果为 null 则使用 URL</param>
        public void SetHyperlink(string url, string? displayText = null)
        {
            ArgumentNullException.ThrowIfNull(url);

            cell.Value = displayText ?? url;
            cell.SetHyperlink(new XLHyperlink(url));
            cell.Style.Font.FontColor = XLColor.Blue;
            cell.Style.Font.Underline = XLFontUnderlineValues.Single;
        }

        /// <summary>
        /// 设置单元格为百分比格式
        /// </summary>
        /// <param name="decimalPlaces">小数位数，默认为 2</param>
        public void SetAsPercentage(int decimalPlaces = 2)
        {
            cell.Style.NumberFormat.Format = $"0.{new string('0', decimalPlaces)}%";
        }

        /// <summary>
        /// 设置单元格为货币格式
        /// </summary>
        /// <param name="currencySymbol">货币符号，默认为 ¥</param>
        /// <param name="decimalPlaces">小数位数，默认为 2</param>
        public void SetAsCurrency(string currencySymbol = "¥", int decimalPlaces = 2)
        {
            var format = $"{currencySymbol}#,##0.{new string('0', decimalPlaces)}";
            cell.Style.NumberFormat.Format = format;
        }

        /// <summary>
        /// 设置单元格为日期格式
        /// </summary>
        /// <param name="format">日期格式，默认为 yyyy-MM-dd</param>
        public void SetAsDate(string format = "yyyy-MM-dd")
        {
            cell.Style.NumberFormat.Format = format;
        }

        /// <summary>
        /// 设置单元格背景高亮
        /// </summary>
        /// <param name="color">高亮颜色，如果为 null 则使用黄色</param>
        public void Highlight(XLColor? color = null)
        {
            cell.Style.Fill.BackgroundColor = color ?? XLColor.Yellow;
        }

        /// <summary>
        /// 检查单元格是否包含错误
        /// </summary>
        /// <returns>如果单元格包含错误返回 true，否则返回 false</returns>
        public bool HasError()
        {
            return cell is { HasFormula: true, Value.IsError: true };
        }

        /// <summary>
        /// 为单元格添加注释
        /// </summary>
        /// <param name="commentText">注释文本</param>
        /// <param name="author">注释作者，默认为空</param>
        public void AddComment(string commentText, string author = "")
        {
            ArgumentNullException.ThrowIfNull(commentText);

            var comment = cell.CreateComment();
            comment.AddText(commentText);
            if (!string.IsNullOrWhiteSpace(author))
            {
                comment.Author = author;
            }
        }

        /// <summary>
        /// 获取相邻的上方单元格
        /// </summary>
        /// <param name="offset">偏移量，默认为 1（上一行）</param>
        /// <returns>上方的单元格</returns>
        public IXLCell GetCellAbove(int offset = 1)
        {
            return cell.CellAbove(offset);
        }

        /// <summary>
        /// 获取相邻的下方单元格
        /// </summary>
        /// <param name="offset">偏移量，默认为 1（下一行）</param>
        /// <returns>下方的单元格</returns>
        public IXLCell GetCellBelow(int offset = 1)
        {
            return cell.CellBelow(offset);
        }

        /// <summary>
        /// 获取相邻的左侧单元格
        /// </summary>
        /// <param name="offset">偏移量，默认为 1（左一列）</param>
        /// <returns>左侧的单元格</returns>
        public IXLCell GetCellLeft(int offset = 1)
        {
            return cell.CellLeft(offset);
        }

        /// <summary>
        /// 获取相邻的右侧单元格
        /// </summary>
        /// <param name="offset">偏移量，默认为 1（右一列）</param>
        /// <returns>右侧的单元格</returns>
        public IXLCell GetCellRight(int offset = 1)
        {
            return cell.CellRight(offset);
        }

        /// <summary>
        /// 仅清除单元格内容（保留格式）
        /// </summary>
        public void ClearContent()
        {
            cell.Clear(XLClearOptions.Contents);
        }

        /// <summary>
        /// 复制单元格到另一个单元格
        /// </summary>
        /// <param name="targetCell">目标单元格</param>
        /// <param name="copyFormulas">是否复制公式，默认为 true</param>
        public void CopyTo(IXLCell targetCell, bool copyFormulas = true)
        {
            ArgumentNullException.ThrowIfNull(targetCell);

            if (copyFormulas && cell.HasFormula)
            {
                targetCell.FormulaA1 = cell.FormulaA1;
            }
            else
            {
                targetCell.Value = cell.Value;
            }

            targetCell.Style = cell.Style;
        }

        /// <summary>
        /// 设置单元格的富文本格式
        /// </summary>
        /// <param name="text">文本内容</param>
        /// <param name="bold">是否粗体</param>
        /// <param name="color">文本颜色</param>
        public void SetRichText(string text, bool bold = false, XLColor? color = null)
        {
            ArgumentNullException.ThrowIfNull(text);

            var richText = cell.CreateRichText();
            var textPart = richText.AddText(text);
            textPart.Bold = bold;
            if (color != null && color.HasValue)
            {
                textPart.FontColor = color;
            }
        }

        /// <summary>
        /// 获取单元格的地址（如 "A1"）
        /// </summary>
        /// <returns>单元格地址字符串</returns>
        public string? GetAddress()
        {
            return cell.Address.ToString();
        }
    }

    /// <summary>
    /// IXLColumn 的扩展方法
    /// </summary>
    extension(IXLColumn column)
    {
        /// <summary>
        /// 自动调整列宽
        /// </summary>
        /// <param name="minWidth">最小宽度</param>
        /// <param name="maxWidth">最大宽度</param>
        public void AutoFit(double minWidth = 8d, double maxWidth = 50d)
        {
            column.AdjustToContents(minWidth, maxWidth);
        }

        /// <summary>
        /// 获取列中的所有非空值
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <returns>非空值集合</returns>
        public IEnumerable<T> GetColumnValues<T>()
        {
            return column.CellsUsed()
                .Where(cell => cell.TryGetValue<T>(out _))
                .Select(cell => cell.GetValue<T>());
        }

        /// <summary>
        /// 获取列的唯一值
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <returns>唯一值集合</returns>
        public IEnumerable<T> GetUniqueValues<T>()
        {
            return column.GetColumnValues<T>().Distinct();
        }
    }

    /// <summary>
    /// IXLRow 的扩展方法
    /// </summary>
    extension(IXLRow row)
    {
        /// <summary>
        /// 获取行中的所有非空值
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <returns>非空值集合</returns>
        public IEnumerable<T> GetRowValues<T>()
        {
            return row.CellsUsed()
                .Where(cell => cell.TryGetValue<T>(out _))
                .Select(cell => cell.GetValue<T>());
        }


        /// <summary>
        /// 检查行是否为空
        /// </summary>
        /// <returns>如果行为空返回 true，否则返回 false</returns>
        public bool IsEmpty()
        {
            return !row.CellsUsed().Any();
        }
    }

    #region 单位转换方法

    /// <summary>
    /// 将 Excel 列宽转换为像素
    /// </summary>
    /// <param name="width">Excel 列宽</param>
    /// <param name="mdw">最大数字宽度（Maximum Digit Width）</param>
    /// <returns>像素值</returns>
    /// <exception cref="ArgumentOutOfRangeException">当 mdw 小于或等于 0 时抛出</exception>
    public static double WidthToPixels(double width, int mdw)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(mdw);
        return width * mdw;
    }

    /// <summary>
    /// 将像素转换为 Excel 列宽
    /// </summary>
    /// <param name="pixels">像素值</param>
    /// <param name="mdw">最大数字宽度（Maximum Digit Width）</param>
    /// <returns>Excel 列宽</returns>
    /// <exception cref="ArgumentOutOfRangeException">当 mdw 小于或等于 0 时抛出</exception>
    public static double PixelsToWidth(double pixels, int mdw)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(mdw);
        return Math.Truncate(pixels / mdw * 256) / 256d;
    }

    /// <summary>
    /// 将像素转换为点（Points）
    /// </summary>
    /// <param name="pixels">像素值</param>
    /// <param name="dpi">每英寸点数（Dots Per Inch），默认为 96</param>
    /// <returns>点值</returns>
    /// <exception cref="ArgumentOutOfRangeException">当 dpi 小于或等于 0 时抛出</exception>
    public static double PixelsToPoints(double pixels, double dpi = 96d)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(dpi);
        return pixels * 72d / dpi;
    }

    /// <summary>
    /// 将点（Points）转换为像素
    /// </summary>
    /// <param name="points">点值</param>
    /// <param name="dpi">每英寸点数（Dots Per Inch），默认为 96</param>
    /// <returns>像素值</returns>
    /// <exception cref="ArgumentOutOfRangeException">当 dpi 小于或等于 0 时抛出</exception>
    public static double PointsToPixels(double points, double dpi = 96d)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(dpi);
        return points * dpi / 72d;
    }

    #endregion

    #region 实用工具方法

    /// <summary>
    /// 将 Excel 列字母转换为列号（A=1, B=2, ..., Z=26, AA=27, ...）
    /// </summary>
    /// <param name="columnLetter">列字母（如 "A", "AB"）</param>
    /// <returns>列号</returns>
    /// <exception cref="ArgumentNullException">当 columnLetter 为 null 或空时抛出</exception>
    public static int ColumnLetterToNumber(string columnLetter)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(columnLetter);

        columnLetter = columnLetter.ToUpperInvariant();
        int sum = 0;

        for (int i = 0; i < columnLetter.Length; i++)
        {
            sum *= 26;
            sum += (columnLetter[i] - 'A' + 1);
        }

        return sum;
    }

    /// <summary>
    /// 将列号转换为 Excel 列字母（1=A, 2=B, ..., 26=Z, 27=AA, ...）
    /// </summary>
    /// <param name="columnNumber">列号</param>
    /// <returns>列字母</returns>
    /// <exception cref="ArgumentOutOfRangeException">当 columnNumber 小于 1 时抛出</exception>
    public static string ColumnNumberToLetter(int columnNumber)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(columnNumber, 1);

        var columnName = new System.Text.StringBuilder();

        while (columnNumber > 0)
        {
            int modulo = (columnNumber - 1) % 26;
            columnName.Insert(0, (char)('A' + modulo));
            columnNumber = (columnNumber - modulo) / 26;
        }

        return columnName.ToString();
    }

    /// <summary>
    /// 解析 Excel 地址（如 "A1"）为行号和列号
    /// </summary>
    /// <param name="address">单元格地址（如 "A1", "AB10"）</param>
    /// <returns>包含行号和列号的元组</returns>
    public static (int Row, int Column) ParseAddress(string address)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(address);

        address = address.ToUpperInvariant();
        int i = 0;

        // 提取列字母
        while (i < address.Length && char.IsLetter(address[i]))
        {
            i++;
        }

        var columnLetter = address[..i];
        var rowNumber = int.Parse(address[i..]);

        return (rowNumber, ColumnLetterToNumber(columnLetter));
    }

    /// <summary>
    /// 创建 Excel 地址（从行号和列号生成，如 (1, 1) -> "A1"）
    /// </summary>
    /// <param name="row">行号</param>
    /// <param name="column">列号</param>
    /// <returns>单元格地址字符串</returns>
    public static string CreateAddress(int row, int column)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(row, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(column, 1);

        return $"{ColumnNumberToLetter(column)}{row}";
    }

    /// <summary>
    /// 生成列范围地址（如 "A:C"）
    /// </summary>
    /// <param name="startColumn">起始列字母或列号</param>
    /// <param name="endColumn">结束列字母或列号</param>
    /// <returns>列范围地址</returns>
    public static string CreateColumnRange(string startColumn, string endColumn)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(startColumn);
        ArgumentException.ThrowIfNullOrWhiteSpace(endColumn);

        return $"{startColumn}:{endColumn}";
    }

    /// <summary>
    /// 生成行范围地址（如 "1:5"）
    /// </summary>
    /// <param name="startRow">起始行号</param>
    /// <param name="endRow">结束行号</param>
    /// <returns>行范围地址</returns>
    public static string CreateRowRange(int startRow, int endRow)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(startRow, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(endRow, 1);

        return $"{startRow}:{endRow}";
    }

    /// <summary>
    /// 验证 Excel 地址格式是否正确
    /// </summary>
    /// <param name="address">要验证的地址</param>
    /// <returns>如果格式正确返回 true，否则返回 false</returns>
    public static bool IsValidAddress(string? address)
    {
        if (string.IsNullOrWhiteSpace(address))
            return false;

        try
        {
            ParseAddress(address);
            return true;
        }
        catch
        {
            return false;
        }
    }

    #endregion
}
