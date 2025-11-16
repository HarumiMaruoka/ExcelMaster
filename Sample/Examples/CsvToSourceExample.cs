using ExcelMaster;

namespace Sample.Samples
{
    internal class CsvToSourceExample
    {
        public static void Run()
        {
            var csvPath = "Assets/Csv/Item_Output.csv";
            var csvContent = CsvGenerator.ImportFromCsv(csvPath);
            var source = CsvClassGenerator.Parse(
                new string[] { "System", "System.Collections.Generic" },
                "GameNamespace",
                "ItemData",
                csvContent);

            Console.WriteLine(source);
        }
    }
}
