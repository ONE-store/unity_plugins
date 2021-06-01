using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

namespace OneStore
{
	[Serializable]
	public class Onestore_PurchaseResponse
	{
		public string productType;    //inapp or auto,
        //productType은 현재  getPurchase, getProductDetails와 같은 명령어를 통해서 확인할수 있고 getPurchaseIntent를 통해서는
        //현재는 알수가 없다. 단순히 "" 값이다. 굳이 필요없을 수도 있으나 개발자 편의를 위해서 getPurchase명령에서만 활용할수 있는 필드이다.

        public string purchaseData; //original purchase json
        public string signature;    //signature

        //data가 없을 경우 tatalCount와 index 모두 0이다.
        public int totalCount;  //Purchase의 총 개수로 해당 숫자만큼 콜백이 불리게 된다.
        public int index;       //tatalCount중 현재 index로 범위는 1 ~ tatalCount 이다. 

        public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append ("type: " + productType + "\n");
			sb.Append ("PurchaseData: " + purchaseData + "\n");
			sb.Append ("Signature: " + signature + "\n");
            sb.Append("index: " + index + "\n");
            sb.Append("totalCount: " + totalCount + "\n");
            return sb.ToString ();
		}
	}

	[Serializable] 
	public class Signature
	{
		public string signature;

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append (signature);
			return sb.ToString ();
		}
	}

	[Serializable]
	public class ProductDetail
	{
		public String productId;
		public String type;
		public String price;
		public String title;
        public int index;
        public int totalCount;
        public String priceCurrencyCode;


        public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append ("productId: " + productId + "\n");
			sb.Append ("type: " + type + "\n");
			sb.Append ("price: " + price + "\n");
			sb.Append ("title: " + title + "\n");
            sb.Append ("index: " + index + "\n");
            sb.Append ("totalCount: " + totalCount + "\n");
            sb.Append ("priceCurrencyCode: " + priceCurrencyCode + "\n");

            return sb.ToString ();
		}
	}

	[Serializable]
	public class PurchaseData
	{
		public string orderId;
		public string packageName;
		public string productId;
		public long purchaseTime;
		public string purchaseId;
		public string developerPayload;
		public int purchaseState;
		public int recurringState;


		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ("[Product]\n");
			sb.Append ("orderId: " + orderId + "\n");
			sb.Append ("packageName: " + packageName + "\n");
			sb.Append ("productId: " + productId + "\n");
			sb.Append ("purchaseTime: " + purchaseTime + "\n");
			sb.Append ("purchaseId: " + purchaseId + "\n");
			sb.Append ("developerPayload: " + developerPayload + "\n");
			sb.Append ("purchaseState: " + purchaseState + "\n");
			sb.Append ("recurringState: " + recurringState + "\n");

			return sb.ToString ();
		}
	}

    [Serializable]
    public class IapResult
    {
        public int code;
        public string description;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("code: " + code + "\n");
            sb.Append("description: " + description + "\n");
            return sb.ToString();
        }
    }
}


