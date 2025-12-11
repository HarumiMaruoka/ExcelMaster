using System;
using System.Collections.Generic;
using System.IO;
using ExcelDataReader;

namespace ExcelMaster
{
    public static class ExcelUtil
    {
        public static string[][] ReadExcelToStringArray(string excelPath, int sheetIndex, int startRow = 1, int startColumn = 1, bool skipHeader = false, bool trimCells = true, bool stopAtEmptyRow = false, int? maxRows = null)
        {
            if (string.IsNullOrEmpty(excelPath)) throw new ArgumentException("excelPath is null or empty", nameof(excelPath));
            if (sheetIndex < 1) throw new ArgumentOutOfRangeException(nameof(sheetIndex), "sheetNum is1-based and must be >=1");

            using var stream = File.Open(excelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = ExcelReaderFactory.CreateReader(stream);

            int currentSheetIndex = 1;
            do
            {
                if (currentSheetIndex == sheetIndex)
                {
                    return ReadSheet(reader, startRow, startColumn, skipHeader, trimCells, stopAtEmptyRow, maxRows);
                }

                currentSheetIndex++;
            } while (reader.NextResult());

            throw new ArgumentException($"Worksheet index '{sheetIndex}' not found.");
        }

        public static string[][] ReadExcelToStringArray(string excelPath, string sheetName, int startRow = 1, int startColumn = 1, bool skipHeader = false, bool trimCells = true, bool stopAtEmptyRow = false, int? maxRows = null)
        {
            if (string.IsNullOrEmpty(excelPath)) throw new ArgumentException("excelPath is null or empty", nameof(excelPath));
            if (string.IsNullOrWhiteSpace(sheetName)) throw new ArgumentException("sheetName is null or empty", nameof(sheetName));

            using var stream = File.Open(excelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = ExcelReaderFactory.CreateReader(stream);

            do
            {
                if (string.Equals(reader.Name, sheetName, StringComparison.OrdinalIgnoreCase))
                {
                    return ReadSheet(reader, startRow, startColumn, skipHeader, trimCells, stopAtEmptyRow, maxRows);
                }
            } while (reader.NextResult());

            throw new ArgumentException($"Worksheet '{sheetName}' not found or empty.");
        }

        private static string[][] ReadSheet(IExcelDataReader reader, int startRow, int startColumn, bool skipHeader, bool trimCells, bool stopAtEmptyRow, int? maxRows)
        {
            if (startRow < 1) startRow = 1;
            if (startColumn < 1) startColumn = 1;

            var rows = new List<string[]>();
            int currentRowIndex = 0; // 1-based logical row number
            int rowsTaken = 0;

            while (reader.Read())
            {
                currentRowIndex++;

                // Skip until startRow (and potential header)
                int effectiveStartRow = skipHeader ? startRow + 1 : startRow;
                if (currentRowIndex < effectiveStartRow)
                {
                    continue;
                }

                if (maxRows.HasValue && rowsTaken >= maxRows.Value)
                {
                    break;
                }

                int fieldCount = reader.FieldCount;
                if (fieldCount < startColumn)
                {
                    // No usable columns in this row
                    if (stopAtEmptyRow)
                    {
                        break;
                    }
                    rows.Add(Array.Empty<string>());
                    rowsTaken++;
                    continue;
                }

                int lastCol = fieldCount;
                var row = new List<string>(capacity: lastCol - startColumn + 1);
                bool entireRowEmpty = true;

                for (int c = startColumn - 1; c < lastCol; c++)
                {
                    object value = reader.GetValue(c);
                    string val = value?.ToString();
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
