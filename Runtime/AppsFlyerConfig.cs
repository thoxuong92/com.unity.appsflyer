using System;
using UnityEngine;
using Unity.Core.Logging;

namespace Unity.AppsFlyer
{
    [Serializable]
    public class AppsFlyerConfig
    {
        public string DevKey = "";
        public string AppId = "";
        public bool IsTCFEnabled = true;
        public int AttTimeoutSeconds = 60;

        public static AppsFlyerConfig LoadFromResources()
        {
            var config = new AppsFlyerConfig();
            try
            {
                string platformSuffix = Application.platform == RuntimePlatform.Android ? "_Android" : (Application.platform == RuntimePlatform.IPhonePlayer ? "_iOS" : "");
                TextAsset data = Resources.Load<TextAsset>($"Info{platformSuffix}") ?? Resources.Load<TextAsset>("Info");

                if (data != null && !string.IsNullOrEmpty(data.text))
                {
                    string text = data.text;
                    config.DevKey = ExtractJsonValue(text, "AppsflyerDevKey", "AppsFlyerDevKey", "DevKey");
                    config.AppId = ExtractJsonValue(text, "AppsflyerAppId", "AppsFlyerAppId", "AppId");
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogError($"[AppsFlyerConfig] Lỗi đọc cấu hình từ Resources: {ex.Message}");
            }
            return config;
        }

        private static string ExtractJsonValue(string json, params string[] keys)
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
