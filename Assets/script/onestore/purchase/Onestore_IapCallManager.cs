using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OneStore
{
	public class Onestore_IapCallManager
	{
		private static AndroidJavaObject iapRequestAdapter = null;
		private static AndroidJavaClass jc = null;
		private static bool isServiceCreated = false;
		private const int IAP_API_VERSION = 5;
        private const int IAP_PLUGIN_VERSION = 1;

        static void setServiceAvailable ()
		{
			isServiceCreated = true;
		}

		//서비스가 연결 및 초기화가 되지 않았을때 다른 명령어가 수행되지 않도록 체크 역할
		public static void checkServiceAvailable ()
		{
			if (isServiceCreated == false) {
				AndroidNative.showMessage ("Warning!!", "Press Init Helper to Create Service", "ok");
				throw new System.Exception ("No Service Created");
			}
		}

		//GameObject 이름이 Onestore_IapCallbackManager 안드로이드로부터 결과를 콜백받기 위해서 초기에 registerCallbackGameObject 를 통해서 등록한다.
		static Onestore_IapCallManager ()
		{
			Onestore_IapCallbackManager.serviceAvailableEvent += setServiceAvailable;       //example의 Onestore_IapCallbackManager 콜백 등록

            jc = new AndroidJavaClass("com.onestore.iap.sdk.unity.IapPlugin");
            iapRequestAdapter = jc.CallStatic<AndroidJavaObject>("initAndGet", "Onestore_IapCallbackManager", IAP_PLUGIN_VERSION);
        }

		~Onestore_IapCallManager ()
		{
			Onestore_IapCallbackManager.serviceAvailableEvent -= setServiceAvailable;       //example의 Onestore_IapCallbackManager 콜백 해지
        }

        //IapPlugin 초기화를 진행하며, 원스토어 서비스로 인앤결제를 위한 서비스 바인딩을 요청합니다.
        //초기화시에 필요한 public key는 원스토어 개발자센터에서 받아온 키를 이용하여아합니다.
        public static void connectService (string publicKey)
		{
			iapRequestAdapter.Call ("connect", publicKey);
		}

        //앱구동시 isBillingSupportedAsync API를 이용하여 인앱결제를 진행할 수 있는 상태인지 확인합니다.
        public static void isBillingSupported ()
		{
            checkServiceAvailable ();
			iapRequestAdapter.Call ("isBillingSupported", IAP_API_VERSION);
		}

        /*
         * 구매내역조회 API를 이용하여 소비되지 않는 관리형상품(inapp)과 자동결제중인 월정액상품(auto) 목록을 받아옵니다.
         * 
         * 네트워크 단절 등의 문제로, 고객은 결제를 완료하였으나 개발사에 결제정보 전달이 누락되어 인앱상품이 지급되지 않는 경우가 발생할 수 있습니다. 
         * 구매정보 가져오기(getPurchases) 및 소비(consume) 기능을 활용하여 구매가 완료되었으나 미지급된 내역이 있는지 확인 후 미지급된 상품이 지급처리될 수 있도록 프로세스를 구현하는 것을 권장합니다. 
         *         
         * 관리형상품(inapp)의 경우 소비를 하지 않을 경우 재구매요청을 하여도 구매가 되지 않습니다.      
         * 개발사에서는 소비되지 않는 관리형상품(inapp)에 대해, 애플리케이션의 적절한 life cycle 에 맞춰 구매내역조회를 진행 후 소비(consume)를 진행해야합니다.
         * 
         * 월정액상품(auto)의 경우 구매내역조회 시 recurringState 정보를 통하여 현재상태정보를 확인할 수 있습니다. -> recurringState 0(자동 결제중), 1(해지 예약중)
         * manageRecurringProduct API를 통해 해지예약요청을 할 경우 recurringState는 0 -> 1로 변경됩니다. 해지예약 취소요청을 할경우 recurringState는 1 -> 0로 변경됩니다. 
         */
        public static void getPurchases ()
		{
			checkServiceAvailable ();
			iapRequestAdapter.Call ("getPurchase", IAP_API_VERSION, "inapp");
			iapRequestAdapter.Call ("getPurchase", IAP_API_VERSION, "auto");
		}

        // 개발자센터에 등록된 상품정보를 조회합니다. 개발사에서는 상품정보 조회를 하고자 하는 상품ID를 String 배열로 전달합니다.
        public static void getProductDetails (string[] products, string productType)
		{
			checkServiceAvailable ();
			iapRequestAdapter.Call ("getProductDetails", new object[]{ IAP_API_VERSION, products, productType }); 
		}

        /*
         * 구매요청을 진행합니다.
         *        
         * payload: 
         * 이 Payload 값은 결제 완료 이후에 응답 값에 다시 전달 받게 되며 결제 요청 시의 값과 차이가 있다면 구매 요청에 변조가 있다고 판단 하면 됩니다.        
         * Developer Payload 는 상품의 구매 요청 시에 개발자가 임의로 지정 할 수 있는 문자열입니다.
         * Payload 검증을 통해 Freedom 과 같은 변조 된 요청을 차단 할 수 있으며, Payload 의 발급 및 검증 프로세스를 자체 서버를 통해 이루어 지도록합니다.
         * 입력 가능한 Developer Payload는 최대 100byte까지 입니다.
         * 개발사의 규칙에 맞는 payload를 생성하여야 한다.
         *         
         * gameUserId, promotionApplicable는 일반적으로 잘 사용하지 않기 때문에 default ("" , false)  값으로 넘겨주며 사용하고자 할때 개발자가 값을 변경해서 넣으면 됩니다. 
         * 1. gameUserId: 
         * 어플리케이션을 사용 중인 사용자의 고유 인식 번호를 입력합니다. 해당 값은 프로모션 참가 가능 여부 및 프로모션 사용 여부를 판가름 하는 key value로 사용됩니다.
         * 2. promotionApplicable: 
         * 프로모션 참가 가능 여부를 전달합니다. true : gameUserId로 전달된 사용자는 단일 프로모션에 1회 참가가 가능합니다. false : gameUserId로 전달된 사용자는 프로모션에 참가가 불가능 합니다.       
         * 3. productName:
         * 값을 넣으면 결제시 해당 값으로 노출. 상품명을 공백("")으로 요청할 경우 개발자센터에 등록된 상품명을 결제화면에 노출됩니다. 구매시 지정할 경우 해당 문자열이 결제화면에 노출됩니다.
         * 
         * 관리형상품(inapp)은 구매완료 이후 consume을 진행합니다.
         * 월정액상품(auto)은 구매완료 후 manageRecurringAuto를 통해 관리합니다. 
         */
        public static void buyProduct (string productId, string productType, string payload)
        {
          checkServiceAvailable ();
          string gameUserId = "";
          bool promotionApplicable = false; 
          iapRequestAdapter.Call ("launchPurchaseFlow", IAP_API_VERSION, productId, productType, payload, gameUserId, promotionApplicable);
        }

        public static void buyProductUseName(string productId, string productName, string productType, string payload)
        {
            checkServiceAvailable();
            string gameUserId = "";
            bool promotionApplicable = false;
            iapRequestAdapter.Call("launchPurchaseFlow", IAP_API_VERSION, productId, productName, productType, payload, gameUserId, promotionApplicable);
        }

        // 관리형상품(inapp)의 구매완료 이후 또는 구매내역조회 이후 소비되지 않는 관리형상품에 대해서 소비를 진행합니다.
        // 관리형상품은 구매한 상품을 소비(consume)하기 전까지는 재구매할 수 없습니다.
        public static void consume (string inapp_json)
		{
			checkServiceAvailable ();
			iapRequestAdapter.Call ("consumeItem", IAP_API_VERSION, inapp_json);
		}


        /*
         * 월정액상품(auto)의 상태변경(해지예약 / 해지예약 취소)를 진행합니다.  
         *  - auto_json : PurchaseData json data
         *  - command : "cancel" or "reactivate"
         * 월정액상품(auto)의 경우 구매내역조회 시 recurringState 정보를 통하여 현재상태정보를 확인할 수 있습니다. -> recurringState 0(자동 결제중), 1(해지 예약중)  
         * 
         * 월정액상품(auto)을 11월 10일에 구매를 할 경우 구매내역조회에서 월정액상품의 recurringState는 0(자동 결제중)으로 내려옵니다.
         * 월정액상품은 매달 구매일(12월 10일)에 자동결제가 발생하므로 11월 10일 ~ 12월 9일까지 현재 상태를 유지합니다.
         * 11월 15일에 월정액상태변경API를 이용하여 해지예약(cancel)을 진행할 경우, 12월 9일까지 월정액상품 상태(recurringState)는 1(해지 예약중)이 됩니다.
         * 12월 9일 이전에 월정액상태변경API를 이용하여 해지예약 취소(reactivate)를 진행할 경우, 해당 상품의 상태(recurringState)는 0(자동 결제중)이 됩니다.          
         */
        public static void manageRecurringAuto (string auto_json, string command)
		{
			checkServiceAvailable ();
			iapRequestAdapter.Call ("manageRecurringAuto", IAP_API_VERSION, auto_json, command); 
		}

        /*
         * 로긴이 필요로 할 경우 launchLoginFlow API를 이용하여 명시적 로그인을 수행합니다.
         * launchLoginFlow API를 호출할 경우 UI적으로 원스토어 로그인화면이 뜰 수 있습니다.
         * 개발사에서는 로그인 성공 후 적합한 다음 flow를 수행합니.
         */
        public static void login ()
		{
            Debug.Log("Onestore_IapCallManager login");
            checkServiceAvailable ();
			iapRequestAdapter.Call ("launchLoginFlow", IAP_API_VERSION);

		}

        //IapPlugin을 terminate하며, 서비스를 언바인딩을 요청합니다.
        public static void destroy ()
		{	
			iapRequestAdapter.Call ("dispose");
			isServiceCreated = false;
		}

        //원스토어 인앱결제를 이용하기 위해서는 원스토어 서비스(ONE store Service, OSS) 앱이 필요합니다.
        //원스토어 서비스 설치나 업데이트 요청을 합니다.
        public static void launchUpdateOrInstallFlow()
        {
            iapRequestAdapter.Call("launchUpdateOrInstallFlow");
        }
    }
}
