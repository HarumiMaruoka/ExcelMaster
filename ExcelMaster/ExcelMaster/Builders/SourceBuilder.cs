using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExcelMaster.Builders
{
    public class SourceBuilder
    {
        // Generates C# source code from a selection range (first row: meta, second row: headers, third row: type hints)
        public static string GenerateClassSource(string @namespace, IEnumerable<string> usingNamespaces, string className, string[][] selection, string tableName = null)
        {
            ResolveLayout(selection, ref @namespace, ref className, out var headers, out var typeHints, out var dataStartRow);

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
            tableName = string.IsNullOrWhiteSpace(tableName)
                ? (className.EndsWith("Data", StringComparison.OrdinalIgnoreCase) ? className[..^4] : className)
                : tableName;

            // Build column groups: each non-empty header starts a group spanning until next non-empty header
            var groups = BuildGroups(headers, typeHints);

            // Class declaration with attributes similar to Sample.cs
            W($"[MemoryTable(\"{tableName}\"), MessagePackObject(true)]");
            W($"public sealed class {className}");
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

            indent--;
            W("}");
            return sb.ToString();
        }

        // New: build a nice standalone file that contains only Data in a builder class with namespace/usings
        public static string GenerateDataSection(string @namespace, IEnumerable<string> usingNamespaces, string className, string[][] selection)
        {
            ResolveLayout(selection, ref @namespace, ref className, out var headers, out var typeHints, out var dataStartRow);
            var groups = BuildGroups(headers, typeHints);

            var builderClassName = className + "Builder";
            var sb = new StringBuilder();
            var usings = new HashSet<string>(usingNamespaces ?? Enumerable.Empty<string>());
            usings.Add("System.Collections.Generic");
            foreach (var ns in usings) sb.AppendLine($"using {ns};");
            sb.AppendLine();
            sb.AppendLine($"namespace {@namespace}");
            sb.AppendLine("{");
            sb.AppendLine($"    public sealed class {builderClassName}");
            sb.AppendLine("    {");
            sb.Append(Indent(EmitDataSection(className, groups, selection, 0, dataStartRow), 2));
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        // New: build a single file that contains both Data section and BinaryBuilder in the same builder class
        public static string GenerateDataAndBuilder(string @namespace, IEnumerable<string> usingNamespaces, string className, string[][] selection, string defaultOutputPath = null, string sheetName = null)
        {
            ResolveLayout(selection, ref @namespace, ref className, out var headers, out var typeHints, out var dataStartRow);
            var groups = BuildGroups(headers, typeHints);

            var builderClassName = className + "Builder";
            var sb = new StringBuilder();
            var usings = new HashSet<string>(usingNamespaces ?? Enumerable.Empty<string>());
            // Required usings
            usings.Add("System");
            usings.Add("System.IO");
            usings.Add("System.Collections.Generic");
            usings.Add("MasterMemory");
            usings.Add("MessagePack");
            usings.Add("MessagePack.Resolvers");
            usings.Add("ExcelMaster");
            foreach (var ns in usings) sb.AppendLine($"using {ns};");
            sb.AppendLine();

            var tableName = className.EndsWith("Data", StringComparison.OrdinalIgnoreCase) ? className[..^4] : className;
            sheetName = string.IsNullOrWhiteSpace(sheetName) ? tableName : sheetName;

            sb.AppendLine($"namespace {@namespace}");
            sb.AppendLine("{");
            sb.AppendLine($"    public sealed class {builderClassName}");
            sb.AppendLine("    {");

            // Data section
            sb.Append(Indent(EmitDataSection(className, groups, selection, 0, dataStartRow), 2));
            sb.AppendLine();

            // Binary builder section
            sb.AppendLine($"        [ExcelBinaryBuilder(\"{sheetName}\")]");
            sb.AppendLine("        public static void BuildBinary(string outputPath = null)");
            sb.AppendLine("        {");
            sb.AppendLine("            BuildBinary(Data, outputPath);");
            sb.AppendLine("        }");
            sb.AppendLine();
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

        // New: build a binary builder class file similar to ItemDataBinaryBuilder.cs (improved indentation)
        public static string GenerateBinaryBuilder(string @namespace, IEnumerable<string> usingNamespaces, string className, string defaultOutputPath = null, string sheetName = null)
        {
            var sb = new StringBuilder();
            var usings = new HashSet<string>(usingNamespaces ?? Enumerable.Empty<string>());
            var builderClassName = className + "Builder";

            // Required usings
            usings.Add("System");
            usings.Add("System.IO");
            usings.Add("System.Collections.Generic");
            usings.Add("MasterMemory");
            usings.Add("MessagePack");
            usings.Add("MessagePack.Resolvers");
            usings.Add("ExcelMaster");
            foreach (var ns in usings) sb.AppendLine($"using {ns};");
            sb.AppendLine();

            var tableName = className.EndsWith("Data", StringComparison.OrdinalIgnoreCase) ? className[..^4] : className;
            sheetName = string.IsNullOrWhiteSpace(sheetName) ? tableName : sheetName;

            sb.AppendLine($"namespace {@namespace}");
            sb.AppendLine("{");
            sb.AppendLine($"    public sealed class {builderClassName}");
            sb.AppendLine("    {");
            sb.AppendLine($"        [ExcelBinaryBuilder(\"{sheetName}\")]");
            sb.AppendLine("        public static void BuildBinary(string outputPath = null)");
            sb.AppendLine("        {");
            sb.AppendLine("            BuildBinary(Data, outputPath);");
            sb.AppendLine("        }");
            sb.AppendLine();
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

        private static string EmitDataSection(string className, List<ColumnGroup> groups, string[][] selection, int baseIndent, int dataStartRow = 2)
        {
            var sb = new StringBuilder();
            int indent = baseIndent;
            void W(string line) => sb.AppendLine(new string(' ', indent * 4) + line);
            W($"public readonly static List<{className}> Data = new List<{className}>()");
            W("{");
            indent++;
            for (int r = dataStartRow; r < selection.Length; r++)
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

        private static void ResolveLayout(string[][] selection, ref string @namespace, ref string className, out string[] headers, out string[] typeHints, out int dataStartRow)
        {
            if (selection == null || selection.Length == 0) throw new ArgumentException("selection must contain at least one row.");

            // Simplified layout: first row is headers, second row is type hints (if present), remaining are data
            int headerRow = 0;
            if (selection.Length <= headerRow) throw new ArgumentException("selection must contain a header row.");
            headers = selection[headerRow] ?? Array.Empty<string>();

            int typeRowIndex = headerRow + 1;
            if (selection.Length > typeRowIndex)
            {
                typeHints = selection[typeRowIndex] ?? Array.Empty<string>();
                dataStartRow = typeRowIndex + 1;
            }
            else
            {
                typeHints = headers.Length > 0 ? new string[headers.Length] : Array.Empty<string>();
                dataStartRow = headerRow + 1;
            }
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
                var (attr, rawType) = ParseType(typeHints, i);
                // Map enum pseudo type (enum or enum:Type) to actual enum name; enums are defined elsewhere.
                bool isEnum = TryResolveEnumType(rawType, name, out var type);
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
            var hint = (typeHints[index] ?? string.Empty).Trim();
            string attr = string.Empty;
            string type = hint;

            // Extract optional attribute like "[JsonIgnore]int" or "[PrimaryKey]int"
            if (hint.StartsWith("["))
            {
                var close = hint.IndexOf(']');
                if (close > 0 && close < hint.Length - 1)
                {
                    attr = hint.Substring(1, close - 1);
                    type = hint.Substring(close + 1).Trim();
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
                default:
                    return (attr, string.IsNullOrWhiteSpace(type) ? "string" : type);
            }
        }

        private static bool TryResolveEnumType(string rawType, string headerName, out string resolvedType)
        {
            if (string.Equals(rawType, "enum", StringComparison.OrdinalIgnoreCase))
            {
                resolvedType = headerName; // fallback to header name when type not specified
                return true;
            }

            const string enumPrefix = "enum:";
            if (!string.IsNullOrWhiteSpace(rawType) && rawType.StartsWith(enumPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var enumType = rawType.Substring(enumPrefix.Length).Trim();
                resolvedType = string.IsNullOrWhiteSpace(enumType) ? headerName : enumType;
                return true;
            }

            resolvedType = rawType;
            return false;
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
