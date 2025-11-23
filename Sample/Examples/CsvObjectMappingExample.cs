using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Examples
{
    internal class CsvObjectMappingExample
    {
        public static void Run()
        {
            var csvData = new string[,]
 {
                { "Id", "Name", "IsActive" },          // ヘッダー行
                { "int", "string", "bool" },          // 型情報行
                { "1", "Alice", "true" },             // データ行1
                { "2", "Bob", "false" },              // データ行2
                { "3", "Charlie", "true" }            // データ行3
 };
            var objects = ExcelToCsv.CsvObjectMapper.MapCsvToObjects<SampleData>(csvData);
            foreach (var obj in objects)
            {
                Console.WriteLine($"Id: {obj.Id}, Name: {obj.Name}, IsActive: {obj.IsActive}");
            }
        }

        public class SampleData
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public bool IsActive { get; set; }
        }
    }
}
