using ExcelMaster.Builders;

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

            // Class and enums (no Data)
            var source = SourceBuilder.Build(
                @namespace: @namespace,
                usingNamespaces: Array.Empty<string>(),
                className: className,
                selection: Selection
            );

            // Data-only file as a nice partial class in the same namespace
            var dataSource = SourceBuilder.BuildDataFile(
                @namespace: @namespace,
                usingNamespaces: Array.Empty<string>(),
                className: className,
                selection: Selection
            );

            var builderSource = SourceBuilder.BuildBinaryBuilderFile(
                @namespace: @namespace,
                usingNamespaces: new string[] { "Sample" },
                className: className,
                defaultOutputPath: "Assets/Generated/ItemData.bytes"
            );

            var outputClassPath = "Assets/Generated/ItemData.cs";
            var outputDataPath = "Assets/Generated/ItemData_DataOnly.cs";
            var outputBuilderPath = "Assets/Generated/ItemData_BinaryBuilder.cs";
            File.WriteAllText(outputClassPath, source);
            File.WriteAllText(outputDataPath, dataSource);
            File.WriteAllText(outputBuilderPath, builderSource);
        }
    }
}
