using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SmartLocalization;

public class SpellbookWidget : MonoBehaviour
{
  public GameObject spellInfoPrefab;
  public GameObject content;
  public GameObject spellDescPanel;

  public Sprite fireIconSprite;
  public Sprite waterIconSprite;
  public Sprite airIconSprite;
  public Sprite earthIconSprite;
  public Sprite natureIconSprite;
  public Sprite lightIconSprite;
  public Sprite darknessIconSprite;
  public Sprite bloodIconSprite;
  public Sprite illusionIconSprite;

  public Sprite bleedingSprite;
  public Sprite blessingSprite;
  public Sprite deathLookSprite;
  public Sprite doppelgangerSprite;
  public Sprite fireballSprite;
  public Sprite iceSpearSprite;
  public Sprite lightningSprite;
  public Sprite natureCallSprite;
  public Sprite stoneskinSprite;

  private Spell[] allSpells;
  private GameObject[] spellInfos;

  private int selectedSpell = -1;

  public void Setup(ProfileData profileData)
  {
    allSpells = (from s in Spellbook.Spells where Array.Exists(profileData.spells, x => x == s.Code) select s)
                .OrderBy(x => x.minLevel).ThenBy(x => x.Combination[0]).ThenBy(x => x.Combination[1]).ToArray();

    for (int i = content.transform.childCount - 1; i >= 0; --i)
      GameObject.Destroy(content.transform.GetChild(i).gameObject);
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
      spellIcon.sprite = GetSprite(allSpells[i]);

      Text manaCost = (from t in spellInfo.GetComponentsInChildren<Text>() where t.gameObject.name == "ManaCost" select t).Single();
      manaCost.text = "" + allSpells[i].manaCost;

      if (allSpells[i].Combination.Length == 2)
      {
        var obj = (from t in spellInfo.GetComponentsInChildren<Image>() where t.gameObject.name == "Icon1" select t).Single();
        obj.gameObject.SetActive(false);

        var sr1 = (from t in spellInfo.GetComponentsInChildren<Image>() where t.gameObject.name == "IconImage2" select t).Single();
        sr1.sprite = GetSprite(allSpells[i].Combination[0]);

        var sr2 = (from t in spellInfo.GetComponentsInChildren<Image>() where t.gameObject.name == "IconImage3" select t).Single();
        sr2.sprite = GetSprite(allSpells[i].Combination[1]);
      }
      else if (allSpells[i].Combination.Length == 3)
      {
        var sr1 = (from t in spellInfo.GetComponentsInChildren<Image>() where t.gameObject.name == "IconImage1" select t).Single();
        sr1.sprite = GetSprite(allSpells[i].Combination[0]);

        var sr2 = (from t in spellInfo.GetComponentsInChildren<Image>() where t.gameObject.name == "IconImage2" select t).Single();
        sr2.sprite = GetSprite(allSpells[i].Combination[1]);

        var sr3 = (from t in spellInfo.GetComponentsInChildren<Image>() where t.gameObject.name == "IconImage3" select t).Single();
        sr3.sprite = GetSprite(allSpells[i].Combination[2]);
      }
      else
      {
        throw new UnityException("Invalid game logic");
      }
    }
    height += kSpacing;
    content.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

    ScrollRect scroll = gameObject.GetComponentInChildren<ScrollRect>();
    scroll.verticalNormalizedPosition = 1.0f;

    spellDescPanel.SetActive(false);

    gameObject.SetActive(true);
  }

  public void Update()
  {
    int index = -1;
    for (int i = 0; i < spellInfos.Length; i++)
    {
      if (Input.GetMouseButton(0) && spellInfos[i].activeSelf &&
          RectTransformUtility.RectangleContainsScreenPoint(spellInfos[i].GetComponent<RectTransform>(), Input.mousePosition, Camera.main))
      {
        index = i;
        break;
      }
    }

    if (index >= 0)
    {
      if (index != selectedSpell)
      {
        selectedSpell = index;
        spellDescPanel.SetActive(true);

        Text title = (from t in spellDescPanel.GetComponentsInChildren<Text>() where t.gameObject.name == "SpellTitle" select t).Single();
        string spellName = LanguageManager.Instance.GetTextValue("Spell." + allSpells[selectedSpell].SpellType);
        if (spellName.Length == 0) spellName = allSpells[selectedSpell].SpellType.ToString();
        title.text = spellName;

        Text desc = (from t in spellDescPanel.GetComponentsInChildren<Text>() where t.gameObject.name == "SpellDesc" select t).Single();
        desc.text = UIUtils.GetSpellDescription(allSpells[selectedSpell]);
      }
    }
    else
    {
      selectedSpell = -1;
    }

    if (selectedSpell == -1 && spellDescPanel.activeSelf)
      spellDescPanel.SetActive(false);
  }

  public void Open()
  {
    gameObject.SetActive(true);
    Persistence.gameConfig.showSpellbookWidget = true;
    Persistence.Save();
  }

  public bool IsOpened()
  {
    return gameObject.activeSelf;
  }

  public void Close()
  {
    gameObject.SetActive(false);
    Persistence.gameConfig.showSpellbookWidget = false;
    Persistence.Save();
  }

  private Sprite GetSprite(Magic magic) 
  {
    switch (magic) 
    {
      case Magic.FIRE:
        return this.fireIconSprite;
      case Magic.WATER:
        return this.waterIconSprite;
      case Magic.AIR:
        return this.airIconSprite;
      case Magic.EARTH:
        return this.earthIconSprite;
      case Magic.NATURE:
        return this.natureIconSprite;
      case Magic.LIGHT:
        return this.lightIconSprite;
      case Magic.DARKNESS:
        return this.darknessIconSprite;
      case Magic.BLOOD:
        return this.bloodIconSprite;
      case Magic.ILLUSION:
        return this.illusionIconSprite;
    }
    return null;
  }

  private Sprite GetSprite(Spell spell) 
  {
    switch (spell.SpellType) 
    {
      case Spell.Type.BLEEDING:
        return this.bleedingSprite;
      case Spell.Type.BLESSING:
        return this.blessingSprite;
      case Spell.Type.DEATH_LOOK:
        return this.deathLookSprite;
      case Spell.Type.DOPPELGANGER:
        return this.doppelgangerSprite;
      case Spell.Type.FIREBALL:
        return this.fireballSprite;
      case Spell.Type.ICE_SPEAR:
        return this.iceSpearSprite;
      case Spell.Type.LIGHTNING:
        return this.lightningSprite;
      case Spell.Type.NATURE_CALL:
        return this.natureCallSprite;
      case Spell.Type.STONESKIN:
        return this.stoneskinSprite;
    }
    return null;
  }
}