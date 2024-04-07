namespace FoxEssCloudPoller
{
    public class OpenApiException : Exception
    {
        public int ErrorCode { get; private set; }

        public OpenApiException(int code, string message) : base(message)
        {
            ErrorCode = code;
        }
    }
}