using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ExcelMaster.Builders
{
    internal sealed class WorkbookBuildConfig
    {
        public string SheetName { get; private set; }
        public string ClassName { get; private set; }
        public string Namespace { get; private set; }
        public string TableName { get; private set; }
        public string BinaryName { get; private set; }
        public string ClassOutputDirectory { get; private set; }
        public string BuilderOutputDirectory { get; private set; }
        public string BinaryOutputDirectory { get; private set; }
        public IReadOnlyList<string> AdditionalUsings { get; private set; } = Array.Empty<string>();

        public string ClassOutputDirectoryResolved => string.IsNullOrWhiteSpace(ClassOutputDirectory) ? "Assets/Generated" : ClassOutputDirectory;
        public string BuilderOutputDirectoryResolved => string.IsNullOrWhiteSpace(BuilderOutputDirectory) ? "Assets/Generated" : BuilderOutputDirectory;
        public string BinaryOutputDirectoryResolved => string.IsNullOrWhiteSpace(BinaryOutputDirectory) ? "Assets/Generated" : BinaryOutputDirectory;
        public string BinaryOutputPath => Path.Combine(BinaryOutputDirectoryResolved, BinaryName ?? $"{ClassName}.bytes");

        public string ClassFilePath => Path.Combine(ClassOutputDirectoryResolved, $"{ClassName}.cs");
        public string DataAndBuilderFilePath => Path.Combine(BuilderOutputDirectoryResolved, $"{ClassName}Builder.cs");

        public static WorkbookBuildConfig FromSheet(string[][] sheet)
        {
            if (sheet == null || sheet.Length == 0) throw new ArgumentException("WorkbookConfig sheet is empty.");

            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (int r = 1; r < sheet.Length; r++)
            {
                var row = sheet[r];
                if (row == null || row.Length == 0) continue;
                var key = row[0]?.Trim();
                if (string.IsNullOrWhiteSpace(key)) continue;
                var value = row.Length > 1 ? row[1]?.Trim() : null;
                map[key] = value;
            }

            var cfg = new WorkbookBuildConfig
            {
                SheetName = Pick(map, "SheetName"),
                ClassName = Pick(map, "ClassName"),
                Namespace = Pick(map, "Namespace"),
                TableName = Pick(map, "TableName"),
                BinaryName = Pick(map, "BinaryName"),
                ClassOutputDirectory = Pick(map, "ClassOutputDirectory"),
                BuilderOutputDirectory = Pick(map, "BuilderOutputDirectory"),
                BinaryOutputDirectory = Pick(map, "BinaryOutputDirectory"),
                AdditionalUsings = ParseAdditionalUsings(map)
            };

            cfg.BinaryName ??= $"{cfg.ClassName}.bytes";
            cfg.Validate();
            return cfg;
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(SheetName)) throw new InvalidOperationException("WorkbookConfig: 'SheetName' is required.");
            if (string.IsNullOrWhiteSpace(ClassName)) throw new InvalidOperationException("WorkbookConfig: 'ClassName' is required.");
            if (string.IsNullOrWhiteSpace(Namespace)) throw new InvalidOperationException("WorkbookConfig: 'Namespace' is required.");
        }

        private static string Pick(IDictionary<string, string> map, string key)
        {
            return map.TryGetValue(key, out var value) ? value : null;
        }

        private static IReadOnlyList<string> ParseAdditionalUsings(Dictionary<string, string> map)
        {
            var list = new List<(int index, string value)>();
            foreach (var kv in map)
            {
                if (kv.Key.StartsWith("AdditionalUsings", StringComparison.OrdinalIgnoreCase))
                {
                    var suffix = kv.Key.Substring("AdditionalUsings".Length);
                    if (int.TryParse(suffix, out var idx))
                    {
                        if (!string.IsNullOrWhiteSpace(kv.Value))
                        {
                            list.Add((idx, kv.Value.Trim()));
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(kv.Value))
                        {
                            list.Add((int.MaxValue, kv.Value.Trim()));
                        }
                    }
                }
            }
            return list
                .OrderBy(x => x.index)
                .Select(x => x.value)
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Distinct(StringComparer.Ordinal)
                .ToArray();
        }
    }
}
