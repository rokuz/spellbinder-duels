using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Analytics;
using KHD;

public class MyAnalytics
{
  private static string version = "1.0.3";

#if UNITY_EDITOR
  private static bool flurryEnabled = false;
#else
  private static bool flurryEnabled = true;
#endif

  private static bool flurryInitialized = false;

  public static void Init()
  {
    if (flurryEnabled)
    {
      if (flurryInitialized)
        return;
      FlurryAnalytics.Instance.SetDebugLogEnabled(true);
      FlurryAnalytics.Instance.SetAppVersion(version);
      FlurryAnalytics.Instance.StartSession(
        "CYJD5KWH3F4CH64BF6J9", // iOS
        "TXSMGS9B7Q4345SKYPD7", // Android
        true);
      flurryInitialized = true;
    }
  }

  public static void CustomEvent(string eventName)
  {
    Analytics.CustomEvent(eventName);
    if (flurryEnabled)
    {
      FlurryAnalytics.Instance.LogEvent(eventName);
    }
  }

  public static void CustomEvent(string eventName, Dictionary<string, object> parameters)
  {
    Analytics.CustomEvent(eventName, parameters);
    if (flurryEnabled)
    {
      var p = new Dictionary<string, string>();
      foreach (var pp in parameters)
        p.Add(pp.Key, pp.Value.ToString());
      FlurryAnalytics.Instance.LogEventWithParameters(eventName, p);
    }
  }

  public static void PaymentEvent(string id, UnityEngine.Purchasing.Product product)
  {
    var p = new Dictionary<string, object>();
    p.Add("product", id);
    MyAnalytics.CustomEvent("Purchase_Successful", p);
    if (flurryEnabled)
    {
      #if UNITY_ANDROID
      FlurryAnalyticsAndroid.LogPayment(product.metadata.localizedTitle, id, 1,
        (double)product.metadata.localizedPrice, product.metadata.isoCurrencyCode,
        product.transactionID, null);
      #else
      var prm = new Dictionary<string, string>();
      foreach (var pp in p)
        prm.Add(pp.Key, pp.Value.ToString());
      FlurryAnalytics.Instance.LogEventWithParameters("Purchase_Successful", prm);
      #endif
    }
  }
}
