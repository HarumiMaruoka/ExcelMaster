using MasterMemory;
using MessagePack;
using MessagePack.Resolvers;
using System;

namespace BinaryBuilder
{
    public class BinaryBuilder
    {
        // TODO: Excelファイルからバイナリデータを生成する機能を実装する
        // 1. 指定のExcelシートから、ExcelToCsvを使ってCSVデータを取得する。
        // 2. CSVデータを解析し、マスターデータの型情報に基づいてSourceGeneratorで生成されたクラスにマッピングする。
        // 3. マッピングされたクラスを基にMasterMemoryバイナリ形式でシリアライズし、ファイルに保存する。

        public void Build<T>(T[] data)
        {
            var options = MessagePackSerializerOptions.Standard
                .WithResolver(CompositeResolver.Create(
                    MasterMemoryResolver.Instance,
                    StandardResolver.Instance
                ));
            byte[] binaryData = MessagePackSerializer.Serialize(data, options);
            // バイナリデータをファイルに保存する処理をここに追加する
            // 例: File.WriteAllBytes("output.bin", binaryData);
        }
    }
}