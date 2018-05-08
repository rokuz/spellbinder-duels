using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Analytics;

public class MyAnalytics
{
  public static void CustomEvent(string eventName)
  {
    Analytics.CustomEvent(eventName);
  }

  public static void CustomEvent(string eventName, Dictionary<string, object> parameters)
  {
    Analytics.CustomEvent(eventName, parameters);
  }

  public static void PaymentEvent(string id, UnityEngine.Purchasing.Product product)
  {
    var p = new Dictionary<string, object>();
    p.Add("product", id);
    MyAnalytics.CustomEvent("Purchase_Successful", p);
  }
}
