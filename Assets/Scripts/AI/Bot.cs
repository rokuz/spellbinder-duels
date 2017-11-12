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

  public Bot(Match matchData, Match.Player caster)
  {
    this.matchData = matchData;
    this.caster = caster;
  }

  public List<int[]> MakeTurns()
  {
    if (memory == null)
      InitMemory();

    Spell[] availableSpells = (from sp in caster.profile.spells
                                 select Spellbook.Find(sp)).ToArray();
    Match.Player opponent = (this.matchData.User == caster) ? this.matchData.Opponent : this.matchData.User;
    var result = new List<int[]>();
    int[] indices = null;
    do
    {
      indices = ChooseSpell(availableSpells, opponent);
      if (indices != null)
        result.Add(indices);
    }
    while (indices != null);

    for (int i = 0; i < GameField.CARDS_COUNT; i++)
    {
      memory[i]--;
      if (memory[i] < 0) memory[i] = 0;
    }

    return result;
  }

  public void OnOpponentOpenedCards(int[] indices)
  {
    for (int i = 0; i < indices.Length; i++)
      memory[indices[i]]++;
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

  private int[] ChooseSpell(Spell[] availableSpells, Match.Player opponent)
  {
    int[] result = null;

    var spells = FindSpellsOnField(availableSpells);
    //TODO: choose spell

    if (result != null)
    {
      for (int i = 0; i < result.Length; i++)
        memory[result[i]]++;
    }
    return result;
  }

  private List<Spell> FindSpellsOnField(Spell[] spells)
  {
    var availableMagic = new List<Magic>();
    for (int i = 0; i < GameField.CARDS_COUNT; i++)
    {
      if (memory[i] > 0)
        availableMagic.Add(matchData.Field.Cards[i]);
    }

    var result = new List<Spell>();
    for (int i = 0; i < spells.Length; i++)
    {
      if (IncludesCombination(availableMagic, spells[i].Combination))
        result.Add(spells[i]);
    }
    return result;
  }

  private bool IncludesCombination(List<Magic> magic, Magic[] combination)
  {
    var dict = new Dictionary<Magic, int>();
    foreach (var c in combination)
    {
      if (dict.ContainsKey(c))
        dict[c] = dict[c] + 1;
      else
        dict.Add(c, 1);
    }

    foreach (Magic m in dict.Keys)
    {
      if (magic.FindAll(x => x == m).Count != dict[m])
        return false;
    }
    return true;
  }
}
