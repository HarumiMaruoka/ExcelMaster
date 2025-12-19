using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace ExcelMaster.Builders
{
    public static class WorkbookGenerator
    {
        public static void Generate(string excelPath, string projectFilePath = null)
        {
            if (string.IsNullOrWhiteSpace(excelPath)) throw new ArgumentException("excelPath is required.", nameof(excelPath));

            var workbook = ExcelUtil.ReadWorkbook(excelPath, startRow: 1, startColumn: 1, trimCells: true);
            var rootNamespace = LoadRootNamespace(projectFilePath) ?? string.Empty;
            var configSheets = FindConfigSheets(workbook);
            if (configSheets.Count == 0) throw new InvalidOperationException("WorkbookConfig sheet was not found in the workbook.");

            foreach (var configSheet in configSheets)
            {
                var config = WorkbookBuildConfig.FromSheet(configSheet);
                var dataSheet = GetDataSheet(workbook, config.SheetName);
                var extraUsings = CollectUsings(rootNamespace, config.AdditionalUsings);

                var classSource = SourceBuilder.GenerateClassSource(config.Namespace, extraUsings, config.ClassName, dataSheet, config.TableName);
                var dataAndBuilderSource = SourceBuilder.GenerateDataAndBuilder(config.Namespace, extraUsings, config.ClassName, dataSheet, config.BinaryOutputPath, config.SheetName);

                Directory.CreateDirectory(config.ClassOutputDirectoryResolved);
                Directory.CreateDirectory(config.BuilderOutputDirectoryResolved);
                File.WriteAllText(config.ClassFilePath, classSource);
                File.WriteAllText(config.DataAndBuilderFilePath, dataAndBuilderSource);
            }
        }

        private static List<string[][]> FindConfigSheets(Dictionary<string, string[][]> workbook)
        {
            var result = new List<string[][]>();
            foreach (var kv in workbook)
            {
                var sheetName = kv.Key;
                var sheet = kv.Value;
                if (sheet == null || sheet.Length == 0) continue;
                var firstRow = sheet[0];
                if (firstRow == null || firstRow.Length == 0) continue;
                // Only support WorkbookConfig sheet (name-based)
                bool isConfigName = string.Equals(sheetName, "WorkbookConfig", StringComparison.OrdinalIgnoreCase);
                if (isConfigName)
                {
                    result.Add(sheet);
                }
            }
            return result;
        }

        private static string[][] GetDataSheet(Dictionary<string, string[][]> workbook, string sheetName)
        {
            if (workbook.TryGetValue(sheetName, out var sheet)) return sheet;
            foreach (var kv in workbook)
            {
                if (string.Equals(kv.Key, sheetName, StringComparison.OrdinalIgnoreCase))
                {
                    return kv.Value;
                }
            }
            throw new InvalidOperationException($"Data sheet '{sheetName}' was not found in the workbook.");
        }

        private static string[] CollectUsings(string rootNamespace, IReadOnlyList<string> additionalUsings)
        {
            IEnumerable<string> src = Array.Empty<string>();
            if (!string.IsNullOrWhiteSpace(rootNamespace))
            {
                src = src.Append(rootNamespace.Trim());
            }
            if (additionalUsings != null)
            {
                foreach (var u in additionalUsings)
                {
                    if (!string.IsNullOrWhiteSpace(u))
                    {
                        src = src.Append(u.Trim());
                    }
                }
            }
            return src.Distinct(StringComparer.Ordinal).ToArray();
        }

        private static string LoadRootNamespace(string projectFilePath)
        {
            var csproj = ResolveProjectFilePath(projectFilePath);
            if (csproj == null) return null;
            if (!File.Exists(csproj)) return null;

            try
            {
                var doc = XDocument.Load(csproj);
                var ns = doc.Root?.GetDefaultNamespace() ?? XNamespace.None;
                string Read(string name) => doc.Root?.Descendants(ns + name).FirstOrDefault()?.Value;
                var rootNs = Read("RootNamespace");
                if (!string.IsNullOrWhiteSpace(rootNs)) return rootNs.Trim();
                var asm = Read("AssemblyName");
                if (!string.IsNullOrWhiteSpace(asm)) return asm.Trim();
                return Path.GetFileNameWithoutExtension(csproj);
            }
            catch
            {
                return null;
            }
        }

        private static string ResolveProjectFilePath(string projectFilePath)
        {
            if (!string.IsNullOrWhiteSpace(projectFilePath)) return projectFilePath;

            var cwd = Directory.GetCurrentDirectory();
            var directHit = Directory.EnumerateFiles(cwd, "*.csproj", SearchOption.TopDirectoryOnly).FirstOrDefault();
            if (directHit != null) return directHit;

            var dir = new DirectoryInfo(cwd);
            while (dir != null)
            {
                var found = Directory.EnumerateFiles(dir.FullName, "*.csproj", SearchOption.TopDirectoryOnly).FirstOrDefault();
                if (found != null) return found;
                dir = dir.Parent;
            }

            return null;
        }
    }
}
