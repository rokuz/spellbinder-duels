using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GiftCode
{
  public class Gift
  {
    public bool removeAds = false;
    public int coins = 0;
    public bool expired = false;

    public Gift(bool removeAds, int coins)
    {
      this.removeAds = removeAds;
      this.coins = coins;
    }

    public Gift(bool expired)
    {
      this.removeAds = false;
      this.coins = 0;
      this.expired = expired;
    }
  }

  public static Gift ApplyCode(string code)
  {
    if (code == "IGRODEN" && !Persistence.preferences.IsUsedGiftcode(code))
    {
      var t = DateTime.Now;
      if (t.Year != 2018)
        return new Gift(true /*expired */);
      if (t.Month > 2)
        return new Gift(true /*expired */);
      if (t.Day > 15 && t.Month == 2)
        return new Gift(true /*expired */);

      var p = new Dictionary<string, object>();
      p.Add("code", code);
      MyAnalytics.CustomEvent("Giftcode", p);

      Persistence.preferences.SetUsedGiftcode(code);
      Persistence.Save();
      return new Gift(false, 300);
    }
    else if (code == "aPbhLx" && !Persistence.preferences.IsUsedGiftcode(code))
    {
      var p = new Dictionary<string, object>();
      p.Add("code", code);
      MyAnalytics.CustomEvent("Giftcode", p);

      Persistence.preferences.SetUsedGiftcode(code);
      Persistence.Save();
      return new Gift(true, 0);
    }
    else if (code == "qykPeM" && !Persistence.preferences.IsUsedGiftcode(code))
    {
      var p = new Dictionary<string, object>();
      p.Add("code", code);
      MyAnalytics.CustomEvent("Giftcode", p);

      Persistence.preferences.SetUsedGiftcode(code);
      Persistence.Save();
      return new Gift(false, 200);
    }
    else if (code == "JvDged" && !Persistence.preferences.IsUsedGiftcode(code))
    {
      var p = new Dictionary<string, object>();
      p.Add("code", code);
      MyAnalytics.CustomEvent("Giftcode", p);

      Persistence.preferences.SetUsedGiftcode(code);
      Persistence.Save();
      return new Gift(false, 500);
    }
    else if (code == "TcLitD" && !Persistence.preferences.IsUsedGiftcode(code))
    {
      var p = new Dictionary<string, object>();
      p.Add("code", code);
      MyAnalytics.CustomEvent("Giftcode", p);

      Persistence.preferences.SetUsedGiftcode(code);
      Persistence.Save();
      return new Gift(false, 1000);
    }

    return null;
  }
}
