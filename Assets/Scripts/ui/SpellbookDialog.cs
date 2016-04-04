using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SmartLocalization;

public class SpellbookDialog : MonoBehaviour
{
    public GameObject spellInfoPrefab;
    public Image splash;
    public GameObject content;

	private SpellData[] allSpells;

    public delegate void OnClose();
    private OnClose onCloseHandler;

    public void Start()
    {
        gameObject.SetActive(false);
    }

	public void Setup(List<SpellData> spells)
	{
		allSpells = spells.ToArray();
		allSpells.OrderBy(x => x.minLevel).ThenBy(x => x.combination[0]).ThenBy(x => x.combination[1]);

		for (int i = content.transform.childCount - 1; i >= 0; --i)
		{
			GameObject.Destroy(content.transform.GetChild(i).gameObject);
		}
		content.transform.DetachChildren();

		const float kSpacing = 10.0f;
		float startOffsetY = -kSpacing;
		float height = 0;
		for (int i = 0; i < allSpells.Length; i++)
		{
			GameObject spellInfo = Instantiate(spellInfoPrefab);
			spellInfo.transform.SetParent(content.transform, false);
			Rect r = spellInfo.GetComponent<RectTransform>().rect;
			float h = r.height + kSpacing;
			spellInfo.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(kSpacing, -i * h + startOffsetY, 0.0f);
			height += h;

			SpellData data = allSpells[i];

			Text title = (from t in spellInfo.GetComponentsInChildren<Text>() where t.gameObject.name == "SpellTitle" select t).Single();
			string spellName = LanguageManager.Instance.GetTextValue("Spell." + data.type);
			if (spellName.Length == 0) spellName = data.type;
			title.text = spellName;

			Text desc = (from t in spellInfo.GetComponentsInChildren<Text>() where t.gameObject.name == "SpellDesc" select t).Single();
			desc.text = data.desc;

			//TODO: min level, combination
		}
		height += kSpacing;
		content.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
	}

    public void Update()
    {
        CloseIfClickedOutside(this.gameObject);
    }

    public void Open(OnClose onCloseHandler)
    {
        this.onCloseHandler = onCloseHandler;

        ScrollRect scroll = gameObject.GetComponentInChildren<ScrollRect>();
        scroll.verticalNormalizedPosition = 1.0f;

        gameObject.SetActive(true);
        if (splash != null && !splash.IsActive())
            splash.gameObject.SetActive(true);
    }

    public void Close()
    {
        if (splash != null && splash.IsActive())
            splash.gameObject.SetActive(false);

        gameObject.SetActive(false);

        if (onCloseHandler != null)
            onCloseHandler();
    }

    private void CloseIfClickedOutside(GameObject panel)
    {
         if (Input.GetMouseButtonDown(0) && panel.activeSelf && 
             !RectTransformUtility.RectangleContainsScreenPoint(
                 panel.GetComponent<RectTransform>(), 
                 Input.mousePosition, 
                 Camera.main)) {
            Close();
         }
     }
}