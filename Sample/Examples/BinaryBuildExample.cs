using System;

namespace Sample.Examples
{
    internal class BinaryBuildExample
    {
        public static void Run()
        {
            // マスタークラス(ItemData)用のバイナリ生成クラスソースコードを生成し表示する例
            var builderSource = CsvToSource.MasterMemoryBinaryGenerator.Generate(
                @namespace: "GameNamespace",
                buildClassName: "ItemDataBinaryBuilder",
                masterClassName: "ItemData"
            );

            Console.WriteLine(builderSource);
        }
    }
}
