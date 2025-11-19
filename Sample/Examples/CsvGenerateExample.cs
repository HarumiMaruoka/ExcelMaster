namespace Sample.Samples
{
    internal class CsvGenerateExample
    {
        public static void Run()
        {
            ExcelToCsv.CsvGenerator.ExportToCsv(
                excelFilePath: "Assets/Excels/Item.xlsx",
                sheetIndex: 1,
                csvFilePath: "Assets/Csv/Item_Output.csv",
                startRow: 1,
                startColumn: 1
            );

            var csv = ExcelToCsv.CsvGenerator.ImportFromCsv(
                csvFilePath: "Assets/Csv/Item_Output.csv"
            );

            var rowCount = csv.GetLength(0);
            var columnCount = csv.GetLength(1);

            // Compute column widths
            var colWidths = new int[columnCount];
            for (int c = 0; c < columnCount; c++)
            {
                int max = 0;
                for (int r = 0; r < rowCount; r++)
                {
                    var cell = csv[r, c] ?? string.Empty;
                    if (cell.Length > max) max = cell.Length;
                }
                colWidths[c] = Math.Max(1, max);
            }

            // Helper: print separator line like +-----+----+
            void PrintSeparator()
            {
                Console.Write('+');
                for (int c = 0; c < columnCount; c++)
                {
                    Console.Write(new string('-', colWidths[c] + 2)); // padding spaces on both sides
                    Console.Write('+');
                }
                Console.WriteLine();
            }

            // Print table
            PrintSeparator();
            for (int r = 0; r < rowCount; r++)
            {
                Console.Write('|');
                for (int c = 0; c < columnCount; c++)
                {
                    var text = csv[r, c] ?? string.Empty;
                    // left-align; adjust padding to column width
                    Console.Write(' ');
                    Console.Write(text);
                    // pad remaining spaces
                    int pad = colWidths[c] - text.Length;
                    if (pad > 0) Console.Write(new string(' ', pad));
                    Console.Write(' ');
                    Console.Write('|');
                }
                Console.WriteLine();
                PrintSeparator();
            }
        }
    }
}
