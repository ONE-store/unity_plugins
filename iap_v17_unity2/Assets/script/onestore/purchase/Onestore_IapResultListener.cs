using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace OneStore
{
	public class Onestore_IapResultListener : MonoBehaviour
	{
        /* api 오류 응답
         *        
         * RESULT_OK                    ( 0, "성공"),
         * RESULT_USER_CANCELED         ( 1, "결제가 취소되었습니다."),
         * RESULT_SERVICE_UNAVAILABLE   ( 2, "구매에 실패했습니다. (단말 또는 서버 네트워크 오류가 발생하였습니다)"),
         * RESULT_BILLING_UNAVAILABLE   ( 3, "구매에 실패했습니다. (구매 처리 과정에서 오류가 발생하였습니다)"),
         * RESULT_ITEM_UNAVAILABLE      ( 4, "구매에 실패했습니다. (상품이 판매중이 아니거나 구매할 수 없는 상태입니다)"),
         * RESULT_DEVELOPER_ERROR       ( 5, "구매에 실패했습니다. (올바르지 않은 구매 요청입니다)"),
         * RESULT_ERROR                 ( 6, "구매에 실패했습니다. (정의되지 않은 기타 오류가 발생했습니다)"),
         * RESULT_ITEM_ALREADY_OWNED    ( 7, "구매에 실패했습니다. (이미 아이템을 소유하고 있습니다)"),
         * RESULT_ITEM_NOT_OWNED        ( 8, "구매에 실패했습니다. (아이템을 소유하고 있지 않아 comsume 할 수 없습니다)"),
         * RESULT_FAIL                  ( 9, "결제에 실패했습니다. 결제 가능 여부 및 결제 수단 확인 후 다시 결제해주세요."),
         * RESULT_NEED_LOGIN            (10, "구매에 실패했습니다. (구매를 위해 원스토어 로그인이 필요합니다)"),
         * RESULT_NEED_UPDATE           (11, "구매에 실패했습니다. (원스토어 서비스앱의 업데이트가 필요합니다)"),
         * RESULT_SECURITY_ERROR        (12, "구매에 실패했습니다. (비정상 앱에서 결제가 요청되었습니다)"),
         *         
         */

        const int IAP_RESULT_CODE_SUCCESS = 0;
        const int IAP_RESULT_CODE_USER_CANCELED = 1;
        const int IAP_RESULT_CODE_ITEM_ALREADY_OWNED = 7;
        const int IAP_RESULT_CODE_NEED_LOGIN = 10;

        public static event Action CallIsBillingSupported;
        public static event Action CallLaunchUpdateOrInstallFlow;
        public static event Action CallLogin;
        public static event Action CallGetPurchase;
        public static event Action CallQueryProducts;
        public static event Action<string> CallConsume;
        public static event Action CallManageRecurringAuto;

        public static event Action<string> ApiSuccessEvent;
        public static event Action<PurchaseData, bool> PurchaseItemChangeEvent;
        public static event Action<ProductDetail> ProductItemChangeEvent;

        public static event Action<PurchaseData> ConsumeSuccessEvent;

        public static event Action<string, Action> ShowPopup;
        public static event Action<string, string, bool> PrintLog;

        private Action nextAction;

        void Awake ()
		{
			Onestore_IapCallbackManager.serviceConnectionSuccessEvent += serviceConnectionSuccessResult;
            Onestore_IapCallbackManager.serviceConnectionErrorEvent += serviceConnectionErrorResult;

            Onestore_IapCallbackManager.isBillingSupportedSuccessEvent += isBillingSupportedSuccessResult;
            Onestore_IapCallbackManager.isBillingSupportedErrorEvent += isBillingSupportedErrorResult;

            Onestore_IapCallbackManager.getPurchaseSuccessEvent += getPurchaseSuccessResult;
			Onestore_IapCallbackManager.getPurchaseErrorEvent += getPurchaseErrorResult;

			Onestore_IapCallbackManager.queryProductsSuccessEvent += queryProductsSuccessResult;
			Onestore_IapCallbackManager.queryProductsErrorEvent += queryProductsErrorResult;

			Onestore_IapCallbackManager.getPurchaseIntentSuccessEvent += getPurchaseIntentSuccessResult;
			Onestore_IapCallbackManager.getPurchaseIntentErrorEvent += getPurchaseIntentErrorResult;

			Onestore_IapCallbackManager.consumeSuccessEvent += consumeSuccessResult;
			Onestore_IapCallbackManager.consumeErrorEvent += consumeErrorResult;

			Onestore_IapCallbackManager.manageRecurringSuccessEvent += manageRecurringSuccessResult;
			Onestore_IapCallbackManager.manageRecurringErrorEvent += manageRecurringErrorResult;

			Onestore_IapCallbackManager.getLoginIntentSuccessEvent += getLoginIntentSuccessEvent;
            Onestore_IapCallbackManager.getLoginIntentErrorEvent += getLoginIntentErrorEvent;
        }

		void OnDestroy ()
		{
            Onestore_IapCallbackManager.serviceConnectionSuccessEvent -= serviceConnectionSuccessResult;
            Onestore_IapCallbackManager.serviceConnectionErrorEvent -= serviceConnectionErrorResult;

            Onestore_IapCallbackManager.isBillingSupportedSuccessEvent -= isBillingSupportedSuccessResult;
            Onestore_IapCallbackManager.isBillingSupportedErrorEvent -= isBillingSupportedErrorResult;

            Onestore_IapCallbackManager.getPurchaseSuccessEvent -= getPurchaseSuccessResult;
			Onestore_IapCallbackManager.getPurchaseErrorEvent -= getPurchaseErrorResult;

			Onestore_IapCallbackManager.queryProductsSuccessEvent -= queryProductsSuccessResult;
			Onestore_IapCallbackManager.queryProductsErrorEvent -= queryProductsErrorResult;

			Onestore_IapCallbackManager.getPurchaseIntentSuccessEvent -= getPurchaseIntentSuccessResult;
			Onestore_IapCallbackManager.getPurchaseIntentErrorEvent -= getPurchaseIntentErrorResult;

			Onestore_IapCallbackManager.consumeSuccessEvent -= consumeSuccessResult;
			Onestore_IapCallbackManager.consumeErrorEvent -= consumeErrorResult;

			Onestore_IapCallbackManager.manageRecurringSuccessEvent -= manageRecurringSuccessResult;
			Onestore_IapCallbackManager.manageRecurringErrorEvent -= manageRecurringErrorResult;

            Onestore_IapCallbackManager.getLoginIntentSuccessEvent -= getLoginIntentSuccessEvent;
            Onestore_IapCallbackManager.getLoginIntentErrorEvent -= getLoginIntentErrorEvent;
        }

        //서비스 Binding 성공
        void serviceConnectionSuccessResult(string result)
		{
            PrintLog("serviceConnectionSuccessResult", result, false);
            ApiSuccessEvent("Connect"); //샘플 UI에 알려줌
            CallIsBillingSupported();   //IsBillingSupported 호출
        }

        //서비스 Binding 실패
        void serviceConnectionErrorResult(string result)
        {
            if (result.Contains(Onestore_IapCallbackManager.preDefinedStrings[Onestore_IapCallbackManager.CBType.NeedUpdate]))
            {
                PrintLog("serviceConnectionErrorResult", result, false);
                //원스토어 서비스 설치 or 업데이트 요청 진행
                CallLaunchUpdateOrInstallFlow();             
            }
            else
            {
                //서비스 disconnected
                PrintLog("serviceConnectionErrorResult", result, true);
            }
        }

        //IAP API를 정상적으로 호출 할 수 있는 상태
        void isBillingSupportedSuccessResult(string result)
		{
            PrintLog("isBillingSupportedSuccessResult", result, false);
            ApiSuccessEvent("IsBillingSupported");  //샘플 UI에 알려줌

            CallGetPurchase();

            CallQueryProducts();
        }

        void isBillingSupportedErrorResult(string result)
        {
            if (result.Contains(Onestore_IapCallbackManager.preDefinedStrings[Onestore_IapCallbackManager.CBType.NeedUpdate]))
            {
                PrintLog("isBillingSupportedErrorResult", result, false);
                //원스토어 서비스 설치 or 업데이트 요청 진행
                CallLaunchUpdateOrInstallFlow();              
            }
            else if (result.Contains(Onestore_IapCallbackManager.preDefinedStrings[Onestore_IapCallbackManager.CBType.IapResult]))
            {
                string iapResultData = Onestore_IapCallbackManager.findStringAfterCBType(result, Onestore_IapCallbackManager.CBType.IapResult);
                IapResult iapResult = JsonUtility.FromJson<IapResult>(iapResultData);
                if (iapResult.code == IAP_RESULT_CODE_NEED_LOGIN)
                {
                    PrintLog("isBillingSupportedErrorResult", iapResult.ToString(), false);
                    //원스토어 로그인을 요청하거나 원스토어 로그인이 필요함을 사용자에게 알림             
                    nextAction = CallIsBillingSupported;
                    ShowPopup("need Login", CallLogin);                  
                }
                else
                {
                    //재시도 등 오류처리
                    PrintLog("isBillingSupportedErrorResult", iapResult.ToString(), true);
                }
            }
            else
            {
                //재시도 등 오류처리 
                PrintLog("isBillingSupportedErrorResult", result, true);
            }
        }


        //구매정보 조회 호출에 대한 성공 응답
        //현재 signature는 IapCallbackFromAndroid에서 받아오고 있으나 여기로 넘겨주지는 않고 있다. 일단 SDK 내부에서
        //구매 데이터와 시그너처를 통한 검증을 디폴트로 수행하고 있기 때문이다. 만약 시그너처가 필요하면 IapCallbackManager에서 가져다 쓸수 있다.
        void getPurchaseSuccessResult (PurchaseData purchaseData, string productType)
		{
            if(purchaseData == null)
            {
                //조회 성공, 구매정보 없음
                PrintLog("getPurchaseSuccessResult", "productType: " + productType + "\nno PurchaseData", false);
            }
            else
            {
                PrintLog("getPurchaseSuccessResult", "productType: " + productType + "\n" + purchaseData, false);
                PurchaseItemChangeEvent(purchaseData, true);    //샘플 UI에 알려줌
             
                //PlayerPref값을 셋팅하는것은 간단히 테스트 앱을 위한 것이지 실제로 아이템에 대한 관리나 저장은 개발사에서 해야 한다.
                PlayerPrefs.SetString(purchaseData.productId, JsonUtility.ToJson(purchaseData));

                //관리형상품(inapp)의 경우 소비를 하지 않을 경우 재구매요청을 하여도 구매가 되지 않습니다. 꼭, 소비(consume) 과정을 통하여 소모성상품 소비를 진행하여야합니다.
                //월정액상품(auto)의 경우 구매내역조회 시 recurringState 정보를 통하여 현재상태정보를 확인할 수 있습니다. -> recurringState 0(자동 결제중), 1(해지 예약중)
                if (ItemInfo.inapp_type.Equals(productType))
                {
                    //inapp 상품일 경우 소비(consume) 진행
                    //CallConsume(purchaseData.productId);
                }              
            }

            ApiSuccessEvent("GetPurchase"); //샘플 UI에 알려줌
        }

		void getPurchaseErrorResult (string result)
		{        
			//AndroidNative.showMessage ("getPurchase error", result, "ok");

            if (result.Contains(Onestore_IapCallbackManager.preDefinedStrings[Onestore_IapCallbackManager.CBType.NeedUpdate]))
            {

                PrintLog("getPurchaseErrorResult", result, false);
                //원스토어 서비스 설치 or 업데이트 요청 진행
                CallLaunchUpdateOrInstallFlow();
            }
            else if (result.Contains(Onestore_IapCallbackManager.preDefinedStrings[Onestore_IapCallbackManager.CBType.IapResult]))
            {
                string iapResultData = Onestore_IapCallbackManager.findStringAfterCBType(result, Onestore_IapCallbackManager.CBType.IapResult);
                IapResult iapResult = JsonUtility.FromJson<IapResult>(iapResultData);
                if (iapResult.code == IAP_RESULT_CODE_NEED_LOGIN)
                {
                    PrintLog("getPurchaseErrorResult", iapResult.ToString(), false);
                    //원스토어 로그인을 요청하거나 원스토어 로그인이 필요함을 사용자에게 알림 
                    nextAction = CallGetPurchase;
                    ShowPopup("need Login", CallLogin);                   
                }
                else
                {
                    //재시도 등 오류처리
                    PrintLog("getPurchaseErrorResult", iapResult.ToString(), true);
                }
            }
            else
            {
                //재시도 등 오류처리 
                PrintLog("getPurchaseErrorResult", result, true);
            }

        }

        //상품정보조회 성공
        void queryProductsSuccessResult (ProductDetail productDetail)
		{
            PrintLog("queryProductsSuccessResult", productDetail.ToString(), false);
   
            ProductItemChangeEvent(productDetail);  //샘플 UI에 알려줌
            ApiSuccessEvent("GetProductDetails");   //샘플 UI에 알려줌
        }

		void queryProductsErrorResult (string result)
		{
            if (result.Contains(Onestore_IapCallbackManager.preDefinedStrings[Onestore_IapCallbackManager.CBType.NeedUpdate]))
            {
                PrintLog("queryProductsErrorResult", result, false);
                //원스토어 서비스 설치 or 업데이트 요청 진행
                CallLaunchUpdateOrInstallFlow();              
            }
            else if (result.Contains(Onestore_IapCallbackManager.preDefinedStrings[Onestore_IapCallbackManager.CBType.IapResult]))
            {
                string iapResultData = Onestore_IapCallbackManager.findStringAfterCBType(result, Onestore_IapCallbackManager.CBType.IapResult);
                IapResult iapResult = JsonUtility.FromJson<IapResult>(iapResultData);
                if (iapResult.code == IAP_RESULT_CODE_NEED_LOGIN)
                {
                    PrintLog("queryProductsErrorResult", iapResult.ToString(), false);
                    //원스토어 로그인을 요청하거나 원스토어 로그인이 필요함을 사용자에게 알림 
                    nextAction = CallQueryProducts;
                    ShowPopup("need Login", CallLogin);                 
                }
                else
                {
                    //재시도 등 오류처리
                    PrintLog("queryProductsErrorResult", iapResult.ToString(), true);
                }
            }
            else
            {
                //재시도 등 오류처리 
                PrintLog("queryProductsErrorResult", result, true);
            }
        }

        //구매요청 성공
        void getPurchaseIntentSuccessResult (PurchaseData purchaseData)
		{
            PrintLog("getPurchaseIntentSuccessResult", purchaseData.ToString(), false);

            //PlayerPref값을 셋팅하는것은 간단히 테스트 앱을 위한 것이지 실제로 아이템에 대한 관리나 저장은 개발사에서 해야 한다.
            PlayerPrefs.SetString (purchaseData.productId, JsonUtility.ToJson (purchaseData));

            if (ItemInfo.auto_a10000.Equals(purchaseData.productId))
            {
                //Auto 상품일 경우 manageRecurringAuto를 통해 관리한다.
                CallGetPurchase();  // auto 상품 상태 확인 
            }
            else
            {
                //Inapp 상품일 경우 consume을 한 후 아이템을 지급한다.
                //CallConsume(purchaseData.productId);

                CallGetPurchase();  // Consume 안된 아이템을 보여주는 용도
            }
        }

		void getPurchaseIntentErrorResult (string result)
		{
            if (result.Contains(Onestore_IapCallbackManager.preDefinedStrings[Onestore_IapCallbackManager.CBType.NeedUpdate]))
            {
                PrintLog("getPurchaseIntentErrorResult", result, false);
                //원스토어 서비스 설치 or 업데이트 요청 진행
                CallLaunchUpdateOrInstallFlow();
            }
            else if (result.Contains(Onestore_IapCallbackManager.preDefinedStrings[Onestore_IapCallbackManager.CBType.IapResult]))
            {
                string iapResultData = Onestore_IapCallbackManager.findStringAfterCBType(result, Onestore_IapCallbackManager.CBType.IapResult);
                IapResult iapResult = JsonUtility.FromJson<IapResult>(iapResultData);
                if (iapResult.code == IAP_RESULT_CODE_NEED_LOGIN)
                {
                    PrintLog("getPurchaseIntentErrorResult", iapResult.ToString(), false);
                    //원스토어 로그인을 요청하거나 원스토어 로그인이 필요함을 사용자에게 알림    
                    nextAction = CallGetPurchase;                     ShowPopup("need Login", CallLogin);
                }
                else if (iapResult.code == IAP_RESULT_CODE_ITEM_ALREADY_OWNED)
                {
                    //이미 아이템을 소유하고 있습니다. inapp일 경우 getPurchases -> consume하고 재시도한다.
                    ShowPopup("Item already onwed. Need Consume and Try again", CallGetPurchase);
                    PrintLog("getPurchaseIntentErrorResult", iapResult.ToString(), false);
                }
                else
                {
                    //재시도 등 오류처리
                    PrintLog("getPurchaseIntentErrorResult", iapResult.ToString(), true);
                }
            }
            else
            {
                //재시도 등 오류처리 
                PrintLog("getPurchaseIntentErrorResult", result, true);
            }
        }

        //소비요청 성공
        void consumeSuccessResult (PurchaseData purchaseData)
		{
            PrintLog("consumeSuccessResult", purchaseData.ToString(), false);

            //PlayerPref값을 셋팅하는것은 간단히 테스트 앱을 위한 것이지 실제로 아이템에 대한 관리나 저장은 개발사에서 해야 한다.
            PlayerPrefs.SetString (purchaseData.productId, "");

            ConsumeSuccessEvent(purchaseData);    //샘플 UI에 알려줌
            PurchaseItemChangeEvent(purchaseData, false);   //샘플 UI에 알려줌
        }

		void consumeErrorResult (string result)
		{
            if (result.Contains(Onestore_IapCallbackManager.preDefinedStrings[Onestore_IapCallbackManager.CBType.NeedUpdate]))
            {
                PrintLog("consumeErrorResult", result, false);
                //원스토어 서비스 설치 or 업데이트 요청 진행
                CallLaunchUpdateOrInstallFlow();
            }
            else if (result.Contains(Onestore_IapCallbackManager.preDefinedStrings[Onestore_IapCallbackManager.CBType.IapResult]))
            {
                string iapResultData = Onestore_IapCallbackManager.findStringAfterCBType(result, Onestore_IapCallbackManager.CBType.IapResult);
                IapResult iapResult = JsonUtility.FromJson<IapResult>(iapResultData);
                if (iapResult.code == IAP_RESULT_CODE_NEED_LOGIN)
                {
                    PrintLog("consumeErrorResult", result, false);
                    //원스토어 로그인을 요청하거나 원스토어 로그인이 필요함을 사용자에게 알림    
                    nextAction = CallGetPurchase;
                    ShowPopup("need Login", CallLogin);
                }
                else
                {
                    //재시도 등 오류처리
                    PrintLog("consumeErrorResult", result, true);
                }
            }
            else
            {
                //재시도 등 오류처리 
                PrintLog("consumeErrorResult", result, true);
            }
        }

        //월정액 상태변경 성공
        //최신 상태는 getPurchase를 통해 확인한다.
        void manageRecurringSuccessResult (PurchaseData purchaseData)
		{
            PrintLog("manageRecurringSuccessResult", purchaseData.ToString(), false);
            CallGetPurchase();  //auto 상품의 상태를 확인
        }


		void manageRecurringErrorResult (string result)
		{
            if (result.Contains(Onestore_IapCallbackManager.preDefinedStrings[Onestore_IapCallbackManager.CBType.NeedUpdate]))
            {
                PrintLog("manageRecurringErrorResult", result, false);
                //원스토어 서비스 설치 or 업데이트 요청 진행
                CallLaunchUpdateOrInstallFlow();
            }
            else if (result.Contains(Onestore_IapCallbackManager.preDefinedStrings[Onestore_IapCallbackManager.CBType.IapResult]))
            {
                string iapResultData = Onestore_IapCallbackManager.findStringAfterCBType(result, Onestore_IapCallbackManager.CBType.IapResult);
                IapResult iapResult = JsonUtility.FromJson<IapResult>(iapResultData);
                if (iapResult.code == IAP_RESULT_CODE_NEED_LOGIN)
                {
                    PrintLog("manageRecurringErrorResult", iapResult.ToString(), false);
                    //원스토어 로그인을 요청하거나 원스토어 로그인이 필요함을 사용자에게 알림  
                    nextAction = CallManageRecurringAuto;
                    ShowPopup("need Login", CallLogin);
                }
                else
                {
                    //재시도 등 오류처리
                    PrintLog("manageRecurringErrorResult", iapResult.ToString(), true);
                }
            }
            else
            {
                //재시도 등 오류처리 
                PrintLog("manageRecurringErrorResult", result, true);
            }
        }

        //Login 성공, 상황에 맞는 다음 flow 진행
        void getLoginIntentSuccessEvent(string result)
		{
            PrintLog("getLoginIntentSuccessEvent", result, false);
            //성공, 상황에 맞는 다음 flow 진행
            nextAction?.Invoke();
            nextAction = null;
        }

        void getLoginIntentErrorEvent(string result)
        {
            if (result.Contains(Onestore_IapCallbackManager.preDefinedStrings[Onestore_IapCallbackManager.CBType.NeedUpdate]))
            {
                PrintLog("getLoginIntentErrorEvent", result, false);
                //원스토어 서비스 설치 or 업데이트 요청 진행
                CallLaunchUpdateOrInstallFlow();
            }
            else if (result.Contains(Onestore_IapCallbackManager.preDefinedStrings[Onestore_IapCallbackManager.CBType.IapResult]))
            {
                string iapResultData = Onestore_IapCallbackManager.findStringAfterCBType(result, Onestore_IapCallbackManager.CBType.IapResult);
                IapResult iapResult = JsonUtility.FromJson<IapResult>(iapResultData);

                //재시도 등 오류처리 
                PrintLog("getLoginIntentErrorEvent", iapResult.ToString(), true);
            }
            else
            {
                //재시도 등 오류처리 
                PrintLog("getLoginIntentErrorEvent", result, true);
            }
        }
    }

}