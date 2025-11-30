using System;
using System.Collections.Generic;
using System.Text;

namespace ExcelMaster
{
    public class EnumDefinition
    {
        public List<string> UsingNamespaces { get; set; } = new List<string>();
        public string Namespace { get; set; } = string.Empty;
        public List<string> Attributes { get; set; } = new List<string>();
        public string Name { get; set; } = string.Empty;
        public List<EnumMemberDefinition> Members { get; set; } = new List<EnumMemberDefinition>();

        public EnumDefinition() { }
        public EnumDefinition(
            string @namespace,
            string name,
            IEnumerable<string>? usingNamespaces = null,
            IEnumerable<string>? attributes = null,
            IEnumerable<EnumMemberDefinition>? members = null)
        {
            Namespace = @namespace;
            Name = name;
            if (usingNamespaces != null) UsingNamespaces = new List<string>(usingNamespaces);
            if (attributes != null) Attributes = new List<string>(attributes);
            if (members != null) Members = new List<EnumMemberDefinition>(members);
        }
    }
}
