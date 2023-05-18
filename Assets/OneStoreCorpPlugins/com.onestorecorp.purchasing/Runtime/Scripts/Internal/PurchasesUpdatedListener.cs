using System;
using UnityEngine;

namespace OneStore.Purchasing.Internal
{
    /// <summary>
    /// Callback for purchase updates which happen when, for example, the user buys something within
    /// the app or initiates a purchase from ONE Store.
    /// Public documentation can be found at
    /// https://dev.onestore.co.kr/wiki/ko/doc/i-purchasesupdatedlistener-37552630.html
    /// </summary>
    public class PurchasesUpdatedListener : AndroidJavaProxy
    {
        public event Action<AndroidJavaObject, AndroidJavaObject> OnPurchasesUpdated = delegate { };
        public PurchasesUpdatedListener() : base(Constants.PurchasesUpdatedListener)
        {
        }

        void onPurchasesUpdated(AndroidJavaObject iapResult, AndroidJavaObject purchasesList)
        {
            OnPurchasesUpdated.Invoke(iapResult, purchasesList);
        }
    }
}
