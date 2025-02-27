using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CrmNx.Xrm.Toolkit.Infrastructure.Batch;

public class BatchResponse : HttpResponseMessage
{
    public List<HttpResponseMessage> HttpResponseMessages
    {
        get
        {
            List<HttpResponseMessage> httpResponseMessages = new List<HttpResponseMessage>();

            if (Content != null)
            {
                httpResponseMessages.AddRange(collection: ParseMultipartContent(Content).GetAwaiter().GetResult());
            }

            return httpResponseMessages;
        }
    }

    private static async Task<List<HttpResponseMessage>> ParseMultipartContent(HttpContent content)
    {
        MultipartMemoryStreamProvider batchResponseContent = await content.ReadAsMultipartAsync();
        List<HttpResponseMessage> responses = new List<HttpResponseMessage>();

        Exception? firstParseException = null;

        batchResponseContent?.Contents?.ToList().ForEach(async void (httpContent) =>
        {
            try
            {
                // This is true for changeset
                if (httpContent.IsMimeMultipartContent())
                {
                    // Recursive call
                    responses.AddRange(await ParseMultipartContent(httpContent));
                }
                else
                {
                    httpContent.Headers.Remove("Content-Type");
                    httpContent.Headers.Add("Content-Type", "application/http;msgtype=response");

                    HttpResponseMessage responseMessage = await httpContent.ReadAsHttpResponseMessageAsync();
                    if (responseMessage != null)
                    {
                        responses.Add(responseMessage);
                    }
                }
            }
            catch (Exception e)
            {
                firstParseException ??= e;
            }
        });

        if (firstParseException is not null)
            throw firstParseException;

        return responses;
    }
}