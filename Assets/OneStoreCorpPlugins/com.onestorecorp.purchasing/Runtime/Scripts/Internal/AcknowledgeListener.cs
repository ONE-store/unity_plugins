using System;
using UnityEngine;

namespace OneStore.Purchasing.Internal
{
    /// <summary>
    /// Callback that notifies when an acknowledgement purchase operation finishes.
    /// Public documentation can be found at
    /// https://dev.onestore.co.kr/wiki/ko/doc/i-acknowledgelistener-37552624.html
    /// </summary>
    public class AcknowledgeListener : AndroidJavaProxy
    {
        private readonly string _productId;
        private readonly ProductType _type;
        public event Action<AndroidJavaObject, string, ProductType> OnAcknowledgeResponse = delegate { };

        public AcknowledgeListener(string productId, ProductType type) : base(Constants.AcknowledgeListener)
        {
            _productId = productId;
            _type = type;
        }

        void onAcknowledgeResponse(AndroidJavaObject iapResult, AndroidJavaObject purchaseData)
        {
            OnAcknowledgeResponse.Invoke(iapResult, _productId, _type);
        }
    }
}
