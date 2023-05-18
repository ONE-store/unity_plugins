using System;
using UnityEngine;

namespace OneStore.Purchasing.Internal
{
    /// <summary>
    /// Callback that notifies when a consumption operation finishes.
    /// Public documentation can be found at
    /// https://dev.onestore.co.kr/wiki/ko/doc/i-consumelistener-37552625.html
    /// </summary>
    public class ConsumeListener : AndroidJavaProxy
    {
        public event Action<AndroidJavaObject, AndroidJavaObject> OnConsumeResponse = delegate { };

        public ConsumeListener() : base(Constants.ConsumeListener){}

        void onConsumeResponse(AndroidJavaObject iapResult, AndroidJavaObject purchaseData)
        {
            OnConsumeResponse.Invoke(iapResult, purchaseData);
        }
    }
}
