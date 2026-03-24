using Android.BillingClient.Api;
using System.Collections.Generic;

namespace Facesofnaija.PaymentGoogle
{
    public static class InAppBillingGoogle
    {
        public static readonly string ProductId = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAkECSEiZqEnB3EE/sIJOJmrpSl6Vc33EoTdXrnOqUWR4ir2dzxfV1l35kuiF7mG3/Hpu7qOU8fZ4Bu77LVihdHv+pPlidADx0Snra+6yTp6za+Tk/VK75NTp5edpHg6vTUOGPOlCapPopvnHJlJVIw3HNu9Sj4UWETadr6c7m91TYClAXWB4nYUDBAEFswUFYVRRTPuYWe4y/7MJAhHFnh3svaIlzaCX5X2vL95xjepCsWjNBqXynMwV0spnUu0Mpbc6IXKIuZpexYDF7CkHwQrREhmGIcLSysXZmAjvjGV9MYKpJCBItOCjnNnTX9rRpoGVrMRQpifbBoZYqrGXhiwIDAQAB";

        public const string Donation5 = "donation5";
        public const string Donation10 = "donation10";
        public const string Donation15 = "donation15";
        public const string Donation20 = "donation20";
        public const string Donation25 = "donation25";
        public const string Donation30 = "donation30";
        public const string Donation35 = "donation35";
        public const string Donation40 = "donation40";
        public const string Donation45 = "donation45";
        public const string Donation50 = "donation50";
        public const string Donation55 = "donation55";
        public const string Donation60 = "donation60";
        public const string Donation65 = "donation65";
        public const string Donation70 = "donation70";
        public const string Donation75 = "donation75";
        public const string DonationDefault = "donationdefulte";
        public const string MembershipStar = "membership_star";
        public const string MembershipHot = "membership_hot";
        public const string MembershipUltima = "membership_ultima";
        public const string MembershipVip = "membership_vip";

        public static readonly List<QueryProductDetailsParams.Product> ListProductSku = new List<QueryProductDetailsParams.Product> // ID Product
        {
            //All products should be of the same product type.
            QueryProductDetailsParams.Product.NewBuilder().SetProductId(Donation5).SetProductType(BillingClient.ProductType.Inapp).Build(),
            QueryProductDetailsParams.Product.NewBuilder().SetProductId(Donation10).SetProductType(BillingClient.ProductType.Inapp).Build(),
            QueryProductDetailsParams.Product.NewBuilder().SetProductId(Donation15).SetProductType(BillingClient.ProductType.Inapp).Build(),
            QueryProductDetailsParams.Product.NewBuilder().SetProductId(Donation20).SetProductType(BillingClient.ProductType.Inapp).Build(),
            QueryProductDetailsParams.Product.NewBuilder().SetProductId(Donation25).SetProductType(BillingClient.ProductType.Inapp).Build(),
            QueryProductDetailsParams.Product.NewBuilder().SetProductId(Donation30).SetProductType(BillingClient.ProductType.Inapp).Build(),
            QueryProductDetailsParams.Product.NewBuilder().SetProductId(Donation35).SetProductType(BillingClient.ProductType.Inapp).Build(),
            QueryProductDetailsParams.Product.NewBuilder().SetProductId(Donation40).SetProductType(BillingClient.ProductType.Inapp).Build(),
            QueryProductDetailsParams.Product.NewBuilder().SetProductId(Donation45).SetProductType(BillingClient.ProductType.Inapp).Build(),
            QueryProductDetailsParams.Product.NewBuilder().SetProductId(Donation50).SetProductType(BillingClient.ProductType.Inapp).Build(),
            QueryProductDetailsParams.Product.NewBuilder().SetProductId(Donation55).SetProductType(BillingClient.ProductType.Inapp).Build(),
            QueryProductDetailsParams.Product.NewBuilder().SetProductId(Donation60).SetProductType(BillingClient.ProductType.Inapp).Build(),
            QueryProductDetailsParams.Product.NewBuilder().SetProductId(Donation65).SetProductType(BillingClient.ProductType.Inapp).Build(),
            QueryProductDetailsParams.Product.NewBuilder().SetProductId(Donation70).SetProductType(BillingClient.ProductType.Inapp).Build(),
            QueryProductDetailsParams.Product.NewBuilder().SetProductId(Donation75).SetProductType(BillingClient.ProductType.Inapp).Build(),
            QueryProductDetailsParams.Product.NewBuilder().SetProductId(DonationDefault).SetProductType(BillingClient.ProductType.Inapp).Build(),
            QueryProductDetailsParams.Product.NewBuilder().SetProductId(MembershipStar).SetProductType(BillingClient.ProductType.Inapp).Build(),
            QueryProductDetailsParams.Product.NewBuilder().SetProductId(MembershipHot).SetProductType(BillingClient.ProductType.Inapp).Build(),
            QueryProductDetailsParams.Product.NewBuilder().SetProductId(MembershipUltima).SetProductType(BillingClient.ProductType.Inapp).Build(),
            QueryProductDetailsParams.Product.NewBuilder().SetProductId(MembershipVip).SetProductType(BillingClient.ProductType.Inapp).Build(),
        };
    }
}