using Sample;
using System.Data;
using System;
using System.IO;
using System.Collections.Generic;
using MasterMemory;
using MessagePack;
using MessagePack.Resolvers;
using ExcelMaster;

namespace ModularPulse.Master
{
    public sealed partial class Item
    {
        public readonly static List<Item> Data = new List<Item>()
        {
            new Item
            {
                Id = 1,
                Name = "HealPotion",
                Parameters = new float[] { 1.0f },
                Addresses = new string[] { "sprite", "model", "description" },
                IntSample = 0,
                floatArraySample = new float[] {  },
                EnumSample = EnumSample.Member1,
                HandType = HandType.Goo,
                ItemCategory = ItemCategory.Potion
            },
            new Item
            {
                Id = 2,
                Name = "AttackPotion",
                Parameters = new float[] { 10.0f, 20.0f },
                Addresses = new string[] {  },
                IntSample = 0,
                floatArraySample = new float[] {  },
                EnumSample = EnumSample.Member2,
                HandType = HandType.Pa,
                ItemCategory = ItemCategory.Equipment
            },
            new Item
            {
                Id = 3,
                Name = "DefencePotion",
                Parameters = new float[] { 30.0f, 33.0f, 55.0f, 66.0f },
                Addresses = new string[] {  },
                IntSample = 0,
                floatArraySample = new float[] {  },
                EnumSample = EnumSample.Member3,
                HandType = HandType.Pa,
                ItemCategory = ItemCategory.Weapon
            }
        };


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
            outputPath ??= "Assets/StreamingAssets/Master\\item.bytes";

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
