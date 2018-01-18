using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Analytics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SmartLocalization;

public class ShopDialog : MonoBehaviour
{
  public Purchaser purchaser;
  public GameObject shopItemPrefab;
  public Image splash;
  public GameObject content;
  public MessageDialog messageDialog;
  public MessageYesNoDialog messageYesNoDialog;

  public Text title;

  public Text coinsText;

  public Sprite spriteRemoveAds;
  public Sprite spriteCoinsSmall;
  public Sprite spriteCoinsMedium;
  public Sprite spriteCoinsBig;
  public Sprite spriteLevelUp;

  public SpellSpritesHolder spellSpritesHolder;

  private ShopItem[] items;
  private GameObject[] itemInfos;

  private ProfileData profileData;

  public delegate void OnClose();
  private OnClose onCloseHandler;

  public delegate void OnRemovedAds();
  public OnRemovedAds onRemovedAds;

  private static string i18n(string s)
  {
    return LanguageManager.Instance.GetTextValue(s);
  }

	private void Setup()
  {
    purchaser.onBuy = OnBuyInStore;

    var itemsList = new List<ShopItem>();
    if (!Persistence.gameConfig.removedAds)
      itemsList.Add(new ShopItem(ShopItemType.REMOVE_ADS, i18n("Shop.RemoveAds"), i18n("Shop.RemoveAds.Desc"), Purchaser.kProductIDRemoveAds));
    itemsList.Add(new ShopItem(ShopItemType.COINS_PACK1, i18n("Shop.SmallBag"), i18n("Shop.SmallBag.Desc"), Purchaser.kProductIDCoinsPack1, 200));
    itemsList.Add(new ShopItem(ShopItemType.COINS_PACK2, i18n("Shop.MediumBag"), i18n("Shop.MediumBag.Desc"), Purchaser.kProductIDCoinsPack2, 500));
    itemsList.Add(new ShopItem(ShopItemType.COINS_PACK3, i18n("Shop.BigBag"), i18n("Shop.BigBag.Desc"), Purchaser.kProductIDCoinsPack3, 1000));
    if (this.profileData.level < Constants.MAX_LEVEL)
      itemsList.Add(new ShopItem(ShopItemType.LEVEL_UP, i18n("Shop.LevelUp"), i18n("Shop.LevelUp.Desc"), 1, 100));

    var allSpells = (from s in Spellbook.Spells where !Array.Exists(this.profileData.spells, x => x == s.Code) select s)
                     .OrderBy(x => x.minLevel).ThenBy(x => x.Combination[0]).ThenBy(x => x.Combination[1]).ToArray();
    foreach (Spell s in allSpells)
    {
      string spellName = i18n("Shop.Spell") + " \"" + i18n("Spell." + s.SpellType)+ "\"";
      string spellDesc = UIUtils.GetShopSpellDescription(this.profileData, s);
      itemsList.Add(new ShopItem(ShopItemType.SPELL, spellName, spellDesc, Constants.GetSpellPrice(s), s));
    }

    items = itemsList.ToArray();

    for (int i = content.transform.childCount - 1; i >= 0; --i)
      GameObject.Destroy(content.transform.GetChild(i).gameObject);
    content.transform.DetachChildren();

    const float kSpacing = 10.0f;
    float startOffsetY = -kSpacing;
    float height = 0;
    itemInfos = new GameObject[items.Length];
    for (int i = 0; i < items.Length; i++)
    {
      GameObject profileInfo = Instantiate(shopItemPrefab);
      profileInfo.transform.SetParent(content.transform, false);
      Rect r = profileInfo.GetComponent<RectTransform>().rect;
      float h = r.height + kSpacing;
      profileInfo.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(kSpacing, -i * h + startOffsetY, 0.0f);
      height += h;

      itemInfos[i] = profileInfo;

      var img = (from t in profileInfo.GetComponentsInChildren<Image>() where t.gameObject.name == "ItemImage" select t).Single();
      img.sprite = GetSprite(items[i]);

      Text title = (from t in profileInfo.GetComponentsInChildren<Text>() where t.gameObject.name == "Name" select t).Single();
      title.text = items[i].title;

      Text desc = (from t in profileInfo.GetComponentsInChildren<Text>() where t.gameObject.name == "ItemDesc" select t).Single();
      desc.text = items[i].description;

      Text priceText = (from t in profileInfo.GetComponentsInChildren<Text>() where t.gameObject.name == "Price" select t).Single();
      if (items[i].storePriceId.Length != 0)
      {
        var coinImg = (from t in profileInfo.GetComponentsInChildren<Image>() where t.gameObject.name == "CoinImage" select t).Single();
        coinImg.gameObject.SetActive(false);
        var tr = priceText.GetComponent<RectTransform>();
        tr.anchoredPosition = new Vector2(tr.anchoredPosition.x - 15.0f, tr.anchoredPosition.y);
        priceText.text = purchaser.GetPrice(items[i].storePriceId);
      }
      else
      {
        priceText.text = "" + items[i].coinsCount;
      }

      Button btn = (from t in profileInfo.GetComponentsInChildren<Button>() where t.gameObject.name == "Button" select t).Single();
      int buttonIndex = i;
      btn.onClick.AddListener(() => { this.OnBuy(buttonIndex); });
		}
		height += kSpacing;
    content.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
    this.title.text = i18n("MainMenu.Shop");

    UpdateCoins();
	}

  public void Update()
  {
    CloseIfClickedOutside(this.gameObject);
  }

  public bool IsOpened()
  {
    return gameObject.activeSelf;
  }

  public void Open(ProfileData profileData, Spell selectedSpell, OnClose onCloseHandler)
  {
    Analytics.CustomEvent("Shop_Open");

    this.profileData = profileData;
    this.onCloseHandler = onCloseHandler;
    Setup();

    if (selectedSpell != null)
    {
      int selectedIndex = -1;
      for (int i = 0; i < items.Length; i++)
      {
        if (items[i].spell != null && items[i].spell.Code == selectedSpell.Code)
        {
          selectedIndex = i;
          break;
        }
      }

      if (selectedIndex >= 0)
      {
        ScrollRect scroll = gameObject.GetComponentInChildren<ScrollRect>();
        Vector3[] corners = new Vector3[4];
        content.GetComponent<RectTransform>().GetWorldCorners(corners);
        float ymax = 0.0f;
        for (int i = 0; i < corners.Length; i++)
        {
          if (corners[i].y > ymax)
            ymax = corners[i].y;
        }

        for (float pos = 0.0f; pos < 1.0f; pos += 0.01f)
        {
          var rt = itemInfos[selectedIndex].GetComponent<RectTransform>();
          var p = rt.position.y + rt.rect.height * 0.5f;
          scroll.verticalNormalizedPosition = 1.0f - pos;
          if (p >= ymax)
            break;
        }
      }
    }

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

  public void OnBuy(int index)
  {
    string s = i18n("Shop.BuyCheck") + " \"" + items[index].title + "\"?";
    messageYesNoDialog.Open(i18n("Shop.BuyMessage"), s, (bool yes) =>
    {
      if (yes)
        this.Buy(items[index]);
    });
  }

  private void Buy(ShopItem item)
  {
    if (item.storePriceId.Length != 0)
    {
      var p = new Dictionary<string, object>();
      p.Add("product", item.type);
      Analytics.CustomEvent("Shop_Buy_For_Cash", p);

      purchaser.Buy(item.storePriceId);
    }
    else
    {
      if (this.profileData.coins < item.coinsCount)
      {
        messageDialog.Open(i18n("Shop.BuyMessage"), i18n("Shop.NotEnoughCoins"), null);
      }
      else
      {
        var p = new Dictionary<string, object>();
        p.Add("product", item.type);
        Analytics.CustomEvent("Shop_Buy_For_Coins", p);

        if (item.type == ShopItemType.LEVEL_UP && this.profileData.level < Constants.MAX_LEVEL)
        {
          this.profileData.coins -= item.coinsCount;
          this.profileData.LevelUp();
          if (this.profileData.level == Constants.MAX_LEVEL)
            Setup();
        }
        else if (item.type == ShopItemType.SPELL)
        {
          this.profileData.coins -= item.coinsCount;
          var lst = this.profileData.spells.ToList();
          lst.Add(item.spell.Code);
          this.profileData.spells = lst.ToArray();
          Setup();
        }
        Persistence.Save();
      }
    }
    UpdateCoins();
  }

  private void OnBuyInStore(string productId, bool success)
  {
    var item = FindItemByProductId(productId);
    if (!success || item == null)
    {
      messageDialog.Open(i18n("Shop.Error"), i18n("Shop.StoreFailure"), null);
      return;
    }

    if (item.type == ShopItemType.REMOVE_ADS)
    {
      Persistence.gameConfig.removedAds = true;
      Setup();
      Persistence.Save();
      if (onRemovedAds != null)
        onRemovedAds();
    }
    else if (item.type == ShopItemType.COINS_PACK1 || item.type == ShopItemType.COINS_PACK2 ||
             item.type == ShopItemType.COINS_PACK3)
    {
      this.profileData.coins += item.coinsCount;
      Persistence.Save();
    }
  }

  private ShopItem FindItemByProductId(string productId)
  {
    foreach (ShopItem i in items)
    {
      if (i.storePriceId == productId)
        return i;
    }
    return null;
  }

  private void CloseIfClickedOutside(GameObject panel)
  {
    if (Input.GetMouseButtonDown(0) && panel.activeSelf &&
        !RectTransformUtility.RectangleContainsScreenPoint(panel.GetComponent<RectTransform>(), Input.mousePosition, null))
    {
      Close();
    }
  }

  private Sprite GetSprite(ShopItem item)
  {
    switch (item.type)
    {
      case ShopItemType.REMOVE_ADS: return spriteRemoveAds;
      case ShopItemType.COINS_PACK1: return spriteCoinsSmall;
      case ShopItemType.COINS_PACK2: return spriteCoinsMedium;
      case ShopItemType.COINS_PACK3: return spriteCoinsBig;
      case ShopItemType.LEVEL_UP: return spriteLevelUp;
      case ShopItemType.SPELL: return spellSpritesHolder.GetSprite(item.spell);
    }
    return null;
  }

  private void UpdateCoins()
  {
    coinsText.text = "" + this.profileData.coins;
  }
}