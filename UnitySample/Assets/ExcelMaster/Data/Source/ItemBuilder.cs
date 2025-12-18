using System;
using System.IO;
using System.Collections.Generic;
using MasterMemory;
using MessagePack;
using MessagePack.Resolvers;
using ExcelMaster;

namespace Confront
{
    public sealed partial class Item
    {
        [ExcelBinaryBuilder("Item")]
        public static void BuildBinary(string outputPath = null)
        {
            BuildBinary(Data, outputPath);
        }

        /// <summary>
        /// Item 配列から MasterMemory バイナリを生成し保存します。
        /// </summary>
        /// <param name="masters">Item 配列</param>
        /// <param name="outputPath">出力パス。未指定時はデフォルトパスが使用されます。</param>
        /// <returns>生成されたバイナリ</returns>
        public static byte[] BuildBinary(IEnumerable<Item> masters, string outputPath = null)
        {
            if (masters == null) throw new ArgumentNullException(nameof(masters));
            outputPath ??= "Assets/ExcelMaster/Data/Binary";

            var messagePackResolvers = CompositeResolver.Create(
                MasterMemoryResolver.Instance,
                StandardResolver.Instance
            );
            var options = MessagePackSerializerOptions.Standard.WithResolver(messagePackResolvers);
            MessagePackSerializer.DefaultOptions = options;

            var builder = new DatabaseBuilder();
            builder.Append(masters);
            var binary = builder.Build();

            var dir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            File.WriteAllBytes(outputPath, binary);

            return binary;
        }
    }
}
