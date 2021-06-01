﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OneStore;
using System;

/*
 loadLoginFlow
 manageRecurringAuto
 buyProduct
 consumeItem
 getPurchase
 isBillingSupported
 getProductDetails
 connect
 */
public class Onestore_IapCallbackManager : MonoBehaviour
{
	public static event Action serviceAvailableEvent;

	public static event Action<string> serviceConnectionSuccessEvent;
    public static event Action<string> serviceConnectionErrorEvent;
    //delegate선언을 안하기 위해서
    public static event Action<string> isBillingSupportedSuccessEvent;
    public static event Action<string> isBillingSupportedErrorEvent;

    public static event Action<PurchaseData, string> getPurchaseSuccessEvent;
	public static event Action<string> getPurchaseErrorEvent;

	public static event Action<ProductDetail> queryProductsSuccessEvent;
	public static event Action<string> queryProductsErrorEvent;

	public static event Action<PurchaseData> getPurchaseIntentSuccessEvent;
	public static event Action<string> getPurchaseIntentErrorEvent;

	public static event Action<PurchaseData> consumeSuccessEvent;
	public static event Action<string> consumeErrorEvent;

	public static event Action<PurchaseData> manageRecurringSuccessEvent;
	public static event Action<string> manageRecurringErrorEvent;

	public static event Action<string> getLoginIntentSuccessEvent;
    public static event Action<string> getLoginIntentErrorEvent;

    public enum CBType
	{
		Connected,
		Disconnected,
		NeedUpdate,
		Success,
		Error,
		RemoteEx,
		SecurityEx,
        IapResult,
    };

    public static Dictionary<CBType, string> preDefinedStrings = new Dictionary<CBType, string> () {
		
		{ CBType.Connected, "onConnected" },
		{ CBType.Disconnected, "onDisconnected" },
		{ CBType.NeedUpdate, "onErrorNeedUpdateException" },
		{ CBType.Success, "onSuccess" },
		{ CBType.Error, "onError" },
		{ CBType.RemoteEx, "onErrorRemoteException" },
		{ CBType.SecurityEx, "onErrorSecurityException" },
        { CBType.IapResult, "onErrorIapResult" }

    };

    void Start ()
	{

	}

	/*
	- descritpion:  connect의 결과를 제공하기 위한 콜백  , 결과값은 1~3 중의 하나의 string으로 받는다.
	- callback 종류:
	1. onConnected: 서비스 Binding 성공
	2. onDisconnected : 서비스 Un-binding 되었을때
	3. onErrorNeedUpdateException: 원스토어 서비스가 최신버전이 아닐 경우 또는 미설치 시에 발생, launchUpdateOrInstallFlow메서드를 이용하여 사용자에세 설치를 유도하거나 개발사에서 직접 처리
	*/
	public void ServiceConnectionListener (string callback)
	{
		if (callback.Contains (preDefinedStrings [CBType.Connected])) 
        {
			serviceAvailableEvent ();
            serviceConnectionSuccessEvent(callback);
        }
        else
        {
            serviceConnectionErrorEvent(callback);
        }
	}

    /*
	 - descritpion: In-app Purchase v17 지원여부를 확인에 대한 응답. 개발사 시나리오에 맞는 에러처리를 진행합니다.
	 - callback 종류:
	1. onSuccess: 상태정보 조회가 성공일 경우
    2. onErrorIapResult: 실패일 경우 애플리케이션으로 에러코드 전달. 숫자로 된  code와 string으로 된 description을 넘겨준다. 
    3. onErrorRemoteException: bind 요청시 에러가 발생하거나 서비스 Un-bind 시점에 Remote call 을 요청할 경우 발생
    4. onErrorSecurityException: 애플리케이션이 변조되거나 정상적이지 않는 APK 형태 일때 발생
	5. onErrorNeedUpdateException: 원스토어 서비스가 최신버전이 아닐 경우 또는 미설치 시에 발생, launchUpdateOrInstallFlow메서드를 이용하여 사용자에세 설치를 유도하거나 개발사에서 직접 처리
	*/
    public void BillingSupportedListener (string callback)
	{
        if (callback.Contains(preDefinedStrings[CBType.Success]))
        {
            //성공 
            isBillingSupportedSuccessEvent(callback);
        }
        else
        {
            isBillingSupportedErrorEvent(callback);
        }
    }

    /*
	 - descritpion: IAP v17 구매 내역 조회를 위한 메서드에 대한 응답
	   관리형상품(inapp)의 경우 소비를 하지 않을 경우 재구매요청을 하여도 구매가 되지 않습니다. 꼭, 소비(consume) 과정을 통하여 소모성상품 소비를 진행하여야합니다.
       월정액상품(auto)의 경우 구매내역조회 시 recurringState 정보를 통하여 현재상태정보를 확인할 수 있습니다. -> recurringState 0(자동 결제중), 1(해지 예약중)    
	 - callback 종류:
	 1. onSuccess:  구매정보 조회 호출에 대한 성공 응답
        - productType : inapp or auto인지 상품타입
        - purchaseData : 구매한 원본 json 구매 데이터 
        - signature:  구매한 시그너처
        - totalCount : 조회한 구매내역의 전체 개수(0일 경우 구매한(소비 안한) 데이터가 없음)
        - index : 조회한 구매내역 중 현재 순서
	 2. onErrorRemoteException
	 3. onErrorSecurityException
	 4. onErrorNeedUpdateException
	 5. onErrorIapResult: 실패일 경우 애플리케이션으로 에러코드 전달. 숫자로 된 code와 string으로 된 description을 넘겨준다. 
	*/
    public void QueryPurchaseListener (string callback)
	{
        if (callback.Contains(preDefinedStrings[CBType.Success]))
        {
            //성공
            string data = findStringAfterCBType(callback, CBType.Success);
            if (data.Length > 0)
            {
                try
                {
                    Onestore_PurchaseResponse purchaseRes = JsonUtility.FromJson<Onestore_PurchaseResponse>(data);
                    if (purchaseRes.totalCount > 0)
                    {
                        PurchaseData purchaseData = JsonUtility.FromJson<PurchaseData>(purchaseRes.purchaseData);
                        getPurchaseSuccessEvent(purchaseData, purchaseRes.productType);
                    }
                    else
                    {
                        getPurchaseSuccessEvent(null, purchaseRes.productType); //success but no data 
                    }

                }
                catch (System.Exception ex)
                {
                    getPurchaseErrorEvent(ex.Message);
                }
            }
            else
            {
                getPurchaseErrorEvent(callback);
            }
        }
        else
        {
            getPurchaseErrorEvent(callback);
        }
	}

    /*
	 - descritpion: IAP v17 상품 정보 조회를 위한 메서드에 대한 응답
	 - callback 종류:
	 1. onSuccess:  상품정보 조회 호출에 대한 성공 응답 , json 형태 
	    - 상품정보: productId, type, price, title
        - totalCount : 조회한 상품정보의 전체 개수
        - index : 조회한 상품정보 중 현재 순서    
	 2. onErrorRemoteException
	 3. onErrorSecurityException
	 4. onErrorNeedUpdateException
	 5. onErrorIapResult: 실패일 경우 애플리케이션으로 에러코드 전달. 숫자로 된  code와 string으로 된 description을 넘겨준다.
	*/
    public void QueryProductsListener (string callback)
	{
        if (callback.Contains(preDefinedStrings[CBType.Success]))         {
            //성공
            string data = findStringAfterCBType(callback, CBType.Success);
            if (data.Length > 0)
            {
                ProductDetail productDetail = JsonUtility.FromJson<ProductDetail>(data);
                queryProductsSuccessEvent(productDetail);
            }
            else
            {
                queryProductsErrorEvent(callback);
            }
        }
        else
        {
            queryProductsErrorEvent(callback);
        }
	}

    /*
	 - descritpion: IAP v17 구매요청을 위한 메서드에 대한 응답
	 - callback 종류:
	 1. onSuccess:  구매요청에 대한 성공 응답
	 2. onErrorRemoteException: bind 요청시 에러가 발생하거나 서비스 Un-bind 시점에 Remote call 을 요청할 경우 발생
     3. onErrorSecurityException: 애플리케이션이 변조되거나 정상적이지 않는 APK 형태 일때 발생
	 4. onErrorNeedUpdateException : 원스토어 서비스가 최신버전이 아닐 경우 또는 미설치 시에 발생, launchUpdateOrInstallFlow메서드를 이용하여 사용자에세 설치를 유도하거나 개발사에서 직접 처리
	 5. onErrorIapResult: 실패일 경우 애플리케이션으로 에러코드 전달. 숫자로 된  code와 string으로 된 description을 넘겨준다.
	*/
    public void PurchaseFlowListener (string callback)
	{
        if (callback.Contains(preDefinedStrings[CBType.Success]))
        {
            //성공
            string data = findStringAfterCBType(callback, CBType.Success);
            if (data.Length > 0)
            {
                Onestore_PurchaseResponse purchaseRes = JsonUtility.FromJson<Onestore_PurchaseResponse>(data);
                PurchaseData purchaseData = JsonUtility.FromJson<PurchaseData>(purchaseRes.purchaseData);
                getPurchaseIntentSuccessEvent(purchaseData);
            }
            else
            {
                getPurchaseIntentErrorEvent(callback);
            }
        }
        else
        {
            getPurchaseIntentErrorEvent(callback);
        }
    }

    /*
	 - descritpion: IAP v17 상품소비 호출 메서드에 대한 응답
	 - callback 종류:
	 1. onSuccess:  소비요청에 대한 성공 응답
	 2. onErrorRemoteException
	 3. onErrorSecurityException
	 4. onErrorNeedUpdateException
	 5. onErrorIapResult: 실패일 경우 애플리케이션으로 에러코드 전달. 숫자로 된  code와 string으로 된 description을 넘겨준다.
	*/
    public void ConsumeListener (string callback)
	{
        if (callback.Contains(preDefinedStrings[CBType.Success]))         {
            //성공
            string data = findStringAfterCBType(callback, CBType.Success);
            if (data.Length > 0)
            {
                PurchaseData purchaseData = JsonUtility.FromJson<PurchaseData>(data);
                consumeSuccessEvent(purchaseData);
            }
            else
            {
                consumeErrorEvent(callback);
            }
        }
        else
        {
            consumeErrorEvent(callback);
        }
	}

    /*
     - descritpion: 월정액상품(auto)의 상태변경(해지예약 / 해지예약 취소)를 진행
	 - callback 종류:
	 1. onSuccess:  월정액 상태변경 호출에 대한 성공 응답
	 2. onErrorRemoteException
	 3. onErrorSecurityException
	 4. onErrorNeedUpdateException
	 5. onErrorIapResult: 실패일 경우 애플리케이션으로 에러코드 전달. 숫자로 된  code와 string으로 된 description을 넘겨준다.
     */
    public void ManageRecurringProductListener (string callback)
	{
        if (callback.Contains(preDefinedStrings[CBType.Success]))         {
            //성공
            string data = findStringAfterCBType(callback, CBType.Success);
            if (data.Length > 0)
            {
                PurchaseData response = JsonUtility.FromJson<PurchaseData>(data);
                manageRecurringSuccessEvent(response);
            }
            else
            {
                manageRecurringErrorEvent(callback);
            }
        }
        else
        {
            manageRecurringErrorEvent(callback);
        }
    }

    /*
     - descritpion: IAP v17 원스토어 로그인요청을 위한 메서드
	 - callback 종류:
	 1. onSuccess:  성공일 경우
	 2. onErrorRemoteException
	 3. onErrorSecurityException
	 4. onErrorNeedUpdateException
	 5. onErrorIapResult: 실패일 경우 애플리케이션으로 에러코드 전달. 숫자로 된  code와 string으로 된 description을 넘겨준다.
     */
    public void LoginFlowListener (string callback)
	{
        if (callback.Contains(preDefinedStrings[CBType.Success]))         {
            //성공, 상황에 맞는 다음 flow 진행
            getLoginIntentSuccessEvent(callback);
        }
        else
        {
            getLoginIntentErrorEvent(callback);
        }
    }

	// 결과 callback string에서 CBType  string을 제외한 다음 문자열을 돌려준다.
	public static string findStringAfterCBType (string data, CBType type)
	{
		int length = preDefinedStrings [type].Length;
		if (data.Substring (0, length).Equals (preDefinedStrings [type])) {
			return data.Substring (length);
		} else {
			return "";
		}
	}
}
