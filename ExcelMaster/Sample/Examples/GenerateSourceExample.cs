using ExcelMaster;

namespace Sample.Examples
{
    internal class GenerateSourceExample
    {
        public static void Run()
        {
            // マスタークラスのソースコードと、バイナリ生成クラスのソースコードを生成し、Assets/Generatedに保存する例。
            var outputDir = Path.Combine("Assets", "Generated");
            if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);

            // using の指定
            var usings = new[] { "MasterMemory", "MessagePack" };

            //生成するマスタークラス情報
            var classDef = new ClassDefinition
            {
                Namespace = "GameNamespace", // 任意の名前空間
                ClassName = "ItemData", // マスタークラス名
                Attributes = new[] { "[MemoryTable(\"Item\"), MessagePackObject(true)]" },
                Properties = new List<PropertyDefinition>
                {
                    new PropertyDefinition { Name = "Id", Type = "int", Attributes = new[] {"[PrimaryKey]" } },
                    new PropertyDefinition { Name = "Name", Type = "string" },
                    new PropertyDefinition { Name = "Price", Type = "int" },
                    new PropertyDefinition { Name = "Rarity", Type = "int" }
                }
            };

            // マスタークラスソース生成
            var masterClassSource = MasterClassGenerator.Generate(usings, classDef);

            // バイナリ生成クラスソース生成
            var binaryBuilderSource = MasterMemoryBinaryGenerator.Generate(
                usingNamespaces: new[] { "Sample" },
                @namespace: "GameNamespace",
                buildClassName: "ItemDataBinaryBuilder",
                masterClassName: classDef.ClassName);

            // 出力ファイルパス
            var masterClassPath = Path.Combine(outputDir, classDef.ClassName + ".cs");
            var binaryBuilderPath = Path.Combine(outputDir, "ItemDataBinaryBuilder.cs");

            File.WriteAllText(masterClassPath, masterClassSource);
            File.WriteAllText(binaryBuilderPath, binaryBuilderSource);

            Console.WriteLine("Generated master class: " + masterClassPath);
            Console.WriteLine("Generated binary builder: " + binaryBuilderPath);
        }
    }
}
