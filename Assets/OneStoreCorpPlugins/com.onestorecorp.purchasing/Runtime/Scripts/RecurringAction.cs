namespace OneStore.Purchasing
{
    /// <summary>
    /// The value to change the status of the product being paid monthly.
    /// Public documentation can be found at
    /// https://dev.onestore.co.kr/wiki/ko/doc/a-purchaseclient-recurringaction-37552596.html
    /// </summary>
    public sealed class RecurringAction
    {
        private readonly string _description;

        public static readonly RecurringAction CANCEL = new RecurringAction("cancel");
        public static readonly RecurringAction REACTIVATE = new RecurringAction("reactivate");

        private RecurringAction(string description)
        {
            _description = description;
        }

        public override string ToString()
        {
            return _description;
        }

        public static RecurringAction Get(string action)
        {
            if (CANCEL.ToString().Equals(action))
                return CANCEL;
            else if (REACTIVATE.ToString().Equals(action))
                return REACTIVATE;
            else    
                return null;
        }
    }
}
