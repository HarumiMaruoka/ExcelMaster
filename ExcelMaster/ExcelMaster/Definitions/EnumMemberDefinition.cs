using System.Collections.Generic;

namespace ExcelMaster
{
    public class EnumMemberDefinition
    {
        public List<string> Attributes { get; set; } = new List<string>();
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;

        public EnumMemberDefinition() { }
        public EnumMemberDefinition(string name, string value, IEnumerable<string>? attributes = null)
        {
            Name = name;
            Value = value;
            if (attributes != null) Attributes = new List<string>(attributes);
        }
    }
}