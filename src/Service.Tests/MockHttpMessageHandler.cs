using System.Net;

namespace Service.Tests
{
    public class MockHttpMessageHandler(string response, HttpStatusCode statusCode) : HttpMessageHandler
    {
        private readonly string _response = response;
        private readonly HttpStatusCode _statusCode = statusCode;

        public string RequestContent { get; private set; } = string.Empty;
        public int CallsCount { get; private set; } = 0;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CallsCount++;
            if (request.Content != null) // Could be a GET-request without a body
            {
                RequestContent = await request.Content.ReadAsStringAsync();
            }
            return new HttpResponseMessage
            {
                StatusCode = _statusCode,
                Content = new StringContent(_response)
            };
        }
    }
}
