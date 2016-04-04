using UnityEngine;
using System.Collections.Generic;

public class SpellData
{
	public string type;
	public Magic[] combination;
	public int minLevel;
	public string desc;
}

public interface ISpellRequestsHandler
{
	void OnGetAllSpells(List<SpellData> spells);
	void OnSpellError(int code);
}

public static class SpellRequests
{
	public const string GET_ALL = "spells";

	public static void OnGetAllResponse(WWW response, ISpellRequestsHandler handler)
	{
		if (response.error == null)
		{
			List<SpellData> result = new List<SpellData>();

			JSONObject json = JSONObject.Create(response.text);
			if (json.IsArray)
			{
				foreach (JSONObject obj in json.list)
				{
					SpellData data = new SpellData();
					JSONObject spellObj = obj.GetField("spell");
					data.type = spellObj.GetField("type").str;
					data.combination = MagicUtils.MagicFromStrings(Utils.ToStringArray(spellObj.GetField("combination").list.ToArray()));
					data.minLevel = (int)spellObj.GetField("minLevel").i;
					data.desc = UIUtils.GetSpellDescription(spellObj);
					result.Add(data);
				}
			}

			if (handler != null)
				handler.OnGetAllSpells(result);
		}
		else
		{
			Debug.Log("" + response.error);
			if (handler != null)
				handler.OnSpellError(400);
		}
	}
}
