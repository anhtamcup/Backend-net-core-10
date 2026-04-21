namespace S3.Gateway.Features
{
    public class ResponseBase<T>
    {
        public bool IsSuccess { get; set; }

        public string Message { get; set; } = string.Empty;

        public T? Data { get; set; }
    }
}
