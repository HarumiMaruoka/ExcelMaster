using System;
using System.IO;
using System.Text;
using ClosedXML.Excel;
using System.Collections.Generic;

namespace ExcelMaster
{
    public static class CsvConverter
    {
        public static string[,] ExportToCsv(string excelFilePath, int sheetIndex, string csvFilePath, int startRow, int startColumn, int endRow = -1, int endColumn = -1)
        {
            if (string.IsNullOrWhiteSpace(excelFilePath)) throw new ArgumentException("excelFilePath is required", nameof(excelFilePath));
            if (!File.Exists(excelFilePath)) throw new FileNotFoundException("Excel file not found", excelFilePath);
            if (sheetIndex <= 0) throw new ArgumentOutOfRangeException(nameof(sheetIndex), "sheetIndex must be >=1");
            if (startRow <= 0) throw new ArgumentOutOfRangeException(nameof(startRow), "startRow must be >=1");
            if (startColumn <= 0) throw new ArgumentOutOfRangeException(nameof(startColumn), "startColumn must be >=1");
            if (string.IsNullOrWhiteSpace(csvFilePath)) throw new ArgumentException("csvFilePath is required", nameof(csvFilePath));

            using var workbook = new XLWorkbook(excelFilePath);
            var worksheet = workbook.Worksheet(sheetIndex);
            if (worksheet == null)
                throw new ArgumentException($"Worksheet at index {sheetIndex} not found.", nameof(sheetIndex));

            // Determine end bounds if not provided
            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? startRow;
            var lastCol = worksheet.LastColumnUsed()?.ColumnNumber() ?? startColumn;
            if (endRow < 0) endRow = lastRow;
            if (endColumn < 0) endColumn = lastCol;
            if (endRow < startRow) throw new ArgumentOutOfRangeException(nameof(endRow), "endRow must be >= startRow");
            if (endColumn < startColumn) throw new ArgumentOutOfRangeException(nameof(endColumn), "endColumn must be >= startColumn");

            int rowCount = endRow - startRow + 1;
            int colCount = endColumn - startColumn + 1;
            var data = new string[rowCount, colCount];
            var sb = new StringBuilder();
            for (int r = startRow; r <= endRow; r++)
            {
                int rr = r - startRow;
                for (int c = startColumn; c <= endColumn; c++)
                {
                    int cc = c - startColumn;
                    var cell = worksheet.Cell(r, c);
                    var text = cell.GetFormattedString();
                    data[rr, cc] = text;
                    sb.Append(EscapeCsv(text));
                    if (c < endColumn) sb.Append(',');
                }
                if (r < endRow) sb.Append(Environment.NewLine);
            }

            var csvText = sb.ToString();

            var dir = Path.GetDirectoryName(csvFilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllText(csvFilePath, csvText, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
            return data;
        }

        public static string[,] ExportToCsv(string excelFilePath, string sheetName, string csvFilePath, int startRow, int startColumn, int endRow = -1, int endColumn = -1)
        {
            if (string.IsNullOrWhiteSpace(excelFilePath)) throw new ArgumentException("excelFilePath is required", nameof(excelFilePath));
            if (!File.Exists(excelFilePath)) throw new FileNotFoundException("Excel file not found", excelFilePath);
            if (string.IsNullOrWhiteSpace(sheetName)) throw new ArgumentException("sheetName is required", nameof(sheetName));
            if (startRow <= 0) throw new ArgumentOutOfRangeException(nameof(startRow), "startRow must be >=1");
            if (startColumn <= 0) throw new ArgumentOutOfRangeException(nameof(startColumn), "startColumn must be >=1");
            if (string.IsNullOrWhiteSpace(csvFilePath)) throw new ArgumentException("csvFilePath is required", nameof(csvFilePath));

            using var workbook = new XLWorkbook(excelFilePath);
            var worksheet = workbook.Worksheet(sheetName);
            if (worksheet == null)
                throw new ArgumentException($"Worksheet '{sheetName}' not found.", nameof(sheetName));

            // Determine end bounds if not provided
            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? startRow;
            var lastCol = worksheet.LastColumnUsed()?.ColumnNumber() ?? startColumn;
            if (endRow < 0) endRow = lastRow;
            if (endColumn < 0) endColumn = lastCol;
            if (endRow < startRow) throw new ArgumentOutOfRangeException(nameof(endRow), "endRow must be >= startRow");
            if (endColumn < startColumn) throw new ArgumentOutOfRangeException(nameof(endColumn), "endColumn must be >= startColumn");

            int rowCount = endRow - startRow + 1;
            int colCount = endColumn - startColumn + 1;
            var data = new string[rowCount, colCount];
            var sb = new StringBuilder();
            for (int r = startRow; r <= endRow; r++)
            {
                int rr = r - startRow;
                for (int c = startColumn; c <= endColumn; c++)
                {
                    int cc = c - startColumn;
                    var cell = worksheet.Cell(r, c);
                    var text = cell.GetFormattedString();
                    data[rr, cc] = text;
                    sb.Append(EscapeCsv(text));
                    if (c < endColumn) sb.Append(',');
                }
                if (r < endRow) sb.Append(Environment.NewLine);
            }

            var csvText = sb.ToString();

            var dir = Path.GetDirectoryName(csvFilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllText(csvFilePath, csvText, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
            return data;
        }

        public static string[,] ExcelToArray(string excelFilePath, int sheetIndex, int startRow, int startColumn, int endRow = -1, int endColumn = -1)
        {
            if (string.IsNullOrWhiteSpace(excelFilePath)) throw new ArgumentException("excelFilePath is required", nameof(excelFilePath));
            if (!File.Exists(excelFilePath)) throw new FileNotFoundException("Excel file not found", excelFilePath);
            if (sheetIndex <= 0) throw new ArgumentOutOfRangeException(nameof(sheetIndex), "sheetIndex must be >=1");
            if (startRow <= 0) throw new ArgumentOutOfRangeException(nameof(startRow), "startRow must be >=1");
            if (startColumn <= 0) throw new ArgumentOutOfRangeException(nameof(startColumn), "startColumn must be >=1");

            using var workbook = new XLWorkbook(excelFilePath);
            var worksheet = workbook.Worksheet(sheetIndex);
            if (worksheet == null)
                throw new ArgumentException($"Worksheet at index {sheetIndex} not found.", nameof(sheetIndex));

            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? startRow;
            var lastCol = worksheet.LastColumnUsed()?.ColumnNumber() ?? startColumn;
            if (endRow < 0) endRow = lastRow;
            if (endColumn < 0) endColumn = lastCol;
            if (endRow < startRow) throw new ArgumentOutOfRangeException(nameof(endRow), "endRow must be >= startRow");
            if (endColumn < startColumn) throw new ArgumentOutOfRangeException(nameof(endColumn), "endColumn must be >= startColumn");

            int rowCount = endRow - startRow + 1;
            int colCount = endColumn - startColumn + 1;
            var data = new string[rowCount, colCount];
            for (int r = startRow; r <= endRow; r++)
            {
                int rr = r - startRow;
                for (int c = startColumn; c <= endColumn; c++)
                {
                    int cc = c - startColumn;
                    var cell = worksheet.Cell(r, c);
                    data[rr, cc] = cell.GetFormattedString();
                }
            }
            return data;
        }

        public static string[,] ExcelToArray(string excelFilePath, string sheetName, int startRow, int startColumn, int endRow = -1, int endColumn = -1)
        {
            if (string.IsNullOrWhiteSpace(excelFilePath)) throw new ArgumentException("excelFilePath is required", nameof(excelFilePath));
            if (!File.Exists(excelFilePath)) throw new FileNotFoundException("Excel file not found", excelFilePath);
            if (string.IsNullOrWhiteSpace(sheetName)) throw new ArgumentException("sheetName is required", nameof(sheetName));
            if (startRow <= 0) throw new ArgumentOutOfRangeException(nameof(startRow), "startRow must be >=1");
            if (startColumn <= 0) throw new ArgumentOutOfRangeException(nameof(startColumn), "startColumn must be >=1");

            using var workbook = new XLWorkbook(excelFilePath);
            var worksheet = workbook.Worksheet(sheetName);
            if (worksheet == null)
                throw new ArgumentException($"Worksheet '{sheetName}' not found.", nameof(sheetName));

            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? startRow;
            var lastCol = worksheet.LastColumnUsed()?.ColumnNumber() ?? startColumn;
            if (endRow < 0) endRow = lastRow;
            if (endColumn < 0) endColumn = lastCol;
            if (endRow < startRow) throw new ArgumentOutOfRangeException(nameof(endRow), "endRow must be >= startRow");
            if (endColumn < startColumn) throw new ArgumentOutOfRangeException(nameof(endColumn), "endColumn must be >= startColumn");

            int rowCount = endRow - startRow + 1;
            int colCount = endColumn - startColumn + 1;
            var data = new string[rowCount, colCount];
            for (int r = startRow; r <= endRow; r++)
            {
                int rr = r - startRow;
                for (int c = startColumn; c <= endColumn; c++)
                {
                    int cc = c - startColumn;
                    var cell = worksheet.Cell(r, c);
                    data[rr, cc] = cell.GetFormattedString();
                }
            }
            return data;
        }

        private static string EscapeCsv(string field)
        {
            if (field == null) return string.Empty;
            bool mustQuote = field.Contains('"') || field.Contains(',') || field.Contains('\n') || field.Contains('\r');
            if (mustQuote)
            {
                var escaped = field.Replace("\"", "\"\"");
                return "\"" + escaped + "\"";
            }
            return field;
        }

        public static string[,] ImportFromCsv(string csvFilePath)
        {
            if (string.IsNullOrWhiteSpace(csvFilePath)) throw new ArgumentException("csvFilePath is required", nameof(csvFilePath));
            if (!File.Exists(csvFilePath)) throw new FileNotFoundException("CSV file not found", csvFilePath);

            var rows = new List<List<string>>();
            var currentRow = new List<string>();
            var field = new StringBuilder();
            bool inQuotes = false;

            using (var stream = File.OpenRead(csvFilePath))
            using (var reader = new StreamReader(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), detectEncodingFromByteOrderMarks: true))
            {
                while (true)
                {
                    int read = reader.Read();
                    if (read == -1)
                    {
                        // End of file: flush last field/row if any
                        if (inQuotes)
                        {
                            // Unterminated quotes: treat as end of field
                            inQuotes = false;
                        }
                        // If there is any data in the current row or at least one field was started, commit it
                        if (field.Length > 0 || currentRow.Count > 0)
                        {
                            currentRow.Add(field.ToString());
                            rows.Add(currentRow);
                        }
                        break;
                    }

                    char ch = (char)read;

                    if (inQuotes)
                    {
                        if (ch == '"')
                        {
                            int peek = reader.Peek();
                            if (peek == '"')
                            {
                                // Escaped quote
                                reader.Read(); // consume second quote
                                field.Append('"');
                            }
                            else
                            {
                                // End of quoted field
                                inQuotes = false;
                            }
                        }
                        else
                        {
                            field.Append(ch);
                        }
                    }
                    else
                    {
                        if (ch == '"')
                        {
                            inQuotes = true;
                        }
                        else if (ch == ',')
                        {
                            currentRow.Add(field.ToString());
                            field.Clear();
                        }
                        else if (ch == '\r' || ch == '\n')
                        {
                            // End of record; handle CRLF
                            if (ch == '\r' && reader.Peek() == '\n') reader.Read();
                            currentRow.Add(field.ToString());
                            field.Clear();
                            rows.Add(currentRow);
                            currentRow = new List<string>();
                        }
                        else
                        {
                            field.Append(ch);
                        }
                    }
                }
            }

            // Determine max columns
            int rowCount = rows.Count;
            int colCount = 0;
            for (int i = 0; i < rowCount; i++)
            {
                if (rows[i].Count > colCount) colCount = rows[i].Count;
            }

            if (rowCount == 0 || colCount == 0)
            {
                return new string[0, 0];
            }

            var result = new string[rowCount, colCount];
            for (int r = 0; r < rowCount; r++)
            {
                var record = rows[r];
                for (int c = 0; c < colCount; c++)
                {
                    result[r, c] = c < record.Count ? record[c] : string.Empty;
                }
            }

            return result;
        }
    }
}
