using System;

namespace OneStore.Purchasing
{
    /// <summary>
    /// It is an object with the code and message of in-app purchase API responses included.
    /// Public documentation can be found at
    /// https://dev.onestore.co.kr/wiki/en/doc/tools/one-store-iap-api-v6-sdk-v19-guide-download/one-store-iap-reference/classes/iapresult
    /// </summary>
    [Serializable]
    public class IapResult
    {
        private int _code;
        private string _message;

        public int Code { get { return _code; } }
        public string Message { get { return _message; } }

        public IapResult(int code, string message)
        {
            _code = code;
            _message = message;
        }

        public bool IsSuccessful()
        {
            return _code == (int) ResponseCode.RESULT_OK;
        }

        public override string ToString()
        {
            return "IapResult(code=" + _code +  ", message: " + _message + ")";
        }
    }
}
