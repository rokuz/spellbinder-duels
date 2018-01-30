///----------------------------------------------
/// Flurry Analytics Plugin
/// Copyright © 2016 Aleksei Kuzin
///----------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace KHD {

    public static class FlurryAnalyticsAndroid {

#if UNITY_ANDROID
        private const string FLURRY_ANGENT_CLASS_NAME = "com.flurry.android.FlurryAgent";
        private const string UNITY_PLAYER_CLASS_NAME = "com.unity3d.player.UnityPlayer";
        private const string UNITY_PLAYER_ACTIVITY_NAME = "currentActivity";

        private static AndroidJavaClass Flurry {
            get {
                if (!IsAndroidPlayer()) {
                    return null;
                }
                if (_flurry == null) {
                    _flurry = new AndroidJavaClass(FLURRY_ANGENT_CLASS_NAME);
                }
                return _flurry;
            }
        }

        private static AndroidJavaClass _flurry;
#endif

#if UNITY_EDITOR
        /// <summary>
        /// Generates debug logs to Editor console.
        /// </summary>
        private static bool _editorDebugLogEnabled = false;
#endif

        public static void Dispose() {
#if UNITY_ANDROID
            if (_flurry != null) {
                _flurry.Dispose();
                _flurry = null;
            }
#endif
        }

        public static void Init(string apiKey, bool captureUncaughtExceptions = false) {
#if UNITY_ANDROID
            DebugLog("Init with key: " + apiKey + " capture exceptions: " + captureUncaughtExceptions);

            if (!IsAndroidPlayer()) {
                return;
            }
            if (apiKey != null) {
                apiKey = apiKey.ToUpper();
            }
            if (string.IsNullOrEmpty(apiKey)) {
                DebugLogError("Android API key is null or empty, please provide valid API key");
            }

            using (var unityPlayer = new AndroidJavaClass(UNITY_PLAYER_CLASS_NAME)) {
                using (var activity = unityPlayer.GetStatic<AndroidJavaObject>(UNITY_PLAYER_ACTIVITY_NAME)) {
                    Flurry.CallStatic("setCaptureUncaughtExceptions", captureUncaughtExceptions);
                    Flurry.CallStatic("init", activity, apiKey.ToUpper());
                }
            }
#endif
        }

        public static void OnStartSession() {
#if UNITY_ANDROID
            DebugLog("Session started");

            if (!IsAndroidPlayer()) {
                return;
            }

            using (var unityPlayer = new AndroidJavaClass(UNITY_PLAYER_CLASS_NAME)) {
                using (var activity = unityPlayer.GetStatic<AndroidJavaObject>(UNITY_PLAYER_ACTIVITY_NAME)) {
                    Flurry.CallStatic("onStartSession", activity);
                }
            }
#endif
        }

        public static void OnEndSession() {
#if UNITY_ANDROID
            DebugLog("Session ended");

            if (!IsAndroidPlayer()) {
                return;
            }

            using (var unityPlayer = new AndroidJavaClass(UNITY_PLAYER_CLASS_NAME)) {
                using (var activity = unityPlayer.GetStatic<AndroidJavaObject>(UNITY_PLAYER_ACTIVITY_NAME)) {
                    Flurry.CallStatic("onStartSession", activity);
                }
            }
#endif
        }

        /// <summary>
        /// Enable/Disable Flurry SDK debug logs.
        ///
        /// By default logs are disabled.
        /// Should be called before StartSession.
        /// </summary>
        public static void SetLogEnabled(bool enabled) {
#if UNITY_EDITOR
            _editorDebugLogEnabled = enabled;
#endif
#if UNITY_ANDROID
            if (!IsAndroidPlayer()) {
                return;
            }
            Flurry.CallStatic("setLogEnabled", enabled);
#endif
        }

        /// <summary>
        /// Explicitly set app version..
        /// Should be called before StartSession.
        /// </summary>
        /// <param name="appVersion">App version.</param>
        public static void SetVersionName(string versionName) {
#if UNITY_ANDROID
            DebugLog("Application version changed to: " + versionName);

            if (!IsAndroidPlayer()) {
                return;
            }
            Flurry.CallStatic("setVersionName", versionName);
#endif
        }

        /// <summary>
        /// Set whether Flurry should record location. Defaults to true.
        /// </summary>
        public static void SetReportLocation(bool report) {
#if UNITY_ANDROID
            DebugLog("Set report location: " + report);

            if (!IsAndroidPlayer()) {
                return;
            }
            Flurry.CallStatic("setReportLocation", report);
#endif
        }

        /// <summary>
        /// Sets the time the app may be in the background before starting a new session upon resume.
        /// Default value 10000 ms.
        ///
        /// Should be called before StartSession.
        /// </summary>
        public static void SetContinueSessionMillis(long milliseconds) {
#if UNITY_ANDROID
            DebugLog("Set continue session seconds to: " + (milliseconds / 1000));

            if (!IsAndroidPlayer()) {
                return;
            }
            Flurry.CallStatic("setContinueSessionMillis", milliseconds);
#endif
        }

        /// <summary>
        /// Used to allow/disallow Flurry SDK to report uncaught exceptions. The feature is enabled by default.
        ///
        /// Should be called before init.
        /// </summary>
        public static void SetCaptureUncaughtExceptions(bool enabled) {
#if UNITY_ANDROID
            DebugLog("Set capture unchaught exceptions: " + enabled);

            if (!IsAndroidPlayer()) {
                return;
            }
            Flurry.CallStatic("setCaptureUncaughtExceptions", enabled);
#endif
        }

        /// <summary>
        /// Enables Flurry Pulse.
        /// Please see https://developer.yahoo.com/flurry-pulse/ for more details.
        ///
        /// Should be called before StartSession.
        /// </summary>
        public static void SetPulseEnabled(bool enabled) {
#if UNITY_ANDROID
            if (!IsAndroidPlayer()) {
                return;
            }
            Flurry.CallStatic("setPulseEnabled", enabled);
#endif
        }

        /// <summary>
        /// Assign a unique id for a user in your app.
        /// </summary>
        public static void SetUserId(string userId) {
#if UNITY_ANDROID
            DebugLog("Set unique user id: " + userId);

            if (!IsAndroidPlayer()) {
                return;
            }
            Flurry.CallStatic("setUserId", userId);
#endif
        }

        /// <summary>
        /// Set user's gender.
        /// </summary>
        public static void SetGender(FlurryAnalytics.Gender gender) {
#if UNITY_ANDROID
            DebugLog("Set user gender: " + gender);

            if (!IsAndroidPlayer()) {
                return;
            }
            Flurry.CallStatic("setGender", (byte)(gender == FlurryAnalytics.Gender.Male ? 1 : 0));
#endif
        }

        /// <summary>
        /// Use this method to capture the age of your user.
        /// </summary>
        public static void SetAge(int age) {
#if UNITY_ANDROID
            DebugLog("Set user age: " + age);

            if (!IsAndroidPlayer()) {
                return;
            }
            Flurry.CallStatic("setAge", age);
#endif
        }

        /// <summary>
        /// Set the default location.
        /// Useful when it is necessary to test analytics
        /// from other countries or places, or if your app is
        /// location-aware and you do not wish the Flurry SDK to determine the user location via GPS.
        /// </summary>
        public static void SetLocation(float latitude, float longitude) {
#if UNITY_ANDROID
            DebugLog("Set location: " + latitude + ", " + longitude);

            if (!IsAndroidPlayer()) {
                return;
            }
            Flurry.CallStatic("setLocation", latitude, longitude);
#endif
        }

        /// <summary>
        /// Records a custom event specified by eventName.
        /// </summary>
        /// <param name="eventName">
        /// Name of the event. For maximum effectiveness, we recommend using a naming scheme
        /// that can be easily understood by non-technical people in your business domain.
        /// </param>
        /// <param name="isTimed">If set to <c>true</c> event will be timed.
        /// Call EndTimedEvent to stop timed event.</param>
        public static void LogEvent(string eventName, bool isTimed = false) {
#if UNITY_ANDROID
            DebugLog("Log event: " + eventName + " isTimed: " + isTimed);

            if (!IsAndroidPlayer()) {
                return;
            }
            Flurry.CallStatic<AndroidJavaObject>("logEvent", eventName, isTimed);
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
#if UNITY_ANDROID
            DebugLog("Log event: " + eventName + " isTimed: " + isTimed + " with parameters");

            if (!IsAndroidPlayer()) {
                return;
            }
            using (var hashMap = ConvertDictionaryToJavaHashMap(parameters)) {
                Flurry.CallStatic<AndroidJavaObject>("logEvent", eventName, hashMap, isTimed);
            }
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
#if UNITY_ANDROID
            DebugLog("End timed event: " + eventName);

            if (!IsAndroidPlayer()) {
                return;
            }

            if (parameters == null) {
                Flurry.CallStatic("endTimedEvent", eventName);
            } else {
                using (var hashMap = ConvertDictionaryToJavaHashMap(parameters)) {
                    Flurry.CallStatic("endTimedEvent", eventName, hashMap);
                }
            }
#endif
        }

        /// <summary>
        /// Log a payment.
        /// </summary>
        /// <param name="productName">The name of the product purchased.</param>
        /// <param name="productId">The id of the product purchased.</param>
        /// <param name="quantity">The number of products purchased.</param>
        /// <param name="price">The price of the the products purchased in the given currency.</param>
        /// <param name="currency">The currency for the price argument.</param>
        /// <param name="transactionId">A unique identifier for the transaction used to make the purchase.</param>
        /// <param name="parameters">the parameters which should be submitted with this event.</param>
        public static void LogPayment(string productName,
                                      string productId,
                                      int quantity,
                                      double price,
                                      string currency,
                                      string transactionId,
                                      Dictionary<string, string> parameters) {
#if UNITY_ANDROID
            DebugLog("Log payment: " + productName);

            if (parameters == null) {
                parameters = new Dictionary<string, string>();
            }
            using (var hashMap = ConvertDictionaryToJavaHashMap(parameters)) {
                Flurry.CallStatic<AndroidJavaObject>("logPayment", productName, productId,
                    quantity, price, currency, transactionId, hashMap);
            }
#endif
        }

#if UNITY_ANDROID
        private static AndroidJavaObject ConvertDictionaryToJavaHashMap(Dictionary<string, string> parameters) {
            var hashMap = new AndroidJavaObject("java.util.HashMap");
            var put = AndroidJNIHelper.GetMethodID(hashMap.GetRawClass(), "put",
                                                   "(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;");

            foreach (var entry in parameters) {
                using (var key = new AndroidJavaObject("java.lang.String", entry.Key)) {
                    using (var value = new AndroidJavaObject("java.lang.String", entry.Value)) {
                        AndroidJNI.CallObjectMethod(hashMap.GetRawObject(), put,
                                                    AndroidJNIHelper.CreateJNIArgArray(new object[] { key, value }));
                    }
                }
            }

            return hashMap;
        }
#endif
        private static bool EditorDebugLogEnabled() {
#if UNITY_EDITOR
            return _editorDebugLogEnabled;
#else
            return false;
#endif
        }

        private static bool IsAndroidPlayer() {
            return Application.platform == RuntimePlatform.Android;
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
