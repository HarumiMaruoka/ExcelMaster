using ExcelMaster;

namespace Sample.Examples
{
    internal class CsvToSourceExample
    {
        public static void Run()
        {
            var csvPath = "Assets/Csv/Item_Output.csv";
            var csvContent = CsvConverter.ImportFromCsv(csvPath);
            var source = CsvClassGenerator.Parse(
                new string[] { "System", "System.Collections.Generic" },
                "GameNamespace",
                "ItemData",
                csvContent);

            Console.WriteLine(source);
        }
    }
}
