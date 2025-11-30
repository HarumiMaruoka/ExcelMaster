using System;
using System.Collections.Generic;
using System.Text;

namespace ExcelMaster
{
    public class ClassDefinition
    {
        public List<string> UsingNamespaces { get; set; } = new List<string>();
        public string Namespace { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public List<string> Attributes { get; set; } = new List<string>();
        public List<PropertyDefinition> Fields { get; set; } = new List<PropertyDefinition>();

        public ClassDefinition()
        {
        }

        public ClassDefinition(
            string @namespace,
            string name,
            IEnumerable<string>? usingNamespaces = null,
            IEnumerable<string>? attributes = null,
            IEnumerable<PropertyDefinition>? fields = null)
        {
            Namespace = @namespace;
            Name = name;
            if (usingNamespaces != null) UsingNamespaces = new List<string>(usingNamespaces);
            if (attributes != null) Attributes = new List<string>(attributes);
            if (fields != null) Fields = new List<PropertyDefinition>(fields);
        }
    }
}
