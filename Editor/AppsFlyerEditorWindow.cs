using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Unity.AppsFlyer.Editor
{
    /// <summary>
    /// Cửa sổ cấu hình AppsFlyer trong Unity Editor:
    /// - Quản lý DevKey, AppID (iOS) và ATT timeout
    /// - Soạn thảo và xuất file cấu hình Assets/Resources/Info.json
    /// </summary>
    public class AppsFlyerEditorWindow : EditorWindow
    {
        private string _devKey = "";
        private string _appId = "";
        private int _attTimeout = 60;
        private string _targetExportPath = "Assets/Resources/Info.json";
        private Vector2 _scrollPos;

        [MenuItem("Unity Core/AppsFlyer/Setup & Configuration", false, 16)]
        public static void ShowWindow()
        {
            var window = GetWindow<AppsFlyerEditorWindow>("AppsFlyer Setup");
            window.minSize = new Vector2(480, 420);
            window.Show();
        }

        private void OnEnable()
        {
            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            try
            {
                string fullPath = Path.Combine(Application.dataPath, "..", _targetExportPath);
                if (File.Exists(fullPath))
                {
                    string json = File.ReadAllText(fullPath);
                    _devKey = ExtractValue(json, "AppsflyerDevKey", "AppsFlyerDevKey", "DevKey");
                    _appId = ExtractValue(json, "AppsflyerAppId", "AppsFlyerAppId", "AppId");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[AppsFlyerEditor] Lỗi đọc cấu hình: {ex.Message}");
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            GUILayout.Label("Unity AppsFlyer - Configuration", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Cấu hình DevKey và AppID (App Store ID) phục vụ phân tích Attribution và doanh thu quảng cáo.", MessageType.Info);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            EditorGUILayout.Space(6);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("1. Cấu Hình Tài Khoản AppsFlyer", EditorStyles.boldLabel);

            _devKey = EditorGUILayout.TextField("AppsFlyer Dev Key:", _devKey);
            _appId = EditorGUILayout.TextField("App ID (iOS App Store ID):", _appId);
            _attTimeout = EditorGUILayout.IntField("iOS ATT Timeout (giây):", _attTimeout);

            EditorGUILayout.Space(4);
            _targetExportPath = EditorGUILayout.TextField("Export File:", _targetExportPath);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Info.json (Mặc định)")) _targetExportPath = "Assets/Resources/Info.json";
            if (GUILayout.Button("Info_Android.json")) _targetExportPath = "Assets/Resources/Info_Android.json";
            if (GUILayout.Button("Info_iOS.json")) _targetExportPath = "Assets/Resources/Info_iOS.json";
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(6);
            if (GUILayout.Button("💾 Lưu Cấu Hình Vào Resources", GUILayout.Height(36)))
            {
                SaveConfigToResources();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();
        }

        private void SaveConfigToResources()
        {
            try
            {
                string fullPath = Path.Combine(Application.dataPath, "..", _targetExportPath);
                string dir = Path.GetDirectoryName(fullPath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                // Đọc nội dung hiện có nếu có để cập nhật hoặc tạo mới
                string existing = File.Exists(fullPath) ? File.ReadAllText(fullPath) : "{\n}";
                string updated = UpsertJsonKey(existing, "AppsflyerDevKey", _devKey);
                updated = UpsertJsonKey(updated, "AppsflyerAppId", _appId);

                File.WriteAllText(fullPath, updated);
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("Thành công", $"Đã lưu cấu hình AppsFlyer vào: {_targetExportPath}", "OK");
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Lỗi", $"Lỗi ghi file cấu hình: {ex.Message}", "OK");
            }
        }

        private string UpsertJsonKey(string json, string key, string value)
        {
            string searchKey = $"\"{key}\"";
            int idx = json.IndexOf(searchKey, StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
            {
                int colon = json.IndexOf(':', idx + searchKey.Length);
                int q1 = json.IndexOf('"', colon + 1);
                int q2 = json.IndexOf('"', q1 + 1);
                if (q1 >= 0 && q2 > q1)
                {
                    return json.Substring(0, q1 + 1) + value + json.Substring(q2);
                }
            }
            else
            {
                int lastBrace = json.LastIndexOf('}');
                if (lastBrace >= 0)
                {
                    string toInsert = $"  \"{key}\": \"{value}\",\n";
                    return json.Insert(lastBrace, toInsert);
                }
            }
            return json;
        }

        private string ExtractValue(string json, params string[] keys)
        {
            if (string.IsNullOrEmpty(json)) return "";
            foreach (var key in keys)
            {
                string searchKey = $"\"{key}\"";
                int idx = json.IndexOf(searchKey, StringComparison.OrdinalIgnoreCase);
                if (idx >= 0)
                {
                    int colon = json.IndexOf(':', idx + searchKey.Length);
                    if (colon >= 0)
                    {
                        int q1 = json.IndexOf('"', colon + 1);
                        if (q1 >= 0)
                        {
                            int q2 = json.IndexOf('"', q1 + 1);
                            if (q2 > q1)
                            {
                                return json.Substring(q1 + 1, q2 - q1 - 1);
                            }
                        }
                    }
                }
            }
            return "";
        }
    }
}
