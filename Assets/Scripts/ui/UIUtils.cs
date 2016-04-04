using System;
using SmartLocalization;
using System.Text;

public static class UIUtils
{
    public static string GetFormattedString(ProfileData profileData)
    {
        return string.Format("{0}\n{1} {2}", profileData.name, i18n("Player.Level"), profileData.level);
    }
       
	public static string GetSpellDescription(JSONObject spellObject)
    {
        StringBuilder builder = new StringBuilder();
        JSONObject obj = spellObject.GetField("damage");
        if (obj != null)
        {
            builder.Append(i18n("SpellDesc.Damage") + " ");
            builder.Append(obj.i);
            builder.Append("\n");
        }

        obj = spellObject.GetField("defense");
        if (obj != null)
        {
            builder.Append(i18n("SpellDesc.Defense") + " +");
            builder.Append(obj.i);
            builder.Append("\n");
        }
			
        obj = spellObject.GetField("healing");
        if (obj != null)
        {
            builder.Append(i18n("SpellDesc.Health") + " +");
            builder.Append(obj.i);
            builder.Append("\n");
        }

        FillBlockProperty(builder, spellObject, "blockDamageTurns", i18n("SpellDesc.BlockDmg"), i18n("SpellDesc.BlockDmg2"));
        FillBlockProperty(builder, spellObject, "blockHealingTurns", i18n("SpellDesc.BlockHeal"), i18n("SpellDesc.BlockHeal2"));
        FillBlockProperty(builder, spellObject, "blockDefenseTurns", i18n("SpellDesc.BlockDef"), i18n("SpellDesc.BlockDef2"));

        FillBlockProperty(builder, spellObject, "blockFireBonusTurns", i18n("SpellDesc.BlockFB"), i18n("SpellDesc.BlockFB2"));
        FillBlockProperty(builder, spellObject, "blockWaterBonusTurns", i18n("SpellDesc.BlockWB"), i18n("SpellDesc.BlockWB2"));
        FillBlockProperty(builder, spellObject, "blockAirBonusTurns", i18n("SpellDesc.BlockAB"), i18n("SpellDesc.BlockAB2"));

        FillBlockProperty(builder, spellObject, "blockFireResistanceTurns", i18n("SpellDesc.BlockFR"), i18n("SpellDesc.BlockFR2"));
        FillBlockProperty(builder, spellObject, "blockWaterResistanceTurns", i18n("SpellDesc.BlockWR"), i18n("SpellDesc.BlockWR2"));
        FillBlockProperty(builder, spellObject, "blockAirResistanceTurns", i18n("SpellDesc.BlockAR"), i18n("SpellDesc.BlockAR2"));

        obj = spellObject.GetField("considerResistance");
        if (obj != null && !obj.b)
        {
            builder.Append(i18n("SpellDesc.ConsiderRes"));
            builder.Append("\n");
        }

        obj = spellObject.GetField("considerDefense");
        if (obj != null && !obj.b)
        {
            builder.Append(i18n("SpellDesc.ConsiderDef"));
            builder.Append("\n");
        }

        obj = spellObject.GetField("removeDefense");
        if (obj != null && obj.b)
        {
            builder.Append(i18n("SpellDesc.RemoveDef"));
            builder.Append("\n");
        }

        FillClearProperty(builder, spellObject, "clearDamageCurse", i18n("SpellDesc.ClearDmg"));
        FillClearProperty(builder, spellObject, "clearHealingCurse", i18n("SpellDesc.ClearHeal"));
        FillClearProperty(builder, spellObject, "clearDefenseCurse", i18n("SpellDesc.ClearDef"));

        FillClearProperty(builder, spellObject, "clearFireBonusCurse", i18n("SpellDesc.ClearFB"));
        FillClearProperty(builder, spellObject, "clearWaterBonusCurse", i18n("SpellDesc.ClearWB"));
        FillClearProperty(builder, spellObject, "clearAirBonusCurse", i18n("SpellDesc.ClearAB"));

        FillClearProperty(builder, spellObject, "clearFireResistanceCurse", i18n("SpellDesc.ClearFR"));
        FillClearProperty(builder, spellObject, "clearWaterResistanceCurse", i18n("SpellDesc.ClearWR"));
        FillClearProperty(builder, spellObject, "clearAirResistanceCurse", i18n("SpellDesc.ClearAR"));

		return builder.ToString();
	}

    private static string i18n(string s)
    {
        return LanguageManager.Instance.GetTextValue(s);
    }

    private static void FillBlockProperty(StringBuilder builder, JSONObject spellObject, string field, string s1, string s2)
    {
        JSONObject obj = spellObject.GetField(field);
        if (obj != null)
        {
            if (obj.i == 1)
            {
                builder.Append(s1);
                builder.Append("\n");
            }
            else
            {
                builder.Append(s2);
                builder.Append(" (");
                builder.Append(obj.i);
                builder.Append(")\n");
            }
        }
    }

    private static void FillClearProperty(StringBuilder builder, JSONObject spellObject, string field, string s)
    {
        JSONObject obj = spellObject.GetField(field);
        if (obj != null && obj.b)
        {
            builder.Append(s);
            builder.Append("\n");
        }
    }
}
