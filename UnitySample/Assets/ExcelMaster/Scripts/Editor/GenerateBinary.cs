using UnityEngine;
using System;
using System.Linq;
using System.Reflection;

namespace ExcelMaster
{
    public class GenerateBinary
    {
        [UnityEditor.MenuItem("ExcelMaster/Generate Binary")]
        public static void TestGenerate()
        {
            string excelFilePath = "Assets/ExcelMaster/Data/Excels/ItemData.xlsx";
            string className = "Item";
            string tableName = "item_table";
            string outputDirectory = "Assets/ExcelMaster/Example/Binary";
            Generate(excelFilePath, className, tableName, outputDirectory);
            Debug.Log("Binary generation completed.");
        }

        public static void Generate(string excelFilePath, string className, string tableName, string outputDirectory, string assembly = "Assembly-CSharp")
        {
            var array = CsvConverter.ExcelToArray(
                excelFilePath: excelFilePath,
                sheetIndex: 1,
                startRow: 1,
                startColumn: 1);

            var masters = CsvObjectMapper.Map(array, className, assembly);

            // 指定アセンブリから Builder 型を取得（名前空間不問で型名一致を検索）
            var builderTypeName = className + "Builder";
            var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == assembly);
            if (asm == null)
            {
                throw new InvalidOperationException($"Assembly '{assembly}' が見つかりません");
            }
            var builder = asm.GetTypes().FirstOrDefault(t => t.Name == builderTypeName);
            if (builder == null)
            {
                throw new InvalidOperationException($"型 '{builderTypeName}' がアセンブリ '{assembly}' に見つかりません");
            }

            var buildMethod = builder.GetMethod("Build", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (buildMethod == null)
            {
                throw new MissingMethodException(builder.FullName, "Build");
            }

            var outputPath = System.IO.Path.Combine(outputDirectory, className + ".mmdb");
            buildMethod.Invoke(null, new object[] { masters, outputPath });
        }
    }
}