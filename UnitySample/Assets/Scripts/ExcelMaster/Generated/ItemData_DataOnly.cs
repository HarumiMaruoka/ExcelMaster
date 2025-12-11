using System.Collections.Generic;

namespace GameNamespace
{
    public sealed partial class ItemData
    {
        public readonly static List<ItemData> Data = new List<ItemData>()
        {
            new ItemData
            {
                Id = 1,
                Name = "HealPotion",
                Parameters = new float[] { 1.0f },
                Addresses = new string[] { "sprite", "model", "description" },
                IntSample = 0,
                floatArraySample = new float[] {  },
                EnumSample = EnumSample.Member1,
                HandType = HandType.Goo,
                Category = Category.Potion
            },
            new ItemData
            {
                Id = 2,
                Name = "AttackPotion",
                Parameters = new float[] { 10.0f, 20.0f },
                Addresses = new string[] {  },
                IntSample = 0,
                floatArraySample = new float[] {  },
                EnumSample = EnumSample.Member2,
                HandType = HandType.Pa,
                Category = Category.Equipment
            },
            new ItemData
            {
                Id = 3,
                Name = "DefencePotion",
                Parameters = new float[] { 30.0f, 33.0f, 55.0f, 66.0f },
                Addresses = new string[] {  },
                IntSample = 0,
                floatArraySample = new float[] {  },
                EnumSample = EnumSample.Member3,
                HandType = HandType.Pa,
                Category = Category.Weapon
            }
        };

    }
}
