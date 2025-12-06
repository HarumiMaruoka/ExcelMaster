using System;
using System.Collections.Generic;
using System.Text;
using ClosedXML.Excel;

namespace ExcelMaster
{
    public static class ExcelUtil
    {
        public static string[][] ReadExcelToStringArray(string excelPath, int sheetNum, int startRow =1, int startColumn =1, bool skipHeader = false, bool trimCells = true, bool stopAtEmptyRow = false, int? maxRows = null)
        {
            if (string.IsNullOrEmpty(excelPath)) throw new ArgumentException("excelPath is null or empty", nameof(excelPath));
            if (sheetNum <1) throw new ArgumentOutOfRangeException(nameof(sheetNum), "sheetNum is1-based and must be >=1");
            using var workbook = new XLWorkbook(excelPath);
            var worksheet = workbook.Worksheet(sheetNum);
            return ReadWorksheet(worksheet, startRow, startColumn, skipHeader, trimCells, stopAtEmptyRow, maxRows);
        }

        public static string[][] ReadExcelToStringArray(string excelPath, string sheetName, int startRow =1, int startColumn =1, bool skipHeader = false, bool trimCells = true, bool stopAtEmptyRow = false, int? maxRows = null)
        {
            if (string.IsNullOrEmpty(excelPath)) throw new ArgumentException("excelPath is null or empty", nameof(excelPath));
            if (string.IsNullOrWhiteSpace(sheetName)) throw new ArgumentException("sheetName is null or empty", nameof(sheetName));
            using var workbook = new XLWorkbook(excelPath);
            var worksheet = workbook.Worksheet(sheetName);
            if (worksheet == null || worksheet.IsEmpty())
                throw new ArgumentException($"Worksheet '{sheetName}' not found or empty.");
            return ReadWorksheet(worksheet, startRow, startColumn, skipHeader, trimCells, stopAtEmptyRow, maxRows);
        }

        private static string[][] ReadWorksheet(IXLWorksheet worksheet, int startRow, int startColumn, bool skipHeader, bool trimCells, bool stopAtEmptyRow, int? maxRows)
        {
            // Determine used range but respect startRow/startColumn
            var used = worksheet.RangeUsed();
            if (used == null)
                return Array.Empty<string[]>();

            int firstRow = Math.Max(startRow, used.FirstRow().RowNumber());
            int firstCol = Math.Max(startColumn, used.FirstColumn().ColumnNumber());
            int lastRow = used.LastRow().RowNumber();
            int lastCol = used.LastColumn().ColumnNumber();

            var rows = new List<string[]>();
            int rowStart = skipHeader ? firstRow +1 : firstRow;
            int rowsTaken =0;
            for (int r = rowStart; r <= lastRow; r++)
            {
                if (maxRows.HasValue && rowsTaken >= maxRows.Value) break;
                var row = new List<string>(capacity: lastCol - firstCol +1);
                bool entireRowEmpty = true;
                for (int c = firstCol; c <= lastCol; c++)
                {
                    var cell = worksheet.Cell(r, c);
                    string val = cell.GetValue<string>();
                    if (trimCells && val != null) val = val.Trim();
                    if (!string.IsNullOrEmpty(val)) entireRowEmpty = false;
                    row.Add(val);
                }
                if (stopAtEmptyRow && entireRowEmpty)
                {
                    break;
                }
                rows.Add(row.ToArray());
                rowsTaken++;
            }

            return rows.ToArray();
        }
    }
}
