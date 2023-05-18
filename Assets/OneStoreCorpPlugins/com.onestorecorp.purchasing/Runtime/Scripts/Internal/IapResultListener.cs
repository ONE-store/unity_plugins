using System;
using UnityEngine;

namespace OneStore.Purchasing.Internal
{
    /// <summary>
    /// It is a default listener for in-app purchase API responses.
    /// Public documentation can be found at
    /// https://dev.onestore.co.kr/wiki/ko/doc/i-iapresultlistener-37552626.html
    /// </summary>
    public class IapResultListener : AndroidJavaProxy
    {
        public event Action<AndroidJavaObject> OnResponse = delegate { };

        public IapResultListener() : base(Constants.IapResultListener) { }

        void onResponse(AndroidJavaObject iapResult)
        {
            OnResponse.Invoke(iapResult);
        }
    }
}
