using ClosedXML.Excel;
using System.Collections.Generic;

namespace ExcelMaster
{
    public abstract class ConverterBase<T>
    {
        public abstract byte[] BuildBinary(T[] masters, string outputPath = null);

        public abstract bool TryParse(IXLRow row, out T master);

        public List<T> ExcelToArray(string excelFilePath, int sheetIndex, int startRow = 0, int endRow = -1)
        {
            var masters = new List<T>();
            using (var workbook = new XLWorkbook(excelFilePath))
            {
                var worksheet = workbook.Worksheet(sheetIndex + 1);
                var lastRow = endRow < 0 ? worksheet.LastRowUsed().RowNumber() : endRow;
                for (int rowIndex = startRow + 1; rowIndex <= lastRow; rowIndex++)
                {
                    var row = worksheet.Row(rowIndex);
                    if (TryParse(row, out T master))
                    {
                        masters.Add(master);
                    }
                }
            }
            return masters;
        }

        public List<T> ExcelToArray(string excelFilePath, string sheetName, int startRow = 0, int endRow = -1)
        {
            var masters = new List<T>();
            using (var workbook = new XLWorkbook(excelFilePath))
            {
                var worksheet = workbook.Worksheet(sheetName);
                var lastRow = endRow < 0 ? worksheet.LastRowUsed().RowNumber() : endRow;
                for (int rowIndex = startRow + 1; rowIndex <= lastRow; rowIndex++)
                {
                    var row = worksheet.Row(rowIndex);
                    if (TryParse(row, out T master))
                    {
                        masters.Add(master);
                    }
                }
            }
            return masters;
        }
    }
}