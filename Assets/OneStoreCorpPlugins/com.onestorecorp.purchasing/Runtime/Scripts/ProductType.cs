namespace OneStore.Purchasing
{
    /// <summary>
    /// Product type supported by ONE Store.
    /// Public documentation can be found at
    /// https://dev.onestore.co.kr/wiki/ko/doc/a-purchaseclient-producttype-37552595.html
    /// </summary>
    public sealed class ProductType
    {
        private readonly string _description;

        /// <summary>
        /// This type is used available only in the QueryProductDetails() method.
        /// </summary>
        public static readonly ProductType ALL = new ProductType("all");
        public static readonly ProductType INAPP = new ProductType("inapp");
        public static readonly ProductType AUTO = new ProductType("auto");
        public static readonly ProductType SUBS = new ProductType("subscription");

        private ProductType(string description)
        {
            _description = description;
        }

        public override string ToString()
        {
            return _description;
        }

        public static ProductType Get(string type)
        {
            if (ALL.ToString().Equals(type))
                return ALL;
            else if (INAPP.ToString().Equals(type))
                return INAPP;
            else if (AUTO.ToString().Equals(type))
                return AUTO;
            else if (SUBS.ToString().Equals(type))
                return SUBS;
            else    
                return null;
        }
    }
}
