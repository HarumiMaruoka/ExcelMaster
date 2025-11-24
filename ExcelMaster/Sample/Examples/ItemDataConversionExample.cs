using ExcelMaster;
using GameNamespace;

namespace Sample.Examples
{
    internal class ItemDataConversionExample
    {
        public static void Run()
        {
            var excelFilePath = "Assets/Excels/Item.xlsx";
            var csvFilePath = "Assets/Csv/Item_Output.csv";
            var binaryPath = "Assets/Binary/ItemData.mmdb";

            CsvConverter.ExportToCsv(excelFilePath, 1, csvFilePath, 1, 1);
            var csv = CsvConverter.ImportFromCsv(csvFilePath);
            var data = CsvObjectMapper.Map<ItemData>(csv);
            var binary = ItemDataBinaryBuilder.Build(data);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(binaryPath)!);
            System.IO.File.WriteAllBytes(binaryPath, binary);

            var bytes = System.IO.File.ReadAllBytes(binaryPath);
            var database = new MemoryDatabase(bytes);
            if (database.ItemDataTable.TryFindById(1001, out var item))
            {
                Console.WriteLine($"Item ID: {item.Id}, Name: {item.Name}, Price: {item.Price}, Rarity: {item.Rarity}");
            }
            else
            {
                Console.WriteLine("Item not found.");
            }

            if (database.ItemDataTable.TryFindById(1, out var item2))
            {
                Console.WriteLine($"Item ID: {item2.Id}, Name: {item2.Name}, Price: {item2.Price}, Rarity: {item2.Rarity}");
            }
            else
            {
                Console.WriteLine("Item not found.");
            }
        }
    }
}
