using System;
using UnityEngine;

namespace OneStore.Purchasing.Internal
{
    /// <summary>
    /// Callback for setup process of Gaa Purchasing Library.
    /// Public documentation can be found at
    /// https://dev.onestore.co.kr/wiki/ko/doc/i-purchaseclientstatelistener-37552628.html
    /// </summary>
    public class PurchaseClientStateListener : AndroidJavaProxy
    {
        public event Action OnServiceDisconnected = delegate { };
        public event Action<AndroidJavaObject> OnSetupFinished = delegate { };

        public PurchaseClientStateListener() : base(Constants.PurchaseClientStateListener) { }

        void onServiceDisconnected()
        {
            OnServiceDisconnected.Invoke();
        }

        void onSetupFinished(AndroidJavaObject iapResult)
        {
            OnSetupFinished.Invoke(iapResult);
        }
    }
}
