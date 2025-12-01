using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExcelMaster.Builders
{
    public class SourceBuilder
    {
        // Generates C# source code from a selection range (first row: headers, second row: type hints)
        public static string Build(string @namespace, IEnumerable<string> usingNamespaces, string className, string[][] selection)
        {
            if (selection == null || selection.Length < 3) throw new ArgumentException("selection must contain at least header, type and one data row.");
            var headers = selection[0];
            var typeHints = selection[1];

            var sb = new StringBuilder();
            int indent = 0;
            void W(string line) => sb.AppendLine(new string(' ', indent * 4) + line);

            // Ensure required usings
            var usings = new HashSet<string>(usingNamespaces ?? Enumerable.Empty<string>());
            usings.Add("MasterMemory");
            usings.Add("MessagePack");
            usings.Add("System.Collections.Generic");
            foreach (var ns in usings) W($"using {ns};");

            sb.AppendLine();
            W($"namespace {@namespace}");
            W("{");
            indent++;

            // Determine MemoryTable name
            var tableName = className.EndsWith("Data", StringComparison.OrdinalIgnoreCase) ? className[..^4] : className;

            // Build column groups: each non-empty header starts a group spanning until next non-empty header
            var groups = BuildGroups(headers, typeHints);

            // Collect enum members from data rows for enum groups
            var enumMembers = new Dictionary<string, HashSet<string>>();
            foreach (var g in groups.Where(g => g.IsEnum)) enumMembers[g.Type] = new HashSet<string>();
            for (int r = 2; r < selection.Length; r++)
            {
                var row = selection[r];
                foreach (var g in groups.Where(g => g.IsEnum))
                {
                    var raw = GetFirstNonEmpty(row, g.Indices) ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(raw)) enumMembers[g.Type].Add(SanitizeIdentifier(raw));
                }
            }

            // Class declaration with attributes similar to Sample.cs, as partial
            W($"[MemoryTable(\"{tableName}\"), MessagePackObject(true)]");
            W($"public sealed partial class {className}");
            W("{");
            indent++;

            // Properties (only for groups with non-empty header)
            foreach (var g in groups)
            {
                if (!string.IsNullOrEmpty(g.Attribute)) W($"[{g.Attribute}]");
                W($"public {g.Type} {g.PropertyName} {{ get; set; }}");
                sb.AppendLine();
            }

            indent--;
            W("}");

            // Emit enums after class, inside namespace
            foreach (var kv in enumMembers)
            {
                sb.AppendLine();
                var enumName = kv.Key;
                var members = kv.Value.ToList();
                if (members.Count == 0) members.Add("None");
                W($"public enum {enumName}");
                W("{");
                indent++;
                for (int i = 0; i < members.Count; i++)
                {
                    var comma = i < members.Count - 1 ? "," : string.Empty;
                    W($"{members[i]}{comma}");
                }
                indent--;
                W("}");
            }

            indent--;
            W("}");
            return sb.ToString();
        }

        // New: build only the Data section for separate output file (raw snippet)
        public static string BuildDataOnly(string className, string[][] selection)
        {
            if (selection == null || selection.Length < 3) throw new ArgumentException("selection must contain at least header, type and one data row.");
            var headers = selection[0];
            var typeHints = selection[1];
            var groups = BuildGroups(headers, typeHints);
            return EmitDataSection(className, groups, selection, 0);
        }

        // New: build a nice standalone file that contains only Data in a partial class with namespace/usings
        public static string BuildDataFile(string @namespace, IEnumerable<string> usingNamespaces, string className, string[][] selection)
        {
            if (selection == null || selection.Length < 3) throw new ArgumentException("selection must contain at least header, type and one data row.");
            var headers = selection[0];
            var typeHints = selection[1];
            var groups = BuildGroups(headers, typeHints);

            var sb = new StringBuilder();
            var usings = new HashSet<string>(usingNamespaces ?? Enumerable.Empty<string>());
            usings.Add("System.Collections.Generic");
            foreach (var ns in usings) sb.AppendLine($"using {ns};");
            sb.AppendLine();
            sb.AppendLine($"namespace {@namespace}");
            sb.AppendLine("{");
            sb.AppendLine($"    public sealed partial class {className}");
            sb.AppendLine("    {");
            sb.Append(Indent(EmitDataSection(className, groups, selection, 0), 2));
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        // New: build a binary builder partial class file similar to ItemDataBinaryBuilder.cs (improved indentation)
        public static string BuildBinaryBuilderFile(string @namespace, IEnumerable<string> usingNamespaces, string className, string defaultOutputPath = null)
        {
            var sb = new StringBuilder();
            var usings = new HashSet<string>(usingNamespaces ?? Enumerable.Empty<string>());
            // Required usings
            usings.Add("System");
            usings.Add("System.IO");
            usings.Add("System.Collections.Generic");
            usings.Add("MasterMemory");
            usings.Add("MessagePack");
            usings.Add("MessagePack.Resolvers");
            foreach (var ns in usings) sb.AppendLine($"using {ns};");
            sb.AppendLine();
            sb.AppendLine($"namespace {@namespace}");
            sb.AppendLine("{");
            sb.AppendLine($"    public sealed partial class {className}");
            sb.AppendLine("    {");
            sb.AppendLine("        /// <summary>");
            sb.AppendLine($"        /// {className} 配列から MasterMemory バイナリを生成し保存します。");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine($"        /// <param name=\"masters\">{className} 配列</param>");
            sb.AppendLine("        /// <param name=\"outputPath\">出力パス。未指定時はデフォルトパスが使用されます。</param>");
            sb.AppendLine("        /// <returns>生成されたバイナリ</returns>");
            sb.AppendLine($"        public static byte[] BuildBinary(IEnumerable<{className}> masters, string outputPath = null)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (masters == null) throw new ArgumentNullException(nameof(masters));");
            sb.AppendLine("            outputPath ??= " + (defaultOutputPath == null ? $"\"Assets/Generated/{className}.bytes\"" : $"\"{Escape(defaultOutputPath)}\"") + ";");
            sb.AppendLine();
            sb.AppendLine("            var messagePackResolvers = CompositeResolver.Create(");
            sb.AppendLine("                MasterMemoryResolver.Instance,");
            sb.AppendLine("                StandardResolver.Instance");
            sb.AppendLine("            );");
            sb.AppendLine("            var options = MessagePackSerializerOptions.Standard.WithResolver(messagePackResolvers);");
            sb.AppendLine("            MessagePackSerializer.DefaultOptions = options;");
            sb.AppendLine();
            sb.AppendLine("            var builder = new DatabaseBuilder();");
            sb.AppendLine("            builder.Append(masters);");
            sb.AppendLine("            var binary = builder.Build();");
            sb.AppendLine();
            sb.AppendLine("            var dir = Path.GetDirectoryName(outputPath);");
            sb.AppendLine("            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);");
            sb.AppendLine("            File.WriteAllBytes(outputPath, binary);");
            sb.AppendLine();
            sb.AppendLine("            return binary;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        private static string Indent(string text, int level)
        {
            var pad = new string(' ', level * 4);
            var lines = text.Replace("\r\n", "\n").Split('\n');
            var sb = new StringBuilder();
            foreach (var line in lines)
            {
                if (line.Length == 0) { sb.AppendLine(); continue; }
                sb.AppendLine(pad + line);
            }
            return sb.ToString();
        }

        private static string EmitDataSection(string className, List<ColumnGroup> groups, string[][] selection, int baseIndent)
        {
            var sb = new StringBuilder();
            int indent = baseIndent;
            void W(string line) => sb.AppendLine(new string(' ', indent * 4) + line);
            W($"public readonly static List<{className}> Data = new List<{className}>()");
            W("{");
            indent++;
            for (int r = 2; r < selection.Length; r++)
            {
                var row = selection[r];
                W($"new {className}");
                W("{");
                indent++;
                for (int gi = 0; gi < groups.Count; gi++)
                {
                    var g = groups[gi];
                    var value = FormatGroupValue(g, row);
                    var comma = gi < groups.Count - 1 ? "," : string.Empty;
                    W($"{g.PropertyName} = {value}{comma}");
                }
                indent--;
                var trailingComma = r < selection.Length - 1 ? "," : string.Empty;
                W($"}}{trailingComma}");
            }
            indent--;
            W("};");
            return sb.ToString();
        }

        private static List<ColumnGroup> BuildGroups(string[] headers, string[] typeHints)
        {
            var groups = new List<ColumnGroup>();
            int i = 0;
            while (i < headers.Length)
            {
                // skip empty header columns
                if (string.IsNullOrWhiteSpace(headers[i])) { i++; continue; }
                var name = SanitizeIdentifier(headers[i]);
                var (attr, type) = ParseType(typeHints, i);
                // Map enum to real enum named by header
                bool isEnum = false;
                if (string.Equals(type, "enum", StringComparison.OrdinalIgnoreCase))
                {
                    type = name; // enum type name is header name
                    isEnum = true;
                }
                var indices = new List<int> { i };
                i++;
                while (i < headers.Length && string.IsNullOrWhiteSpace(headers[i]))
                {
                    indices.Add(i);
                    i++;
                }
                groups.Add(new ColumnGroup
                {
                    PropertyName = name,
                    Type = type,
                    Attribute = attr,
                    Indices = indices,
                    IsEnum = isEnum
                });
            }
            return groups;
        }

        private static string GetFirstNonEmpty(string[] row, List<int> indices)
        {
            foreach (var idx in indices)
            {
                if (idx < row.Length && !string.IsNullOrWhiteSpace(row[idx])) return row[idx];
            }
            return null;
        }

        private static string FormatGroupValue(ColumnGroup g, string[] row)
        {
            // enum single value: use first non-empty, default to (Type)0
            if (g.IsEnum)
            {
                var raw = GetFirstNonEmpty(row, g.Indices) ?? string.Empty;
                if (string.IsNullOrWhiteSpace(raw)) return $"({g.Type})0";
                return $"{g.Type}.{SanitizeIdentifier(raw)}";
            }

            // Arrays: aggregate across indices
            switch (g.Type)
            {
                case "int[]":
                    {
                        var items = CollectValues(row, g.Indices);
                        return $"new int[] {{ {JoinInts(string.Join(",", items))} }}";
                    }
                case "float[]":
                    {
                        var items = CollectValues(row, g.Indices);
                        return $"new float[] {{ {JoinFloats(string.Join(",", items))} }}";
                    }
                case "string[]":
                    {
                        var items = CollectValues(row, g.Indices);
                        return $"new string[] {{ {JoinStrings(string.Join(",", items))} }}";
                    }
            }

            // Scalars: use first non-empty value
            var first = GetFirstNonEmpty(row, g.Indices) ?? string.Empty;
            switch (g.Type)
            {
                case "int":
                    return int.TryParse(first, out var i) ? i.ToString() : "0";
                case "float":
                    return float.TryParse(first, out var f) ? f.ToString("0.0#################") + "f" : "0f";
                case "string":
                    return $"\"{Escape(first)}\"";
                default:
                    // Unknown types treated as strings
                    return $"\"{Escape(first)}\"";
            }
        }

        private static List<string> CollectValues(string[] row, List<int> indices)
        {
            var result = new List<string>();
            foreach (var idx in indices)
            {
                if (idx < row.Length)
                {
                    var v = row[idx];
                    if (!string.IsNullOrWhiteSpace(v)) result.Add(v);
                }
            }
            return result;
        }

        private static (string attr, string type) ParseType(string[] typeHints, int index)
        {
            if (typeHints == null || index >= typeHints.Length) return (string.Empty, "string");
            var hint = typeHints[index] ?? string.Empty;
            string attr = string.Empty;
            string type = hint;

            // Extract optional attribute like "[JsonIgnore]int" or "[PrimaryKey]int"
            if (hint.StartsWith("["))
            {
                var close = hint.IndexOf(']');
                if (close > 0 && close < hint.Length - 1)
                {
                    attr = hint.Substring(1, close - 1);
                    type = hint.Substring(close + 1);
                }
            }

            // Map pseudo types
            switch (type)
            {
                case "int": return (attr, "int");
                case "float": return (attr, "float");
                case "string": return (attr, "string");
                case "float[]": return (attr, "float[]");
                case "string[]": return (attr, "string[]");
                case "int[]": return (attr, "int[]");
                case "enum": return (attr, "enum"); // will be replaced with header name later
                default:
                    return (attr, string.IsNullOrWhiteSpace(type) ? "string" : type);
            }
        }

        private static string JoinInts(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
            var parts = raw.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
            var list = new List<string>();
            foreach (var p in parts)
            {
                list.Add(int.TryParse(p, out var v) ? v.ToString() : "0");
            }
            return string.Join(", ", list);
        }

        private static string JoinFloats(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
            var parts = raw.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
            var list = new List<string>();
            foreach (var p in parts)
            {
                list.Add(float.TryParse(p, out var v) ? v.ToString("0.0#################") + "f" : "0f");
            }
            return string.Join(", ", list);
        }

        private static string JoinStrings(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
            var parts = raw.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
            var list = new List<string>();
            foreach (var p in parts)
            {
                list.Add($"\"{Escape(p)}\"");
            }
            return string.Join(", ", list);
        }

        private static string SanitizeIdentifier(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "Field";
            var id = name.Trim();
            foreach (var ch in new[] { ' ', '-', '.', ':', ';', '/', '\\' }) id = id.Replace(ch, '_');
            if (!(char.IsLetter(id[0]) || id[0] == '_')) id = "_" + id;
            return id;
        }

        private static string Escape(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"");

        private class ColumnGroup
        {
            public string PropertyName { get; set; }
            public string Type { get; set; }
            public string Attribute { get; set; }
            public List<int> Indices { get; set; }
            public bool IsEnum { get; set; }
        }
    }
}