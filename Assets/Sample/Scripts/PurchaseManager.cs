using System.Collections.Generic;
using System.Collections.ObjectModel;
using OneStore.Purchasing;
using System;
using UnityEngine;
using OneStore.Sample.Common;

public class PurchaseManager
{
    /// <summary>
    /// Warning! This key is very important. You should take extra care to ensure that this key is never exposed.
    /// </summary>
    private string _licenseKey = Constants.PublicKey;   // input your license key
    
    private readonly object _inventoryLock = new object();
    private Dictionary<string, ProductDetail> _productDetailInventory = new Dictionary<string, ProductDetail>();
    private Dictionary<string, PurchaseData> _purchaseDataInventory = new Dictionary<string, PurchaseData>();

    private PurchaseClientImpl _purchaseClient;

    // Product ID registered in the Developer Center
    public static string[] _consumableItems = {"p500", "p510"};
    public static string[] _subscriptionItems = {"s300", "s400", "s500", "s600", "s700", "s800", "s900"};

    public Action<LogType, string, object[]> logger;

    public PurchaseManager(IPurchaseCallback callback)
    {
        _purchaseClient = new PurchaseClientImpl(_licenseKey);
        _purchaseClient?.Initialize(callback);
    }

    #region PurchaseClientImpl Request Method

    public void RetrieveProducts()
    {
        var list = new List<string>();
        list.AddRange(_consumableItems);
        list.AddRange(_subscriptionItems);
        QueryProductDetails(list.AsReadOnly(), ProductType.ALL);
    }

    public void AcknowledgePurchase(PurchaseData purchase, ProductType type)
    {
        _purchaseClient?.AcknowledgePurchase(purchase, type);
    }

    public void ConsumePurchase(PurchaseData purchase)
    {
        _purchaseClient?.ConsumePurchase(purchase);
    }

    public void EndConnection()
    {
        _purchaseClient?.EndConnection();
    }

    public void LaunchManageSubscription(string productId)
    {
        PurchaseData purchase;
        GetPurchase(productId, out purchase);
        _purchaseClient?.LaunchManageSubscription(purchase);
    }

    public void ManageRecurringProduct(string productId)
    {
        PurchaseData purchase;
        if (GetPurchase(productId, out purchase))
        {      
            RecurringAction recurringAction = (purchase.RecurringState == (int) RecurringState.RECURRING)
                            ? RecurringAction.CANCEL : RecurringAction.REACTIVATE;    
            _purchaseClient?.ManageRecurringProduct(purchase, recurringAction);
        }
        else
        {
            logger?.Invoke(LogType.Warning, "ManageRecurringProduct(): PurchaseData is null", null);
        }
    }

    public void Purchase(string productId, string developerPayload, int quantity = 1)
    {
        ProductDetail productDetail;
        if (GetProductDetail(productId, out productDetail))
        {
            var productType = ProductType.Get(productDetail.type);
            var purchaseFlowParams = new PurchaseFlowParams.Builder()
                        .SetProductId(productId)                // mandatory
                        .SetProductType(productType)            // mandatory

                        .SetDeveloperPayload(developerPayload)  // optional
                        .SetQuantity(quantity)                  // optional
                        // .SetProductName(null)                // optional: Change the name of the product to appear on the purchase screen.
                        
                        // It should be used only in advance consultation with the person in charge of the One Store business, and is not normally used.
                        // .SetGameUserId(null)                 // optional: User ID to use for promotion.
                        // .SetPromotionApplicable(false)       // optional: Whether to participate in the promotion.
                        .Build();

            _purchaseClient?.Purchase(purchaseFlowParams);
        }
        else
        {
            logger?.Invoke(LogType.Warning, "Purchase(): ProductDetail is null", null);
        }
    }

    public void UpdateSubscription(string productId, PurchaseData oldPurchase, OneStoreProrationMode prorationMode, string developerPayload)
    {
        ProductDetail productDetail;
        if (GetProductDetail(productId, out productDetail))
        {
            var purchaseFlowParams = new PurchaseFlowParams.Builder()
                        .SetProductId(productId)                        // mandatory
                        .SetProductType(ProductType.SUBS)               // mandatory
                        .SetOldPurchaseToken(oldPurchase.PurchaseToken) // mandatory
                        .SetProrationMode(prorationMode)                // mandatory

                        .SetDeveloperPayload(developerPayload)          // optional
                        // .SetProductName(null)                        // optional: Change the name of the product to appear on the purchase screen.
                        .Build();

            _purchaseClient?.UpdateSubscription(purchaseFlowParams);
        }
        else
        {
            logger?.Invoke(LogType.Warning, "UpdateSubscription(): ProductDetail is null", null);
        }
    }

    public void QueryProductDetails(ReadOnlyCollection<string> productIds, ProductType type)
    {
        _purchaseClient?.QueryProductDetails(productIds, type);
    }

    public void QueryPurchases(ProductType type)
    {
        _purchaseClient?.QueryPurchases(type);
    }

    public void LaunchUpdateOrInstallFlow(Action<IapResult> callback)
    {
        _purchaseClient?.LaunchUpdateOrInstallFlow(callback);
    }

    #endregion

    #region Manage Inventory

    public void UpdateProductDetailInventory(IEnumerable<ProductDetail> productDetailsList)
    {
        lock (_inventoryLock)
        {
            foreach (var productDetail in productDetailsList)
            {
                _productDetailInventory[productDetail.productId] = productDetail;
            }
        }
    }
    
    public void UpdatePurchaseInventory(IEnumerable<PurchaseData> purchasesList)
    {
        lock (_inventoryLock)
        {
            foreach (var purchase in purchasesList)
            {
                _purchaseDataInventory[purchase.ProductId] = purchase;
            }
        }
    }

    public bool ContainsKeyByProductDetail(string productId)
    {
        lock (_inventoryLock)
        {
            return _productDetailInventory.ContainsKey(productId);
        }
    }

    public bool ContainsKeyByPurchase(string productId)
    {
        lock (_inventoryLock)
        {
            return _purchaseDataInventory.ContainsKey(productId);
        }
    }

    public bool GetProductDetail(string productId, out ProductDetail productDetail)
    {
        if (String.IsNullOrEmpty(productId))
        {
            productDetail = null;
            return false;
        }

        lock (_inventoryLock)
        {
            return _productDetailInventory.TryGetValue(productId, out productDetail);
        }
    }

    public ProductType GetProductType(string productId)
    {
        lock (_inventoryLock)
        {
            ProductDetail productDetail;
            if (GetProductDetail(productId, out productDetail))
            {
                return ProductType.Get(productDetail.type);
            }

            return null;
        }
    }

    public bool GetPurchase(string productId, out PurchaseData purchase)
    {
        if (String.IsNullOrEmpty(productId))
        {
            purchase = null;
            return false;
        }

        lock (_inventoryLock)
        {
            return _purchaseDataInventory.TryGetValue(productId, out purchase);
        }
    }

    public bool RemovePurchase(string productId)
    {
        if (String.IsNullOrEmpty(productId))
        {
            return false;
        }
        
        lock (_inventoryLock)
        {
            var purchaseValue = _purchaseDataInventory[productId];
            _purchaseDataInventory.Remove(productId);
            return purchaseValue != null;
        }
    }

    #endregion
}

