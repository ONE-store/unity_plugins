using System;
using UnityEngine;

namespace OneStore.Core.Internal
{
    public class ResultListener : AndroidJavaProxy
    {
        public event Action<int, string> OnResponse = delegate { };

        public ResultListener() : base("com.gaa.sdk.base.ResultListener") { }

        void onResponse(int code, string message)
        {
            OnResponse.Invoke(code, message);
        }
    }
}
