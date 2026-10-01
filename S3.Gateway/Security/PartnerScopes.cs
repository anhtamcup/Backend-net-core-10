namespace S3.Gateway.Security
{
    public static class PartnerScopes
    {
        public const string PaymentQrCreate = "payment.qr.create";
        public const string PaymentInvestigation = "payment.investigation";
        public const string PaymentRefund = "payment.refund";
        public const string PaymentHistory = "payment.history";
        public const string EkycDeepLinkCreate = "ekyc.deeplink.create";
        public const string EkycMerchantCreate = "ekyc.merchant.create";
        public const string EkycMerchantUpdate = "ekyc.merchant.update";
        public const string EkycMerchantQuery = "ekyc.merchant.query";

        public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
        {
            PaymentQrCreate, PaymentInvestigation, PaymentRefund, PaymentHistory, EkycDeepLinkCreate,
            EkycMerchantCreate, EkycMerchantUpdate, EkycMerchantQuery
        };
    }
}
