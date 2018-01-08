using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SmartLocalization;

public class ShopDialog : MonoBehaviour
{
  public GameObject shopItemPrefab;
  public Image splash;
  public GameObject content;
  public MessageDialog messageDialog;
  public MessageYesNoDialog messageYesNoDialog;

  public Text title;

  public Sprite spriteRemoveAds;
  public Sprite spriteCoinsSmall;
  public Sprite spriteCoinsMedium;
  public Sprite spriteCoinsBig;
  public Sprite spriteLevelUp;

  private ShopItem[] items;
  private GameObject[] itemInfos;

  private ProfileData profileData;

  public delegate void OnClose();
  private OnClose onCloseHandler;

	public void Setup()
  {
    var itemsList = new List<ShopItem>();
    itemsList.Add(new ShopItem(ShopItemType.REMOVE_ADS, "Remove Ads", "We will remove ads completely", "store-id"));
    itemsList.Add(new ShopItem(ShopItemType.COINS_PACK1, "Small bag of coins", "100 coins", "store-id", 100));
    itemsList.Add(new ShopItem(ShopItemType.COINS_PACK2, "Medium bag of coins", "500 coins", "store-id", 500));
    itemsList.Add(new ShopItem(ShopItemType.COINS_PACK3, "Big bag of coins", "1000 coins", "store-id", 1000));
    if (this.profileData.level < Constants.MAX_LEVEL)
      itemsList.Add(new ShopItem(ShopItemType.LEVEL_UP, "Level up", "Increase your level immediately", 1, 100));

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
      img.sprite = GetSprite(items[i].type);

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
        priceText.text = "$0.99"; //TODO
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
    this.title.text = LanguageManager.Instance.GetTextValue("MainMenu.Shop");
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
    this.profileData = profileData;
    this.onCloseHandler = onCloseHandler;
    Setup();

    /*ScrollRect scroll = gameObject.GetComponentInChildren<ScrollRect>();
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
      var rt = profileInfos[playerIndex].GetComponent<RectTransform>();
      var p = rt.position.y + rt.rect.height * 0.5f;
      scroll.verticalNormalizedPosition = 1.0f - pos;
      if (p >= ymax)
        break;
    }*/

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
    string s = LanguageManager.Instance.GetTextValue("Shop.BuyCheck") + " \"" + items[index].title + "\"?";
    messageYesNoDialog.Open(LanguageManager.Instance.GetTextValue("Shop.BuyMessage"), s, (bool yes) =>
    {
      if (yes)
        this.Buy(items[index]);
    });
  }

  private void Buy(ShopItem item)
  {
    if (item.storePriceId.Length != 0)
    {
      // TODO: Buy in store.
    }
    else
    {
      if (this.profileData.coins < item.coinsCount)
      {
        messageDialog.Open(LanguageManager.Instance.GetTextValue("Shop.BuyMessage"),
          LanguageManager.Instance.GetTextValue("Shop.NotEnoughCoins"), null);
      }
      else
      {
        if (item.type == ShopItemType.LEVEL_UP && this.profileData.level < Constants.MAX_LEVEL)
        {
          this.profileData.coins -= item.coinsCount;
          this.profileData.LevelUp();
          if (this.profileData.level == Constants.MAX_LEVEL)
            Setup();
        }
        Persistence.Save();
      }
    }
  }

  private void CloseIfClickedOutside(GameObject panel)
  {
    if (Input.GetMouseButtonDown(0) && panel.activeSelf &&
        !RectTransformUtility.RectangleContainsScreenPoint(panel.GetComponent<RectTransform>(), Input.mousePosition, null))
    {
      Close();
    }
  }

  private Sprite GetSprite(ShopItemType itemType)
  {
    switch (itemType)
    {
      case ShopItemType.REMOVE_ADS: return spriteRemoveAds;
      case ShopItemType.COINS_PACK1: return spriteCoinsSmall;
      case ShopItemType.COINS_PACK2: return spriteCoinsMedium;
      case ShopItemType.COINS_PACK3: return spriteCoinsBig;
      case ShopItemType.LEVEL_UP: return spriteLevelUp;
    }
    return null;
  }
}