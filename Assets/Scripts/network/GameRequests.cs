using UnityEngine;
using System.Collections.Generic;

public class PlayerData
{
    public int health;
    public int defence;
    public int blockedDamageTurns;
    public int blockedHealingTurns;
    public int blockedDefenseTurns;
    public int[] blockedBonusTurns;
    public int[] blockedResistanceTurns;
}

public class RewardData
{
    public int experience;
    public int levelsUp;
    public int[] bonuses;
    public int[] resistance;
    public int coins;
}

public class GameData
{
    public Magic[] gameField;
    public bool firstTurn;
    public int showTime;
    public int turnTime;
    public PlayerData player;
    public PlayerData opponent;
    public RewardData reward;
}

public interface IGameRequestsHandler
{
    void OnStartMatch(GameData gameData);
    void OnYouDisconnected(RewardData rewardData);
    void OnOpponentDisconnected(RewardData rewardData);
    void OnError(int code);
}

public static class GameRequests
{
    public const string START_MATCH = "start_match";

    public static void OnStartMatchResponse(WWW response, IGameRequestsHandler handler)
    {
        if (response.error == null)
        {
            JSONObject json = JSONObject.Create(response.text);
            int code = (int)json.GetField("code").i;
            if (code == ServerCode.OK)
            {
                GameData gameData = new GameData();
                JSONObject[] cards = json.GetField("cards").list.ToArray();
                if (cards.Length != 12)
                {
                    if (handler != null)
                        handler.OnError(1000);
                    return;
                }
                gameData.gameField = new Magic[cards.Length];
                for (int i = 0; i < cards.Length; i++)
                {
                    gameData.gameField[i] = MagicUtils.MagicFromString(cards[i].str);
                }
                //TODO

                if (handler != null)
                    handler.OnStartMatch(gameData);
            }
            else if (code == ServerCode.YOU_DISCONNECTED)
            {
                //TODO:reward
                if (handler != null)
                    handler.OnYouDisconnected(null);
            }
            else if (code == ServerCode.OPPONENT_DISCONNECTED)
            {
                //TODO:reward
                if (handler != null)
                    handler.OnOpponentDisconnected(null);
            }
            else if (code == ServerCode.BAD_SIGNATURE || code == ServerCode.UNKNOWN_PROFILE ||
                     code == ServerCode.INVALID_MATCH_ID || code == ServerCode.INVALID_PLAYER_ID)
            {
                Debug.Log("Error (" + code + "): Server request has finished with error");
                if (handler != null)
                    handler.OnError(code);
            }
        }
        else
        {
            Debug.Log("" + response.error);
            if (handler != null)
                handler.OnError(400);
        }
    }
}
