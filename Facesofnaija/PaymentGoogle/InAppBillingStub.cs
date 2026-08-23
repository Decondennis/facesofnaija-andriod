using Android.App;
using System.Collections.Generic;

namespace InAppBilling.Lib
{
    public interface IBillingPaymentListener
    {
        void OnPaymentError(string error);
        void OnPaymentSuccess(IList<Android.BillingClient.Api.Purchase> result);
    }

    public class BillingSupport
    {
        public BillingSupport(Activity activity, string productId, string tripleDesKey, List<Android.BillingClient.Api.QueryProductDetailsParams.Product> skus, IBillingPaymentListener listener)
        {
        }

        public void Destroy()
        {
        }

        public void PurchaseNow(string sku)
        {
        }
    }
}

namespace Android.BillingClient.Api
{
    public class Purchase
    {
        public string OrderId { get; set; }
        public string PackageName { get; set; }
        public string PurchaseToken { get; set; }
    }

    public static class BillingClient
    {
        public static class ProductType
        {
            public const string Inapp = "inapp";
            public const string Subs = "subs";
        }
    }

    public class QueryProductDetailsParams
    {
        public class Product
        {
            public string ProductId { get; set; }
            public string ProductType { get; set; }

            public static Builder NewBuilder() => new Builder();

            public class Builder
            {
                private readonly Product _product = new Product();
                public Builder SetProductId(string id) { _product.ProductId = id; return this; }
                public Builder SetProductType(string type) { _product.ProductType = type; return this; }
                public Product Build() => _product;
            }
        }
    }
}
