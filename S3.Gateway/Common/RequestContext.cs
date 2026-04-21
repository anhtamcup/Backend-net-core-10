namespace S3.Gateway.Common
{
    public static class RequestContext
    {
        private static readonly AsyncLocal<string> _requestId = new();

        public static string RequestID
        {
            get
            {
                if (string.IsNullOrEmpty(_requestId.Value))
                {
                    _requestId.Value = Guid.NewGuid().ToString();
                }

                return _requestId.Value;
            }
            set
            {
                _requestId.Value = value;
            }
        }
    }
}
