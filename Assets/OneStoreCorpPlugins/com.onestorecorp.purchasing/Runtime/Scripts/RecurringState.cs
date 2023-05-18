namespace OneStore.Purchasing
{
    /// <summary>
    /// Represents recurring state for a ONE Store in-app purchase data.
    /// Public documentation can be found at
    /// https://dev.onestore.co.kr/wiki/ko/doc/a-purchasedata-recurringstate-37552600.html
    /// </summary>
    public enum RecurringState
    {
        RECURRING = 0,
        CANCEL = 1,
        NON_AUTO_PRODUCT = -1,
    }
}
