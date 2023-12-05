
#if UNITY_ANDROID || !UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OneStore.Purchasing.Internal;
using OneStore.Common;
using UnityEngine;

namespace OneStore.Purchasing
{
    public class PurchaseClientImpl : IPurchaseExtensions
    {
        private IPurchaseCallback _callback;
        private AndroidJavaObject _purchaseClient;

        private PurchasesUpdatedListener _purchaseUpdatedListener;

        private readonly OneStorePurchasingInventory _inventory;
        private readonly OneStoreLogger _logger;
        private readonly IapHelper _iapHelper;

        private volatile string _productInPurchaseFlow;

        private readonly string _licenseKey;

        public string StoreCode { get; private set; }

        private Queue<Action> _requestQueue = new Queue<Action>();

        private volatile ConnectionStatus _connectionStatus = ConnectionStatus.DISCONNECTED;

        private enum ConnectionStatus {
            DISCONNECTED, CONNECTING, CONNECTED,
        }

        private volatile Dictionary<ProductType, AsyncRequestStatus> _queryPurchasesCallStatus =
            new Dictionary<ProductType, AsyncRequestStatus>
            {
                {ProductType.INAPP, AsyncRequestStatus.Succeed},
                {ProductType.SUBS, AsyncRequestStatus.Succeed},
                {ProductType.AUTO, AsyncRequestStatus.Succeed},
            };

        private volatile Dictionary<ProductType, AsyncRequestStatus> _queryProductDetailsCallStatus =
            new Dictionary<ProductType, AsyncRequestStatus>
            {
                {ProductType.INAPP, AsyncRequestStatus.Succeed},
                {ProductType.SUBS, AsyncRequestStatus.Succeed},
                {ProductType.AUTO, AsyncRequestStatus.Succeed},
                {ProductType.ALL, AsyncRequestStatus.Succeed},
            };

        private enum AsyncRequestStatus
        {
            Pending, Failed, Succeed,
        }

        public PurchaseClientImpl(string licenseKey)
        {
            if (Application.platform != RuntimePlatform.Android)
            {
                throw new PlatformNotSupportedException("Operation is not supported on this platform.");
            }

            _inventory = new OneStorePurchasingInventory();
            _logger = new OneStoreLogger();
            _iapHelper = new IapHelper(_logger);
            _licenseKey = licenseKey;
        }

        public void Initialize(IPurchaseCallback callback)
        {
            _callback = callback;
            _purchaseClient = new AndroidJavaObject(Constants.PurchaseClient, Constants.Version);

            var context = JniHelper.GetApplicationContext();
            _purchaseUpdatedListener = new PurchasesUpdatedListener();
            _purchaseUpdatedListener.OnPurchasesUpdated += ProcessPurchaseUpdatedResult;

            _purchaseClient.Call(
                Constants.PurchasseClientSetupMethod,
                context,
                _licenseKey,
                _purchaseUpdatedListener
            );

            StartConnection(()=> {
                _logger.Log("Initialize: Successfully connected to the service.");
                GetStoreCode();
            });
        }

        private void StartConnection(Action action)
        {
            if (_connectionStatus == ConnectionStatus.CONNECTING) {
                _requestQueue.Enqueue(action);
                _logger.Log("Client is already in the process of connecting to service.");
                return;
            }

            _connectionStatus = ConnectionStatus.CONNECTING;

            var purchaseClientStateListener = new PurchaseClientStateListener();
            purchaseClientStateListener.OnServiceDisconnected += () =>
            {
                _logger.Warning("Client is disconnected from the service.");
                _productInPurchaseFlow = null;
                _connectionStatus = ConnectionStatus.DISCONNECTED;
            };

            purchaseClientStateListener.OnSetupFinished += (javaIapResult) =>
            {
                var iapResult = _iapHelper.ParseJavaIapResult(javaIapResult);
                if (iapResult.IsSuccessful())
                {
                    _connectionStatus = ConnectionStatus.CONNECTED;
                    action?.Invoke();
                    
                    while (_requestQueue.Count > 0) {
                        _requestQueue.Dequeue()?.Invoke();
                    }
                }
                else
                {
                    _logger.Error("Failed to connect to service with error code '{0}' and message: '{1}'.",
                        iapResult.Code, iapResult.Message);

                    _requestQueue.Clear();
                    HandleErrorCode(iapResult, () => {
                        RunOnMainThread(() => _callback.OnSetupFailed(iapResult));
                    });
                }
            };
            
            _purchaseClient.Call(
                Constants.PurchaseClientStartConnectionMethod,
                purchaseClientStateListener
            );
        }

        private void ExecuteService(Action action)
        {
            if (_connectionStatus == ConnectionStatus.CONNECTED)
            {
                action?.Invoke();
            }
            else
            {
                StartConnection(action);
            }
        }

        public void EndConnection()
        {        
            _productInPurchaseFlow = null;
            if (!IsPurchasingServiceAvailable()) return;
            _purchaseClient.Call(Constants.PurchaseClientEndConnectionMethod);
            _connectionStatus = ConnectionStatus.DISCONNECTED;
        }

        public void QueryProductDetails(ReadOnlyCollection<string> productIds, ProductType type)
        {
            if (_queryProductDetailsCallStatus[type] == AsyncRequestStatus.Pending)
            {
                return;
            }

            _queryProductDetailsCallStatus[type] = AsyncRequestStatus.Pending;

            ExecuteService(() => {
                var details = _inventory.GetProductDetails(productIds);
                if (details.Count == productIds.Count)
                {
                    _queryProductDetailsCallStatus[type] = AsyncRequestStatus.Succeed;
                    RunOnMainThread(() => _callback.OnProductDetailsSucceeded(details));
                    return;
                }

                var productDetailsParamsBuilder = new AndroidJavaObject(Constants.ProductDetailsParamBuilder);
                productDetailsParamsBuilder.Call<AndroidJavaObject>(Constants.ProductDetailsParamBuilderSetProductIdListMethod,
                    JniHelper.CreateJavaArrayList(productIds.ToArray()));
                productDetailsParamsBuilder.Call<AndroidJavaObject>(Constants.ProductDetailParamBuilderSetProductTypeMethod,
                    type.ToString());

                var productDetailsParams = productDetailsParamsBuilder.Call<AndroidJavaObject>(Constants.BuildMethod);

                var productDetailsListener = new ProductDetailsListener(type);
                productDetailsListener.OnProductDetailsResponse += ProcessProductDetailsResult;

                _purchaseClient.Call(
                    Constants.PurchaseClientQueryProductDetailsMethod,
                    productDetailsParams,
                    productDetailsListener
                );
            });
        }

        public Dictionary<string, string> GetProductJSONDictionary()
        {
            return IsPurchasingServiceAvailable() ? _inventory.GetAllProductDetails() : null;
        }

        public void QueryPurchases(ProductType type)
        {
            if (ProductType.ALL == type)
            {
                var message = "ProductType.ALL is not supported. This is supported only by the QueryProductDetails.";
                _logger.Error(message);
                RunOnMainThread(() => _callback.OnPurchaseFailed(new IapResult((int) ResponseCode.ERROR_ILLEGAL_ARGUMENT, message)));
                return;
            }
            else if (type == null)
            {
                var message = "ProductType is null";
                var iapResult = new IapResult((int) ResponseCode.ERROR_ILLEGAL_ARGUMENT, message);
                RunOnMainThread(() => _callback.OnPurchaseFailed(iapResult));
                return;
            }
            else if (_queryPurchasesCallStatus[type] == AsyncRequestStatus.Pending)
            {
                return;
            }

            _queryPurchasesCallStatus[type] = AsyncRequestStatus.Pending;

            ExecuteService(() => {
                var queryPurchasesListener = new QueryPurchasesListener(type);
                queryPurchasesListener.OnPurchasesResponse += ProcessQueryPurchasesResult;
                _purchaseClient.Call(
                    Constants.PurchaseClientQueryPurchasesMethod,
                    type.ToString(),
                    queryPurchasesListener
                );
            });
        }

        public void Purchase(PurchaseFlowParams purchaseFlowParams)
        {
            if (_productInPurchaseFlow != null)
            {
                var message = string.Format("A purchase for {0} is already in progress.", _productInPurchaseFlow);
                _logger.Error(message);
                RunOnMainThread(() => _callback.OnPurchaseFailed(new IapResult((int) ResponseCode.RESULT_ERROR, message)));
                return;
            }

            ExecuteService(() => {
                _productInPurchaseFlow = purchaseFlowParams.ProductId;

                using (var purchaseFlowParamsBuilder = new AndroidJavaObject(Constants.PurchaseFlowParamsBuilder))
                {
                    purchaseFlowParamsBuilder.Call<AndroidJavaObject>(
                        Constants.PurchaseFlowParamsBuilderSetProductIdMethod, purchaseFlowParams.ProductId);
                    purchaseFlowParamsBuilder.Call<AndroidJavaObject>(
                        Constants.PurchaseFlowParamsBuilderSetProductTypeMethod, purchaseFlowParams.ProductType);
                    purchaseFlowParamsBuilder.Call<AndroidJavaObject>(
                        Constants.PurchaseFlowParamsBuilderSetQuantityMethod, purchaseFlowParams.Quantity);
                    purchaseFlowParamsBuilder.Call<AndroidJavaObject>(
                        Constants.PurchaseFlowParamsBuilderSetDeveloperPayloadMethod, purchaseFlowParams.DeveloperPayload);
                    
                    purchaseFlowParamsBuilder.Call<AndroidJavaObject>(
                        Constants.PurchaseFlowParamsBuilderSetProductNameMethod, purchaseFlowParams.ProductName);
                    
                    purchaseFlowParamsBuilder.Call<AndroidJavaObject>(
                        Constants.PurchaseFlowParamsBuilderSetGameUserIdMethod, purchaseFlowParams.GameUserId);
                    purchaseFlowParamsBuilder.Call<AndroidJavaObject>(
                        Constants.PurchaseFlowParamsBuilderSetPromotionApplicableMethod, purchaseFlowParams.PromotinApplicable);
                    
                    LaunchPurchaseFlow(purchaseFlowParamsBuilder);
                }
            });
        }

        public void UpdateSubscription(PurchaseFlowParams purchaseFlowParams)
        {
            if (_productInPurchaseFlow != null)
            {
                var message = string.Format("The update subscription for {0} is already in progress.", _productInPurchaseFlow);
                _logger.Error(message);
                RunOnMainThread(() => _callback.OnPurchaseFailed(new IapResult((int) ResponseCode.RESULT_ERROR, message)));
                return;
            }

            ExecuteService(() => {
                _productInPurchaseFlow = purchaseFlowParams.ProductId;

                using (var purchaseFlowParamsBuilder = new AndroidJavaObject(Constants.PurchaseFlowParamsBuilder))
                {
                    purchaseFlowParamsBuilder.Call<AndroidJavaObject>(
                        Constants.PurchaseFlowParamsBuilderSetProductIdMethod, purchaseFlowParams.ProductId);
                    purchaseFlowParamsBuilder.Call<AndroidJavaObject>(
                        Constants.PurchaseFlowParamsBuilderSetProductTypeMethod, purchaseFlowParams.ProductType);
                    purchaseFlowParamsBuilder.Call<AndroidJavaObject>(
                        Constants.PurchaseFlowParamsBuilderSetDeveloperPayloadMethod, purchaseFlowParams.DeveloperPayload);

                    purchaseFlowParamsBuilder.Call<AndroidJavaObject>(
                        Constants.PurchaseFlowParamsBuilderSetProductNameMethod, purchaseFlowParams.ProductName);
                    
                    using (var updateParamsBuilder = new AndroidJavaObject(Constants.SubscriptionUpdateParamsBuilder))
                    {
                        updateParamsBuilder.Call<AndroidJavaObject>(
                            Constants.SubscriptionUpdateParamsBuilderSetProrationModeMehtod, purchaseFlowParams.ProrationMode);
                        updateParamsBuilder.Call<AndroidJavaObject>(
                            Constants.SubscriptionUpdateParamsBuilderSetOldPurchaseTokenMethod, purchaseFlowParams.OldPurchaseToken);

                        purchaseFlowParamsBuilder.Call<AndroidJavaObject>(
                            Constants.PurchaseFlowParamsBuilderSetSubscriptionUpdateParamsMethod,
                            updateParamsBuilder.Call<AndroidJavaObject>(Constants.BuildMethod)
                        );
                    }

                    LaunchPurchaseFlow(purchaseFlowParamsBuilder);
                }
            });
        }

        private void LaunchPurchaseFlow(AndroidJavaObject purchaseFlowParamsBuilder)
        {
            _purchaseClient.Call<AndroidJavaObject>(
                Constants.PurchaseClientLaunchPurchaseFlowMethod,
                JniHelper.GetUnityAndroidActivity(),
                purchaseFlowParamsBuilder.Call<AndroidJavaObject>(Constants.BuildMethod));
        }

        public void ConsumePurchase(PurchaseData purchaseData)
        {
            if (purchaseData == null)
            {
                var message = "PurchaseData is null";
                var iapResult = new IapResult((int) ResponseCode.ERROR_ILLEGAL_ARGUMENT, message);
                RunOnMainThread(() => _callback.OnConsumeFailed(iapResult));
                return;
            }

            ExecuteService(() => {
                using (var consumeParamsBuilder = new AndroidJavaObject(Constants.ConsumeParamsBuilder))
                {
                    consumeParamsBuilder.Call<AndroidJavaObject>(
                        Constants.ConsumeParamsBuilderSetPurchaseDataMethod, purchaseData.ToJava());
                    
                    var consumeListener = new ConsumeListener();
                    consumeListener.OnConsumeResponse += ProcessConsumePurchaseResult;
                    
                    _purchaseClient.Call(
                        Constants.PurchaseClientConsumePurchaseMethod,
                        consumeParamsBuilder.Call<AndroidJavaObject>(Constants.BuildMethod),
                        consumeListener
                    );
                }
            });
        }

        public void AcknowledgePurchase(PurchaseData purchaseData, ProductType type)
        {
            if (purchaseData == null)
            {
                var message = "PurchaseData is null";
                var iapResult = new IapResult((int) ResponseCode.ERROR_ILLEGAL_ARGUMENT, message);
                RunOnMainThread(() => _callback.OnAcknowledgeFailed(iapResult));
                return;
            }
            else if (type == null)
            {
                var message = "ProductType is null";
                var iapResult = new IapResult((int) ResponseCode.ERROR_ILLEGAL_ARGUMENT, message);
                RunOnMainThread(() => _callback.OnAcknowledgeFailed(iapResult));
                return;
            }
            else if (ProductType.ALL == type)
            {
                var message = "ProductType.ALL is not supported. This is supported only by the QueryProductDetails.";
                _logger.Error(message);
                RunOnMainThread(() => _callback.OnAcknowledgeFailed(new IapResult((int) ResponseCode.ERROR_ILLEGAL_ARGUMENT, message)));
                return;
            }

            ExecuteService(() => {
                using (var acknowledgeParamsBuilder = new AndroidJavaObject(Constants.AcknowledgeParamsBuilder))
                {
                    acknowledgeParamsBuilder.Call<AndroidJavaObject>(
                        Constants.AcknowledgeParamsBuilderSetPurchaseDataMethod, purchaseData.ToJava());
                    
                    var acknowledgeListener = new AcknowledgeListener(purchaseData.ProductId, type);
                    acknowledgeListener.OnAcknowledgeResponse += ProcessAcknowledgePurchaseResult;
                    
                    _purchaseClient.Call(
                        Constants.PurchaseClientAcknowledgePurchaseMethod,
                        acknowledgeParamsBuilder.Call<AndroidJavaObject>(Constants.BuildMethod),
                        acknowledgeListener
                    );
                }
            });
        }

        [Obsolete]
        public void ManageRecurringProduct(PurchaseData purchaseData, RecurringAction action)
        {
            if (purchaseData == null)
            {
                var message = "PurchaseData is null";
                var iapResult = new IapResult((int) ResponseCode.ERROR_ILLEGAL_ARGUMENT, message);
                RunOnMainThread(() => _callback.OnManageRecurringProduct(iapResult, null, action));
                return;
            }
            else if (ProductType.AUTO != _inventory.GetProductType(purchaseData.ProductId))
            {
                var message = "Purchasing data is not auto-type and does not support this feature.";
                var iapResult = new IapResult((int) ResponseCode.ERROR_ILLEGAL_ARGUMENT, message);
                RunOnMainThread(() => _callback.OnManageRecurringProduct(iapResult, null, action));
                return;
            }

            ExecuteService(() => {
                using (var recurringParamsBuilder = new AndroidJavaObject(Constants.RecurringProductParamsBuilder))
                {
                    recurringParamsBuilder.Call<AndroidJavaObject>(
                        Constants.RecurringProductParamsBuilderSetPurchaseDataMethod, purchaseData.ToJava());
                    recurringParamsBuilder.Call<AndroidJavaObject>(
                        Constants.RecurringProductParamsBuilderSetRecurringActionMethod, action.ToString());
                    
                    var recurringProductListener = new RecurringProductListener(purchaseData.ProductId);
                    recurringProductListener.OnRecurringResponse += ProcessRecurringProductResult;

                    _purchaseClient.Call(
                        Constants.PurchaseClientManageRecurringProductMethod,
                        recurringParamsBuilder.Call<AndroidJavaObject>(Constants.BuildMethod),
                        recurringProductListener
                    );
                }
            });
        }

        public void LaunchUpdateOrInstallFlow(Action<IapResult> callback)
        {
            var iapResultListener = new IapResultListener();
            iapResultListener.OnResponse += (javaIapResult) =>
            {
                var iapResult = _iapHelper.ParseJavaIapResult(javaIapResult);
                HandleErrorCode(iapResult, () => {
                    RunOnMainThread(() => callback?.Invoke(iapResult));
                });
            };

            _purchaseClient.Call(
                Constants.PurchaseClientLaunchUpdateOrInstallMethod,
                JniHelper.GetUnityAndroidActivity(),
                iapResultListener
            );
        }

        public void LaunchManageSubscription(PurchaseData purchaseData)
        {
            using (var subscriptionParamsBuilder = new AndroidJavaObject(Constants.SubscriptionParamsBuilder))
            {
                if (purchaseData != null) {
                    subscriptionParamsBuilder.Call<AndroidJavaObject>(
                        Constants.SubscriptionParamsBuilderSetPurchaseDataMethod, purchaseData.ToJava());
                }

                _purchaseClient.Call(
                    Constants.PurchaseClientLaunchManageSubscriptionMethod,
                    JniHelper.GetUnityAndroidActivity(),
                    subscriptionParamsBuilder.Call<AndroidJavaObject>(Constants.BuildMethod)
                );
            }
        }

        private void GetStoreCode()
        {
            var storeCodeListener = new StoreInfoListener();
            storeCodeListener.OnStoreInfoResponse += (storeCode) => StoreCode = storeCode;
            
            _purchaseClient.Call(
                Constants.PurchaseClientGetStoreInfoMethod,
                storeCodeListener
            );
        }

        private bool IsPurchasingServiceAvailable()
        {
            if (_connectionStatus == ConnectionStatus.CONNECTED)
            {
                return true;
            }
            var message = string.Format("Purchasing service unavailable. ConnectionStatus: {0}", _connectionStatus.ToString());
            _logger.Warning(message);
            return false;
        }

        private void ProcessProductDetailsResult(ProductType type, AndroidJavaObject javaIapResult, AndroidJavaObject javaProductDetailList)
        {
            var iapResult = _iapHelper.ParseJavaIapResult(javaIapResult);
            if (!iapResult.IsSuccessful())
            {
                _logger.Warning("Retrieve product failed with error code '{0}' and message: '{1}'",
                    iapResult.Code, iapResult.Message);

                _queryProductDetailsCallStatus[type] = AsyncRequestStatus.Failed;

                HandleErrorCode(iapResult, () => {
                    RunOnMainThread(() => _callback.OnProductDetailsFailed(iapResult));
                });
                return;
            }
            
            var productDetailList = _iapHelper.ParseProductDetailsResult(javaIapResult, javaProductDetailList);
            _inventory.UpdateProductDetailInventory(productDetailList);
            _queryProductDetailsCallStatus[type] = AsyncRequestStatus.Succeed;
            RunOnMainThread(() => _callback.OnProductDetailsSucceeded(productDetailList.ToList()));
        }

        private void ProcessPurchaseUpdatedResult(AndroidJavaObject javaIapResult, AndroidJavaObject javaPurchasesList)
        {
            _productInPurchaseFlow = null;
            var iapResult = _iapHelper.ParseJavaIapResult(javaIapResult);
            if (!iapResult.IsSuccessful())
            {
                _logger.Warning("Purchase failed with error code '{0}' and message: '{1}'",
                    iapResult.Code, iapResult.Message);

                HandleErrorCode(iapResult, () => {
                    RunOnMainThread(() => _callback.OnPurchaseFailed(iapResult));
                });
                return;
            }

            var purchasesList = _iapHelper.ParseJavaPurchasesList(javaPurchasesList);
            if (purchasesList.Any())
            {
                _inventory.UpdatePurchaseInventory(purchasesList);
                RunOnMainThread(() => _callback.OnPurchaseSucceeded(purchasesList.ToList()));
            }
        }

        private void ProcessQueryPurchasesResult(ProductType type, AndroidJavaObject javaIapResult, AndroidJavaObject javaPurchasesList)
        {
            _productInPurchaseFlow = null;
            var iapResult = _iapHelper.ParseJavaIapResult(javaIapResult);
            if (!iapResult.IsSuccessful())
            {
                _logger.Warning("Purchase failed with error code '{0}' and message: '{1}'",
                    iapResult.Code, iapResult.Message);

                _queryPurchasesCallStatus[type] = AsyncRequestStatus.Failed;

                HandleErrorCode(iapResult, () => {
                    RunOnMainThread(() => _callback.OnPurchaseFailed(iapResult));
                });
                return;
            }

            _queryPurchasesCallStatus[type] = AsyncRequestStatus.Succeed;
            var purchasesList = _iapHelper.ParseJavaPurchasesList(javaPurchasesList);
            _inventory.UpdatePurchaseInventory(purchasesList);
            RunOnMainThread(() => _callback.OnPurchaseSucceeded(purchasesList.ToList()));
        }

        private void ProcessConsumePurchaseResult(AndroidJavaObject javaIapResult, AndroidJavaObject javaPurchaseData)
        {
            var iapResult = _iapHelper.ParseJavaIapResult(javaIapResult);
            if (!iapResult.IsSuccessful())
            {
                _logger.Error("Failed to finish the consume purchase with error code {0} and message: {1}",
                    iapResult.Code, iapResult.Message);

                HandleErrorCode(iapResult, () => {
                    RunOnMainThread(() => _callback.OnConsumeFailed(iapResult));
                });
                return;
            }

            var purchaseData = _iapHelper.ParseJavaPurchaseData(javaPurchaseData);
            _inventory.RemovePurchase(purchaseData.ProductId);
            RunOnMainThread(() => _callback.OnConsumeSucceeded(purchaseData));
        }

        private void ProcessAcknowledgePurchaseResult(AndroidJavaObject javaIapResult, string productId, ProductType type)
        {
            var iapResult = _iapHelper.ParseJavaIapResult(javaIapResult);
            if (!iapResult.IsSuccessful())
            {
                _logger.Error("Failed to finish the acknowledge purchase with error code {0} and message: {1}",
                    iapResult.Code, iapResult.Message);
             
                HandleErrorCode(iapResult, () => {
                    RunOnMainThread(() => _callback.OnAcknowledgeFailed(iapResult));
                });
                return;
            }
            
            QueryPurchasesInternal(productId, type, (iapRseult, purchaseData) => {
                RunOnMainThread(() => {
                    if (iapResult.IsSuccessful())
                    {
                        _callback.OnAcknowledgeSucceeded(purchaseData, type);
                    }
                    else
                    {
                        _callback.OnAcknowledgeFailed(iapResult);
                    }
                });
            });
        }

        private void ProcessRecurringProductResult(AndroidJavaObject javaIapResult, string productId, string recurringAction)
        {
            var iapResult = _iapHelper.ParseJavaIapResult(javaIapResult);
            if (!iapResult.IsSuccessful())
            {
                _logger.Error("Failed to finish the manage recurring with error code {0} and message: {1}",
                    iapResult.Code, iapResult.Message);

                HandleErrorCode(iapResult, () => {
                    RunOnMainThread(() => _callback.OnManageRecurringProduct(iapResult, null, RecurringAction.Get(recurringAction)));
                });
                return;
            }

            QueryPurchasesInternal(productId, ProductType.AUTO, (result, purchaseData) => {
                RunOnMainThread(() => _callback.OnManageRecurringProduct(result, purchaseData, RecurringAction.Get(recurringAction)));
            });
        }

        private void QueryPurchasesInternal(string productId, ProductType type, Action<IapResult, PurchaseData> callback)
        {
            var queryPurchasesListener = new QueryPurchasesListener(type);
            queryPurchasesListener.OnPurchasesResponse += (_, javaIapResult, javaPurchasesList) => {
                var iapResult = _iapHelper.ParseJavaIapResult(javaIapResult);
                if (!iapResult.IsSuccessful())
                {
                    _logger.Warning("QueryPurchasesInternal failed with error code '{0}' and message: '{1}'",
                        iapResult.Code, iapResult.Message);
                    
                    HandleErrorCode(iapResult, () => {
                        callback?.Invoke(iapResult, null);
                    });
                    return;
                }

                var purchasesList = _iapHelper.ParseJavaPurchasesList(javaPurchasesList);
                if (purchasesList.Any())
                {
                    _inventory.UpdatePurchaseInventory(purchasesList);
                    PurchaseData purchaseData;
                    if (_inventory.GetPurchaseData(productId, out purchaseData))
                    {
                        callback?.Invoke(iapResult, purchaseData);
                    }
                }
            };

            _purchaseClient.Call(
                Constants.PurchaseClientQueryPurchasesMethod,
                type.ToString(),
                queryPurchasesListener
            );
        }

        private void HandleErrorCode(IapResult iapResult, Action action = null)
        {
            var responseCode = _iapHelper.GetResponseCodeFromIapResult(iapResult);
            switch (responseCode)
            {
                case ResponseCode.RESULT_NEED_UPDATE:
                    RunOnMainThread(() => _callback.OnNeedUpdate());
                    break;
                case ResponseCode.RESULT_NEED_LOGIN:
                    RunOnMainThread(() => _callback.OnNeedLogin());
                    break;
                case ResponseCode.ERROR_SERVICE_DISCONNECTED:
                    _productInPurchaseFlow = null;
                    _connectionStatus = ConnectionStatus.DISCONNECTED;
                    break;
                default:
                    action?.Invoke();
                    break;
            }
        }

        private void RunOnMainThread(Action action)
        {
            OneStoreDispatcher.RunOnMainThread(() => action());
        }
    }
}

#endif
