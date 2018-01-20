using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Spellbook
{
  public static List<Spell> Spells = new List<Spell>();

  public static void Init()
  {
    if (Spells.Count != 0) return;

    // Basic
    var spell = new Spell(new Magic[] { Magic.FIRE, Magic.FIRE }, Spell.Type.FIREBALL);
    spell.manaCost = 1;
    spell.damage = 2;
    Spells.Add(spell);

    spell = new Spell(new Magic[] { Magic.WATER, Magic.WATER }, Spell.Type.ICE_SPEAR);
    spell.manaCost = 1;
    spell.damage = 2;
    Spells.Add(spell);

    spell = new Spell(new Magic[] { Magic.AIR, Magic.AIR }, Spell.Type.LIGHTNING);
    spell.manaCost = 1;
    spell.damage = 2;
    Spells.Add(spell);

    spell = new Spell(new Magic[] { Magic.DARKNESS, Magic.DARKNESS }, Spell.Type.DEATH_LOOK);
    spell.manaCost = 1;
    spell.blockDamageTurns = 1;
    Spells.Add(spell);

    spell = new Spell(new Magic[] { Magic.BLOOD, Magic.BLOOD }, Spell.Type.BLEEDING);
    spell.manaCost = 1;
    spell.blockHealingTurns = 1;
    Spells.Add(spell);

    spell = new Spell(new Magic[] { Magic.EARTH, Magic.EARTH }, Spell.Type.STONESKIN);
    spell.manaCost = 1;
    spell.defense = 1;
    Spells.Add(spell);

    spell = new Spell(new Magic[] { Magic.ILLUSION, Magic.ILLUSION }, Spell.Type.DOPPELGANGER);
    spell.manaCost = 1;
    spell.clearHealingCurse = true;
    Spells.Add(spell);

    spell = new Spell(new Magic[] { Magic.NATURE, Magic.NATURE }, Spell.Type.NATURE_CALL);
    spell.manaCost = 1;
    spell.healing = 1;
    Spells.Add(spell);

    spell = new Spell(new Magic[] { Magic.LIGHT, Magic.LIGHT }, Spell.Type.BLESSING);
    spell.manaCost = 1;
    spell.clearDamageCurse = true;
    Spells.Add(spell);

    spell = new Spell(new Magic[] { Magic.FIRE, Magic.AIR, Magic.FIRE }, Spell.Type.METEORITE);
    spell.manaCost = 1;
    spell.minLevel = 3;
    spell.damage = 4;
    Spells.Add(spell);

    spell = new Spell(new Magic[] { Magic.WATER, Magic.AIR, Magic.WATER }, Spell.Type.ICE_RAIN);
    spell.manaCost = 1;
    spell.minLevel = 3;
    spell.damage = 4;
    Spells.Add(spell);

    spell = new Spell(new Magic[] { Magic.AIR, Magic.NATURE, Magic.AIR }, Spell.Type.STORM);
    spell.manaCost = 1;
    spell.minLevel = 5;
    spell.damage = 3;
    spell.clearDamageCurse = true;
    Spells.Add(spell);

    spell = new Spell(new Magic[] { Magic.DARKNESS, Magic.EARTH, Magic.DARKNESS }, Spell.Type.DARKNESS_SHIELD);
    spell.manaCost = 1;
    spell.minLevel = 6;
    spell.defense = 3;
    Spells.Add(spell);

    spell = new Spell(new Magic[] { Magic.BLOOD, Magic.ILLUSION, Magic.BLOOD }, Spell.Type.BLOOD_SIGN);
    spell.manaCost = 1;
    spell.minLevel = 6;
    spell.blockHealingTurns = 1;
    spell.blockDefenseTurns = 1;
    spell.blockDamageTurns = 1;
    Spells.Add(spell);

    spell = new Spell(new Magic[] { Magic.NATURE, Magic.DARKNESS, Magic.NATURE }, Spell.Type.POISONING);
    spell.manaCost = 2;
    spell.minLevel = 7;
    spell.damage = 2;
    spell.blockHealingTurns = 2;
    spell.removeDefense = true;
    Spells.Add(spell);
  }

  public static Spell Find(string code)
  {
    foreach (var s in Spells)
    {
      if (s.Code == code)
        return s;
    }
    return null;
  }

  public static Spell Find(Spell.Type type)
  {
    foreach (var s in Spells)
    {
      if (s.SpellType == type)
        return s;
    }
    return null;
  }
}
