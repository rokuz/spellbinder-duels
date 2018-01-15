﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Analytics;

public class Purchaser : MonoBehaviour, IStoreListener
{
  private static IStoreController storeController;
  private static IExtensionProvider storeExtensionProvider;

  public static string kProductIDRemoveAds = "com.rokuz.spellbinder.removeads";
  public static string kProductIDCoinsPack1 = "com.rokuz.spellbinder.coinspack1";
  public static string kProductIDCoinsPack2 = "com.rokuz.spellbinder.coinspack2";
  public static string kProductIDCoinsPack3 = "com.rokuz.spellbinder.coinspack3";

  public delegate void OnButEvent(string productId, bool success);
  public delegate void OnRestorePurchases();

  public OnButEvent onBuy;

  void Start()
  {
    if (storeController == null)
      InitializePurchasing();
  }

  public void InitializePurchasing() 
  {
    if (IsInitialized())
      return;

    print("InitializePurchasing()");
    var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
    builder.AddProduct(kProductIDRemoveAds, ProductType.NonConsumable);
    builder.AddProduct(kProductIDCoinsPack1, ProductType.Consumable);
    builder.AddProduct(kProductIDCoinsPack2, ProductType.Consumable);
    builder.AddProduct(kProductIDCoinsPack3, ProductType.Consumable);
    UnityPurchasing.Initialize(this, builder);
  }
    
  private bool IsInitialized()
  {
    return storeController != null && storeExtensionProvider != null;
  }

  public string GetPrice(string productId)
  {
    if (!IsInitialized())
      return "";

    Product product = storeController.products.WithID(productId);
    return product.metadata.localizedPriceString;
  }

  public void Buy(string productId)
  {
    if (!IsInitialized())
      return;

    Product product = storeController.products.WithID(productId);
    if (product != null && product.availableToPurchase)
      storeController.InitiatePurchase(product);
  }

  public void RestorePurchases(OnRestorePurchases onRestorePurchases)
  {
    if (!IsInitialized())
      return;

    if (Application.platform == RuntimePlatform.IPhonePlayer || 
        Application.platform == RuntimePlatform.OSXPlayer)
    {
      var apple = storeExtensionProvider.GetExtension<IAppleExtensions>();
      apple.RestoreTransactions((result) => {
        print(result ? "Transactions restored" : "Transactions are not restored");
        if (result && onRestorePurchases != null)
          onRestorePurchases();
      });
    }
  }

  public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
  {
    storeController = controller;
    storeExtensionProvider = extensions;

    print("Purchaser initialized");
  }
    
  public void OnInitializeFailed(InitializationFailureReason error)
  {
    print("Purchaser not initialized");
  }

  public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
  {
    string id = "";
    if (String.Equals(args.purchasedProduct.definition.id, kProductIDRemoveAds, StringComparison.Ordinal))
      id = kProductIDRemoveAds;
    else if (String.Equals(args.purchasedProduct.definition.id, kProductIDCoinsPack1, StringComparison.Ordinal))
      id = kProductIDCoinsPack1;
    else if (String.Equals(args.purchasedProduct.definition.id, kProductIDCoinsPack2, StringComparison.Ordinal))
      id = kProductIDCoinsPack2;
    else if (String.Equals(args.purchasedProduct.definition.id, kProductIDCoinsPack3, StringComparison.Ordinal))
      id = kProductIDCoinsPack3;

    if (id.Length > 0)
    {
      var p = new Dictionary<string, object>();
      p.Add("product", id);
      Analytics.CustomEvent("SuccessfulPurchase", p);
      Debug.Log("Successful Purchase: " + id);

      if (id == kProductIDRemoveAds)
      {
        Persistence.gameConfig.removedAds = true;
        Persistence.Save();
      }

      if (this.onBuy != null)
        this.onBuy(id, true);
    }
    return PurchaseProcessingResult.Complete;
  }
    
  public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
  {
    string id = "";
    if (String.Equals(product.definition.id, kProductIDRemoveAds, StringComparison.Ordinal))
      id = kProductIDRemoveAds;
    else if (String.Equals(product.definition.id, kProductIDCoinsPack1, StringComparison.Ordinal))
      id = kProductIDCoinsPack1;
    else if (String.Equals(product.definition.id, kProductIDCoinsPack2, StringComparison.Ordinal))
      id = kProductIDCoinsPack2;
    else if (String.Equals(product.definition.id, kProductIDCoinsPack3, StringComparison.Ordinal))
      id = kProductIDCoinsPack3;

    if (failureReason == PurchaseFailureReason.DuplicateTransaction && id == kProductIDRemoveAds)
    {
      Debug.Log("Duplicate Purchase");
      if (this.onBuy != null)
        this.onBuy(kProductIDRemoveAds, true);
      return;
    }

    Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", id, failureReason));
    if (this.onBuy != null)
      this.onBuy(id, false);
  }
}
