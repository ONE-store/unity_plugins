namespace OneStore.Purchasing
{
    /// <summary>
    /// Represents purchase state for a ONE Store in-app purchase data.
    /// Public documentation can be found at
    /// https://dev.onestore.co.kr/wiki/ko/doc/a-purchasedata-purchasestate-37552599.html
    /// </summary>
    public enum PurchaseState
    {
        PURCHASED = 0,
        CANCEL = 1,
        REFUND = 2,
    }
}
