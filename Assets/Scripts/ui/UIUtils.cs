using System;
using SmartLocalization;
using System.Text;

public static class UIUtils
{
  public static string GetFormattedString(ProfileData profileData)
  {
    return string.Format("{0}\n{1} {2}", profileData.name, i18n("Player.Level"), profileData.level);
  }

  public static string GetProfileDesc(ProfileData profileData)
  {
    return string.Format("{0}: {1}\n{2}: {3}\n{4}: {5}", i18n("Player.Level"), profileData.level,
                                                         i18n("Leaderboard.Victories"), profileData.victories,
                                                         i18n("Leaderboard.Defeats"), profileData.defeats);
  }
     
  public static string GetSpellDescription(Spell spellObject)
  {
    StringBuilder builder = new StringBuilder();
    if (spellObject.damage != 0)
    {
      builder.Append(i18n("SpellDesc.Damage") + " ");
      builder.Append(spellObject.damage);
      builder.Append("\n");
    }

    if (spellObject.defense != 0)
    {
      builder.Append(i18n("SpellDesc.Defense") + " +");
      builder.Append(spellObject.defense);
      builder.Append("\n");
    }
	
    if (spellObject.healing != 0)
    {
      builder.Append(i18n("SpellDesc.Health") + " +");
      builder.Append(spellObject.healing);
      builder.Append("\n");
    }

    FillBlockProperty(builder, spellObject, spellObject.blockDamageTurns, i18n("SpellDesc.BlockDmg"), i18n("SpellDesc.BlockDmg2"));
    FillBlockProperty(builder, spellObject, spellObject.blockHealingTurns, i18n("SpellDesc.BlockHeal"), i18n("SpellDesc.BlockHeal2"));
    FillBlockProperty(builder, spellObject, spellObject.blockDefenseTurns, i18n("SpellDesc.BlockDef"), i18n("SpellDesc.BlockDef2"));

    FillBlockProperty(builder, spellObject, spellObject.blockBonusTurns[Spell.FIRE_INDEX], i18n("SpellDesc.BlockFB"), i18n("SpellDesc.BlockFB2"));
    FillBlockProperty(builder, spellObject, spellObject.blockBonusTurns[Spell.WATER_INDEX], i18n("SpellDesc.BlockWB"), i18n("SpellDesc.BlockWB2"));
    FillBlockProperty(builder, spellObject, spellObject.blockBonusTurns[Spell.AIR_INDEX], i18n("SpellDesc.BlockAB"), i18n("SpellDesc.BlockAB2"));

    FillBlockProperty(builder, spellObject, spellObject.blockResistanceTurns[Spell.FIRE_INDEX], i18n("SpellDesc.BlockFR"), i18n("SpellDesc.BlockFR2"));
    FillBlockProperty(builder, spellObject, spellObject.blockResistanceTurns[Spell.WATER_INDEX], i18n("SpellDesc.BlockWR"), i18n("SpellDesc.BlockWR2"));
    FillBlockProperty(builder, spellObject, spellObject.blockResistanceTurns[Spell.AIR_INDEX], i18n("SpellDesc.BlockAR"), i18n("SpellDesc.BlockAR2"));

    if (!spellObject.considerResistance)
    {
      builder.Append(i18n("SpellDesc.ConsiderRes"));
      builder.Append("\n");
    }

    if (!spellObject.considerDefense)
    {
      builder.Append(i18n("SpellDesc.ConsiderDef"));
      builder.Append("\n");
    }

    if (spellObject.removeDefense)
    {
      builder.Append(i18n("SpellDesc.RemoveDef"));
      builder.Append("\n");
    }

    FillClearProperty(builder, spellObject, spellObject.clearDamageCurse, i18n("SpellDesc.ClearDmg"));
    FillClearProperty(builder, spellObject, spellObject.clearHealingCurse, i18n("SpellDesc.ClearHeal"));
    FillClearProperty(builder, spellObject, spellObject.clearDefenseCurse, i18n("SpellDesc.ClearDef"));

    FillClearProperty(builder, spellObject, spellObject.clearBonusCurse[Spell.FIRE_INDEX], i18n("SpellDesc.ClearFB"));
    FillClearProperty(builder, spellObject, spellObject.clearBonusCurse[Spell.WATER_INDEX], i18n("SpellDesc.ClearWB"));
    FillClearProperty(builder, spellObject, spellObject.clearBonusCurse[Spell.AIR_INDEX], i18n("SpellDesc.ClearAB"));

    FillClearProperty(builder, spellObject, spellObject.clearResistanceCurse[Spell.FIRE_INDEX], i18n("SpellDesc.ClearFR"));
    FillClearProperty(builder, spellObject, spellObject.clearResistanceCurse[Spell.WATER_INDEX], i18n("SpellDesc.ClearWR"));
    FillClearProperty(builder, spellObject, spellObject.clearResistanceCurse[Spell.AIR_INDEX], i18n("SpellDesc.ClearAR"));

		return builder.ToString();
	}

  public static string GetShopSpellDescription(ProfileData profile, Spell spellObject)
  {
    StringBuilder builder = new StringBuilder();
    if (profile.level < spellObject.minLevel)
      builder.Append("<color=red>");
    builder.Append(i18n("Shop.RequiredLevel"));
    builder.Append(" ");
    builder.Append(spellObject.minLevel);
    if (profile.level < spellObject.minLevel)
      builder.Append("</color>");
    builder.Append("\n");
    builder.Append(GetSpellDescription(spellObject));
    return builder.ToString();
  }

  private static string i18n(string s)
  {
    return LanguageManager.Instance.GetTextValue(s);
  }

  private static void FillBlockProperty(StringBuilder builder, Spell spellObject, int field, string s1, string s2)
  {
    if (field == 0)
      return;

    if (field == 1)
    {
      builder.Append(s1);
      builder.Append("\n");
    }
    else
    {
      builder.Append(s2);
      builder.Append(" (");
      builder.Append(field);
      builder.Append(")\n");
    }
  }

  private static void FillClearProperty(StringBuilder builder, Spell spellObject, bool field, string s)
  {
    if (field)
    {
      builder.Append(s);
      builder.Append("\n");
    }
  }
}
