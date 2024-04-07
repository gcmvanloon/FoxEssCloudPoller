namespace FoxEssCloudPoller
{
    internal class CustomHttpClient : IDisposable
    {
        private readonly HttpClient _client;

        public delegate void RequestEventHandler(string url, HttpContent content);

        public event RequestEventHandler? BeforeSend;

        public CustomHttpClient()
        {
            _client = new HttpClient();
        }

        public async Task<HttpResponseMessage> PostAsync(string url, HttpContent content)
        {
            BeforeSend?.Invoke(url, content);
            return await _client.PostAsync(url, content);
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}