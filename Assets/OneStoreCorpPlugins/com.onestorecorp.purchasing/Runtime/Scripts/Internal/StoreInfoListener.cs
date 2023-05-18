using System;
using UnityEngine;

namespace OneStore.Purchasing.Internal
{
    /// <summary>
    /// Callback when a fetch the store code.
    /// Public documentation can be found at
    /// https://dev.onestore.co.kr/wiki/ko/doc/i-storeinfolistener-37552633.html
    /// </summary>
    public class StoreInfoListener : AndroidJavaProxy
    {
        public event Action<string> OnStoreInfoResponse = delegate { };

        public StoreInfoListener() : base(Constants.StoreInfoListener) { }

        void onStoreInfoResponse(AndroidJavaObject iapResult, string storeCode)
        {
            OnStoreInfoResponse.Invoke(storeCode);
        }
    }
}
