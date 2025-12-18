using System.Collections.Generic;

namespace Confront
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
            },
            new Item
            {
                Id = 0,
                Name = "",
                Parameters = new float[] {  },
                Addresses = new string[] {  },
                IntSample = 0,
                floatArraySample = new float[] {  },
                EnumSample = (EnumSample)0,
                HandType = (HandType)0,
                ItemCategory = (ItemCategory)0
            }
        };

    }
}
