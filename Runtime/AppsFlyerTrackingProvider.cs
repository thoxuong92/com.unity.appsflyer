using System;
using System.Collections.Generic;
using AppsFlyerSDK;
using UnityEngine;
using Unity.Core.Logging;
using Unity.Core.Services.Ads;
using Unity.Core.Services.Analytics;
using Unity.Core.Services.Tracking;

namespace Unity.AppsFlyer
{
    /// <summary>
    /// Adapter tích hợp AppsFlyer Attribution & Analytics với Unity Core Framework.
    /// Quản lý vòng đời SDK, gửi sự kiện In-App Events, và gửi Impression-Level Ad Revenue.
    /// </summary>
    public class AppsFlyerTrackingProvider : ITrackingProvider, IAnalyticsProvider
    {
        public string ProviderName => "AppsFlyer";
        public AppsFlyerConfig Config { get; private set; }

        private bool _isInitialized;
        public bool IsInitialized => _isInitialized;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoRegister()
        {
            var provider = new AppsFlyerTrackingProvider();
            TrackingService.AddProvider(provider);
            AnalyticsService.AddProvider(provider);
        }

        public void Initialize()
        {
            if (_isInitialized) return;

            AppLogger.Log("[AppsFlyerTrackingProvider] Khởi tạo AppsFlyer SDK...");
            Config = AppsFlyerConfig.LoadFromResources();

#if UNITY_IOS && !UNITY_EDITOR
            if (Config.AttTimeoutSeconds > 0)
            {
                AppsFlyerSDK.AppsFlyer.waitForATTUserAuthorizationWithTimeoutInterval(Config.AttTimeoutSeconds);
            }
#endif

            if (!string.IsNullOrEmpty(Config.DevKey))
            {
                AppsFlyerSDK.AppsFlyer.initSDK(Config.DevKey, Config.AppId, null);
                if (Config.IsTCFEnabled)
                {
                    AppsFlyerSDK.AppsFlyer.enableTCFDataCollection(true);
                }
                AppsFlyerSDK.AppsFlyer.startSDK();
                _isInitialized = true;
                AppLogger.Log($"[AppsFlyerTrackingProvider] AppsFlyer SDK started (DevKey: {Config.DevKey.Substring(0, Math.Min(4, Config.DevKey.Length))}***).");
            }
            else
            {
                AppLogger.LogWarning("[AppsFlyerTrackingProvider] DevKey rỗng. Chạy ở chế độ Mock.");
                _isInitialized = true;
            }
        }

        public void Shutdown()
        {
            AppLogger.Log("[AppsFlyerTrackingProvider] Đóng AppsFlyer Adapter.");
            _isInitialized = false;
        }

        #region ITrackingProvider
        public void TrackRevenue(AdRevenueInfo revenueInfo)
        {
            if (revenueInfo == null) return;

            try
            {
                var additionalParams = new Dictionary<string, string>
                {
                    { AdRevenueScheme.COUNTRY, string.IsNullOrEmpty(revenueInfo.CountryCode) ? "" : revenueInfo.CountryCode },
                    { AdRevenueScheme.AD_UNIT, string.IsNullOrEmpty(revenueInfo.AdUnitId) ? "" : revenueInfo.AdUnitId },
                    { AdRevenueScheme.AD_TYPE, string.IsNullOrEmpty(revenueInfo.Format) ? "" : revenueInfo.Format },
                    { AdRevenueScheme.PLACEMENT, string.IsNullOrEmpty(revenueInfo.Placement) ? "" : revenueInfo.Placement }
                };

                MediationNetwork network = MediationNetwork.ApplovinMax;
                if (!string.IsNullOrEmpty(revenueInfo.Source) && revenueInfo.Source.IndexOf("AdMob", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    network = MediationNetwork.GoogleAdMob;
                }

                var logRevenue = new AFAdRevenueData(
                    string.IsNullOrEmpty(revenueInfo.NetworkName) ? "Unknown" : revenueInfo.NetworkName,
                    network,
                    string.IsNullOrEmpty(revenueInfo.Currency) ? "USD" : revenueInfo.Currency,
                    revenueInfo.Revenue
                );

                AppsFlyerSDK.AppsFlyer.logAdRevenue(logRevenue, additionalParams);
                AppLogger.Log($"[AppsFlyerTrackingProvider] Logged Ad Revenue: {revenueInfo.Revenue} {revenueInfo.Currency} ({revenueInfo.Format})");
            }
            catch (Exception ex)
            {
                AppLogger.LogError($"[AppsFlyerTrackingProvider] Lỗi log Ad Revenue: {ex.Message}");
            }
        }

        public void TrackEvent(string eventToken, double? revenue = null, string currency = null)
        {
            if (string.IsNullOrEmpty(eventToken)) return;

            var values = new Dictionary<string, string>();
            if (revenue.HasValue) values[AFInAppEvents.REVENUE] = revenue.Value.ToString();
            if (!string.IsNullOrEmpty(currency)) values[AFInAppEvents.CURRENCY] = currency;

            AppsFlyerSDK.AppsFlyer.sendEvent(eventToken, values);
            AppLogger.Log($"[AppsFlyerTrackingProvider] TrackEvent: {eventToken}");
        }
        #endregion

        #region IAnalyticsProvider
        public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            if (string.IsNullOrEmpty(eventName)) return;

            var strParams = new Dictionary<string, string>();
            if (parameters != null)
            {
                foreach (var kvp in parameters)
                {
                    if (kvp.Value != null)
                    {
                        strParams[kvp.Key] = kvp.Value.ToString();
                    }
                }
            }

            AppsFlyerSDK.AppsFlyer.sendEvent(eventName, strParams);
        }

        public void SetUserProperty(string propertyName, string propertyValue)
        {
            if (string.IsNullOrEmpty(propertyName)) return;
            var data = new Dictionary<string, string> { { propertyName, propertyValue } };
            AppsFlyerSDK.AppsFlyer.setAdditionalData(data);
        }

        public void SetUserId(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return;
            AppsFlyerSDK.AppsFlyer.setCustomerUserId(userId);
        }
        #endregion
    }
}
