using System;
using System.Collections.Generic;
using System.Text;

namespace ExcelMaster
{
    public class ClassBuilder
    {
        public static string Build(ClassDefinition classDef)
        {
            var sb = new StringBuilder();
            // Using directives
            foreach (var ns in classDef.UsingNamespaces)
            {
                sb.AppendLine($"using {ns};");
            }
            sb.AppendLine();
            // Namespace declaration
            sb.AppendLine($"namespace {classDef.Namespace}");
            sb.AppendLine("{");
            // Class attributes
            foreach (var attr in classDef.Attributes)
            {
                sb.AppendLine($"    [{attr}]");
            }
            // Class declaration
            sb.AppendLine($"    public class {classDef.Name}");
            sb.AppendLine("    {");
            // Fields / Properties
            foreach (var field in classDef.Fields)
            {
                foreach (var attr in field.Attributes)
                {
                    sb.AppendLine($"        [{attr}]");
                }
                sb.AppendLine($"        public {field.Type} {field.Name} {{ get; set; }}");
                sb.AppendLine();
            }
            sb.AppendLine("    }"); // End of class
            sb.AppendLine("}"); // End of namespace
            return sb.ToString();
        }
    }
}