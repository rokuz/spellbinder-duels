using System.Collections;
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

    if (caster.profile.level < 5)
      initialMemoryIndex = 2;
    else if (caster.profile.level < 7)
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

      // Increase defense.
      index = FindDefenseSpell(spells, player, opponent);
      if (index >= 0)
        return index;

      // Heal yourself.
      index = FindHealSpell(spells, player, opponent);
      if (index >= 0 && player.data.health.Value < Constants.HEALTH_POINTS)
        return index;
    }

    if (player.data.health.Value < 10)
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
        int index = FindDefenseSpell(spells, player, opponent);
        if (index >= 0)
          return index;

        // Heal yourself.
        index = FindHealSpell(spells, player, opponent);
        if (index >= 0)
          return index;
      }
    }

    int maxDamage = 0;
    int maxDamageSpellIndex = FindSpellWithMaxDamage(spells, player, opponent, out maxDamage);

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

    // Cast random spell.
    return Random.Range(0, spells.Count);
  }

  private int FindDefenseSpell(List<KeyValuePair<Spell, int[]>> spells, Match.Player player, Match.Player opponent)
  {
    if (player.data.blockedDefenseTurns == 0)
    {
      int index = spells.FindIndex(x => x.Key.defense > 0);
      if (index >= 0)
        return index;
    }
    else
    {
      int index = spells.FindIndex(x => x.Key.clearDefenseCurse);
      if (index >= 0)
        return index;
    }
    return -1;
  }

  private int FindHealSpell(List<KeyValuePair<Spell, int[]>> spells, Match.Player player, Match.Player opponent)
  {
    if (player.data.blockedHealingTurns == 0)
    {
      int index = spells.FindIndex(x => x.Key.healing > 0);
      if (index >= 0)
        return index;
    }
    else
    {
      int index = spells.FindIndex(x => x.Key.clearHealingCurse);
      if (index >= 0)
        return index;
    }
    return -1;
  }

  private int FindSpellWithMaxDamage(List<KeyValuePair<Spell, int[]>> spells, Match.Player player, Match.Player opponent, out int maxDamage)
  {
    int index = -1;
    maxDamage = 0;
    for (int i = 0; i < spells.Count; i++)
    {
      var dmg = spells[i].Key.damage;
      if (dmg == 0)
        continue;
      var elemIndex = spells[i].Key.Index;
      if (spells[i].Key.Index >= 0)
        dmg += ((player.profile.bonuses[elemIndex] - opponent.profile.resistance[elemIndex]) / 1000);

      if (dmg > maxDamage && spells[i].Key.manaCost <= player.data.RestMana)
      {
        maxDamage = spells[i].Key.damage;
        index = i;
      }
    }
    return index;
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
}
