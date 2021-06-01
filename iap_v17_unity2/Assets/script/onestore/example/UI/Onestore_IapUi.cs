using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OneStore;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public static class ItemInfo
{
    //개발자센터에 등록한 상품 ID를 사용
    public static string inapp_p5000 = "p5000";  
    public static string inapp_p10000 = "p10000";
    public static string inapp_p50000 = "p50000";
    public static string auto_a10000 = "a100000";

    //상품 타입
    public static string inapp_type = "inapp";
    public static string auto_type = "auto";

    public static string[] inapp_products = { inapp_p5000, inapp_p10000, inapp_p50000 };
    public static string[] auto_products = { auto_a10000 };
}

public class Onestore_IapUi : MonoBehaviour
{
    private const string TAG = "Onestore_IapUi";

    // 개발사의 규칙에 맞는 payload를 생성하여야 하며 입력 가능한 Developer Payload는 최대 100byte까지 입니다.
    private string devPayload = "this is test payload!";
    //원스토어 개발자 센터에서 발급받은 해당 앱의 라이센스키(public key)를 사용해야 합니다.
    private string base64EncodedPublicKey = "input your public key";

    WaitForSeconds ws = new WaitForSeconds(1.0f);

    private Onestore_Log onestore_Log;

    public Sprite s_check;
    public Sprite s_not_check;

    GameObject Item_p5000;
    GameObject Item_p10000;
    GameObject Item_p50000;
    GameObject Item_a100000;
    GameObject Purcase_p5000;
    GameObject Purcase_p10000;
    GameObject Purcase_p50000;
    GameObject Purcase_a100000;
    Text Purcase_a100000_Text;

    Text Cash_Text;

    public GameObject Popup;

    private void Awake()
    {
        //api callback 등록
        Onestore_IapResultListener.ApiSuccessEvent += setIcon;

        Onestore_IapResultListener.CallIsBillingSupported += isBillingSupported;
        Onestore_IapResultListener.CallLaunchUpdateOrInstallFlow += launchUpdateOrInstallFlow;
        Onestore_IapResultListener.CallLogin += login;

        Onestore_IapResultListener.CallGetPurchase += getPurchases;
        Onestore_IapResultListener.CallQueryProducts += getProductDetails;

        Onestore_IapResultListener.CallConsume += consume;
        Onestore_IapResultListener.CallManageRecurringAuto += manageRecurringAuto;

        Onestore_IapResultListener.PurchaseItemChangeEvent += setPurchaseItem;
        Onestore_IapResultListener.ProductItemChangeEvent += setProductItem;

        Onestore_IapResultListener.ConsumeSuccessEvent += consumeSuccess;

        Onestore_IapResultListener.ShowPopup += ShowPopup;
        Onestore_IapResultListener.PrintLog += PrintLog;
    }

    private void OnDestroy()
    {
        //api callback 해제
        Onestore_IapResultListener.ApiSuccessEvent -= setIcon;

        Onestore_IapResultListener.CallIsBillingSupported -= isBillingSupported;
        Onestore_IapResultListener.CallLaunchUpdateOrInstallFlow -= launchUpdateOrInstallFlow;
        Onestore_IapResultListener.CallLogin -= login;

        Onestore_IapResultListener.CallGetPurchase -= getPurchases;
        Onestore_IapResultListener.CallQueryProducts -= getProductDetails;

        Onestore_IapResultListener.CallConsume -= consume;
        Onestore_IapResultListener.CallManageRecurringAuto -= manageRecurringAuto;

        Onestore_IapResultListener.PurchaseItemChangeEvent -= setPurchaseItem;
        Onestore_IapResultListener.ProductItemChangeEvent -= setProductItem;

        Onestore_IapResultListener.ConsumeSuccessEvent -= consumeSuccess;

        Onestore_IapResultListener.ShowPopup -= ShowPopup;
        Onestore_IapResultListener.PrintLog -= PrintLog;

        destroy();
    }

    private void Start()
    {
        InitObject();

        StartCoroutine(StartConnectService());
    }

    private void Update ()
	{
		if (Input.GetKeyDown (KeyCode.Escape)) {
			Application.Quit ();
		}
	}

    IEnumerator StartConnectService()
    {
        yield return ws;
        connectService();
    }


    /**********************************************************
                            API 호출
     **********************************************************/
    public void connectService ()
	{
        onestore_Log.Log(TAG, "connectService");
        //원스토어 개발자 센터에서 발급받은 해당 앱의 라이센스키(public key)를 사용해야 합니다.
        base64EncodedPublicKey = "MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQC195Gq6htuAeoJT/yy1fZvHXOAGGYYqfnoGUCyjtoscoMHBEfobaTSL5QjSKu0ghXbqNWGJMNOsah9l71xM4LbCUdMv8fi4/SvzVBCd3SifS4euf/gJJSwGyVHmrknLSRu9sexBr/LCbpQ+USqQGgErSt/gPPg/GZ65FEhD2Zj1QIDAQAB";
		Onestore_IapCallManager.connectService (base64EncodedPublicKey);
	}

    public void destroy()
    {
        onestore_Log.Log(TAG, "destroy");
        Onestore_IapCallManager.destroy();
    }

    public void isBillingSupported ()
	{
        onestore_Log.Log(TAG, "isBillingSupported");
        Onestore_IapCallManager.isBillingSupported ();
	}

    public void getPurchases ()
	{
        onestore_Log.Log(TAG, "getPurchases");
        Onestore_IapCallManager.getPurchases ();
	}

    public void getProductDetails()
	{
        onestore_Log.Log(TAG, "getProductDetails");
        Onestore_IapCallManager.getProductDetails (ItemInfo.inapp_products, ItemInfo.inapp_type);
		Onestore_IapCallManager.getProductDetails (ItemInfo.auto_products, ItemInfo.auto_type);
    }

    public void buyProductInapp_p5000 ()
	{
        onestore_Log.Log(TAG, "buyProductInapp_p5000");
        Onestore_IapCallManager.buyProduct (ItemInfo.inapp_p5000, ItemInfo.inapp_type, devPayload);
	}

    public void buyProductInapp_p10000()
    {
        onestore_Log.Log(TAG, "buyProductInapp_p10000");
        Onestore_IapCallManager.buyProductUseName(ItemInfo.inapp_p10000, "10000Won",ItemInfo.inapp_type, devPayload);
    }

    public void buyProductInapp_p50000()
    {
        onestore_Log.Log(TAG, "buyProductInapp_p50000");
        Onestore_IapCallManager.buyProduct(ItemInfo.inapp_p50000, ItemInfo.inapp_type, devPayload);
    }

    public void buyProductInapp_a100000()
    {
        onestore_Log.Log(TAG, "buyProductInapp_a100000");
        Onestore_IapCallManager.buyProduct(ItemInfo.auto_a10000, ItemInfo.auto_type, devPayload);
    }

    public void consume (string productId)
    {
        onestore_Log.Log(TAG, "consume productId: " + productId);
        string inapp_json = PlayerPrefs.GetString (productId);
        onestore_Log.Log(TAG, "consume inapp_json: " + inapp_json);
        if (inapp_json.Length > 0) {
    		Onestore_IapCallManager.consume (inapp_json);
    	} else {
    		AndroidNative.showMessage ("error", "no data to consume", "ok");
    	}
    }

    public void manageRecurringAuto()
    {
        onestore_Log.Log(TAG, "manageRecurringAuto");
        string auto_json = PlayerPrefs.GetString (ItemInfo.auto_a10000);
    	PurchaseData response = JsonUtility.FromJson<PurchaseData> (auto_json);
        onestore_Log.Log(TAG, "manageRecurringAuto auto_json: " + auto_json);
        if (auto_json.Length > 0) {
    		string command = "";
    		if (response.recurringState == 0) {
    			command = "cancel"; 
    		} else if (response.recurringState == 1) {
    			command = "reactivate";
    		}
    		Onestore_IapCallManager.manageRecurringAuto (auto_json, command);
    	} else {
    		AndroidNative.showMessage ("Warning!!", "no data for manageRecurringAuto", "ok");
    	}
    }

    public void login ()
    {
    	Onestore_IapCallManager.login ();
    }

    public void launchUpdateOrInstallFlow()
    {
        Onestore_IapCallManager.launchUpdateOrInstallFlow();
    }


    /**********************************************************
                        UI 작업
     **********************************************************/

    //GameObject 초기화
    void InitObject()
    {
        onestore_Log = GameObject.Find("Onestore_Log").GetComponent<Onestore_Log>();

        //s_check = Resources.Load<Sprite>("check");
        //s_not_check = Resources.Load<Sprite>("not_check");
        Item_p5000 = GameObject.Find("Item_p5000");
        Item_p10000 = GameObject.Find("Item_p10000");
        Item_p50000 = GameObject.Find("Item_p50000");
        Item_a100000 = GameObject.Find("Item_a100000");

        Purcase_p5000 = GameObject.Find("Purcase_p5000");
        Purcase_p10000 = GameObject.Find("Purcase_p10000");
        Purcase_p50000 = GameObject.Find("Purcase_p50000");
        Purcase_a100000 = GameObject.Find("Purcase_a100000");
        Purcase_a100000_Text = Purcase_a100000.transform.GetChild(0).GetComponent<Text>();

        Cash_Text = GameObject.Find("Cash").GetComponent<Text>();
        Cash_Text.text = PlayerPrefs.GetInt("Cash").ToString();

        Item_p5000.SetActive(false);
        Item_p10000.SetActive(false);
        Item_p50000.SetActive(false);
        Item_a100000.SetActive(false);
        Purcase_p5000.SetActive(false);
        Purcase_p10000.SetActive(false);
        Purcase_p50000.SetActive(false);
        Purcase_a100000.SetActive(false);

        //Popup = GameObject.Find("Popup");
        Popup.SetActive(false);
    }


    /*
     * Onestore_IapCallbackManager.cs 응답중 성공 처리 부분
     */

    // API 버튼 아이콘 변경
    void setIcon(string gameObjectName)
    {
        GameObject.Find(gameObjectName).transform.GetChild(1).GetComponent<Image>().overrideSprite = s_check;
    }

    //getPurchases 응답으로 해당 아이템 표시
    void setPurchaseItem(PurchaseData purchaseData, bool acitive)
    {
        switch (purchaseData.productId)
        {
            case "p5000":
                Purcase_p5000.SetActive(acitive);
                break;
            case "p10000":
                Purcase_p10000.SetActive(acitive);
                break;
            case "p50000":
                Purcase_p50000.SetActive(acitive);
                break;
            case "a100000":
                Purcase_a100000.SetActive(acitive);

                string state = "";
                if (purchaseData.recurringState == 0)
                {
                    state = "자동 결제중";
                }
                else if (purchaseData.recurringState == 1)
                {
                    state = "해지 예약중";
                }

                Purcase_a100000_Text.text = "Auto\na100000\n " + state;
                break;
        }
    }

    //getProductDetails 응답으로 해당 아이템 표시
    void setProductItem(ProductDetail productDetail)
    {
        switch (productDetail.productId)
        {
            case "p5000":
                Item_p5000.SetActive(true);
                break;
            case "p10000":
                Item_p10000.SetActive(true);
                break;
            case "p50000":
                Item_p50000.SetActive(true);
                break;
            case "a100000":
                Item_a100000.SetActive(true);
                break;
        }
    }

    //consume 성공시 가상 cash지급
    void consumeSuccess(PurchaseData purchaseData)
    {
        int cash = PlayerPrefs.GetInt("Cash");
        switch (purchaseData.productId)
        {
            case "p5000":
                PlayerPrefs.SetInt("Cash", cash + 500);
                break;
            case "p10000":
                PlayerPrefs.SetInt("Cash", cash + 1000);
                break;
            case "p50000":
                PlayerPrefs.SetInt("Cash", cash + 5000);
                break;
            case "a100000":    
                break;
        }
        setCashText();
    }

    //가상 cash 차감
    public void useCash(int use)
    {
        int cash = PlayerPrefs.GetInt("Cash");
        if(cash >= use)
        {
            PlayerPrefs.SetInt("Cash", cash - use);
            setCashText();
        }
        else
        {
            onestore_Log.Log(TAG, "not enough cash");
        }
    }

    void setCashText()
    {
        Cash_Text.text = PlayerPrefs.GetInt("Cash").ToString();
    }

    void ShowPopup(string msg, Action action)
    {       
        Popup.transform.GetChild(0).GetComponent<Text>().text = msg;
        Popup.transform.GetChild(1).GetComponent<Button>().onClick.RemoveAllListeners();
        Popup.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() => {
            Popup.SetActive(false);
            action();
        });
        Popup.SetActive(true);
    }

    void PrintLog(string title, string msg, bool showDialog)
    {
        onestore_Log.Log(TAG, "title: " + title + "\nmsg: " + msg);
        if(showDialog)
        {
            AndroidNative.showMessage(title, msg, "ok");
        }
    }


}
