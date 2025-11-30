using System;
using System.Collections.Generic;
using System.Text;

namespace ExcelMaster
{
    public class EnumBuilder
    {
        public static string Build(EnumDefinition enumDef)
        {
            var sb = new StringBuilder();
            // Using directives
            foreach (var ns in enumDef.UsingNamespaces)
            {
                sb.AppendLine($"using {ns};");
            }
            if (enumDef.UsingNamespaces.Count > 0) sb.AppendLine();
            // Namespace declaration
            sb.AppendLine($"namespace {enumDef.Namespace}");
            sb.AppendLine("{");
            // Enum attributes
            foreach (var attr in enumDef.Attributes)
            {
                sb.AppendLine($"    [{attr}]");
            }
            // Enum declaration
            sb.AppendLine($"    public enum {enumDef.Name}");
            sb.AppendLine("    {");
            // Enum members
            for (int i = 0; i < enumDef.Members.Count; i++)
            {
                var member = enumDef.Members[i];
                foreach (var attr in member.Attributes)
                {
                    sb.AppendLine($"        [{attr}]");
                }
                var comma = i < enumDef.Members.Count - 1 ? "," : string.Empty;
                sb.AppendLine($"        {member.Name} = {member.Value}{comma}");
            }
            sb.AppendLine("    }"); // End of enum
            sb.AppendLine("}"); // End of namespace
            return sb.ToString();
        }
    }
}
