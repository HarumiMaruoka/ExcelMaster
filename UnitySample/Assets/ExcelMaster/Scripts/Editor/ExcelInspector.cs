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

    // デフォルト出力先（古い設定との互換用）
    private string _defaultClassDirectoryPath = "Assets/ExcelMaster/Data/Source";
    private string _defaultDataDirectoryPath = "Assets/ExcelMaster/Data/Source";
    private string _defaultBuilderDirectoryPath = "Assets/ExcelMaster/Data/Source";
    private string _defaultBinaryDirectoryPath = "Assets/ExcelMaster/Data/Binary";

    // シート名キャッシュ
    private string[] _sheetNames = System.Array.Empty<string>();

    //生成済みマスター(クラス名)仮取得
    private List<string> _generatedClassNames = new List<string>();

    // シートごとの設定
    [Serializable]
    private class SheetSettings
    {
        public string sheetName;
        public bool generateClass = true;
        public bool generateData = true;
        public bool generateBuilder = true;
        public string classDir;   // null / empty の場合はデフォルトを使用
        public string dataDir;    // null / empty の場合はデフォルトを使用
        public string builderDir; // null / empty の場合はデフォルトを使用
        public string binaryDir;  // null / empty の場合はデフォルトを使用
    }

    // シート名 -> 設定
    private readonly Dictionary<string, SheetSettings> _sheetSettingsMap = new Dictionary<string, SheetSettings>();

    private void OnEnable()
    {
        // 選択中アセットのパス取得
        _assetPath = AssetDatabase.GetAssetPath(target);

        // 拡張子チェック
        string ext = Path.GetExtension(_assetPath)?.ToLowerInvariant();

        _isExcelFile = ExcelExtensions.Contains(ext);

        // 設定読み込み（meta: AssetImporter.userData）
        LoadSettingsFromMeta();

        if (_isExcelFile)
        {
            LoadSheetNames();
            LoadGeneratedClasses();
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

        // グローバル（デフォルト）設定 UI
        DrawGlobalSettingsUI();

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Excel シート一覧", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        if (_sheetNames.Length == 0)
        {
            EditorGUILayout.HelpBox("シートが見つかりません", MessageType.Info);
        }

        // シート表示
        foreach (var sheet in _sheetNames)
        {
            DrawSheetRow(sheet);
        }

        EditorGUILayout.Space(8);

        //生成済みだが現在のシート一覧に存在しないもの
        var missing = _generatedClassNames.Where(c => !_sheetNames.Contains(c)).ToList();
        if (missing.Count != 0)
        {
            EditorGUILayout.LabelField("欠落した生成済みマスター", EditorStyles.boldLabel);
            var yellowStyle = new GUIStyle(EditorStyles.label) { normal = { textColor = Color.yellow } };
            foreach (var cls in missing)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(cls + " (シートなし)", yellowStyle);
                if (GUILayout.Button("置き換え", GUILayout.Width(70)))
                {
                    // TODO:置き換え処理（再生成など）
                    Debug.Log($"Replace class {cls}");
                }
                if (GUILayout.Button("削除", GUILayout.Width(60)))
                {
                    // TODO: 削除処理
                    Debug.Log($"Delete class {cls}");
                }
                EditorGUILayout.EndHorizontal();
            }
        }
    }

    private void DrawGlobalSettingsUI()
    {
        EditorGUILayout.LabelField("デフォルト出力設定", EditorStyles.boldLabel);

        // クラス出力先(既定)
        EditorGUILayout.BeginHorizontal();
        _defaultClassDirectoryPath = EditorGUILayout.TextField("クラス出力先(既定)", _defaultClassDirectoryPath);
        if (GUILayout.Button("参照…", GUILayout.Width(70)))
        {
            var abs = ToAbsolutePathIfPossible(_defaultClassDirectoryPath);
            var folder = EditorUtility.OpenFolderPanel("クラス出力先(既定)を選択", abs, "");
            if (!string.IsNullOrEmpty(folder))
            {
                _defaultClassDirectoryPath = ToAssetsRelativePath(folder) ?? _defaultClassDirectoryPath;
            }
        }
        EditorGUILayout.EndHorizontal();

        // データ出力先(既定)
        EditorGUILayout.BeginHorizontal();
        _defaultDataDirectoryPath = EditorGUILayout.TextField("データ出力先(既定)", _defaultDataDirectoryPath);
        if (GUILayout.Button("参照…", GUILayout.Width(70)))
        {
            var abs = ToAbsolutePathIfPossible(_defaultDataDirectoryPath);
            var folder = EditorUtility.OpenFolderPanel("データ出力先(既定)を選択", abs, "");
            if (!string.IsNullOrEmpty(folder))
            {
                _defaultDataDirectoryPath = ToAssetsRelativePath(folder) ?? _defaultDataDirectoryPath;
            }
        }
        EditorGUILayout.EndHorizontal();

        // ビルダー出力先(既定)
        EditorGUILayout.BeginHorizontal();
        _defaultBuilderDirectoryPath = EditorGUILayout.TextField("ビルダー出力先(既定)", _defaultBuilderDirectoryPath);
        if (GUILayout.Button("参照…", GUILayout.Width(70)))
        {
            var abs = ToAbsolutePathIfPossible(_defaultBuilderDirectoryPath);
            var folder = EditorUtility.OpenFolderPanel("ビルダー出力先(既定)を選択", abs, "");
            if (!string.IsNullOrEmpty(folder))
            {
                _defaultBuilderDirectoryPath = ToAssetsRelativePath(folder) ?? _defaultBuilderDirectoryPath;
            }
        }
        EditorGUILayout.EndHorizontal();

        // バイナリ出力先(既定)
        EditorGUILayout.BeginHorizontal();
        _defaultBinaryDirectoryPath = EditorGUILayout.TextField("バイナリ出力先(既定)", _defaultBinaryDirectoryPath);
        if (GUILayout.Button("参照…", GUILayout.Width(70)))
        {
            var abs = ToAbsolutePathIfPossible(_defaultBinaryDirectoryPath);
            var folder = EditorUtility.OpenFolderPanel("バイナリ出力先(既定)を選択", abs, "");
            if (!string.IsNullOrEmpty(folder))
            {
                _defaultBinaryDirectoryPath = ToAssetsRelativePath(folder) ?? _defaultBinaryDirectoryPath;
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("設定を保存", GUILayout.Width(120)))
        {
            SaveSettingsToMeta();
        }
        if (GUILayout.Button("設定を読み込み", GUILayout.Width(120)))
        {
            LoadSettingsFromMeta();
        }
        EditorGUILayout.EndHorizontal();
    }

    private SheetSettings GetOrCreateSheetSettings(string sheetName)
    {
        if (!_sheetSettingsMap.TryGetValue(sheetName, out var s) || s == null)
        {
            s = new SheetSettings
            {
                sheetName = sheetName,
                classDir = _defaultClassDirectoryPath,
                dataDir = _defaultDataDirectoryPath,
                builderDir = _defaultBuilderDirectoryPath,
                binaryDir = _defaultBinaryDirectoryPath
            };
            _sheetSettingsMap[sheetName] = s;
        }
        else
        {
            if (string.IsNullOrEmpty(s.classDir)) s.classDir = _defaultClassDirectoryPath;
            if (string.IsNullOrEmpty(s.dataDir)) s.dataDir = _defaultDataDirectoryPath;
            if (string.IsNullOrEmpty(s.builderDir)) s.builderDir = _defaultBuilderDirectoryPath;
            if (string.IsNullOrEmpty(s.binaryDir)) s.binaryDir = _defaultBinaryDirectoryPath;
        }
        return s;
    }

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

    private string ToAssetsRelativePath(string absoluteFolder)
    {
        if (string.IsNullOrEmpty(absoluteFolder)) return null;
        absoluteFolder = absoluteFolder.Replace("\\", "/");
        var dataPath = Application.dataPath.Replace("\\", "/");
        if (absoluteFolder.StartsWith(dataPath, StringComparison.OrdinalIgnoreCase))
        {
            var rel = absoluteFolder.Substring(dataPath.Length).TrimStart('/');
            return string.IsNullOrEmpty(rel) ? "Assets" : ($"Assets/{rel}");
        }
        Debug.LogWarning("選択したフォルダはプロジェクト外です。Assets 配下を選択してください。");
        return null;
    }

    private void DrawSheetRow(string sheetName)
    {
        var settings = GetOrCreateSheetSettings(sheetName);

        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(sheetName, EditorStyles.boldLabel, GUILayout.Width(160));
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        // クラス生成トグル + クラス出力先（同一行、フィールドはフレキシブル）
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("クラス生成", GUILayout.Width(80));
        settings.generateClass = EditorGUILayout.Toggle(string.Empty, settings.generateClass, GUILayout.Width(18));
        GUILayout.Space(8);
        // ラベル無しの TextField にして残り幅を全部使わせる
        settings.classDir = EditorGUILayout.TextField(
            GUIContent.none,
            string.IsNullOrEmpty(settings.classDir) ? _defaultClassDirectoryPath : settings.classDir
        );
        if (GUILayout.Button("参照…", GUILayout.Width(70)))
        {
            var abs = ToAbsolutePathIfPossible(settings.classDir ?? _defaultClassDirectoryPath);
            var folder = EditorUtility.OpenFolderPanel($"{sheetName} のクラス出力先を選択", abs, "");
            if (!string.IsNullOrEmpty(folder))
            {
                settings.classDir = ToAssetsRelativePath(folder) ?? settings.classDir;
            }
        }
        EditorGUILayout.EndHorizontal();

        // データ生成トグル + データ出力先
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("データ生成", GUILayout.Width(80));
        settings.generateData = EditorGUILayout.Toggle(string.Empty, settings.generateData, GUILayout.Width(18));
        GUILayout.Space(8);
        settings.dataDir = EditorGUILayout.TextField(string.Empty, string.IsNullOrEmpty(settings.dataDir) ? _defaultDataDirectoryPath : settings.dataDir);
        if (GUILayout.Button("参照…", GUILayout.Width(70)))
        {
            var abs = ToAbsolutePathIfPossible(settings.dataDir ?? _defaultDataDirectoryPath);
            var folder = EditorUtility.OpenFolderPanel($"{sheetName} のデータ出力先を選択", abs, "");
            if (!string.IsNullOrEmpty(folder))
            {
                settings.dataDir = ToAssetsRelativePath(folder) ?? settings.dataDir;
            }
        }
        EditorGUILayout.EndHorizontal();

        // ビルダー生成トグル + ビルダー出力先
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("ビルダー生成", GUILayout.Width(80));
        settings.generateBuilder = EditorGUILayout.Toggle(string.Empty, settings.generateBuilder, GUILayout.Width(18));
        GUILayout.Space(8);
        settings.builderDir = EditorGUILayout.TextField(string.Empty, string.IsNullOrEmpty(settings.builderDir) ? _defaultBuilderDirectoryPath : settings.builderDir);
        if (GUILayout.Button("参照…", GUILayout.Width(70)))
        {
            var abs = ToAbsolutePathIfPossible(settings.builderDir ?? _defaultBuilderDirectoryPath);
            var folder = EditorUtility.OpenFolderPanel($"{sheetName} のビルダー出力先を選択", abs, "");
            if (!string.IsNullOrEmpty(folder))
            {
                settings.builderDir = ToAssetsRelativePath(folder) ?? settings.builderDir;
            }
        }
        EditorGUILayout.EndHorizontal();

        // バイナリ出力先
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("バイナリ出力先", GUILayout.Width(110));
        settings.binaryDir = EditorGUILayout.TextField(string.Empty, string.IsNullOrEmpty(settings.binaryDir) ? _defaultBinaryDirectoryPath : settings.binaryDir);
        if (GUILayout.Button("参照…", GUILayout.Width(70)))
        {
            var abs = ToAbsolutePathIfPossible(settings.binaryDir ?? _defaultBinaryDirectoryPath);
            var folder = EditorUtility.OpenFolderPanel($"{sheetName} のバイナリ出力先を選択", abs, "");
            if (!string.IsNullOrEmpty(folder))
            {
                settings.binaryDir = ToAssetsRelativePath(folder) ?? settings.binaryDir;
            }
        }
        EditorGUILayout.EndHorizontal();

        var classDir = string.IsNullOrEmpty(settings.classDir) ? _defaultClassDirectoryPath : settings.classDir;
        var dataDir = string.IsNullOrEmpty(settings.dataDir) ? _defaultDataDirectoryPath : settings.dataDir;
        var builderDir = string.IsNullOrEmpty(settings.builderDir) ? _defaultBuilderDirectoryPath : settings.builderDir;
        var binaryDir = string.IsNullOrEmpty(settings.binaryDir) ? _defaultBinaryDirectoryPath : settings.binaryDir;

        // 出力ファイル存在チェック (シート名ベースの仮仕様)
        var classFileRel = Path.Combine(classDir, sheetName + ".cs").Replace("\\", "/");
        var binaryFileRel = Path.Combine(binaryDir, sheetName + ".mmdb").Replace("\\", "/");

        bool classExists = File.Exists(ToAbsolutePathIfPossible(classFileRel));
        bool binaryExists = File.Exists(ToAbsolutePathIfPossible(binaryFileRel));

        string classButtonLabel = classExists ? "マスタークラス更新" : "マスタークラス生成";
        string binaryButtonLabel = binaryExists ? "バイナリ更新" : "バイナリ生成";

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button(classButtonLabel, GUILayout.Width(130)))
        {
            Debug.Log($"{classButtonLabel} for sheet {sheetName} (Class:{classDir}, Data:{dataDir}, Builder:{builderDir})");

            string @namespace = "GameNamespace";
            string className = sheetName;

            if (settings.generateClass)
            {
                var selection = ExcelMaster.ExcelUtil.ReadExcelToStringArray(_assetPath, sheetName);
                ExcelMaster.Builders.SourceBuilder.GenerateClassSource(@namespace, Array.Empty<string>(), className, selection);
            }
            if (settings.generateData)
            {
                var selection = ExcelMaster.ExcelUtil.ReadExcelToStringArray(_assetPath, sheetName);
                ExcelMaster.Builders.SourceBuilder.GenerateDataSection(@namespace, Array.Empty<string>(), className, selection);
            }
            if (settings.generateBuilder)
            {
                var selection = ExcelMaster.ExcelUtil.ReadExcelToStringArray(_assetPath, sheetName);
                ExcelMaster.Builders.SourceBuilder.ParseMetaFromSelection(selection, ref @namespace, ref className);
                ExcelMaster.Builders.SourceBuilder.GenerateBinaryBuilder(@namespace, Array.Empty<string>(), className, binaryDir);
            }

            AssetDatabase.Refresh();
        }
        if (GUILayout.Button(binaryButtonLabel, GUILayout.Width(110)))
        {
            // TODO: バイナリ生成/更新処理（現状そのまま）
            Debug.Log($"{binaryButtonLabel} for sheet {sheetName} -> {binaryFileRel}");
        }
        if (binaryExists || classExists)
        {
            if (GUILayout.Button("削除", GUILayout.Width(90)))
            {
                // TODO: 削除処理 (関連ファイル削除)
                Debug.Log($"Delete outputs for sheet {sheetName}");
            }
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    private void LoadSheetNames()
    {
        // ExcelDataReader を用いてシート一覧を取得
        try
        {
            var fullPath = Path.GetFullPath(_assetPath);
            if (!File.Exists(fullPath))
            {
                Debug.LogWarning($"ExcelInspector: ファイルが存在しません: {fullPath}");
                _sheetNames = Array.Empty<string>();
                return;
            }

            using (var stream = File.Open(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = ExcelDataReader.ExcelReaderFactory.CreateReader(stream))
            {
                var sheetNames = new List<string>();

                do
                {
                    var name = reader.Name;
                    if (!string.IsNullOrEmpty(name))
                    {
                        sheetNames.Add(name);
                    }
                } while (reader.NextResult());

                _sheetNames = sheetNames.ToArray();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"ExcelInspector: シート取得に失敗しました ({_assetPath}) - {ex.GetType().Name}: {ex.Message}");
            _sheetNames = Array.Empty<string>();
        }
    }

    private void LoadGeneratedClasses()
    {
        _generatedClassNames.Clear();
        // TODO: 実際は Generated ディレクトリ走査やクラスファイルの MemoryTable 属性から判定
        _generatedClassNames.Add("Item");
    }

    [Serializable]
    private class InspectorSettings
    {
        public string classesDir;  // 旧: クラス出力先(既定)
        public string binaryDir;   // 旧: バイナリ出力先(既定)
        public string classDir;
        public string dataDir;
        public string builderDir;
        public string binaryDirectory;
        public List<SheetSettings> sheets;
    }

    private void LoadSettingsFromMeta()
    {
        try
        {
            var importer = AssetImporter.GetAtPath(_assetPath);
            if (importer == null) return;
            var json = importer.userData;
            if (string.IsNullOrEmpty(json))
            {
                return;
            }
            var settings = JsonUtility.FromJson<InspectorSettings>(json);
            if (settings != null)
            {
                // 旧プロパティとの互換も考慮
                if (!string.IsNullOrEmpty(settings.classDir)) _defaultClassDirectoryPath = settings.classDir;
                else if (!string.IsNullOrEmpty(settings.classesDir)) _defaultClassDirectoryPath = settings.classesDir;

                if (!string.IsNullOrEmpty(settings.dataDir)) _defaultDataDirectoryPath = settings.dataDir;
                else _defaultDataDirectoryPath = _defaultClassDirectoryPath;

                if (!string.IsNullOrEmpty(settings.builderDir)) _defaultBuilderDirectoryPath = settings.builderDir;
                else _defaultBuilderDirectoryPath = _defaultClassDirectoryPath;

                if (!string.IsNullOrEmpty(settings.binaryDirectory)) _defaultBinaryDirectoryPath = settings.binaryDirectory;
                else if (!string.IsNullOrEmpty(settings.binaryDir)) _defaultBinaryDirectoryPath = settings.binaryDir;

                _sheetSettingsMap.Clear();
                if (settings.sheets != null)
                {
                    foreach (var s in settings.sheets)
                    {
                        if (s == null || string.IsNullOrEmpty(s.sheetName)) continue;
                        _sheetSettingsMap[s.sheetName] = s;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"設定読み込みに失敗しました: {ex.Message}");
        }
    }

    private void SaveSettingsToMeta()
    {
        try
        {
            var importer = AssetImporter.GetAtPath(_assetPath);
            if (importer == null) return;

            var settings = new InspectorSettings
            {
                classDir = _defaultClassDirectoryPath,
                dataDir = _defaultDataDirectoryPath,
                builderDir = _defaultBuilderDirectoryPath,
                binaryDirectory = _defaultBinaryDirectoryPath,
                sheets = _sheetSettingsMap.Values.ToList()
            };

            importer.userData = JsonUtility.ToJson(settings);
            importer.SaveAndReimport();
            Debug.Log("ExcelInspector: 設定を保存しました (meta)");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"設定保存に失敗しました: {ex.Message}");
        }
    }
}
