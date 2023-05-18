using System;
using UnityEngine;

namespace OneStore.Purchasing.Internal
{
    /// <summary>
    /// Callback for purchase updates which happen when, for example, the user buys something within
    /// the app or initiates a purchase from ONE Store.
    /// Public documentation can be found at
    /// https://dev.onestore.co.kr/wiki/ko/doc/i-querypurchaseslistener-37552631.html
    /// </summary>
    public class QueryPurchasesListener : AndroidJavaProxy
    {
        private ProductType _type;
        public event Action<ProductType, AndroidJavaObject, AndroidJavaObject> OnPurchasesResponse = delegate { };

        public QueryPurchasesListener(ProductType type) : base(Constants.QueryPurchasesListener)
        {
            _type = type;
        }

        void onPurchasesResponse(AndroidJavaObject iapResult, AndroidJavaObject purchasesList)
        {
            OnPurchasesResponse.Invoke(_type, iapResult, purchasesList);
        }
    }
}
