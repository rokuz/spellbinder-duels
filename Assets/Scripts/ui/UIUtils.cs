using System;

public static class UIUtils
{
    public static string GetFormattedString(ProfileData profileData)
    {
        return string.Format("{0}\nLevel {1}", profileData.name, profileData.level);
    }
}
