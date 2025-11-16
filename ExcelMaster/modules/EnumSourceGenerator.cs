using System;
using System.Collections.Generic;
using System.Text;

namespace ExcelMaster
{
    public static class EnumSourceGenerator
    {
        public static string Generate(IEnumerator<string> usingNamespaces, EnumDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            if (string.IsNullOrWhiteSpace(definition.Name)) throw new ArgumentException("Name is required", nameof(definition));
            var sb = new StringBuilder();
            // using directives
            if (usingNamespaces != null)
            {
                var set = new HashSet<string>(StringComparer.Ordinal);
                while (usingNamespaces.MoveNext())
                {
                    var u = usingNamespaces.Current?.Trim();
                    if (string.IsNullOrWhiteSpace(u)) continue;
                    // remove trailing semicolon if caller provided
                    if (u.EndsWith(";")) u = u.Substring(0, u.Length - 1).TrimEnd();
                    set.Add(u);
                }
                foreach (var u in set)
                {
                    sb.AppendLine($"using {u};");
                }
                if (set.Count > 0) sb.AppendLine();
            }
            bool hasNamespace = !string.IsNullOrWhiteSpace(definition.Namespace);
            if (hasNamespace)
            {
                sb.AppendLine($"namespace {definition.Namespace}");
                sb.AppendLine("{");
            }
            sb.AppendLine($"public enum {definition.Name}");
            sb.AppendLine("{");
            // Iterate members
            if (definition.Members != null)
            {
                var memberEnumerator = definition.Members;
                bool first = true;
                while (memberEnumerator.MoveNext())
                {
                    var member = memberEnumerator.Current;
                    if (string.IsNullOrWhiteSpace(member)) continue;
                    if (!first)
                    {
                        sb.AppendLine(",");
                    }
                    sb.Append($"    {member.Trim()}");
                    first = false;
                }
                sb.AppendLine();
            }
            sb.AppendLine("}");
            if (hasNamespace)
            {
                sb.AppendLine("}");
            }
            return sb.ToString();
        }
    }

    public class EnumDefinition
    {
        public string Namespace { get; set; }
        public string Name { get; set; }
        public IEnumerator<string> Members { get; set; }
    }
}
