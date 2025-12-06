using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System;
using ClosedXML.Excel;
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

    private string _classesDirectoryPath = "Assets/ExcelMaster/Data/Source"; // クラス保存先ディレクトリ
    private string _binaryDirectoryPath = "Assets/ExcelMaster/Data/Binary"; // バイナリ保存先ディレクトリ

    // シート名キャッシュ
    private string[] _sheetNames = System.Array.Empty<string>();

    //生成済みマスター(クラス名)仮取得
    private List<string> _generatedClassNames = new List<string>();

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

        // 設定 UI
        DrawSettingsUI();

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

    private void DrawSettingsUI()
    {
        EditorGUILayout.LabelField("出力設定", EditorStyles.boldLabel);

        // クラス出力先
        EditorGUILayout.BeginHorizontal();
        _classesDirectoryPath = EditorGUILayout.TextField("クラス出力先", _classesDirectoryPath);
        if (GUILayout.Button("参照…", GUILayout.Width(70)))
        {
            var abs = ToAbsolutePathIfPossible(_classesDirectoryPath);
            var folder = EditorUtility.OpenFolderPanel("クラス出力先を選択", abs, "");
            if (!string.IsNullOrEmpty(folder))
            {
                _classesDirectoryPath = ToAssetsRelativePath(folder) ?? _classesDirectoryPath;
            }
        }
        EditorGUILayout.EndHorizontal();

        // バイナリ出力先
        EditorGUILayout.BeginHorizontal();
        _binaryDirectoryPath = EditorGUILayout.TextField("バイナリ出力先", _binaryDirectoryPath);
        if (GUILayout.Button("参照…", GUILayout.Width(70)))
        {
            var abs = ToAbsolutePathIfPossible(_binaryDirectoryPath);
            var folder = EditorUtility.OpenFolderPanel("バイナリ出力先を選択", abs, "");
            if (!string.IsNullOrEmpty(folder))
            {
                _binaryDirectoryPath = ToAssetsRelativePath(folder) ?? _binaryDirectoryPath;
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
        // 出力ファイル存在チェック (シート名ベースの仮仕様)
        var classFileRel = Path.Combine(_classesDirectoryPath, sheetName + ".cs").Replace("\\", "/");
        var binaryFileRel = Path.Combine(_binaryDirectoryPath, sheetName + ".mmdb").Replace("\\", "/");

        bool classExists = File.Exists(ToAbsolutePathIfPossible(classFileRel));
        bool binaryExists = File.Exists(ToAbsolutePathIfPossible(binaryFileRel));

        string classButtonLabel = classExists ? "マスタークラス更新" : "マスタークラス生成";
        string binaryButtonLabel = binaryExists ? "バイナリ更新" : "バイナリ生成";

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(sheetName, GUILayout.Width(160));

        GUILayout.FlexibleSpace();

        if (GUILayout.Button(classButtonLabel, GUILayout.Width(130)))
        {
            // TODO: クラス生成/更新処理
            Debug.Log($"{classButtonLabel} for sheet {sheetName} -> {classFileRel}");

            // GenerateSource.Generate(
            //     excelFilePath: _assetPath,
            //     className: sheetName,
            //     tableName: sheetName,
            //     outputDirectory: _classesDirectoryPath);

            AssetDatabase.Refresh();
        }
        if (GUILayout.Button(binaryButtonLabel, GUILayout.Width(110)))
        {
            // TODO: バイナリ生成/更新処理
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
        // ClosedXML を用いてシート一覧を取得
        try
        {
            // Unity の "Assets/..." パスを絶対パスへ
            var fullPath = Path.GetFullPath(_assetPath);
            if (!File.Exists(fullPath))
            {
                Debug.LogWarning($"ExcelInspector: ファイルが存在しません: {fullPath}");
                _sheetNames = Array.Empty<string>();
                return;
            }

            using (var wb = new XLWorkbook(fullPath))
            {
                _sheetNames = wb.Worksheets
                    .Where(ws => ws.Visibility == ClosedXML.Excel.XLWorksheetVisibility.Visible) // 非表示除外
                    .Select(ws => ws.Name)
                    .ToArray();
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
        // 現状のサンプルとして ItemData を追加
        _generatedClassNames.Add("Item");
        // _generatedClassNames.Add("OldSheetSample"); // コメント解除で欠落テスト
    }

    [Serializable]
    private class InspectorSettings
    {
        public string classesDir;
        public string binaryDir;
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
                // デフォルトは現在の値を使用
                return;
            }
            var settings = JsonUtility.FromJson<InspectorSettings>(json);
            if (settings != null)
            {
                if (!string.IsNullOrEmpty(settings.classesDir)) _classesDirectoryPath = settings.classesDir;
                if (!string.IsNullOrEmpty(settings.binaryDir)) _binaryDirectoryPath = settings.binaryDir;
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
                classesDir = _classesDirectoryPath,
                binaryDir = _binaryDirectoryPath
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
