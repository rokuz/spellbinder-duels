using UnityEngine;
using System.Collections.Generic;

public interface IMatchingRequestsHandler
{
    void OnMatchingSuccess(string matchId, ProfileData opponentData);
    void OnRecoverMatch(string matchId, ProfileData opponentData);
    void OnOpponentNotFound();
    void OnLookingForOpponent();
    void OnMatchingError(int code);
}

public static class MatchingRequests
{
    public const string MATCH = "match";

    public static void OnMatchResponse(WWW response, IMatchingRequestsHandler handler)
    {
        if (response.error == null)
        {
            JSONObject json = JSONObject.Create(response.text);
            int code = (int)json.GetField("code").i;
            if (code == ServerCode.OK || code == ServerCode.YOU_ARE_PLAYING)
            {
                string matchId = json.GetField("matchId").str;
                JSONObject profile = json.GetField("profile");
                ProfileData data = new ProfileData();
                data.name = profile.GetField("name").str;
                data.level = (int)profile.GetField("level").n;
                data.bonuses = Utils.ToIntArray(profile.GetField("bonuses").list.ToArray());
                data.resistance = Utils.ToIntArray(profile.GetField("resistance").list.ToArray());
                if (handler != null)
                {
                    if (code == ServerCode.OK)
                        handler.OnMatchingSuccess(matchId, data);
                    else
                        handler.OnRecoverMatch(matchId, data);
                }
            }
            else if (code == ServerCode.LOOKING_FOR_OPPONENT)
            {
                if (handler != null)
                    handler.OnLookingForOpponent();
            }
            else if (code == ServerCode.OPPONENT_DISCONNECTED || code == ServerCode.OPPONENT_NOT_FOUND)
            {
                if (handler != null)
                    handler.OnOpponentNotFound();
            }
            else if (code == ServerCode.BAD_SIGNATURE || code == ServerCode.UNKNOWN_PROFILE)
            {
                Debug.Log("Error (" + code + "): Server request has finished with error");
                if (handler != null)
                    handler.OnMatchingError(code);
            }
        }
        else
        {
            Debug.Log("" + response.error);
            if (handler != null)
                handler.OnMatchingError(400);
        }
    }
}
