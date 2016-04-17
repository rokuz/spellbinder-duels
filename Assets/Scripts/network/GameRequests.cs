using UnityEngine;
using System.Collections.Generic;

public class PlayerData
{
    public int mana;
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
    public string[] newSpells;
}

public class GameData
{
    public Magic[] gameField;
    public bool firstTurn;
    public float showTime;
    public float turnTime;
    public float showSubstitutesTime;
    public int maxMana;
    public int opponentMaxMana;
    public PlayerData player;
    public PlayerData opponent;
    public RewardData reward;
}

public interface IGameRequestsHandler
{
    void OnStartMatch(GameData gameData);
    void OnYouDisconnected(RewardData rewardData);
    void OnOpponentDisconnected(RewardData rewardData);
    void OnShowCards();
    void OnWaitForStart();
    void OnWin(PlayerData player, PlayerData opponent, string spell, int spellCost, RewardData reward);
    void OnLose(PlayerData player, PlayerData opponent, RewardData reward, bool delay);
    void OnYourTurn(PlayerData player, PlayerData opponent, bool delay);
    void OnOpponentTurn(PlayerData player, PlayerData opponent, bool delay);
    void OnSpellCasted(PlayerData player, PlayerData opponent, string spell, int spellCost, Dictionary<int, Magic> substitutes);
    void OnSpellMiscasted(int spellCost);
    void OnOpponentSpellCasted(int[] openedCards, string spell, int spellCost, Dictionary<int, Magic> substitutes);
    void OnOpponentSpellMiscasted(int[] openedCards, string spell, int spellCost);
    void OnError(int code);
}

public static class GameRequests
{
    public const string START_MATCH = "start_match";
    public const string PING = "ping";
    public const string TURN = "turn";

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
                gameData.maxMana = (int)json.GetField("maxMana").i;
                gameData.opponentMaxMana = (int)json.GetField("opponentMaxMana").i;
                gameData.firstTurn = json.GetField("firstTurn").b;
                gameData.showTime = json.GetField("showTime").i / 1000.0f;
                gameData.turnTime = json.GetField("turnTime").i / 1000.0f;
                gameData.showSubstitutesTime = json.GetField("showSubstitutes").i / 1000.0f;
                gameData.player = new PlayerData();
                ParsePlayerData(json.GetField("player"), gameData.player);
                gameData.opponent = new PlayerData();
                ParsePlayerData(json.GetField("opponent"), gameData.opponent);
                var rewardObj = json.GetField("reward");
                if (rewardObj != null && !rewardObj.IsNull)
                {
                    gameData.reward = new RewardData();
                    ParseReward(rewardObj, gameData.reward);
                }

                if (handler != null)
                    handler.OnStartMatch(gameData);
            }
            else if (code == ServerCode.YOU_DISCONNECTED)
            {
                RewardData reward = new RewardData();
                ParseReward(json.GetField("reward"), reward);
                if (handler != null)
                    handler.OnYouDisconnected(reward);
            }
            else if (code == ServerCode.OPPONENT_DISCONNECTED)
            {
                RewardData reward = new RewardData();
                ParseReward(json.GetField("reward"), reward);
                if (handler != null)
                    handler.OnOpponentDisconnected(reward);
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

    public static void OnPingResponse(WWW response, IGameRequestsHandler handler)
    {
        if (response.error == null)
        {
            JSONObject json = JSONObject.Create(response.text);
            int code = (int)json.GetField("code").i;
            if (code == ServerCode.SHOW_CARDS)
            {
                if (handler != null)
                    handler.OnShowCards();
            }
            else if (code == ServerCode.WAIT_FOR_START)
            {
                if (handler != null)
                    handler.OnWaitForStart();
            }
            else if (code == ServerCode.YOU_LOSE)
            {
                bool delay = ParseOpponentSpell(json, handler);
                RewardData reward = new RewardData();
                ParseReward(json.GetField("reward"), reward);
                PlayerData player = new PlayerData();
                ParsePlayerData(json.GetField("player"), player);
                PlayerData opponent = new PlayerData();
                ParsePlayerData(json.GetField("opponent"), opponent);
                if (handler != null)
                    handler.OnLose(player, opponent, reward, delay);
            }
            else if (code == ServerCode.YOUR_TURN)
            {
                bool delay = ParseOpponentSpell(json, handler);
                PlayerData player = new PlayerData();
                ParsePlayerData(json.GetField("player"), player);
                PlayerData opponent = new PlayerData();
                ParsePlayerData(json.GetField("opponent"), opponent);
                if (handler != null)
                    handler.OnYourTurn(player, opponent, delay);
            }
            else if (code == ServerCode.OPPONENT_TURN)
            {
                bool delay = ParseOpponentSpell(json, handler);
                PlayerData player = new PlayerData();
                ParsePlayerData(json.GetField("player"), player);
                PlayerData opponent = new PlayerData();
                ParsePlayerData(json.GetField("opponent"), opponent);
                if (handler != null)
                    handler.OnOpponentTurn(player, opponent, delay);
            }
            else if (code == ServerCode.YOU_DISCONNECTED)
            {
                RewardData reward = new RewardData();
                ParseReward(json.GetField("reward"), reward);
                if (handler != null)
                    handler.OnYouDisconnected(reward);
            }
            else if (code == ServerCode.OPPONENT_DISCONNECTED)
            {
                RewardData reward = new RewardData();
                ParseReward(json.GetField("reward"), reward);
                if (handler != null)
                    handler.OnOpponentDisconnected(reward);
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

    public static void OnTurnResponse(WWW response, IGameRequestsHandler handler)
    {
        if (response.error == null)
        {
            JSONObject json = JSONObject.Create(response.text);
            int code = (int)json.GetField("code").i;
            if (code == ServerCode.OK)
            {
                PlayerData player = new PlayerData();
                ParsePlayerData(json.GetField("player"), player);
                PlayerData opponent = new PlayerData();
                ParsePlayerData(json.GetField("opponent"), opponent);
                string spell = json.GetField("spell").str;
                int spellCost = (int)json.GetField("spellCost").i;
                var subs = json.GetField("substitutes").ToDictionary();
                Dictionary<int, Magic> substitutes = new Dictionary<int, Magic>();
                foreach (var key in subs.Keys)
                    substitutes.Add(System.Int32.Parse(key), MagicUtils.MagicFromString(subs[key]));
                if (handler != null)
                    handler.OnSpellCasted(player, opponent, spell, spellCost, substitutes);
            }
            else if (code == ServerCode.SPELL_MISCAST)
            {
                JSONObject obj = json.GetField("spellCost");
                int spellCost = (obj != null ? (int)obj.i : 0);
                if (handler != null)
                    handler.OnSpellMiscasted(spellCost);
            }
            else if (code == ServerCode.YOU_WIN)
            {
                PlayerData player = new PlayerData();
                ParsePlayerData(json.GetField("player"), player);
                PlayerData opponent = new PlayerData();
                ParsePlayerData(json.GetField("opponent"), opponent);
                string spell = json.GetField("spell").str;
                int spellCost = (int)json.GetField("spellCost").i;
                RewardData reward = new RewardData();
                ParseReward(json.GetField("reward"), reward);
                if (handler != null)
                    handler.OnWin(player, opponent, spell, spellCost, reward);
            }
            else if (code == ServerCode.OPPONENT_TURN)
            {
                PlayerData player = new PlayerData();
                ParsePlayerData(json.GetField("player"), player);
                PlayerData opponent = new PlayerData();
                ParsePlayerData(json.GetField("opponent"), opponent);
                if (handler != null)
                    handler.OnOpponentTurn(player, opponent, false);
            }
            else if (code == ServerCode.YOU_DISCONNECTED)
            {
                RewardData reward = new RewardData();
                ParseReward(json.GetField("reward"), reward);
                if (handler != null)
                    handler.OnYouDisconnected(reward);
            }
            else if (code == ServerCode.OPPONENT_DISCONNECTED)
            {
                RewardData reward = new RewardData();
                ParseReward(json.GetField("reward"), reward);
                if (handler != null)
                    handler.OnOpponentDisconnected(reward);
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

    private static bool ParseOpponentSpell(JSONObject json, IGameRequestsHandler handler)
    {
        List<JSONObject> openedCardsLists = json.GetField("openCards").list;
        if (openedCardsLists.Count > 0)
        {
            int[] openedCards = Utils.ToIntArray(openedCardsLists.ToArray());
            string opponentSpell = json.GetField("opponentSpell").str;
            int spellCost = (int)json.GetField("opponentSpellCost").i;
            var subs = json.GetField("substitutes").ToDictionary();
            Dictionary<int, Magic> substitutes = new Dictionary<int, Magic>();
            foreach (var key in subs.Keys)
                substitutes.Add(System.Int32.Parse(key), MagicUtils.MagicFromString(subs[key]));
            if (handler != null)
            {
                if (substitutes.Count == 0)
                    handler.OnOpponentSpellMiscasted(openedCards, opponentSpell, spellCost);
                else
                    handler.OnOpponentSpellCasted(openedCards, opponentSpell, spellCost, substitutes);
            }
            return true;
        }
        return false;
    }

    private static void ParsePlayerData(JSONObject playerObject, PlayerData player)
    {
        player.mana = (int)playerObject.GetField("mana").i;
        player.health = (int)playerObject.GetField("health").i;
        player.defence = (int)playerObject.GetField("defence").i;
        player.blockedDamageTurns = (int)playerObject.GetField("blockedDamageTurns").i;
        player.blockedHealingTurns = (int)playerObject.GetField("blockedHealingTurns").i;
        player.blockedDefenseTurns = (int)playerObject.GetField("blockedDefenseTurns").i;
        player.blockedBonusTurns = Utils.ToIntArray(playerObject.GetField("blockedBonusTurns").list.ToArray());
        player.blockedResistanceTurns = Utils.ToIntArray(playerObject.GetField("blockedResistanceTurns").list.ToArray());
    }

    private static void ParseReward(JSONObject rewardObject, RewardData reward)
    {
        reward.experience = (int)rewardObject.GetField("experience").i;
        reward.levelsUp = (int)rewardObject.GetField("levelsUp").i;
        reward.coins = (int)rewardObject.GetField("coins").i;
        reward.bonuses = Utils.ToIntArray(rewardObject.GetField("bonuses").list.ToArray());
        reward.resistance = Utils.ToIntArray(rewardObject.GetField("resistance").list.ToArray());
        reward.newSpells = Utils.ToStringArray(rewardObject.GetField("newSpells").list.ToArray());
    }
}
