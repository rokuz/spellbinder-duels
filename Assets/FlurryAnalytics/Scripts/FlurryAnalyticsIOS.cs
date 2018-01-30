///----------------------------------------------
/// Flurry Analytics Plugin
/// Copyright © 2016 Aleksei Kuzin
///----------------------------------------------

using UnityEngine;
using System.Collections.Generic;
using System.Text;

#if UNITY_IOS
using System.Runtime.InteropServices;
#endif

namespace KHD {

    public static class FlurryAnalyticsIOS {

#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern void FlurryAnalyticsStartSession(string apiKey, bool sendCrashReports);

        [DllImport("__Internal")]
        private static extern void FlurryAnalyticsSetDebugLogEnabled(bool enabled);

        [DllImport("__Internal")]
        private static extern void FlurryAnalyticsSetAppVersion(string version);

        [DllImport("__Internal")]
        private static extern void FlurryAnalyticsSetSessionContinueSeconds(int seconds);

        [DllImport("__Internal")]
        private static extern void FlurryAnalyticsSetUserId(string userId);

        [DllImport("__Internal")]
        private static extern void FlurryAnalyticsSetAge(int age);

        [DllImport("__Internal")]
        private static extern void FlurryAnalyticsSetGender(int genderType);

        [DllImport("__Internal")]
        private static extern void FlurryAnalyticsLogEvent(string eventName, bool isTimed);

        [DllImport("__Internal")]
        private static extern void FlurryAnalyticsLogEventWithParameters(string eventName,
                                                                         string parameters,
                                                                         bool isTimed);

        [DllImport("__Internal")]
        private static extern void FlurryAnalyticsEndTimedEvent(string eventName,
                                                                string parameters);

        [DllImport("__Internal")]
        private static extern void FlurryAnalyticsSetSessionReportsOnCloseEnabled(bool enabled);

        [DllImport("__Internal")]
        private static extern void FlurryAnalyticsSetSessionReportsOnPauseEnabled(bool enabled);

        [DllImport("__Internal")]
        private static extern void FlurryAnalyticsSetPulseEnabled(bool enabled);

        [DllImport("__Internal")]
        private static extern void FlurryAnalyticsSetLatitude(float latitude, float longitude,
                                                              float horizontalAccuracy,
                                                              float verticalAccuracy);

        [DllImport("__Internal")]
        private static extern void FlurryAnalyticsSetIAPReportingEnabled(bool enabled);
#endif

#if UNITY_EDITOR
        /// <summary>
        /// Generates debug logs to Editor console.
        /// </summary>
        private static bool _editorDebugLogEnabled = false;
#endif

        /// <summary>
        /// Start Flurry session.
        /// </summary>
        /// <param name="apiKey">API key.</param>
        /// <param name="sendCrashReports">If set to <c>true</c> send crash reports.</param>
        public static void StartSession(string apiKey, bool sendCrashReports) {
#if UNITY_IOS
            DebugLog("Session started with apiKey: " + apiKey + " send crash reports: " + sendCrashReports);

            if (!IsIPhonePlayer()) {
                return;
            }
            if (apiKey != null) {
                apiKey = apiKey.ToUpper();
            }

            if (string.IsNullOrEmpty(apiKey)) {
                DebugLogError("iOS API key is null or empty, please provide valid API key");
            }

            FlurryAnalyticsStartSession(apiKey, sendCrashReports);
#endif
        }

        /// <summary>
        /// Enable/Disable Flurry SDK debug logs.
        ///
        /// By default logs are disabled.
        /// </summary>
        public static void SetDebugLogEnabled(bool enabled) {
#if UNITY_EDITOR
            _editorDebugLogEnabled = enabled;
#endif
#if UNITY_IOS
            if (!IsIPhonePlayer()) {
                return;
            }
            FlurryAnalyticsSetDebugLogEnabled(enabled);
#endif
        }

        /// <summary>
        /// Set app version. By default Flurry SDK use CFBundleVersion.
        /// Should be called before StartSession.
        /// </summary>
        /// <param name="appVersion">App version.</param>
        public static void SetAppVersion(string appVersion) {
#if UNITY_IOS
            DebugLog("Application version changed to: " + appVersion);

            if (!IsIPhonePlayer()) {
                return;
            }
            FlurryAnalyticsSetAppVersion(appVersion);
#endif
        }

        /// <summary>
        /// Sets the time the app may be in the background before starting a new session upon resume.
        /// Default value 10 seconds.
        ///
        /// Should be called before StartSession.
        /// </summary>
        public static void SetSessionContinueSeconds(int seconds) {
#if UNITY_IOS
            DebugLog("Set continue session seconds to: " + seconds);

            if (!IsIPhonePlayer()) {
                return;
            }
            FlurryAnalyticsSetSessionContinueSeconds(seconds);
#endif
        }

        /// <summary>
        /// Enables Flurry Pulse.
        /// Please see https://developer.yahoo.com/flurry-pulse/ for more details.
        ///
        /// Should be called before StartSession.
        /// </summary>
        public static void SetPulseEnabled(bool enabled) {
#if UNITY_IOS
            DebugLog("Set Pulse enabled: " + enabled);

            if (!IsIPhonePlayer()) {
                return;
            }
            FlurryAnalyticsSetPulseEnabled(enabled);
#endif
        }

        /// <summary>
        /// Enables implicit recording of Apple Store transactions.
        /// true to enable transaction logging, false to stop transaction logging.
        ///
        /// This method should be called after StartSession.
        /// </summary>
        public static void SetIAPReportingEnabled(bool enabled) {
#if UNITY_IOS
            DebugLog("Set IAP reporting enabled: " + enabled);

            if (!IsIPhonePlayer()) {
                return;
            }
            FlurryAnalyticsSetIAPReportingEnabled(enabled);
#endif
        }

        /// <summary>
        /// Assign a unique id for a user in your app.
        /// </summary>
        public static void SetUserId(string userId) {
#if UNITY_IOS
            DebugLog("Set unique user id: " + userId);

            if (!IsIPhonePlayer()) {
                return;
            }
            FlurryAnalyticsSetUserId(userId);
#endif
        }

        /// <summary>
        /// Use this method to capture the age of your user.
        /// </summary>
        public static void SetAge(int age) {
#if UNITY_IOS
            DebugLog("Set user age: " + age);

            if (!IsIPhonePlayer()) {
                return;
            }
            FlurryAnalyticsSetAge(age);
#endif
        }

        /// <summary>
        /// Set user's gender.
        /// </summary>
        public static void SetGender(FlurryAnalytics.Gender gender) {
#if UNITY_IOS
            DebugLog("Set user gender: " + gender);

            if (!IsIPhonePlayer()) {
                return;
            }
            FlurryAnalyticsSetGender((int)gender);
#endif
        }

        /// <summary>
        /// Set the location of the session.
        /// Only the last location entered is captured per session.
        /// Regardless of accuracy specified, the Flurry SDK will only report location at city level or higher.
        /// Location is aggregated across all users of your app and not available on a per user basis.
        /// This information should only be captured if it is germaine to the use of your app.
        /// </summary>
        public static void SetLocation(float latitude, float longitude,
                                       float horizontalAccuracy, float verticalAccuracy) {
#if UNITY_IOS
            DebugLog("Set location: " + latitude + ", " + longitude);

            if (!IsIPhonePlayer()) {
                return;
            }
            FlurryAnalyticsSetLatitude(
                latitude, longitude, horizontalAccuracy, verticalAccuracy);
#endif
        }

        /// <summary>
        /// Records a custom event specified by eventName.
        /// </summary>
        /// <param name="eventName">
        /// Name of the event. For maximum effectiveness, we recommend using a naming scheme
        /// that can be easily understood by non-technical people in your business domain.
        /// </param>
        /// <param name="isTimed">If set to <c>true</c> this will be a timed event.
        /// Call EndTimedEvent to stop timed event.</param>
        public static void LogEvent(string eventName, bool isTimed = false) {
#if UNITY_IOS
            DebugLog("Log event: " + eventName + " isTimed: " + isTimed);

            if (!IsIPhonePlayer()) {
                return;
            }
            FlurryAnalyticsLogEvent(eventName, isTimed);
#endif
        }

        /// <summary>
        /// Log custom event with parameters.
        /// A maximum of 10 parameter names may be associated with any event.
        /// </summary>
        /// <param name="eventName">
        /// Name of the event. For maximum effectiveness, we recommend using a naming scheme
        /// that can be easily understood by non-technical people in your business domain.
        /// </param>
        /// <param name="parameters">Parameters.</param>
        /// <param name="isTimed">If set to <c>true</c> this will be a timed event.
        /// Call EndTimedEvent to stop timed event.</param>
        public static void LogEventWithParameters(string eventName, Dictionary<string, string> parameters,
                                                  bool isTimed = false) {
#if UNITY_IOS
            DebugLog("Log event: " + eventName + " isTimed: " + isTimed + " with parameters");

            if (!IsIPhonePlayer()) {
                return;
            }

            FlurryAnalyticsLogEventWithParameters(eventName, ConvertParameters(parameters), isTimed);
#endif
        }

        /// <summary>
        /// Ends the timed event.
        /// </summary>
        /// <param name="eventName">
        /// Name of the event. For maximum effectiveness, we recommend using a naming scheme
        /// that can be easily understood by non-technical people in your business domain.
        /// </param>
        /// <param name="parameters">Parameters.</param>
        public static void EndTimedEvent(string eventName, Dictionary<string, string> parameters = null) {
#if UNITY_IOS
            DebugLog("End timed event: " + eventName);

            if (!IsIPhonePlayer()) {
                return;
            }
            FlurryAnalyticsEndTimedEvent(eventName, ConvertParameters(parameters));
#endif
        }

        /// <summary>
        /// Sets the session reports on close enabled.
        /// Enabled by default.
        /// </summary>
        public static void SetSessionReportsOnCloseEnabled(bool enabled) {
#if UNITY_IOS
            DebugLog("Set session reports on close: " + enabled);

            if (!IsIPhonePlayer()) {
                return;
            }
            FlurryAnalyticsSetSessionReportsOnCloseEnabled(enabled);
#endif
        }

        /// <summary>
        /// Sets the session reports on pause enabled.
        /// Enabled by default.
        /// </summary>
        public static void SetSessionReportsOnPauseEnabled(bool enabled) {
#if UNITY_IOS
            DebugLog("Set session reports on pause: " + enabled);

            if (!IsIPhonePlayer()) {
                return;
            }
            FlurryAnalyticsSetSessionReportsOnPauseEnabled(enabled);
#endif
        }

        /// <summary>
        /// Converts the parameters to json.
        /// </summary>
        private static string ConvertParameters(Dictionary<string, string> parameters) {
            if (parameters == null) {
                return null;
            }
            StringBuilder builder = new StringBuilder();
            builder.Append("{\n");

            var first = true;
            foreach (var pair in parameters) {
                if (!first) {
                    builder.Append(',');
                }

                SerializeString(builder, pair.Key);
                builder.Append(":");
                SerializeString(builder, pair.Value);

                first = false;
            }

            builder.Append("}\n");
            return builder.ToString();
        }

        /// <summary>
        /// Serialize string to json string.
        /// </summary>
        private static void SerializeString(StringBuilder builder, string str) {
            builder.Append('\"');

            var charArray = str.ToCharArray();
            foreach (var c in charArray) {
                switch (c) {
                    case '"':
                        builder.Append("\\\"");
                        break;
                    case '\\':
                        builder.Append("\\\\");
                        break;
                    case '\b':
                        builder.Append("\\b");
                        break;
                    case '\f':
                        builder.Append("\\f");
                        break;
                    case '\n':
                        builder.Append("\\n");
                        break;
                    case '\r':
                        builder.Append("\\r");
                        break;
                    case '\t':
                        builder.Append("\\t");
                        break;
                    default:
                        var codepoint = System.Convert.ToInt32(c);
                        if ((codepoint >= 32) && (codepoint <= 126)) {
                            builder.Append(c);
                        } else {
                            builder.Append("\\u" + System.Convert.ToString(codepoint, 16).PadLeft(4, '0'));
                        }
                        break;
                }
            }

            builder.Append('\"');
        }

        /// <returns><c>true</c> if is iPhone player.</returns>
        private static bool IsIPhonePlayer() {
            return Application.platform == RuntimePlatform.IPhonePlayer;
        }

        private static bool EditorDebugLogEnabled() {
#if UNITY_EDITOR
            return _editorDebugLogEnabled;
#else
            return false;
#endif
        }

        private static void DebugLog(string log) {
            if (!EditorDebugLogEnabled()) {
                return;
            }
            Debug.Log("[FlurryAnalyticsPlugin]: " + log);
        }

        private static void DebugLogError(string error) {
            Debug.Log("[FlurryAnalyticsPlugin]: " + error);
        }
    }
}
