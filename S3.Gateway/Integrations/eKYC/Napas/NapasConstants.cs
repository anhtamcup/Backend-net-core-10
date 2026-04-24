namespace S3.Gateway.Integrations.eKYC.Napas
{
    public class NapasConstants
    {
        public static string CONST_KEY_GRANT_TYPE = "grant_type";
        public static string CONST_KEY_CLIENT_ID = "client_id";
        public static string CONST_KEY_CLIENT_SECRET = "client_secret";
    }

    public class ApiErrorCode
    {
        public int HttpCode { get; }
        public string Code { get; }
        public string Description { get; }
        public string Details { get; }

        private ApiErrorCode(int httpCode, string code, string description, string details)
        {
            HttpCode = httpCode;
            Code = code;
            Description = description;
            Details = details;
        }

        public static readonly ApiErrorCode SUCCESS =
            new(200, "00", "SUCCESS", "Thành công");

        public static readonly ApiErrorCode INVALID_PLATFORM =
            new(200, "01", "INVALID_PLATFORM", "ĐVPTML không tồn tại");

        public static readonly ApiErrorCode INVALID_MOBILE_OS =
            new(200, "02", "INVALID_MOBILE_OS", "Nền tảng mobile chưa được hỗ trợ");

        public static readonly ApiErrorCode INVALID_ALIAS =
            new(200, "03", "INVALID_ALIAS", "ĐVCNTT không tồn tại");

        public static readonly ApiErrorCode INVALID_CONTRACT =
            new(200, "04", "INVALID_CONTRACT", "Mã hợp đồng không tồn tại");

        public static readonly ApiErrorCode INVALID_PLATFORM_MERCHANT =
            new(200, "05", "INVALID_PLATFORM_MERCHANT",
                "Không xác định được mã định danh của ĐVCNTT được sinh ra bởi hệ thống ĐVPTML");

        public static readonly ApiErrorCode PLATFORM_MERCHANT_EXISTED =
            new(200, "06", "PLATFORM_MERCHANT_EXISTED",
                "ĐVCNTT được sinh ra bởi hệ thống ĐVPTML đã được xác minh trên hệ thống Napas");

        public static readonly ApiErrorCode OTHER_ERRORS =
            new(200, "89", "OTHER_ERRORS", "Lỗi khác");

        public static readonly ApiErrorCode UNAUTHORIZED =
            new(401, "90", "UNAUTHORIZED", "Thông tin xác thực không hợp lệ");

        public static readonly ApiErrorCode MISSING_REQUIRED_FIELD =
            new(400, "91", "MISSING_REQUIRED_FIELD", "Truyền thiếu tham số bắt buộc");

        public static readonly ApiErrorCode INVALID_PARAMETER =
            new(400, "92", "INVALID_PARAMETER", "Dữ liệu đầu vào không hợp lệ");

        public static readonly ApiErrorCode SYSTEM_ERROR =
            new(500, "96", "SYSTEM_ERROR", "Có lỗi hệ thống trong quá trình xử lý");
    }
}
