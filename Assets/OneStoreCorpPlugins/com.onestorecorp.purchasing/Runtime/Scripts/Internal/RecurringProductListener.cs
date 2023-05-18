using System;
using UnityEngine;

namespace OneStore.Purchasing.Internal
{
    /// <summary>
    /// Callback when a fetch Recurring product operation has finished
    /// Public documentation can be found at
    /// https://dev.onestore.co.kr/wiki/ko/doc/i-recurringproductlistener-37552632.html
    /// </summary>
    public class RecurringProductListener : AndroidJavaProxy
    {
        private readonly string _productId;

        public event Action<AndroidJavaObject, string, string> OnRecurringResponse = delegate { };
        public RecurringProductListener(string productId) : base(Constants.RecurringProductListener)
        {
            _productId = productId;
        }

        void onRecurringResponse(AndroidJavaObject iapResult, AndroidJavaObject purchaseData, string recurringAction)
        {
            OnRecurringResponse.Invoke(iapResult, _productId, recurringAction);
        }
    }
}
