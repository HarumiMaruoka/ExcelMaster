using ExcelMaster;
using ExcelMaster.Builders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;



namespace Sample.Examples
{
    internal class ClassBuildExample
    {
        // Excel-like selection range sample
        private readonly static string[][] Selection =
        {
            new[] { "Id","Name","Parameters","","","","Addresses","","","IntSample","floatArraySample","EnumSample","HandType","Category" },
            new[] { "[PrimaryKey]int", "string","float[]","","","","string[]","","","int","float[]","enum","enum","enum" },
            new[] { "1","HealPotion","1","","","","sprite","model","description","","","Member1","Goo","Potion" },
            new[] { "2","AttackPotion","10","20","","","","","","","","Member2","Pa","Equipment" },
            new[] { "3","DefencePotion","30","33","55","66","","","","","","Member3","Pa","Weapon" }
        };

        public static void Run()
        {
            var @namespace = "GameNamespace";
            var className = "ItemData";
            var usingNamespaces = new List<string> { "MasterMemory", "MessagePack", "System.Collections.Generic" };

            // Class and enums (no Data)
            var source = SourceBuilder.Build(
                @namespace: @namespace,
                usingNamespaces: usingNamespaces,
                className: className,
                selection: Selection
            );

            // Data-only file as a nice partial class in the same namespace
            var dataSource = SourceBuilder.BuildDataFile(
                @namespace: @namespace,
                usingNamespaces: new List<string> { "System.Collections.Generic" },
                className: className,
                selection: Selection
            );

            var outputClassPath = "Assets/Generated/ItemData.cs";
            var outputDataPath = "Assets/Generated/ItemData_DataOnly.cs";
            File.WriteAllText(outputClassPath, source);
            File.WriteAllText(outputDataPath, dataSource);
        }
    }
}
