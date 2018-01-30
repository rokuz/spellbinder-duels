///----------------------------------------------
/// Flurry Analytics Plugin
/// Copyright © 2016 Aleksei Kuzin
///----------------------------------------------

using UnityEngine;

namespace KHD {

    public class FlurryAnalyticsHelper : MonoBehaviour {

        /// <summary>
        /// iOS API Key.
        /// </summary>
        [SerializeField] private string _iOSApiKey;

        /// <summary>
        /// Android API Key.
        /// </summary>
        [SerializeField] private string _androidApiKey;

        /// <summary>
        /// Enable debug log.
        /// </summary>
        [SerializeField] private bool _enableDebugLog = false;

        /// <summary>
        /// Send crash reports to Flurry.
        /// </summary>
        [SerializeField] private bool _sendCrashReports = true;

#if (UNITY_5_2 || UNITY_5_3_OR_NEWER)
        /// <summary>
        /// Enabled data replication to Unity Analytics.
        /// </summary>
        [SerializeField] private bool _replicateDataToUnityAnalytics = false;
#endif

#pragma warning disable 0414    // The private field is assigned but its value is never used
#if !UNITY_IOS
        [HideInInspector]
#endif
        /// <summary>
        /// iOS: Enables implicit recording of Apple Store transactions.
        /// </summary>
        [Space(10)][SerializeField] private bool _iOSIAPReportingEnabled = false;
#pragma warning restore 0414

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        private void Awake() {
            FlurryAnalytics.Instance.SetDebugLogEnabled(_enableDebugLog);

#if (UNITY_5_2 || UNITY_5_3_OR_NEWER)
            FlurryAnalytics.Instance.replicateDataToUnityAnalytics = _replicateDataToUnityAnalytics;
#endif

            FlurryAnalytics.Instance.StartSession(_iOSApiKey, _androidApiKey, _sendCrashReports);

#if UNITY_IOS
            FlurryAnalyticsIOS.SetIAPReportingEnabled(_iOSIAPReportingEnabled);
#endif
        }
    }
}
