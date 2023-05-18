using System;
using UnityEngine;
using OneStore.Purchasing.Internal;

namespace OneStore.Purchasing
{
    /// <summary>
    /// Represents a ONE Store in-app product details.
    /// Public documentation can be found at
    /// https://dev.onestore.co.kr/wiki/ko/doc/c-productdetail-37552609.html
    /// </summary>
    [Serializable]
    public class ProductDetail
    {
#pragma warning disable 649
        public string productId;
        public string type;
        public string title;
        public string price;
        public string priceCurrencyCode;
        public long priceAmountMicros;
        public string subscriptionPeriodUnitCode;
        public int subscriptionPeriod;
        public int freeTrialPeriod;
        public string promotionPrice;
        public long promotionPriceMicros;
        public int promotionUsePeriod;
        public int paymentGracePeriod;
#pragma warning restore 649

        public string JsonProductDetail { get; private set; }

        /// <summary>
        /// Creates a ProductDetail object from its JSON representation.
        /// </summary>
        public static bool FromJson(string jsonProductDetail, out ProductDetail productDetail)
        {
            try
            {
                productDetail = JsonUtility.FromJson<ProductDetail>(jsonProductDetail);
                productDetail.JsonProductDetail = jsonProductDetail;
                return true;
            }
            catch (Exception)
            {
                productDetail = null;
                return false;
            }
        }

        /// <summary>
        /// Creates a Java ProductDetail object.
        /// </summary>
        public AndroidJavaObject ToJava()
        {
            return new AndroidJavaObject(Constants.ProductDetailClass, JsonProductDetail);
        }
    }
}
