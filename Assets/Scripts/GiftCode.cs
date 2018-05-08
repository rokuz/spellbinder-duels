using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GiftCode
{
  public class Gift
  {
    public int coins = 0;
    public bool expired = false;

    public Gift(int coins)
    {
      this.coins = coins;
    }

    public Gift(bool expired)
    {
      this.coins = 0;
      this.expired = expired;
    }
  }

  public static Gift ApplyCode(string code)
  {
    if (code == "qykPeM" && !Persistence.preferences.IsUsedGiftcode(code))
    {
      var p = new Dictionary<string, object>();
      p.Add("code", code);
      MyAnalytics.CustomEvent("Giftcode", p);

      Persistence.preferences.SetUsedGiftcode(code);
      Persistence.Save();
      return new Gift(200);
    }
    else if (code == "JvDged" && !Persistence.preferences.IsUsedGiftcode(code))
    {
      var p = new Dictionary<string, object>();
      p.Add("code", code);
      MyAnalytics.CustomEvent("Giftcode", p);

      Persistence.preferences.SetUsedGiftcode(code);
      Persistence.Save();
      return new Gift(500);
    }
    else if (code == "TcLitD" && !Persistence.preferences.IsUsedGiftcode(code))
    {
      var p = new Dictionary<string, object>();
      p.Add("code", code);
      MyAnalytics.CustomEvent("Giftcode", p);

      Persistence.preferences.SetUsedGiftcode(code);
      Persistence.Save();
      return new Gift(1000);
    }

    return null;
  }
}
