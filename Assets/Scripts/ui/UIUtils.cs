using System;
using SmartLocalization;

public static class UIUtils
{
    public static string GetFormattedString(ProfileData profileData)
    {
        return string.Format("{0}\n{1} {2}", profileData.name, LanguageManager.Instance.GetTextValue("Player.Level"), profileData.level);
    }
}
