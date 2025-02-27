using System;
using System.Linq;
using System.Net.Http;

namespace CrmNx.Xrm.Toolkit.Extensions;

public static class HttpResponseMessageExtensions
{
    public static T As<T>(this HttpResponseMessage response) where T : HttpResponseMessage, new()
    {
        T? typedResponse = (T)Activator.CreateInstance(typeof(T));
        
        typedResponse.StatusCode = response.StatusCode;
        response.Headers.ToList().ForEach(h => typedResponse.Headers.TryAddWithoutValidation(h.Key, h.Value));
        typedResponse.Content = response.Content;
        
        return typedResponse;
    }
}