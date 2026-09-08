namespace S3.Gateway.Integrations.Payment.Napas
{
    public static class Constants
    {
        public static string CONST_KEY_GRANT_TYPE = "grant_type";
        public static string CONST_KEY_CLIENT_ID = "client_id";
        public static string CONST_KEY_CLIENT_SECRET = "client_secret";

        public static string SERVICE_ID_PAYMENT_NOTIFICATION = "01";
        public static string SERVICE_ID_INVESTIGATION = "02";
        public static string SERVICE_ID_RECONCILIATIONREPORT = "03";
        public static string SERVICE_ID_REFUND = "04";


        public static string MESSAGE_IDENTIFIER_INVEST_REQUEST = "investrequest";
        public static string MESSAGE_IDENTIFIER_REFUND_REQUEST = "refundrequest";
    }
}
