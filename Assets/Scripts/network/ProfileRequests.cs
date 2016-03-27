using UnityEngine;
using System.Collections.Generic;

public class ProfileData
{
    public string id = null;
    public string name;
    public int level;
    public int experience;
    public int[] bonuses;
    public int[] resistance;
    public int coins;
    public string[] spells;
}

public interface IProfileRequestsHandler
{
    void OnGetProfile(ProfileData profileData);
    void OnUnknownProfile();
    void OnProfileError(int code);
}

public interface ISetProfileNameRequestHandler
{
    void OnSetupProfileName();
    void OnDuplicateProfileName();
    void OnProfileError(int code);
}

public static class ProfileRequests
{
    public const string CREATE = "create_profile";
    public const string GET = "profile";
    public const string SET_NAME = "set_name";

    public static void OnCreateResponse(WWW response, IProfileRequestsHandler handler)
    {
        OnResponse(response, handler);
    }

    public static void OnGetResponse(WWW response, IProfileRequestsHandler handler)
    {
        OnResponse(response, handler);
    }

    public static void OnSetNameResponse(WWW response, ISetProfileNameRequestHandler handler)
    {
        if (response.error == null)
        {
            JSONObject json = JSONObject.Create(response.text);
            int code = (int)json.GetField("code").i;
            if (code == ServerCode.OK)
            {
                if (handler != null)
                    handler.OnSetupProfileName();
            }
            else if (code == ServerCode.DUPLICATE_NAME)
            {
                if (handler != null)
                    handler.OnDuplicateProfileName();
            }
            else if (code == ServerCode.BAD_SIGNATURE || code == ServerCode.UNKNOWN_PROFILE)
            {
                Debug.Log("Error (" + code + "): Server request has finished with error");
                if (handler != null)
                    handler.OnProfileError(code);
            }
        }
        else
        {
            Debug.Log("" + response.error);
            if (handler != null)
                handler.OnProfileError(400);
        }
    }

    private static void OnResponse(WWW response, IProfileRequestsHandler handler)
    {
        if (response.error == null)
        {
            JSONObject json = JSONObject.Create(response.text);
            int code = (int)json.GetField("code").i;
            if (code == ServerCode.OK)
            {
                JSONObject profile = json.GetField("profile");
                ProfileData data = new ProfileData();
                JSONObject id = profile.GetField("id");
                if (id != null)
                    data.id = id.str;
                data.name = profile.GetField("name").str;
                data.level = (int)profile.GetField("level").n;
                data.experience = (int)profile.GetField("experience").n;
                data.bonuses = Utils.ToIntArray(profile.GetField("bonuses").list.ToArray());
                data.resistance = Utils.ToIntArray(profile.GetField("resistance").list.ToArray());
                data.coins = (int)profile.GetField("coins").n;
                var spells = profile.GetField("spells").list.ToArray();
                data.spells = new string[spells.Length];
                for (int i = 0; i < spells.Length; i++) data.spells[i] = spells[i].str;
                if (handler != null)
                    handler.OnGetProfile(data);
            }
            else if (code == ServerCode.BAD_SIGNATURE)
            {
                Debug.Log("Error (" + code + "): Server request has finished with error");
                if (handler != null)
                    handler.OnProfileError(code);
            }
            else if (code == ServerCode.UNKNOWN_PROFILE)
            {
                if (handler != null)
                    handler.OnUnknownProfile();
            }
        }
        else
        {
            Debug.Log("" + response.error);
            if (handler != null)
                handler.OnProfileError(400);
        }
    }
}
