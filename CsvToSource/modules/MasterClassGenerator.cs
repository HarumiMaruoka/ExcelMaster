using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExcelMaster
{
    public static class MasterClassGenerator
    {
        public static string Generate(IEnumerable<string> usingNamespaces, ClassDefinition classDefinition, IEnumerable<EnumDefinition> enumDefinitions = null)
        {
            if (classDefinition == null) throw new ArgumentNullException(nameof(classDefinition));
            if (string.IsNullOrWhiteSpace(classDefinition.ClassName)) throw new ArgumentException("ClassName is required", nameof(classDefinition));

            var sb = new StringBuilder();

            // helper for indentation (4 spaces per indent level)
            static void AppendIndentedLine(StringBuilder builder, int indentLevel, string text)
            {
                if (indentLevel > 0)
                {
                    builder.Append(new string(' ', indentLevel * 4));
                }
                builder.AppendLine(text);
            }

            int indent = 0;

            // using directives
            if (usingNamespaces != null)
            {
                var set = new HashSet<string>(StringComparer.Ordinal);
                foreach (var ns in usingNamespaces)
                {
                    var u = ns?.Trim();
                    if (string.IsNullOrWhiteSpace(u)) continue;
                    // remove trailing semicolon if caller provided
                    if (u.EndsWith(";")) u = u.Substring(0, u.Length - 1).TrimEnd();
                    set.Add(u);
                }
                foreach (var u in set.OrderBy(x => x, StringComparer.Ordinal))
                {
                    AppendIndentedLine(sb, 0, $"using {u};");
                }
                if (set.Count > 0) sb.AppendLine();
            }

            bool hasNamespace = !string.IsNullOrWhiteSpace(classDefinition.Namespace);
            if (hasNamespace)
            {
                AppendIndentedLine(sb, indent, $"namespace {classDefinition.Namespace}");
                AppendIndentedLine(sb, indent, "{");
                indent++;
            }

            AppendIndentedLine(sb, indent, $"public class {classDefinition.ClassName}");
            AppendIndentedLine(sb, indent, "{");
            indent++;

            // Iterate properties
            if (classDefinition.Properties != null)
            {
                // filter and materialize to control blank lines between properties
                var props = classDefinition.Properties
                    .Where(p => p != null && !string.IsNullOrWhiteSpace(p.Name) && !string.IsNullOrWhiteSpace(p.Type))
                    .ToList();

                for (int i = 0; i < props.Count; i++)
                {
                    var prop = props[i];

                    // Attributes
                    if (prop.Attributes != null)
                    {
                        foreach (var attr in prop.Attributes)
                        {
                            if (string.IsNullOrWhiteSpace(attr)) continue;
                            // remove surrounding brackets if provided
                            var trimmed = attr.Trim();
                            if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                            {
                                trimmed = trimmed.Substring(1, trimmed.Length - 2).Trim();
                            }
                            AppendIndentedLine(sb, indent, $"[{trimmed}]");
                        }
                    }

                    AppendIndentedLine(sb, indent, $"public {prop.Type} {prop.Name} {{ get; set; }}");

                    // insert blank line only between properties, not after the last one
                    if (i < props.Count - 1)
                    {
                        sb.AppendLine();
                    }
                }
            }

            indent--;
            AppendIndentedLine(sb, indent, "}"); // end class

            // enums (same namespace as the class; ignore EnumDefinition.Namespace)
            if (enumDefinitions != null)
            {
                // materialize valid enum definitions
                var enums = enumDefinitions
                    .Where(e => e != null && !string.IsNullOrWhiteSpace(e.Name))
                    .ToList();

                for (int i = 0; i < enums.Count; i++)
                {
                    var e = enums[i];

                    // collect members from IEnumerator<string>
                    var members = new List<string>();
                    if (e.Members != null)
                    {
                        var enumerator = e.Members;
                        while (enumerator.MoveNext())
                        {
                            var m = enumerator.Current;
                            if (!string.IsNullOrWhiteSpace(m))
                            {
                                members.Add(m.Trim());
                            }
                        }
                    }

                    // blank line between class and first enum, and between enums
                    sb.AppendLine();

                    AppendIndentedLine(sb, indent, $"public enum {e.Name}");
                    AppendIndentedLine(sb, indent, "{");

                    // members
                    if (members.Count > 0)
                    {
                        for (int mi = 0; mi < members.Count; mi++)
                        {
                            var suffix = mi < members.Count - 1 ? "," : string.Empty;
                            AppendIndentedLine(sb, indent + 1, members[mi] + suffix);
                        }
                    }

                    AppendIndentedLine(sb, indent, "}");
                }
            }

            if (hasNamespace)
            {
                indent--;
                AppendIndentedLine(sb, indent, "}"); // end namespace
            }

            return sb.ToString();
        }
    }

    public class ClassDefinition
    {
        public string Namespace { get; set; }
        public string ClassName { get; set; }
        public IEnumerable<PropertyDefinition> Properties { get; set; }
    }

    public class PropertyDefinition
    {
        public IEnumerable<string> Attributes { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }
}
