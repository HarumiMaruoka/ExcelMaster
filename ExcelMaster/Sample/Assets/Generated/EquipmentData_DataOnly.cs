using System.Collections.Generic;

namespace GameNamespace
{
    public sealed partial class EquipmentData
    {
        public readonly static List<EquipmentData> Data = new List<EquipmentData>()
        {
            new EquipmentData
            {
                Id = 1,
                Name = "剣",
                Parameters = new float[] { 1.0f },
                IntSample = 1,
                Category = Category.Weapon,
                Addresses = new string[] { "sprite", "model" }
            },
            new EquipmentData
            {
                Id = 2,
                Name = "盾",
                Parameters = new float[] { 10.0f, 20.0f },
                IntSample = 2,
                Category = Category.Shield,
                Addresses = new string[] {  }
            },
            new EquipmentData
            {
                Id = 3,
                Name = "アーマー",
                Parameters = new float[] { 30.0f, 33.0f, 55.0f, 66.0f },
                IntSample = 3,
                Category = Category.Armor,
                Addresses = new string[] {  }
            }
        };

    }
}
