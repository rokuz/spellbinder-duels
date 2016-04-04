using System;
using SmartLocalization;
using System.Text;

public static class UIUtils
{
    public static string GetFormattedString(ProfileData profileData)
    {
        return string.Format("{0}\n{1} {2}", profileData.name, LanguageManager.Instance.GetTextValue("Player.Level"), profileData.level);
    }

	/*
	// Number of turns in which opponent's damage bonuses are blocked
	public int[] blockBonusTurns = new int[] { 0, 0, 0 };

	// Number of turns in which opponent's resistance bonuses are blocked
	public int[] blockResistanceTurns = new int[] { 0, 0, 0 };

	// If a spell consider opponent's resistance
	public boolean considerResistance = true;

	// If a spell consider opponent's defense
	public boolean considerDefense = true;

	// If a spell remove opponent's defense
	public boolean removeDefense = false;

	// If a spell clear player's damage curse
	public boolean clearDamageCurse = false;

	// If a spell clear player's healing curse
	public boolean clearHealingCurse = false;

	// If a spell clear player's defense curse
	public boolean clearDefenseCurse = false;

	// If a spell clear player's bonus curse
	public boolean[] clearBonusCurse = new boolean[] { false, false, false };

	// If a spell clear player's resistance curse
	public boolean[] clearResistanceCurse = new boolean[] { false, false, false };
	*/

	public static string GetSpellDescription(JSONObject spellObject)
	{
		StringBuilder builder = new StringBuilder();
		JSONObject obj = spellObject.GetField("damage");
		if (obj != null)
		{
			builder.Append("Damage" + " ");
			builder.Append(obj.i);
			builder.Append("\n");
		}

		obj = spellObject.GetField("defense");
		if (obj != null)
		{
			builder.Append("Defense" + " +");
			builder.Append(obj.i);
			builder.Append("\n");
		}
			
		obj = spellObject.GetField("healing");
		if (obj != null)
		{
			builder.Append("Health" + " +");
			builder.Append(obj.i);
			builder.Append("\n");
		}

		obj = spellObject.GetField("blockDamageTurns");
		if (obj != null)
		{
			if (obj.i == 1)
			{
				builder.Append ("Blocks damage for a turn\n");
			}
			else
			{
				builder.Append ("Blocks damage for several turn (");
				builder.Append (obj.i);
				builder.Append (")\n");
			}
		}

		obj = spellObject.GetField("blockHealingTurns");
		if (obj != null)
		{
			if (obj.i == 1)
			{
				builder.Append ("Blocks healing for a turn\n");
			}
			else
			{
				builder.Append ("Blocks healing for several turn (");
				builder.Append (obj.i);
				builder.Append (")\n");
			}
		}

		obj = spellObject.GetField("blockDefenseTurns");
		if (obj != null)
		{
			if (obj.i == 1)
			{
				builder.Append ("Blocks defense for a turn\n");
			}
			else
			{
				builder.Append ("Blocks defense for several turn (");
				builder.Append (obj.i);
				builder.Append (")\n");
			}
		}
			
		return builder.ToString();
	}
}
