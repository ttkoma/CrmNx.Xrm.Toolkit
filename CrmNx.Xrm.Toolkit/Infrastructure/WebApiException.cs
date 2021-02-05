using System;
using System.Net;

namespace CrmNx.Xrm.Toolkit.Infrastructure
{
    public class WebApiException : Exception
    {
        public HttpStatusCode StatusCode { get; set; }

        public Guid RequestId { get; set; }

        public string InnerError { get; set; }

        public WebApiException()
        {
        }

        public WebApiException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public WebApiException(string message) : base(message)
        {
        }
    }
}
