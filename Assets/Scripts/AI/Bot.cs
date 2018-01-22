﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bot
{
  private Match matchData;
  private Match.Player caster;

  private int[] memory = null;
  private int initialMemoryIndex = 0;

  private float[] randomCardsProbs = new float[] { 1.0f, 0.5f, 0.25f, 0.1f, 0.0f, 0.0f, 0.0f };

  public Bot(Match matchData, Match.Player caster)
  {
    this.matchData = matchData;
    this.caster = caster;
  }

  public int[] MakeTurn()
  {
    if (memory == null)
      InitMemory();

    Spell[] availableSpells = (from sp in caster.profile.spells
                                 select Spellbook.Find(sp)).ToArray();
    Match.Player opponent = (this.matchData.User == caster) ? this.matchData.Opponent : this.matchData.User;
    int[] indices = ChooseSpell(availableSpells, opponent);

    return indices;
  }

  public int[] RecommendationForPlayer()
  {
    Spell[] availableSpells = (from sp in this.matchData.User.profile.spells select Spellbook.Find(sp)).ToArray();
    var spellsOnField = FindSpellsOnField(availableSpells, this.matchData.User.data.RestMana, true);
    var spellIndex = GetChosenSpellIndex(spellsOnField, this.matchData.User, this.matchData.Opponent);
    return spellsOnField[spellIndex].Value;
  }

  public void OnOpponentOpenedCards(int[] indices)
  {
    if (memory == null)
      InitMemory();

    for (int i = 0; i < indices.Length; i++)
      memory[indices[i]]++;
  }

  public void Forget()
  {
    if (memory == null)
      InitMemory();

    for (int i = 0; i < GameField.CARDS_COUNT; i++)
    {
      memory[i]--;
      if (memory[i] < 0) memory[i] = 0;
    }
  }

  private void InitMemory()
  {
    memory = new int[GameField.CARDS_COUNT];

    if (caster.profile.level < 7)
      initialMemoryIndex = 3;
    else if (caster.profile.level < 9)
      initialMemoryIndex = 4;
    else if (caster.profile.level < 11)
      initialMemoryIndex = 5;
    else
      initialMemoryIndex = 6;

    for (int i = 0; i < GameField.CARDS_COUNT; i++)
      memory[i] = initialMemoryIndex;

    // Forget some.
    int forgetPairsCount = GameField.CARDS_COUNT / 2 - initialMemoryIndex;
    while (forgetPairsCount != 0)
    {
      var indices = matchData.Field.GetRandomMagic();
      if (memory[indices[0]] == 0)
        continue;

      memory[indices[0]] = 0;
      memory[indices[1]] = 0;
      forgetPairsCount--;
    }
  }

  private bool CheckOpenRandomCardsProbability(int spellsOnFieldCount)
  {
    if (spellsOnFieldCount == 0)
      return true;

    if (spellsOnFieldCount > 6)
      spellsOnFieldCount = 6;

    bool result = UnityEngine.Random.value <= randomCardsProbs[spellsOnFieldCount];
    if (result)
      return true;

    int levelIndex = caster.profile.level - 1;
    if (levelIndex >= Constants.OPEN_RANDOM_CARDS.Length)
      levelIndex = Constants.OPEN_RANDOM_CARDS.Length - 1;
    return UnityEngine.Random.value <= Constants.OPEN_RANDOM_CARDS[levelIndex];
  }

  private int[] ChooseSpell(Spell[] availableSpells, Match.Player opponent)
  {
    int[] resultIndices = null;

    var spellsOnField = FindSpellsOnField(availableSpells, caster.data.RestMana, false);
    if (CheckOpenRandomCardsProbability(spellsOnField.Count))
    {
      // Open random cards.
      resultIndices = matchData.Field.GetRandomCards().ToArray();
      for (int i = 0; i < resultIndices.Length; i++)
        memory[resultIndices[i]]++;
      return resultIndices;
    }

    var spellIndex = GetChosenSpellIndex(spellsOnField, caster, opponent);
    resultIndices = spellsOnField[spellIndex].Value;

    if (resultIndices != null)
    {
      for (int i = 0; i < resultIndices.Length; i++)
        memory[resultIndices[i]]++;
    }
    return resultIndices;
  }

  private int GetChosenSpellIndex(List<KeyValuePair<Spell, int[]>> spells, Match.Player player, Match.Player opponent)
  {
    // Clear damage curse.
    if (player.data.blockedDamageTurns > 0)
    {
      int index = spells.FindIndex(x => x.Key.clearDamageCurse);
      if (index >= 0 && (player.data.RestMana - spells[index].Key.manaCost) > 0)
        return index;

      if (player.profile.level > 1)
      {
        // Increase defense.
        index = SpellEstimator.FindBestDefenseSpell(spells, player, opponent);
        if (index >= 0)
          return index;

        // Heal yourself.
        index = SpellEstimator.FindBestHealSpell(spells, player, opponent);
        if (index >= 0 && player.data.health.Value < Constants.HEALTH_POINTS)
          return index;
      }
    }

    if (player.data.health.Value < 10 && player.profile.level > 1)
    {
      // Cast damage curse.
      if (opponent.data.blockedDamageTurns == 0)
      {
        int index = spells.FindIndex(x => x.Key.blockDamageTurns > 0);
        if (index >= 0)
          return index;
      }

      {
        // Increase defense.
        int index = SpellEstimator.FindBestDefenseSpell(spells, player, opponent);
        if (index >= 0)
          return index;

        // Heal yourself.
        index = SpellEstimator.FindBestHealSpell(spells, player, opponent);
        if (index >= 0)
          return index;
      }
    }

    int maxDamage = 0;
    int maxDamageSpellIndex = SpellEstimator.FindBestDamageSpell(spells, player, opponent, out maxDamage);

    // Try to do maximum damage.
    if (maxDamageSpellIndex >= 0)
    {
      if (spells[maxDamageSpellIndex].Key.manaCost == player.data.RestMana)
      {
        if (opponent.data.health.Value <= maxDamage)
          return maxDamageSpellIndex;

        // Cast damage curse.
        if (opponent.data.blockedDamageTurns == 0)
        {
          int index = spells.FindIndex(x => x.Key.blockDamageTurns > 0);
          if (index >= 0)
            return index;
        }
      }
      return maxDamageSpellIndex;
    }

    if (player.profile.level > 1)
    {
      // Cast curses.
      if (opponent.data.blockedDamageTurns == 0)
      {
        int index = spells.FindIndex(x => x.Key.blockDamageTurns > 0);
        if (index >= 0)
          return index;
      }
      if (opponent.data.blockedHealingTurns == 0)
      {
        int index = spells.FindIndex(x => x.Key.blockHealingTurns > 0);
        if (index >= 0)
          return index;
      }
      if (opponent.data.blockedDefenseTurns == 0)
      {
        int index = spells.FindIndex(x => x.Key.blockDefenseTurns > 0);
        if (index >= 0)
          return index;
      }

      {
        // Increase defense.
        int index = SpellEstimator.FindBestDefenseSpell(spells, player, opponent);
        if (index >= 0)
          return index;

        // Heal yourself.
        index = SpellEstimator.FindBestHealSpell(spells, player, opponent);
        if (index >= 0 && player.data.health.Value < Constants.HEALTH_POINTS)
          return index;
      }
    }

    {
      int index = SpellEstimator.SelectMostRatedSpell(FilterUselessSpells(spells, player, opponent), player, opponent);
      if (index >= 0)
        return index;
    }
      
    // Cast random spell.
    return Random.Range(0, spells.Count);
  }
    
  private List<KeyValuePair<Spell, int[]>> FindSpellsOnField(Spell[] spells, int availableMana, bool forceMemory)
  {
    var availableMagic = new List<KeyValuePair<Magic, int>>();
    for (int i = 0; i < GameField.CARDS_COUNT; i++)
    {
      if (forceMemory || memory[i] > 0)
        availableMagic.Add(new KeyValuePair<Magic, int>(matchData.Field.Cards[i], i));
    }

    var result = new List<KeyValuePair<Spell, int[]>>();
    for (int i = 0; i < spells.Length; i++)
    {
      if (spells[i].manaCost > availableMana)
        continue;

      var indices = IncludesCombination(availableMagic, spells[i].Combination);
      if (indices != null)
        result.Add(new KeyValuePair<Spell, int[]>(spells[i], indices));
    }
    return result;
  }

  private int[] IncludesCombination(List<KeyValuePair<Magic, int>> magic, Magic[] combination)
  {
    var dict = new Dictionary<Magic, int>();
    foreach (var c in combination)
    {
      if (dict.ContainsKey(c))
        dict[c] = dict[c] + 1;
      else
        dict.Add(c, 1);
    }

    List<int> indices = new List<int>();
    foreach (Magic m in dict.Keys)
    {
      var pairs = magic.FindAll(x => x.Key == m);
      if (pairs.Count < dict[m])
        return null;

      for (int i = 0; i < dict[m]; i++)
        indices.Add(pairs[i].Value);
    }
    if (combination.Length != indices.Count)
      throw new UnityException("Bad logic");

    return indices.ToArray();
  }

  private List<KeyValuePair<Spell, int[]>> FilterUselessSpells(List<KeyValuePair<Spell, int[]>> spells,
                                                               Match.Player player, Match.Player opponent)
  {
    var result = new List<KeyValuePair<Spell, int[]>>();
    foreach (var s in spells)
    {
      if (s.Key.healing > 0 && (player.data.health.Value == Constants.HEALTH_POINTS || player.data.blockedHealingTurns > 0))
        continue;
      if (s.Key.defense > 0 && player.data.blockedDefenseTurns > 0)
        continue;
      if (s.Key.damage > 0 && player.data.blockedDamageTurns > 0)
        continue;
      if (s.Key.clearDamageCurse && player.data.blockedDamageTurns == 0)
        continue;
      if (s.Key.clearHealingCurse && player.data.blockedHealingTurns == 0)
        continue;
      if (s.Key.clearDefenseCurse && player.data.blockedDefenseTurns == 0)
        continue;
      if (s.Key.blockDamageTurns > 0 && opponent.data.blockedDamageTurns > 0)
        continue;
      if (s.Key.blockHealingTurns > 0 && opponent.data.blockedHealingTurns > 0)
        continue;
      if (s.Key.blockDefenseTurns > 0 && opponent.data.blockedDefenseTurns > 0)
        continue;

      result.Add(s);
    }
    return result;
  }
}
