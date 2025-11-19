using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace CsvToSource
{
    public class CsvClassGenerator
    {
        public static string Parse(IEnumerable<string> usingNamespaces, string @namespace, string className, string[,] csv)
        {
            if (csv == null) throw new ArgumentNullException(nameof(csv));

            int rows = csv.GetLength(0);
            int cols = csv.GetLength(1);
            if (rows < 2 || cols == 0)
            {
                throw new ArgumentException("csv must contain at least header and type rows.", nameof(csv));
            }

            string GetCell(int r, int c)
            {
                var v = csv[r, c];
                return v == null ? string.Empty : v.Trim();
            }

            // header row (property names)
            var headers = new string[cols];
            for (int c = 0; c < cols; c++) headers[c] = GetCell(0, c);

            // type row (may contain attributes like [JsonIgnore]int)
            var types = new string[cols];
            for (int c = 0; c < cols; c++) types[c] = GetCell(1, c);

            var properties = new List<PropertyDefinition>();
            var enumDefs = new List<EnumDefinition>();

            // Traverse columns, grouping array slices (extra empty header cells belong to the previous array column)
            for (int c = 0; c < cols; c++)
            {
                string header = headers[c];
                string rawTypeToken = types[c];

                if (string.IsNullOrWhiteSpace(header))
                {
                    // skip continuation cells of an array field
                    continue;
                }

                // determine the group span for this header (until next non-empty header or end)
                int groupStart = c;
                int groupEnd = c; // inclusive
                int look = c + 1;
                while (look < cols && string.IsNullOrWhiteSpace(headers[look]))
                {
                    groupEnd = look;
                    look++;
                }

                // parse attributes from the type token (pattern: [Attr][Attr2]Type)
                var attrList = new List<string>();
                string typeToken = rawTypeToken; // will strip attributes below
                if (!string.IsNullOrWhiteSpace(typeToken))
                {
                    typeToken = typeToken.Trim();
                    while (typeToken.StartsWith("[") )
                    {
                        int close = typeToken.IndexOf(']');
                        if (close <= 0) break; // malformed, stop parsing attributes
                        string attrContent = typeToken.Substring(1, close - 1).Trim();
                        if (!string.IsNullOrWhiteSpace(attrContent))
                        {
                            // store without surrounding brackets (MasterClassBuilder re-wraps)
                            attrList.Add(attrContent);
                        }
                        typeToken = typeToken.Substring(close + 1).TrimStart();
                    }
                }

                string propName = ToPascalIdentifier(header);
                string propType;

                if (string.Equals(typeToken, "enum", StringComparison.OrdinalIgnoreCase))
                {
                    // enum type: use property name as enum type name
                    string enumTypeName = propName;
                    propType = enumTypeName;

                    // collect distinct enum members from data rows across the group columns
                    var membersOrdered = new List<string>();
                    var seen = new HashSet<string>(StringComparer.Ordinal);
                    for (int r = 2; r < rows; r++)
                    {
                        for (int gc = groupStart; gc <= groupEnd; gc++)
                        {
                            var raw = GetCell(r, gc);
                            if (string.IsNullOrWhiteSpace(raw)) continue;
                            var mem = ToValidIdentifier(raw);
                            if (seen.Add(mem)) membersOrdered.Add(mem);
                        }
                    }

                    enumDefs.Add(new EnumDefinition
                    {
                        Namespace = @namespace,
                        Name = enumTypeName,
                        Members = membersOrdered.GetEnumerator()
                    });
                }
                else
                {
                    // non-enum: use type token as-is (trim only)
                    propType = typeToken?.Trim();
                }

                properties.Add(new PropertyDefinition
                {
                    Name = propName,
                    Type = propType,
                    Attributes = attrList.Count > 0 ? attrList : null
                });

                // skip the grouped columns we just processed
                c = groupEnd;
            }

            // Build using MasterClassBuilder
            var classDef = new ClassDefinition
            {
                Namespace = @namespace,
                ClassName = className,
                Properties = properties
            };

            var source = MasterClassGenerator.Generate(usingNamespaces, classDef, enumDefs);
            return source;
        }

        private static string ToPascalIdentifier(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "Field";
            var parts = new List<string>();
            var sb = new StringBuilder();
            foreach (var ch in input)
            {
                if (char.IsLetterOrDigit(ch))
                {
                    sb.Append(ch);
                }
                else
                {
                    if (sb.Length > 0)
                    {
                        parts.Add(sb.ToString());
                        sb.Clear();
                    }
                }
            }
            if (sb.Length > 0) parts.Add(sb.ToString());
            if (parts.Count == 0) parts.Add("Field");
            var result = string.Concat(parts.Select(p => char.ToUpperInvariant(p[0]) + p.Substring(1)));
            if (string.IsNullOrEmpty(result)) result = "Field";
            if (!IsIdentifierStart(result[0])) result = "_" + result;
            var final = new StringBuilder();
            foreach (var ch in result)
            {
                final.Append(IsIdentifierPart(ch) ? ch : '_');
            }
            return final.ToString();
        }

        private static string ToValidIdentifier(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "Member";
            var trimmed = input.Trim();
            var sb = new StringBuilder();
            for (int i = 0; i < trimmed.Length; i++)
            {
                var ch = trimmed[i];
                if (i == 0)
                {
                    if (!IsIdentifierStart(ch)) sb.Append('_');
                }
                sb.Append(IsIdentifierPart(ch) ? ch : '_');
            }
            var s = sb.ToString();
            if (string.IsNullOrWhiteSpace(s)) s = "Member";
            return s;
        }

        private static bool IsIdentifierStart(char ch) => char.IsLetter(ch) || ch == '_';
        private static bool IsIdentifierPart(char ch) => char.IsLetterOrDigit(ch) || ch == '_';
    }
}
