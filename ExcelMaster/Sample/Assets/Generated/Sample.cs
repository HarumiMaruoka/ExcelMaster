using MasterMemory;
using MessagePack;

namespace SampleGameNamespace
{
    [MemoryTable("SampleItem"), MessagePackObject(true)]
    public sealed class StageMaster
    {
        [PrimaryKey]
        public string Id { get; set; }

        public string Name { get; set; }

        public float[] Parameters { get; set; }

        public string[] Addresses { get; set; }

        public int IntSample { get; set; }

        public float[] floatArraySample { get; set; }
        public EnumSample EnumSample { get; set; }
        public HandType HandType { get; set; }
        public Category Category { get; set; }

        public readonly static List<StageMaster> Data = new List<StageMaster>()
        {
            new StageMaster
            {
                Id = "1",
                Name = "HealPotion",
                Parameters = new float[] { 1f },
                Addresses = new string[] { "sprite", "model", "description" },
                IntSample = 0,
                floatArraySample = new float[] {  },
                EnumSample = EnumSample.Member1,
                HandType = HandType.Goo,
                Category = Category.Potion
            },
            new StageMaster
            {
                Id = "2",
                Name = "AttackPotion",
                Parameters = new float[] { 10f, 20f },
                Addresses = new string[] {  },
                IntSample = 0,
                floatArraySample = new float[] {  },
                EnumSample = EnumSample.Member2,
                HandType = HandType.Pa,
                Category = Category.Equipment
            },
            new StageMaster
            {
                Id = "3",
                Name = "DefencePotion",
                Parameters = new float[] { 30f, 33f, 55f, 66f },
                Addresses = new string[] {  },
                IntSample = 0,
                floatArraySample = new float[] {  },
                EnumSample = EnumSample.Member3,
                HandType = HandType.Pa,
                Category = Category.Weapon
            }
        };
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

    public enum Category
    {
        Potion,
        Equipment,
        Weapon
    }
}