using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpellEstimator
{
  private static Spell emptySpell = new Spell();

  public static int CalculateSpellRating(Spell s, Match.Player player, Match.Player opponent)
  {
    var elemIndex = s.Index;
      
    int rating = 0;

    if (s.damage != emptySpell.damage)
    {
      int r = (s.damage - emptySpell.damage);
      if (elemIndex >= 0)
      {
        int opponentResistance = opponent.profile.resistance[elemIndex];
        if (!s.considerResistance || opponent.data.blockedResistanceTurns[elemIndex] > 0)
          opponentResistance = 0;

        int playerBonus = player.profile.bonuses[elemIndex];
        if (player.data.blockedBonusTurns[elemIndex] > 0 && !s.clearBonusCurse[elemIndex])
          playerBonus = 0;

        r += ((playerBonus - opponentResistance) / 1000);
      }
      rating += r;
    }
      
    if (player.data.blockedDefenseTurns == 0 || s.clearDefenseCurse)
    {
      if (s.defense != emptySpell.defense)
        rating += (s.defense - emptySpell.defense);
    }

    if (player.data.health.Value < Constants.HEALTH_POINTS || s.clearHealingCurse)
    {
      if (s.healing != emptySpell.healing)
        rating += (s.healing - emptySpell.healing);
    }

    if (opponent.data.blockedDamageTurns == 0)
    {
      if (s.blockDamageTurns != emptySpell.blockDamageTurns)
        rating += (s.blockDamageTurns - emptySpell.blockDamageTurns);
    }
    if (opponent.data.blockedHealingTurns == 0)
    {
      if (s.blockHealingTurns != emptySpell.blockHealingTurns)
        rating += (s.blockHealingTurns - emptySpell.blockHealingTurns);
    }
    if (opponent.data.blockedDefenseTurns == 0)
    {
      if (s.blockDefenseTurns != emptySpell.blockDefenseTurns)
        rating += (s.blockDefenseTurns - emptySpell.blockDefenseTurns);
    } 

    if (elemIndex >= 0 && opponent.profile.resistance[elemIndex] > 0 && 
        opponent.data.blockedResistanceTurns[elemIndex] == 0)
    {
      if (s.considerResistance != emptySpell.considerResistance)
        rating++;
    }
    
    if (opponent.data.defense.Value > 0)
    {
      if (s.considerDefense != emptySpell.considerDefense)
        rating++;
      
      if (s.removeDefense != emptySpell.removeDefense)
        rating++;
    }

    if (player.data.blockedDamageTurns > 0)
    {
      if (s.clearDamageCurse != emptySpell.clearDamageCurse)
        rating++;
    }
    if (player.data.blockedHealingTurns > 0)
    {
      if (s.clearHealingCurse != emptySpell.clearHealingCurse)
        rating++;
    }
    if (player.data.blockedDefenseTurns > 0)
    {
      if (s.clearDefenseCurse != emptySpell.clearDefenseCurse)
        rating++;
    }

    for (int i = 0; i < 3; i++)
    {
      if (elemIndex >= 0 && opponent.data.blockedBonusTurns[elemIndex] == 0)
      {
        if (s.blockBonusTurns[i] != emptySpell.blockBonusTurns[i])
          rating += (s.blockBonusTurns[i] - emptySpell.blockBonusTurns[i]);
      }

      if (elemIndex >= 0 && opponent.data.blockedResistanceTurns[elemIndex] == 0)
      {
        if (s.blockResistanceTurns[i] != emptySpell.blockResistanceTurns[i])
          rating += (s.blockResistanceTurns[i] - emptySpell.blockResistanceTurns[i]);
      }

      if (elemIndex >= 0 && player.data.blockedBonusTurns[elemIndex] > 0)
      {
        if (s.clearBonusCurse[i] != emptySpell.clearBonusCurse[i])
          rating++;
      }
      if (elemIndex >= 0 && player.data.blockedResistanceTurns[elemIndex] > 0)
      {
        if (s.clearResistanceCurse[i] != emptySpell.clearResistanceCurse[i])
          rating++;
      }
    }
    return rating;
  }
    
  public static KeyValuePair<Spell, int[]>? FindBestDefenseSpell(List<KeyValuePair<Spell, int[]>> spells,
                                                                 Match.Player player, Match.Player opponent)
  {
    if (player.data.blockedDefenseTurns == 0)
    {
      var s = SelectMostRatedSpell(spells.FindAll(x => x.Key.defense > 0), player, opponent);
      if (s != null)
        return s;
    }
    else
    {
      var s = SelectMostRatedSpell(spells.FindAll(x => x.Key.clearDefenseCurse), player, opponent);
      if (s != null)
        return s;
    }
    return null;
  }

  public static KeyValuePair<Spell, int[]>? FindBestHealSpell(List<KeyValuePair<Spell, int[]>> spells,
                                                              Match.Player player, Match.Player opponent)
  {
    if (player.data.blockedHealingTurns == 0)
    {
      var s = SelectMostRatedSpell(spells.FindAll(x => x.Key.healing > 0), player, opponent);
      if (s != null)
        return s;
    }
    else
    {
      var s = SelectMostRatedSpell(spells.FindAll(x => x.Key.clearHealingCurse), player, opponent);
      if (s != null)
        return s;
    }
    return null;
  }

  public static KeyValuePair<Spell, int[]>? SelectMostRatedSpell(List<KeyValuePair<Spell, int[]>> spells,
                                                                 Match.Player player, Match.Player opponent)
  {
    if (spells.Count == 0)
      return null;

    int index = 0;
    int maxRating = CalculateSpellRating(spells[index].Key, player, opponent);
    for (int i = 1; i < spells.Count; i++)
    {
      int rating = CalculateSpellRating(spells[i].Key, player, opponent);
      if (rating > maxRating)
        index = i;
    }
    return spells[index];
  }

  public static KeyValuePair<Spell, int[]>? FindBestDamageSpell(List<KeyValuePair<Spell, int[]>> spells, 
                                                                Match.Player player, Match.Player opponent,
                                                                out int maxDamage)
  {
    maxDamage = 0;
    var indicesList = new List<KeyValuePair<int, int>>();
    for (int i = 0; i < spells.Count; i++)
    {
      var dmg = spells[i].Key.damage;
      if (dmg == 0)
        continue;
      var elemIndex = spells[i].Key.Index;
      if (elemIndex >= 0)
      {
        int opponentResistance = opponent.profile.resistance[elemIndex];
        if (!spells[i].Key.considerResistance || opponent.data.blockedResistanceTurns[elemIndex] > 0)
          opponentResistance = 0;

        int playerBonus = player.profile.bonuses[elemIndex];
        if (player.data.blockedBonusTurns[elemIndex] > 0 && !spells[i].Key.clearBonusCurse[elemIndex])
          playerBonus = 0;

        dmg += ((playerBonus - opponentResistance) / 1000);
      }

      // Virtually increase damage if a spell does not consider defense.
      if (!spells[i].Key.considerDefense && opponent.data.defense.Value > 0)
        dmg += opponent.data.defense.Value;
        
      if (dmg > maxDamage && spells[i].Key.manaCost <= player.data.RestMana)
      {
        maxDamage = spells[i].Key.damage;
        indicesList.Clear();
        indicesList.Add(new KeyValuePair<int, int>(i, CalculateSpellRating(spells[i].Key, player, opponent)));
      }
    }

    if (indicesList.Count == 0)
      return null;

    int index = 0;
    for (int i = 1; i < indicesList.Count; i++)
    {
      if (indicesList[i].Value > indicesList[index].Value)
        index = i;
    }

    return spells[indicesList[index].Key];
  }
}
