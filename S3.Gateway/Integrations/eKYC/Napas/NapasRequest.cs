using System.Reflection.PortableExecutable;
using System.Text.Json.Serialization;

namespace S3.Gateway.Integrations.Ekyc.Napas
{
    public static class RequestUtility
    {
        public static string CreateSenderReference(string platformCode)
        {
            if (string.IsNullOrWhiteSpace(platformCode) || platformCode.Length != 2)
                throw new ArgumentException("platformCode must be 2 characters.");

            DateTime now = DateTime.Now;

            string datePart = now.ToString("yyyyMMdd");

            string refId = now.ToString("HHmmssfff"); // 9 ký tự
            refId += now.Ticks.ToString()[^3..];      // lấy 3 ký tự cuối

            return $"{datePart}{platformCode}{refId}";
        }
    }

    public class NpGetDeepLinkRequest
    {
        public string SenderReference { get; set; } = string.Empty;
        public string CreationDateTime { get; set; } = string.Empty;
        public string PlatformCode { get; set; } = string.Empty;
        public string PlatformName { get; set; } = string.Empty;
        public string PlatformMerchantId { get; set; } = string.Empty;
        public string MobileOS { get; set; } = string.Empty;
    }

    public class NpCallBackStatusRequest
    {
        public string SenderReference { get; set; } = string.Empty;
        public string CreationDateTime { get; set; } = string.Empty;
        public string PlatformCode { get; set; } = string.Empty;
        public string PlatformMerchantId { get; set; } = string.Empty;
        public string MerchantName { get; set; } = string.Empty;
        public string MerchantAddress { get; set; } = string.Empty;
        public string MerchantCode { get; set; } = string.Empty;
        public string MerchantAlias { get; set; } = string.Empty;
        public string MerchantType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public List<BranchInfo> Branch { get; set; } = new();
        public string QrName { get; set; } = string.Empty;
        public string PaymentChannel { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string TaxId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ContractId { get; set; } = string.Empty;
        public string Mcc { get; set; } = string.Empty;
        public decimal Fee { get; set; }
        public decimal MasterMerchantFee { get; set; }
        public class BranchInfo
        {
            public string PlatformBranchId { get; set; } = string.Empty;
            public string BranchName { get; set; } = string.Empty;
            public string BranchCode { get; set; } = string.Empty;
            public string BranchAccount { get; set; } = string.Empty;
            public string BranchBankId { get; set; } = string.Empty;
            public string BeneficiaryName { get; set; } = string.Empty;
        }
    }

    public class NpCreateMerchantRequest
    {
        public string SenderReference { get; set; } = string.Empty;
        public string CreationDateTime { get; set; } = string.Empty;
        public string PlatformCode { get; set; } = string.Empty;
        public string PlatformMerchantId { get; set; } = string.Empty;
        public string MerchantName { get; set; } = string.Empty;
        public string MerchantAddress { get; set; } = string.Empty;
        public List<CreateMerchantBranch> Branch { get; set; } = new();
        public string ContractId { get; set; } = string.Empty;

        public class CreateMerchantBranch
        {
            public string PlatformBranchId { get; set; } = string.Empty;
            public string BranchName { get; set; } = string.Empty;
            public string QrName { get; set; } = string.Empty;
        }
    }

    public class NpUpdateMerchantRequest
    {
        public string SenderReference { get; set; } = string.Empty;

        // Format: YYYY-MMDDThh:mm:ss±hh:mm
        public string CreationDateTime { get; set; } = string.Empty;

        public string PlatformCode { get; set; } = string.Empty;

        public string PlatformMerchantId { get; set; } = string.Empty;

        public string BranchAlias { get; set; } = string.Empty;

        public string NewMerchantName { get; set; } = string.Empty;

        public string NewMerchantAddress { get; set; } = string.Empty;

        public string NewBranchName { get; set; } = string.Empty;

        public string NewQrName { get; set; } = string.Empty;

        public string NewPaymentChannel { get; set; } = string.Empty;

        public string NewWebsite { get; set; } = string.Empty;

        public string NewTaxId { get; set; } = string.Empty;

        public string NewEmail { get; set; } = string.Empty;
    }

    public class NpQueryMerchantRequest
    {
        // YYYYMMDD{platformCode}{refId}
        public string SenderReference { get; set; } = string.Empty;

        // Format: YYYY-MMDDThh:mm:ss±hh:mm
        public string CreationDateTime { get; set; } = string.Empty;

        public string PlatformCode { get; set; } = string.Empty;

        public string PlatformMerchantId { get; set; } = string.Empty;

        // Optional
        public string MerchantCode { get; set; } = string.Empty;

        // Optional
        public string MerchantAlias { get; set; } = string.Empty;
    }

    public class NpUpdateBankAccountRequest
    {
        public string SenderReference { get; set; } = string.Empty;
        public string CreationDateTime { get; set; } = string.Empty;
        public string PlatformCode { get; set; } = string.Empty;
        public string PlatformMerchantId { get; set; } = string.Empty;
        public string BranchAlias { get; set; } = string.Empty;
        public string BranchAccount { get; set; } = string.Empty;
        public string BranchBankId { get; set; } = string.Empty;
    }



    public class NpRequestBase<T>
    {
        public string Code { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public NpHeader Header { get; set; } = new();

        public T Payload { get; set; }
    }

    public class NpPaymentNotificationRequest
    {
        public string Status { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;
        public string TransDateTime { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string IssueBank { get; set; } = string.Empty;
        public string BeneficiaryBank { get; set; } = string.Empty;
        public string RealMerchantAccount { get; set; } = string.Empty;
        public string Mcc { get; set; } = string.Empty;
        public string SourceAccount { get; set; } = string.Empty;
        public string SystemTrace { get; set; } = string.Empty;
        public string LocalTime { get; set; } = string.Empty;
        public string LocalDate { get; set; } = string.Empty;
        public string TerminalId { get; set; } = string.Empty;
        public string RefId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public NpSettlementInfo SettlementInfo { get; set; } = new();
        public string CaseId { get; set; } = string.Empty;
        public string CreationDateTime { get; set; } = string.Empty;
    }

    public class NpSettlementInfo
    {
        public string SettlementDate { get; set; } = string.Empty;
        public int SettlementCode { get; set; }
    }

    public class NpHeader
    {
        public string MessageIdentifier { get; set; } = string.Empty;
        public string CreationDateTime { get; set; } = string.Empty;
        public string SenderReference { get; set; } = string.Empty;
        public string SenderId { get; set; } = string.Empty;
        public string ReceiverId { get; set; } = string.Empty;
        public string Signature { get; set; } = string.Empty;
    }
}