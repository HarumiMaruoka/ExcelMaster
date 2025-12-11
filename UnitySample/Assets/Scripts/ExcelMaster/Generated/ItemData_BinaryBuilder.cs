using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using MasterMemory;
using MessagePack;
using MessagePack.Resolvers;

namespace GameNamespace
{
    public sealed partial class ItemData
    {
        /// <summary>
        /// ItemData 配列から MasterMemory バイナリを生成し保存します。
        /// </summary>
        /// <param name="masters">ItemData 配列</param>
        /// <param name="outputPath">出力パス。未指定時はデフォルトパスが使用されます。</param>
        /// <returns>生成されたバイナリ</returns>
        public static byte[] BuildBinary(IEnumerable<ItemData> masters, string outputPath = null)
        {
            if (masters == null) throw new ArgumentNullException(nameof(masters));
            outputPath ??= "Assets/StreamingAssets/ExcelMaster/Binaries/ItemData.bytes";

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
