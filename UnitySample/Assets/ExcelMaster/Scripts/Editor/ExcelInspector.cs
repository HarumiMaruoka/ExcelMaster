using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System;
using ExcelMaster;

/// <summary>
/// Excelファイル選択時のみ Inspector を拡張するテンプレート
/// </summary>
[CustomEditor(typeof(DefaultAsset))]
public class ExcelInspector : Editor
{
    // 対象かどうかのフラグ
    private bool _isExcelFile;
    private string _assetPath;

    private static readonly string[] ExcelExtensions = new[]
    {
        ".xlsx",
        ".xlsm",
        ".xls",
    };

    // WorkbookConfig シートから読み取った設定一覧
    private readonly List<WorkbookConfigEntry> _workbookConfigs = new List<WorkbookConfigEntry>();

    //生成済みマスター(クラス名)仮取得
    private List<string> _generatedClassNames = new List<string>();

    [Serializable]
    private class WorkbookConfigEntry
    {
        public string configSheetName;
        public string sheetName;
        public string className;
        public string namespaceName;
        public string tableName;
        public string binaryName;
        public string classOutputDirectory;
        public string builderOutputDirectory;
        public string binaryOutputDirectory;
        public List<string> additionalUsings = new List<string>();
    }

    private void OnEnable()
    {
        // 選択中アセットのパス取得
        _assetPath = AssetDatabase.GetAssetPath(target);

        // 拡張子チェック
        string ext = Path.GetExtension(_assetPath)?.ToLowerInvariant();

        _isExcelFile = ExcelExtensions.Contains(ext);

        if (_isExcelFile)
        {
            LoadWorkbookConfigs();
        }
    }
    
    public override void OnInspectorGUI()
    {
        // Excelファイルでなければ、デフォルトInspectorを表示して終了
        if (!_isExcelFile)
        {
            base.OnInspectorGUI();
            return;
        }

        GUI.enabled = true;
        EditorGUILayout.LabelField("WorkbookConfig 一覧", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        if (_workbookConfigs.Count == 0)
        {
            EditorGUILayout.HelpBox("WorkbookConfig シートが見つかりません", MessageType.Info);
        }

        // シート表示
        foreach (var config in _workbookConfigs)
        {
            DrawSheetRow(config);
        }

        EditorGUILayout.Space(8);
    }

    private string _defaultClassDirectoryPath = "Assets/ExcelMaster/Data/Source";
    private string _defaultBuilderDirectoryPath = "Assets/ExcelMaster/Data/Source";
    private string _defaultBinaryDirectoryPath = "Assets/ExcelMaster/Data/Binary";

    private string ToAbsolutePathIfPossible(string maybeAssetsPath)
    {
        if (string.IsNullOrEmpty(maybeAssetsPath)) return Application.dataPath;
        if (maybeAssetsPath.Replace("\\", "/").StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
        {
            var rel = maybeAssetsPath.Substring("Assets".Length).TrimStart('/', '\\');
            return Path.Combine(Application.dataPath, rel);
        }
        return maybeAssetsPath;
    }

    private void DrawSheetRow(WorkbookConfigEntry config)
    {
        if (config == null) return;

        var sheetName = string.IsNullOrEmpty(config.sheetName) ? config.configSheetName : config.sheetName;
        var className = string.IsNullOrEmpty(config.className) ? sheetName : config.className;
        var namespaceName = string.IsNullOrEmpty(config.namespaceName) ? "GameNamespace" : config.namespaceName;
        var classDir = string.IsNullOrEmpty(config.classOutputDirectory) ? _defaultClassDirectoryPath : config.classOutputDirectory;
        var builderDir = string.IsNullOrEmpty(config.builderOutputDirectory) ? _defaultBuilderDirectoryPath : config.builderOutputDirectory;
        var binaryDir = string.IsNullOrEmpty(config.binaryOutputDirectory) ? _defaultBinaryDirectoryPath : config.binaryOutputDirectory;
        var classFileName = className + ".cs";
        var builderClassName = className + "Builder";
        var dataAndBuilderFileName = builderClassName + ".cs";
        var binaryFileName = string.IsNullOrEmpty(config.binaryName) ? (className + ".mmdb") : config.binaryName;
        var additionalUsings = NormalizeUsings(config.additionalUsings);

        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField(className, EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Sheet: {sheetName}");
        EditorGUILayout.LabelField($"Namespace: {namespaceName}");
        if (!string.IsNullOrEmpty(config.tableName))
        {
            EditorGUILayout.LabelField($"Table: {config.tableName}");
        }
        EditorGUILayout.LabelField($"Class Out: {classDir}");
        EditorGUILayout.LabelField($"Builder Out: {builderDir}");
        EditorGUILayout.LabelField($"Binary Out: {binaryDir}");
        EditorGUILayout.LabelField($"Binary Name: {binaryFileName}");
        if (additionalUsings.Length > 0)
        {
            EditorGUILayout.LabelField($"Usings: {string.Join(", ", additionalUsings)}");
        }

        var classFileRel = Path.Combine(classDir, classFileName).Replace("\\", "/");
        var builderFileRel = Path.Combine(builderDir, dataAndBuilderFileName).Replace("\\", "/");
        var binaryFileRel = Path.Combine(binaryDir, binaryFileName).Replace("\\", "/");

        bool classExists = File.Exists(ToAbsolutePathIfPossible(classFileRel));
        bool builderExists = File.Exists(ToAbsolutePathIfPossible(builderFileRel));
        bool binaryExists = File.Exists(ToAbsolutePathIfPossible(binaryFileRel));

        string classButtonLabel = classExists ? "クラス更新" : "クラス生成";
        string builderButtonLabel = builderExists ? "ビルダー更新" : "ビルダー生成";
        string binaryButtonLabel = binaryExists ? "バイナリ更新" : "バイナリ生成";

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button(classButtonLabel, GUILayout.Width(130)))
        {
            if (string.IsNullOrEmpty(sheetName))
            {
                Debug.LogError("WorkbookConfig の SheetName が未設定です。");
                return;
            }

            Debug.Log($"{classButtonLabel} for sheet {sheetName} (Class:{classDir}, Builder:{builderDir})");

            var selection = ExcelMaster.ExcelUtil.ReadExcelToStringArray(_assetPath, sheetName);
            string source = ExcelMaster.Builders.SourceBuilder.GenerateClassSource(namespaceName, additionalUsings, className, selection, config.tableName);
            var directoryPath = ToAbsolutePathIfPossible(classDir);
            var filePath = Path.Combine(directoryPath, classFileName);
            Directory.CreateDirectory(directoryPath);
            File.WriteAllText(filePath, source);

            AssetDatabase.Refresh();
        }
        if (GUILayout.Button(builderButtonLabel, GUILayout.Width(130)))
        {
            if (string.IsNullOrEmpty(sheetName))
            {
                Debug.LogError("WorkbookConfig の SheetName が未設定です。");
                return;
            }

            Debug.Log($"{builderButtonLabel} for sheet {sheetName} (Class:{classDir}, Builder:{builderDir})");

            var selection = ExcelMaster.ExcelUtil.ReadExcelToStringArray(_assetPath, sheetName);
            var defaultBinaryPath = Path.Combine(binaryDir, binaryFileName).Replace("\\", "/");
            string source = ExcelMaster.Builders.SourceBuilder.GenerateDataAndBuilder(
                namespaceName,
                additionalUsings,
                className,
                selection,
                defaultBinaryPath,
                sheetName);
            var directoryPath = ToAbsolutePathIfPossible(builderDir);
            var filePath = Path.Combine(directoryPath, dataAndBuilderFileName);
            Directory.CreateDirectory(directoryPath);
            File.WriteAllText(filePath, source);

            AssetDatabase.Refresh();
        }
        if (GUILayout.Button(binaryButtonLabel, GUILayout.Width(110)))
        {
            try
            {
                var targetSheetName = sheetName;

                // Assembly-CSharp 系だけを対象にするなど、必要に応じてフィルタ
                var targetMethod = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .SelectMany(a =>
                    {
                        // 不要なアセンブリを弾いて高速化（必要に応じて調整）
                        if (!a.FullName.StartsWith("Assembly-CSharp", StringComparison.Ordinal))
                        {
                            return Array.Empty<System.Reflection.MethodInfo>();
                        }

                        return a.GetTypes()
                            .SelectMany(t => t.GetMethods(
                                System.Reflection.BindingFlags.Public
                                | System.Reflection.BindingFlags.NonPublic
                                | System.Reflection.BindingFlags.Static));
                    })
                    .FirstOrDefault(m =>
                    {
                        var attr = (ExcelMaster.ExcelBinaryBuilderAttribute)Attribute.GetCustomAttribute(
                            m, typeof(ExcelMaster.ExcelBinaryBuilderAttribute));
                        return attr != null
                            && string.Equals(attr.SheetName, targetSheetName, StringComparison.Ordinal);
                    });

                if (targetMethod == null)
                {
                    Debug.LogError($"[{targetSheetName}] 用の ExcelBinaryBuilderAttribute を持つメソッドが見つかりません。ビルダーを生成済みか確認してください。");
                    return;
                }

                // シグネチャ検証: static void M(string excelPath, string sheetName, string outputBinaryPath)
                var parameters = targetMethod.GetParameters();
                if (parameters.Length != 1
                    || parameters[0].ParameterType != typeof(string)
                    || targetMethod.ReturnType != typeof(void))
                {
                    Debug.LogError(
                        $"[{targetSheetName}] のビルドメソッドのシグネチャが不正です: " +
                        $"{targetMethod.DeclaringType.FullName}.{targetMethod.Name}");
                    return;
                }

                var binaryDirAbs = ToAbsolutePathIfPossible(binaryDir);
                Directory.CreateDirectory(binaryDirAbs);
                var binaryPathAbs = Path.Combine(binaryDirAbs, binaryFileName);

                targetMethod.Invoke(
                    null,
                    new object[]
                    {
                        binaryPathAbs
                    });

                Debug.Log($"バイナリ出力完了: {binaryPathAbs}");
                AssetDatabase.Refresh();
            }
            catch (Exception ex)
            {
                Debug.LogError($"バイナリ生成に失敗しました: {ex}");
            }
        }
        if (binaryExists || classExists || builderExists)
        {
            if (GUILayout.Button("削除", GUILayout.Width(90)))
            {
                // TODO: 削除処理 (関連ファイル削除)
                Debug.Log($"TOTO: Delete outputs for sheet {sheetName}");
            }
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    private void LoadWorkbookConfigs()
    {
        // ExcelDataReader を用いて WorkbookConfig シート一覧を取得
        try
        {
            _workbookConfigs.Clear();

            var fullPath = Path.GetFullPath(_assetPath);
            if (!File.Exists(fullPath))
            {
                Debug.LogWarning($"ExcelInspector: ファイルが存在しません: {fullPath}");
                return;
            }

            var configSheetNames = new List<string>();
            using (var stream = File.Open(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = ExcelDataReader.ExcelReaderFactory.CreateReader(stream))
            {
                do
                {
                    var name = reader.Name;
                    if (!string.IsNullOrEmpty(name) && reader.Read())
                    {
                        var a1 = reader.GetValue(0)?.ToString()?.Trim();
                        if (string.Equals(a1, "WorkbookConfig", StringComparison.Ordinal))
                        {
                            configSheetNames.Add(name);
                        }
                    }
                } while (reader.NextResult());
            }

            foreach (var configSheetName in configSheetNames)
            {
                var sheet = ExcelMaster.ExcelUtil.ReadExcelToStringArray(_assetPath, configSheetName);
                var config = ParseWorkbookConfig(configSheetName, sheet);
                if (config != null)
                {
                    _workbookConfigs.Add(config);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"ExcelInspector: WorkbookConfig 読み込みに失敗しました ({_assetPath}) - {ex.GetType().Name}: {ex.Message}");
            _workbookConfigs.Clear();
        }
    }

    private WorkbookConfigEntry ParseWorkbookConfig(string configSheetName, string[][] sheet)
    {
        if (sheet == null || sheet.Length == 0)
        {
            Debug.LogWarning($"ExcelInspector: WorkbookConfig シートが空です: {configSheetName}");
            return null;
        }

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

        var entry = new WorkbookConfigEntry
        {
            configSheetName = configSheetName,
            sheetName = Pick(map, "SheetName"),
            className = Pick(map, "ClassName"),
            namespaceName = Pick(map, "Namespace"),
            tableName = Pick(map, "TableName"),
            binaryName = Pick(map, "BinaryName"),
            classOutputDirectory = Pick(map, "ClassOutputDirectory"),
            builderOutputDirectory = Pick(map, "BuilderOutputDirectory"),
            binaryOutputDirectory = Pick(map, "BinaryOutputDirectory"),
            additionalUsings = ParseAdditionalUsings(map)
        };

        if (string.IsNullOrWhiteSpace(entry.sheetName)
            || string.IsNullOrWhiteSpace(entry.className)
            || string.IsNullOrWhiteSpace(entry.namespaceName))
        {
            Debug.LogWarning($"ExcelInspector: WorkbookConfig の必須項目が不足しています: {configSheetName}");
            return null;
        }

        return entry;
    }

    private static string Pick(IDictionary<string, string> map, string key)
    {
        return map.TryGetValue(key, out var value) ? value : null;
    }

    private static string[] NormalizeUsings(IEnumerable<string> usings)
    {
        if (usings == null) return Array.Empty<string>();
        return usings
            .Where(u => !string.IsNullOrWhiteSpace(u))
            .Select(u => u.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    private static List<string> ParseAdditionalUsings(Dictionary<string, string> map)
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
            .ToList();
    }
}
