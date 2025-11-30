using System;
using UnityEditor;
using UnityEngine;

namespace ExcelMaster
{
    public static class GenerateSource
    {
        public static void Generate(string excelFilePath, string className, string tableName, string outputDirectory)
        {
            var excelArray = CsvConverter.ExcelToArray(
                excelFilePath: excelFilePath,
                sheetIndex: 1,
                startRow: 1,
                startColumn: 1);

            var classSource = CsvClassGenerator.Parse(
                usingNamespaces: new string[]
                {
                    "System",
                    "System.Collections.Generic",
                    "UnityEngine",
                    "ExcelMaster",
                    "MasterMemory",
                    "MessagePack",
                },

                classAttributes: new string[]
                {
                    $"[MemoryTable(\"{tableName}\")]",
                    "[MessagePackObject(true)]",
                },

                @namespace: "GameData",
                className: className,
                csv: excelArray);

            var builderSource = MasterMemoryBinaryGenerator.Generate(
                usingNamespaces: Array.Empty<string>(),
                @namespace: "GameData",
                buildClassName: className + "Builder",
                masterClassName: className);

            var classFilePath = System.IO.Path.Combine(outputDirectory, className + ".cs");
            var builderFilePath = System.IO.Path.Combine(outputDirectory, className + "Builder.cs");

            System.IO.Directory.CreateDirectory(outputDirectory);
            System.IO.File.WriteAllText(classFilePath, classSource);
            System.IO.File.WriteAllText(builderFilePath, builderSource);
        }
    }
}