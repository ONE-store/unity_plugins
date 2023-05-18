using OneStore.Purchasing;
using OneStore.Sample;
using UnityEngine;
using System;

[System.Serializable]
public class ProductItem
{
    public Sprite icon;
    public string productId;
    public string productType;
    public string title;
    public string price;
    public SubscriptionState state = SubscriptionState.PURCHASE;
    public enum SubscriptionState
    {
        PURCHASE,
        OWNED,
        UPDATE,
    }
    public ProductItem(ProductDetail productDetail, Sprite _icon)
    {
        productId = productDetail.productId;
        productType = productDetail.type;
        title = productDetail.title;
        icon = _icon;
        price = GetPrice(productDetail);
    }

    private string GetPrice(ProductDetail productDetail)
    {
        var price = Utils.GetThousandFormat(productDetail.price);
        ProductType type = ProductType.Get(productDetail.type);
        if (ProductType.INAPP == type)
        {
            return price;
        }
    
        // else if (ProductType.SUBS == type)
        if (productDetail.subscriptionPeriodUnitCode.Equals("DAY") && productDetail.subscriptionPeriod == 7)
        {
            return String.Format("{0}/WEEK", price);
        }

        return String.Format("{0}/{1}", price, productDetail.subscriptionPeriodUnitCode);
    }
}


