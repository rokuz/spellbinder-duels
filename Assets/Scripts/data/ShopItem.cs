using UnityEngine;

public enum ShopItemType
{
  COINS_PACK1,
  COINS_PACK2,
  COINS_PACK3,
  REMOVE_ADS,

  LEVEL_UP,
  SPELL
}

class ShopItem
{
  public ShopItemType type;
  public string title;
  public string description;
  public string storePriceId;
  public int coinsCount;
  public int levelsUp;
  public Spell spell;

  public ShopItem(ShopItemType type, string title, string description, string storePriceId)
  {
    this.type = type;
    this.title = title;
    this.description = description;
    this.storePriceId = storePriceId;
    this.coinsCount = 0;
    this.levelsUp = 0;
    this.spell = null;
  }

  public ShopItem(ShopItemType type, string title, string description, string storePriceId, int coinsCount)
  {
    this.type = type;
    this.title = title;
    this.description = description;
    this.storePriceId = storePriceId;
    this.coinsCount = coinsCount;
    this.levelsUp = 0;
    this.spell = null;
  }

  public ShopItem(ShopItemType type, string title, string description, int levelsUp, int coinsCount)
  {
    this.type = type;
    this.title = title;
    this.description = description;
    this.storePriceId = "";
    this.coinsCount = coinsCount;
    this.levelsUp = levelsUp;
    this.spell = null;
  }
}