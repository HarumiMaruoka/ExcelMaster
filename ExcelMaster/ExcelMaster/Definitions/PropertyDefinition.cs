using System.Collections.Generic;

namespace ExcelMaster
{
    public class PropertyDefinition
    {
        public List<string> Attributes { get; set; } = new List<string>();
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;

        public PropertyDefinition() { }
        public PropertyDefinition(string name, string type, IEnumerable<string>? attributes = null)
        {
            Name = name;
            Type = type;
            if (attributes != null) Attributes = new List<string>(attributes);
        }
    }
}