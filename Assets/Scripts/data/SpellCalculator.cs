using System;

public class SpellCalculator
{
  private int[] initialBonuses = new int[] { 0, 0, 0 };
  private int[] initialResistance = new int[] { 0, 0, 0 };

  private int[] bonuses = new int[] { 0, 0, 0 };
  private int[] resistance = new int[] { 0, 0, 0 };

  public SpellCalculator(int[] initialBonuses, int[] initialResistance)
  {
    Array.Copy(initialBonuses, this.initialBonuses, this.initialBonuses.Length);
    Array.Copy(initialResistance, this.initialResistance, this.initialResistance.Length);
  }

  public int[] Bonuses
  {
    get { return bonuses; }
  }

  public int[] Resistance
  {
    get { return resistance; }
  }

  public void OnCastSpell(Spell spell)
  {
    int index = spell.Index;
    if (index < 0)
      return;

    if (initialBonuses[index] == 0)
    {
      bonuses[index] += spell.Complex ? 10 : 5;
    }
    else if (initialBonuses[index] < 3)
    {
      bonuses[index] += spell.Complex ? 5 : 2;
    }
    else
    {
      bonuses[index] += spell.Complex ? 2 : 1;
    }
  }

  public void OnReceiveSpell(Spell spell)
  {
    int index = spell.Index;
    if (index < 0)
      return;

    if (initialResistance[index] == 0)
    {
      resistance[index] += spell.Complex ? 8 : 3;
    }
    else if (initialResistance[index] < 3)
    {
      resistance[index] += spell.Complex ? 4 : 2;
    }
    else
    {
      resistance[index] += spell.Complex ? 2 : 1;
    }
  }
}
