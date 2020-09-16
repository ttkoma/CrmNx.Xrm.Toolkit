using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TestFramework
{
    public class MockedHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<Task<HttpResponseMessage>> _responseFactory;
        private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _responseAtRequestFactory;

        private readonly HttpResponseMessage _httpResponseMessage;

        public MockedHttpMessageHandler(HttpResponseMessage httpResponseMessage)
        {
            _httpResponseMessage = httpResponseMessage;
        }

        public MockedHttpMessageHandler(Func<Task<HttpResponseMessage>> responseFactory)
        {
            _responseFactory = responseFactory ?? throw new ArgumentNullException(nameof(responseFactory));
        }

        public MockedHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> responseAtRequestFactory)
        {
            _responseAtRequestFactory = responseAtRequestFactory ?? throw new ArgumentNullException(nameof(responseAtRequestFactory));
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (_responseFactory != null)
            {
                return await _responseFactory.Invoke();
            }
            else if (_responseAtRequestFactory != null)
            {
                return await _responseAtRequestFactory.Invoke(request);
            }

            return _httpResponseMessage;
        }
    }
}