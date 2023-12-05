using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using OneStore.Auth;
using OneStore.Common;
using OneStore.Purchasing;
using OneStore.Sample;
using OneStore.Sample.Common;
using static ProductItem;

public class MainUIManager : MonoBehaviour, IPurchaseCallback
{
    private static readonly string TAG = "MainUIScript";

    private LogManager logger;

    public GameObject _consumableItem;
    public GameObject _consumableContent;

    public GameObject _subscriptionItem;
    public GameObject _subscriptionContent;

    public GameObject scrollLog;

    public GameObject _coinView;

    public GameObject _mySubscriptionItem;
    private PurchaseData _ownedSubsPurchaseData;

    private PurchaseManager _purchaseManager;

    private List<ProductItem> _consumableList = new List<ProductItem>();
    private List<ProductItem> _subscriptionList = new List<ProductItem>();

    private bool _doubleBackToExitPressedOnce = false;
    private bool _isPurchaseFlow = false;

    void Awake()
    {
        Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
    }

    void OnDestroy()
    {
        _purchaseManager?.EndConnection();
    }

    void Start()
    {
#if UNITY_ANDROID
        Screen.fullScreen = false;
#endif

        logger = GameObject.Find("LogManager").GetComponent<LogManager>();
        var coin = PlayerPrefs.GetInt("Coins", 0);
        _coinView.GetComponent<Text>().text = Utils.GetThousandFormat(coin);
        _mySubscriptionItem.SetActive(false);
    
        StartOneStoreSignInFlow();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (DialogManager.Instance.IsShowing())
            {
                DialogManager.Instance.Dismiss();
            }
            else
            {
                if (!_doubleBackToExitPressedOnce)
                {
                    _doubleBackToExitPressedOnce = true;
                    Utils.ShowAndroidToast("Press the back key once more to exit.");
                    StartCoroutine(QuitingTimer());
                }
            }
        }
    }

    IEnumerator QuitingTimer()
    {
        yield return null;
        const float INTERVAL_TIME = 3f;
        float counter = 0;
        while (counter < INTERVAL_TIME)
        {
            counter += Time.deltaTime;
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Quit();
            }
            yield return null;
        }

        _doubleBackToExitPressedOnce = false;
    }

    void Quit()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    void OnApplicationPause(bool pauseStatus) {
        if (!pauseStatus && _purchaseManager != null && !_isPurchaseFlow)
        {
            StartCoroutine(QueryPurchases());
        }
    }

    public void Log(LogType type, string format, params object[] args)
    {
        if (logger != null)
        { 
            logger.Log(type, format, args);
        }
        else
        { 
            Debug.unityLogger.LogFormat(type, "[" + TAG + "]: " + format, args);
        }
    }

    public void AddCoin(int coin)
    {
        Log(LogType.Log, "AddCoin: {0} coin", coin);
        int savedCoin = PlayerPrefs.GetInt("Coins", 0);
        int result = savedCoin + coin;
        UpdateCoin(savedCoin, result);
    }

    void UpdateCoin(int current, int target)
    {
        PlayerPrefs.SetInt("Coins", target);
        if (current != target)
        {
            StartCoroutine(Counting(current, target));
        }
        else
        {
            _coinView.GetComponent<Text>().text = Utils.GetThousandFormat(target);
        }
    }

    IEnumerator Counting(int current, int target)
    {
        var coinLabel = _coinView.GetComponent<Text>();
        float duration = 0.5f; // 카운팅에 걸리는 시간 설정. 
        float offset = (target - current) / duration;

        while (current < target)
        {
            current += (int) (offset * Time.deltaTime);
            coinLabel.text = Utils.GetThousandFormat(current);
            yield return null;
        }
        current = target;
        coinLabel.text = Utils.GetThousandFormat(current);
    }

    private void SetOwnedSubscriptionItem(PurchaseData purchase, Sprite icon)
    {
        _ownedSubsPurchaseData = purchase;
        _mySubscriptionItem.SetActive(true);
        _mySubscriptionItem.GetComponent<Image>().sprite = icon;
    }

    private void StartOneStoreSignInFlow()
    {
        /// <summary>
        /// Warning! This code must be deleted during release build as it may be vulnerable to security.
        /// Use <seealso cref="OneStoreLogger.SetLogLevel(int)"/> only for development.
        /// </summary>
        OneStoreLogger.SetLogLevel(2);

        new OneStoreAuthClientImpl().LaunchSignInFlow((signInResult) => {
            if (signInResult.IsSuccessful())
            {
                Log(LogType.Log, "login success");
                InstantiatePurchaseManager();
            }
            else
            {
                Log(LogType.Error, "SignIn failed with error code: '{0}' and message: '{1}'", signInResult.Code, signInResult.Message);
            }
        });
    }

    private void InstantiatePurchaseManager()
    {
        _purchaseManager = new PurchaseManager(this);
        _purchaseManager.logger += Log;
        StartCoroutine(RetrieveProducts());
    }

    #region Use method of the PurchaseClientImpl.

    IEnumerator RetrieveProducts()
    {
        yield return new WaitForSeconds(1.0f);
        _purchaseManager.RetrieveProducts();
    }

    IEnumerator QueryPurchases()
    {
        yield return new WaitForSeconds(1.0f);
        _purchaseManager.QueryPurchases(ProductType.INAPP);
        _purchaseManager.QueryPurchases(ProductType.SUBS);
    }

    void ManageRecurringProduct(string productId)
    {
        Log(LogType.Log, "ManageRecurringProduct: productId: {0}", productId);
        _purchaseManager.ManageRecurringProduct(productId);
    }

    public void MySucscriptions()
    {
        LaunchManageSubscription(null);
    }

    void LaunchManageSubscription(string productId)
    {
        Log(LogType.Log, "LaunchSubscriptionsMenu: productId: {0}", productId);
        _purchaseManager.LaunchManageSubscription(productId);
    }

    #endregion

    #region Called when an item on the UI is clicked.

    /// <summary>
    /// </summary>
    /// <param name="productItem"></param>
    /// <seealso cref="ProductItem.productId"/>
    /// <seealso cref="ProductItem.productType"/>
    /// productType: inapp
    /// If you consume it immediately after purchasing it, it will be used like a consumer type of product.
    /// If you don't consume in-app products, you can use them like a permanent product type.
    /// It can also be used as a fixed-term product if consumed after a certain period of time.
    /// 
    /// productType: subscription
    /// It's a subscription product type that allows you to make automatic payments weekly, monthly, and yearly.
    /// 
    /// productType: auto (deprecated)
    /// It's a monthly fixed-rate product, and you can pay automatically every month.
    /// You can change the status value after acknowledge purchase.
    /// Use the ProductType.SUBS instead of the ProductType.AUTO.
    /// </param>
    void OnProcutItemClick(ProductItem productItem)
    {
        var id = productItem.productId;
        var type = ProductType.Get(productItem.productType);

        if (ProductType.SUBS == type && _ownedSubsPurchaseData != null)
        {
            if (productItem.productId.Equals(_ownedSubsPurchaseData.ProductId))
            {
                _purchaseManager.LaunchManageSubscription(productItem.productId);
            }
            else
            {
                DialogManager.Instance.ShowUpdateSubscription(productItem, (productId, prorationMode) => {
                    Log(LogType.Log, "UpdateSubscription - ConfirmClick - {0}", productId);
                    _isPurchaseFlow = true;
                    _purchaseManager.UpdateSubscription(productId, _ownedSubsPurchaseData, prorationMode, /* developerPayload */null);
                });
            }
            return;
        }

        DialogManager.Instance.ShowPurchase(productItem, (productId, quantity) => {
            Log(LogType.Log, "ShowPurchase - productId: {0}, quantity: {1} ", productId, quantity);
            _isPurchaseFlow = true;
            _purchaseManager.Purchase(productId,/* developerPayload */null, quantity);
        });
    }

    #endregion




    #region IPurchaseCallback implementations.

    public void OnSetupFailed(IapResult iapResult)
    {
        Log(LogType.Error, "Setup failed with error code '{0}' and message: '{1}'", iapResult.Code, iapResult.Message);
    }

    public void OnProductDetailsSucceeded(List<ProductDetail> productDetails)
    {
        _purchaseManager.UpdateProductDetailInventory(productDetails);
        for (int i = 0; i < productDetails.Count; i++)
        {
            var productDetail = productDetails[i];
            Log(LogType.Log, "ProductDetail[{0}]: {1} ({2})", productDetail.productId, productDetail.title, productDetail.type);

            var sprite = Resources.Load<Sprite>("item" + (i % 4)) as Sprite;
            ProductItem product = new ProductItem(productDetail, sprite);

            var type = ProductType.Get(productDetail.type);
            if (ProductType.INAPP == type)
            {
                if (_consumableList.Find((f) => f.productId == productDetail.productId) == null)
                {
                    GenerateItem(_consumableItem, _consumableContent, product);
                    _consumableList.Add(product);
                }
            }
            else if (ProductType.SUBS == type)
            {
                if (_subscriptionList.Find((f) => f.productId == productDetail.productId) == null)
                {
                    GenerateItem(_subscriptionItem, _subscriptionContent, product);
                    _subscriptionList.Add(product);
                }
            }
        }

        StartCoroutine(QueryPurchases());
    }

    public void OnProductDetailsFailed(IapResult iapResult)
    {
        Log(LogType.Error, "QueryProductDetails failed with error code '{0}' and message: '{1}'", iapResult.Code, iapResult.Message);
    }

    private void GenerateItem(GameObject item, GameObject parent, ProductItem data)
    {
        GameObject scrollItem = Instantiate<GameObject>(item, transform);
        scrollItem.transform.SetParent(parent.transform, false);
        scrollItem.transform.Find("Title").gameObject.GetComponent<Text>().text = data.title;
        scrollItem.transform.Find("Price").gameObject.GetComponent<Text>().text = data.price;
        scrollItem.transform.Find("Icon").GetChild(0).gameObject.GetComponent<Image>().sprite = data.icon;
        if (ProductType.SUBS == ProductType.Get(data.productType))
        {
            scrollItem.transform.Find("Status").GetChild(0).GetComponent<Text>().text = data.state.ToString();
        }
        scrollItem.GetComponent<Button>().onClick.AddListener(() =>
            OnProcutItemClick(data));
    }

    public void OnPurchaseSucceeded(List<PurchaseData> purchases)
    {
        foreach (var purchase in purchases)
        {
            var type = _purchaseManager.GetProductType(purchase.ProductId);
            Log(LogType.Log, "PurchaseData: {0}, ({1})", purchase.ProductId, type);

            if (ProductType.INAPP == type)
            {
                _purchaseManager.ConsumePurchase(purchase);
            }
            else if (ProductType.SUBS == type)
            {
                if (!purchase.Acknowledged)
                {
                    _purchaseManager.AcknowledgePurchase(purchase, type);
                }
                else
                {
                    UpdateItem(purchase, type);
                }
            }
        }

        _purchaseManager.UpdatePurchaseInventory(purchases);
        _isPurchaseFlow = false;
    }

    public void OnPurchaseFailed(IapResult iapResult)
    {
        Log(LogType.Error, "Purchase failed with error code '{0}' and message: '{1}'",
                    iapResult.Code, iapResult.Message);
        _isPurchaseFlow = false;
    }

    public void OnConsumeSucceeded(PurchaseData purchaseData)
    {
        Log(LogType.Log, "OnConsumeSucceeded:\n\t\t-> productId: {0}", purchaseData.ProductId);
        PurchaseData purchase;
        if (_purchaseManager.GetPurchase(purchaseData.ProductId, out purchase))
        {
            ProductDetail productDetail;
            if (_purchaseManager.GetProductDetail(purchaseData.ProductId, out productDetail))
            {
                AddCoin(int.Parse(productDetail.price) * purchase.Quantity);
                _purchaseManager.RemovePurchase(purchaseData.ProductId);
            }
        }
    }

    public void OnConsumeFailed(IapResult iapResult)
    {
        Log(LogType.Error, "ConsumePurchase failed with error code '{0}' and message: '{1}'", iapResult.Code, iapResult.Message);
    }

    public void OnAcknowledgeSucceeded(PurchaseData purchaseData, ProductType type)
    {
        Log(LogType.Log, "OnAcknowledgeSucceeded:\n\t\t-> productId: {0}", purchaseData.ProductId);
        PurchaseData purchase;
        if (_purchaseManager.GetPurchase(purchaseData.ProductId, out purchase))
        {
            UpdateItem(purchase, type);
        }
    }

    public void OnAcknowledgeFailed(IapResult iapResult)
    {
        Log(LogType.Error, "AcknowledgePurchase failed with error code '{0}' and message: '{1}'", iapResult.Code, iapResult.Message);
    }

    private void UpdateItem(PurchaseData purchase, ProductType type)
    {
        if (ProductType.SUBS == type)
        {
            for(var i = 0; i < _subscriptionList.Count; i++)
            {
                var item = _subscriptionList[i];
                var child = _subscriptionContent.transform.GetChild(i);

                item.state = item.productId.Equals(purchase.ProductId) ? SubscriptionState.OWNED : SubscriptionState.UPDATE;
                child.Find("Status").GetChild(0).GetComponent<Text>().text = item.state.ToString();
                if (item.state == SubscriptionState.OWNED)
                {
                    SetOwnedSubscriptionItem(purchase, item.icon);
                }
            }
        }
    }

    public void OnManageRecurringProduct(IapResult iapResult, PurchaseData purchaseData, RecurringAction action)
    {
        if (iapResult.IsSuccessful())
        {
            Log(LogType.Log, "OnManageRecurringProduct:\n\t\t-> productId: {0}, action: {1}", purchaseData.ProductId, action.ToString());
        }
        else
        {
            Log(LogType.Error, "OnManageRecurringProduct failed with error code '{0}' and message: '{1}'", iapResult.Code, iapResult.Message);
        }
    }

    public void OnNeedUpdate()
    {
        _purchaseManager.LaunchUpdateOrInstallFlow((iapResult) => {
            if (iapResult.IsSuccessful())
            {
                Log(LogType.Log, "The installation succeeded.");
                StartOneStoreSignInFlow();
            }
            else
            {
                Log(LogType.Error, "LaunchUpdateOrInstallFlow failed with error code: '{0}' and message: '{1}'", iapResult.Code, iapResult.Message);
            }
        });
    }

    public void OnNeedLogin()
    {
        StartOneStoreSignInFlow();
    }

    #endregion

    public void CheckLicense() {
        Log(LogType.Log, "checklicense");
        SceneManager.LoadScene(SampleScene.License);
    }
}
