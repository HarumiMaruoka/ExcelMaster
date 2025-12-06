using MasterMemory;
using MessagePack;
using System.Collections.Generic;

namespace GameNamespace
{
    [MemoryTable("Equipment"), MessagePackObject(true)]
    public sealed partial class EquipmentData
    {
        [PrimaryKey]
        public int Id { get; set; }

        public string Name { get; set; }

        public float[] Parameters { get; set; }

        public int IntSample { get; set; }

        public Category Category { get; set; }

        public string[] Addresses { get; set; }

    }

    public enum Category
    {
        Weapon,
        Shield,
        Armor
    }
}
