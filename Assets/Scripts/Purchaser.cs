using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Analytics;

public class Purchaser : MonoBehaviour, IStoreListener
{
  private static IStoreController storeController;
  private static IExtensionProvider storeExtensionProvider;

  public static string kProductIDCoinsPack1 = "com.rokuz.spellbinder.coinspack1";
  public static string kProductIDCoinsPack2 = "com.rokuz.spellbinder.coinspack2";
  public static string kProductIDCoinsPack3 = "com.rokuz.spellbinder.coinspack3";

  public delegate void OnButEvent(string productId, bool success);
  public delegate void OnRestorePurchases();

  private static Dictionary<string, string> defaultPrices = new Dictionary<string, string>();

  public OnButEvent onBuy;

  void Start()
  {
    #if !UNITY_STANDALONE
    if (storeController == null)
      InitializePurchasing();
    #endif
  }

  public void InitializePurchasing() 
  {
    if (IsInitialized())
      return;

    if (defaultPrices.Count == 0)
    {
      defaultPrices.Add(kProductIDCoinsPack1, "$0.99");
      defaultPrices.Add(kProductIDCoinsPack2, "$1.99");
      defaultPrices.Add(kProductIDCoinsPack3, "$2.99");
    }

    print("InitializePurchasing()");
    var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
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
    var p = product.metadata.localizedPriceString;
    if (p == null || p.Length == 0)
      return defaultPrices[productId];
    return p;
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
    if (String.Equals(args.purchasedProduct.definition.id, kProductIDCoinsPack1, StringComparison.Ordinal))
      id = kProductIDCoinsPack1;
    else if (String.Equals(args.purchasedProduct.definition.id, kProductIDCoinsPack2, StringComparison.Ordinal))
      id = kProductIDCoinsPack2;
    else if (String.Equals(args.purchasedProduct.definition.id, kProductIDCoinsPack3, StringComparison.Ordinal))
      id = kProductIDCoinsPack3;

    if (id.Length > 0)
    {
      MyAnalytics.PaymentEvent(id, args.purchasedProduct);
      Debug.Log("Successful Purchase: " + id);

      if (this.onBuy != null)
        this.onBuy(id, true);
    }
    return PurchaseProcessingResult.Complete;
  }
    
  public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
  {
    string id = "";
    if (String.Equals(product.definition.id, kProductIDCoinsPack1, StringComparison.Ordinal))
      id = kProductIDCoinsPack1;
    else if (String.Equals(product.definition.id, kProductIDCoinsPack2, StringComparison.Ordinal))
      id = kProductIDCoinsPack2;
    else if (String.Equals(product.definition.id, kProductIDCoinsPack3, StringComparison.Ordinal))
      id = kProductIDCoinsPack3;

    Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", id, failureReason));
    if (this.onBuy != null)
      this.onBuy(id, false);

    var p = new Dictionary<string, object>();
    p.Add("product", id);
    MyAnalytics.CustomEvent("Purchase_Failed", p);
  }
}
