using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Analytics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SmartLocalization;

public class SpellbookDialog : MonoBehaviour
{
  public GameObject spellInfoPrefab;
  public Image splash;
  public GameObject content;

  public Text title;

  public SpellSpritesHolder spellSpritesHolder;

  public ShopDialog shopDialog;

  private Spell[] allSpells;
  private GameObject[] spellInfos;

  private ProfileData profileData;

  public delegate void OnClose();
  private OnClose onCloseHandler;

	public void Setup()
	{
		allSpells = Spellbook.Spells.ToArray().OrderBy(x => x.minLevel).ThenBy(x => x.Combination[0]).ThenBy(x => x.Combination[1]).ToArray();

		for (int i = content.transform.childCount - 1; i >= 0; --i)
		{
			GameObject.Destroy(content.transform.GetChild(i).gameObject);
		}
		content.transform.DetachChildren();

		const float kSpacing = 10.0f;
		float startOffsetY = -kSpacing;
		float height = 0;
    spellInfos = new GameObject[allSpells.Length];
		for (int i = 0; i < allSpells.Length; i++)
		{
			GameObject spellInfo = Instantiate(spellInfoPrefab);
			spellInfo.transform.SetParent(content.transform, false);
			Rect r = spellInfo.GetComponent<RectTransform>().rect;
			float h = r.height + kSpacing;
			spellInfo.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(kSpacing, -i * h + startOffsetY, 0.0f);
			height += h;

      spellInfos[i] = spellInfo;

      var spellIcon = (from t in spellInfo.GetComponentsInChildren<Image>() where t.gameObject.name == "Spell" select t).Single();
      spellIcon.sprite = spellSpritesHolder.GetSprite(allSpells[i]);

			Text title = (from t in spellInfo.GetComponentsInChildren<Text>() where t.gameObject.name == "SpellTitle" select t).Single();
      string spellName = LanguageManager.Instance.GetTextValue("Spell." + allSpells[i].SpellType);
      if (spellName.Length == 0) spellName = allSpells[i].SpellType.ToString();
      title.text = spellName;

      Text level = (from t in spellInfo.GetComponentsInChildren<Text>() where t.gameObject.name == "SpellLevel" select t).Single();
      level.text = LanguageManager.Instance.GetTextValue("SpellDesc.Level") + " " + allSpells[i].minLevel;

			Text desc = (from t in spellInfo.GetComponentsInChildren<Text>() where t.gameObject.name == "SpellDesc" select t).Single();
      desc.text = UIUtils.GetSpellDescription(allSpells[i]);

      Text manaCost = (from t in spellInfo.GetComponentsInChildren<Text>() where t.gameObject.name == "ManaCost" select t).Single();
      manaCost.text = "" + allSpells[i].manaCost;

      if (allSpells[i].Combination.Length == 2)
      {
        var obj = (from t in spellInfo.GetComponentsInChildren<Image>() where t.gameObject.name == "Icon1" select t).Single();
        obj.gameObject.SetActive(false);

        var sr1 = (from t in spellInfo.GetComponentsInChildren<Image>() where t.gameObject.name == "IconImage2" select t).Single();
        sr1.sprite = spellSpritesHolder.GetSprite(allSpells[i].Combination[0]);

        var sr2 = (from t in spellInfo.GetComponentsInChildren<Image>() where t.gameObject.name == "IconImage3" select t).Single();
        sr2.sprite = spellSpritesHolder.GetSprite(allSpells[i].Combination[1]);
      }
      else if (allSpells[i].Combination.Length == 3)
      {
        var sr1 = (from t in spellInfo.GetComponentsInChildren<Image>() where t.gameObject.name == "IconImage1" select t).Single();
        sr1.sprite = spellSpritesHolder.GetSprite(allSpells[i].Combination[0]);

        var sr2 = (from t in spellInfo.GetComponentsInChildren<Image>() where t.gameObject.name == "IconImage2" select t).Single();
        sr2.sprite = spellSpritesHolder.GetSprite(allSpells[i].Combination[1]);

        var sr3 = (from t in spellInfo.GetComponentsInChildren<Image>() where t.gameObject.name == "IconImage3" select t).Single();
        sr3.sprite = spellSpritesHolder.GetSprite(allSpells[i].Combination[2]);
      }
      else
      {
        throw new UnityException("Invalid game logic");
      }
		}
		height += kSpacing;
		content.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
    this.title.text = LanguageManager.Instance.GetTextValue("Spellbook.Title");
	}

  public void Update()
  {
    CloseIfClickedOutside(this.gameObject);
  }

  public bool IsOpened()
  {
    return gameObject.activeSelf;
  }

  public void Open(ProfileData profileData, OnClose onCloseHandler)
  {
    Analytics.CustomEvent("Spellbook_Open");

    this.profileData = profileData;
    this.onCloseHandler = onCloseHandler;
    UpdateUserSpells();

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
    if (Input.GetMouseButtonDown(0) && panel.activeSelf && !shopDialog.IsOpened() &&
        !RectTransformUtility.RectangleContainsScreenPoint(panel.GetComponent<RectTransform>(), Input.mousePosition, null))
    {
      Close();
    }
  }

  private void UpdateUserSpells()
  {
    if (this.profileData != null)
    {
      for (int i = 0; i < allSpells.Length; i++)
      {
        bool hasSpell = ((from s in this.profileData.spells where s == allSpells[i].Code select s).Count() > 0);
        var buyButton = (from t in spellInfos[i].GetComponentsInChildren<Button>(true) where t.gameObject.name == "BuyButton" select t).Single();
        var buyButtonText = (from t in buyButton.GetComponentsInChildren<Text>(true) where t.gameObject.name == "Text" select t).Single();
        buyButtonText.text = LanguageManager.Instance.GetTextValue("Spell.Learn");
        buyButton.gameObject.SetActive(!hasSpell);
        Spell sp = allSpells[i];
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => { buyButton.GetComponent<AudioSource>().Play(); this.OnBuy(sp); });
      }
    }
  }

  private void OnBuy(Spell spell)
  {
    var p = new Dictionary<string, object>();
    p.Add("spell", spell.SpellType);
    Analytics.CustomEvent("Spellbook_Learn", p);

    shopDialog.Open(Persistence.gameConfig.profile, spell, () => { UpdateUserSpells(); });
  }
}