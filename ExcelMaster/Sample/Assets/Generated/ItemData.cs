using MasterMemory;
using MessagePack;
using System.Collections.Generic;

namespace GameNamespace
{
    [MemoryTable("Item"), MessagePackObject(true)]
    public sealed partial class ItemData
    {
        [PrimaryKey]
        public int Id { get; set; }

        public string Name { get; set; }

        public float[] Parameters { get; set; }

        public string[] Addresses { get; set; }

        public int IntSample { get; set; }

        public float[] floatArraySample { get; set; }

        public EnumSample EnumSample { get; set; }

        public HandType HandType { get; set; }

        public ItemCategory ItemCategory { get; set; }

    }

    public enum EnumSample
    {
        Member1,
        Member2,
        Member3
    }

    public enum HandType
    {
        Goo,
        Pa
    }

    public enum ItemCategory
    {
        Potion,
        Equipment,
        Weapon
    }
}
