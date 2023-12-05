using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using OneStore.Common;

namespace OneStore.Purchasing.Internal
{
    /// <summary>
    /// A collection of utils methods to process AndroidJavaObject returned by Gaa Purchasing Library.
    /// </summary>
    public class IapHelper
    {
        private readonly OneStoreLogger _logger;
        public IapHelper(OneStoreLogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Parses the Iap results returned by Gaa Purchasing Client.
        /// </summary>
        /// <returns> The IapResult that indicates the outcome of the Java IapResult. </returns>
        public IapResult ParseJavaIapResult(AndroidJavaObject javaIapResult)
        {
            var code = javaIapResult.Call<int>("getResponseCode");
            var message = javaIapResult.Call<string>("getMessage");

            return new IapResult(code, message);
        }

        /// <summary>
        /// Parses the IapResults returned by Gaa Purchasing Client.
        /// </summary>
        /// <returns>Returns the code value of the IapResult in ResponseCode.</returns>
        public ResponseCode GetResponseCodeFromIapResult(IapResult iapResult)
        {
            var resultResponseCode = ResponseCode.RESULT_ERROR;
            try
            {
                resultResponseCode = (ResponseCode) Enum.Parse(typeof(ResponseCode), iapResult.Code.ToString());
            }
            catch (ArgumentNullException)
            {
                _logger.Error("Missing response code, return ResponseCode.RESULT_ERROR.");
            }
            catch (ArgumentException)
            {
                _logger.Error("Unknown response code {0}, return ResponseCode.RESULT_ERROR.", iapResult.Code);
            }
            return resultResponseCode;
        }

        /// <summary>
        /// Parses the ProductDetail list results returned by the Gaa Purchasing Library.
        /// </summary>
        /// <returns>An IEnumerable of <cref="SkuDetails"/>. The IEnumerable could be empty.</returns>
        public IEnumerable<ProductDetail> ParseProductDetailsResult(AndroidJavaObject javaIapResult, AndroidJavaObject productDetailsList)
        {
            var iapResult = ParseJavaIapResult(javaIapResult);
            if (!iapResult.IsSuccessful())
            {
                _logger.Warning("Failed to retrieve products information! Error code {0}, message: {1}.",
                    iapResult.Code, iapResult.Message);
                return Enumerable.Empty<ProductDetail>();
            }

            var parsedProductDetailsList = new List<ProductDetail>();
            var size = productDetailsList.Call<int>("size");
            for (var i = 0; i < size; i++)
            {
                var javaProductDetail = productDetailsList.Call<AndroidJavaObject>("get", i);
                var originalJson = javaProductDetail.Call<string>(Constants.ProductDetailGetOriginalJson);
                ProductDetail productDetail;
                if (ProductDetail.FromJson(originalJson, out productDetail))
                {
                    parsedProductDetailsList.Add(productDetail);
                }
                else
                {
                    _logger.Warning("Failed to parse productDetails {0} ", originalJson);
                }
            }

            return parsedProductDetailsList;
        }

        /// <summary>
        /// Parses the Java list of purchaseData list returned by the Gaa Purchasing Library.
        /// </summary>
        /// <returns>An IEnumerable of <cref="PurchaseData"/>. The IEnumerable could be empty.</returns>
        public IEnumerable<PurchaseData> ParseJavaPurchasesList(AndroidJavaObject javaPurchasesList)
        {
            var parsedPurchasesList = new List<PurchaseData>();
            var size = javaPurchasesList.Call<int>("size");
            for (var i = 0; i < size; i++)
            {
                var javaPurchase = javaPurchasesList.Call<AndroidJavaObject>("get", i);
                var originalJson = javaPurchase.Call<string>(Constants.PurchaseDataGetOriginalJsonMethod);
                var signature = javaPurchase.Call<string>(Constants.PurchaseDataGetSignatureMethod);
                PurchaseData purchaseData;
                if (PurchaseData.FromJson(originalJson, signature, out purchaseData))
                {
                    parsedPurchasesList.Add(purchaseData);
                }
                else
                {
                    _logger.Warning("Failed to parse purchase {0} ", originalJson);
                }
            }

            return parsedPurchasesList;
        }

        public PurchaseData ParseJavaPurchaseData(AndroidJavaObject javaPurchaseData)
        {
            var originalJson = javaPurchaseData.Call<string>("getOriginalJson");
            var signature = javaPurchaseData.Call<string>("getSignature");
            PurchaseData purchaseData;
            if (PurchaseData.FromJson(originalJson, signature, out purchaseData))
            {
                return purchaseData;
            }
            
            return null;
        }

        // /// <summary>
        // /// Parses the purchase results returned by Gaa Purchasing Client.
        // /// </summary>
        // /// <returns> A response code that indicates the outcome of the IapResult. </returns>
        // public IapResult GetResponseCodeFromQueryPurchasesResult(AndroidJavaObject javaPurchasesResult)
        // {
        //     var iapResult = javaPurchasesResult.Call<AndroidJavaObject>(Constants.PurchasesResultGetPurchasesDataListMethod);
        //     return ParseJavaIapResult(iapResult);
        // }

        // /// <summary>
        // /// Parses the purchases result returned by the Gaa Purchasing Library.
        // /// </summary>
        // /// <returns>An IEnumerable of<cref="PurchaseData"/>.The IEnumerable could be empty.</returns>
        // public IEnumerable<PurchaseData> ParseQueryPurchasesResult(AndroidJavaObject javaPurchasesResult)
        // {
        //     var javaIapResult = javaPurchasesResult.Call<AndroidJavaObject>(Constants.PurchasesResultGetIapResultMethod);
        //     var iapResult = ParseJavaIapResult(javaIapResult);
        //     var responseCode = GetResponseCodeFromIapResult(iapResult);
        //     if (responseCode != ResponseCode.RESULT_OK)
        //     {
        //         _logger.Error("Failed to retrieve purchases information! Error code {0}, message: {1}.",
        //             iapResult.Code, iapResult.Message);
        //         return Enumerable.Empty<PurchaseData>();
        //     }

        //     return ParseJavaPurchaseList(javaPurchasesResult.Call<AndroidJavaObject>(Constants.PurchasesResultGetPurchasesDataListMethod));
        // }
    }

}
