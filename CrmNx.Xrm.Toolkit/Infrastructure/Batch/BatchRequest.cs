using System;
using System.Collections.Generic;
using System.Net.Http;

namespace CrmNx.Xrm.Toolkit.Infrastructure.Batch;

public class BatchRequest : HttpRequestMessage
{
    public BatchRequest(Uri serviceBaseAddress)
    {
        Method = HttpMethod.Post;
        RequestUri = new Uri("$batch", UriKind.Relative);
        Content = new MultipartContent("mixed", $"batch_{Guid.NewGuid()}");
        _serviceBaseAddress = serviceBaseAddress;
    }

    private readonly Uri _serviceBaseAddress;
    private bool _continueOnError;

    public bool ContinueOnError
    {
        get => _continueOnError;
        set
        {
            if (_continueOnError != value)
            {
                if (value)
                {
                    Headers.Add("Prefer", "odata.continue-on-error");
                }
                else
                {
                    Headers.Remove("Prefer");
                }
            }

            _continueOnError = value;
        }
    }

    public List<ChangeSet> ChangeSets
    {
        set
        {
            value.ForEach(changeSet =>
            {
                MultipartContent content = new MultipartContent("mixed", $"changeset_{Guid.NewGuid()}");
                int count = 1;
                changeSet.Requests.ForEach(req =>
                {
                    HttpMessageContent messageContent = ToHttpMessageContent(req);
                    messageContent.Headers.Add("Content-ID", count.ToString());

                    content.Add(messageContent);

                    count++;
                });

                ((MultipartContent)Content).Add(content);
            });
        }
    }

    public List<HttpRequestMessage> Requests
    {
        set { value.ForEach(request => { ((MultipartContent)Content).Add(ToHttpMessageContent(request)); }); }
    }

    private HttpMessageContent ToHttpMessageContent(HttpRequestMessage request)
    {
        request.RequestUri = new Uri(
            baseUri: _serviceBaseAddress,
            relativeUri: request.RequestUri.ToString()
        );

        if (request.Content != null)
        {
            if (request.Content.Headers.Contains("Content-Type"))
            {
                request.Content.Headers.Remove("Content-Type");
            }

            request.Content.Headers.Add("Content-Type", "application/json;type=entry");
        }

        HttpMessageContent messageContent = new HttpMessageContent(request);
        if (messageContent.Headers.Contains("Content-Type"))
        {
            messageContent.Headers.Remove("Content-Type");
        }

        messageContent.Headers.Add("Content-Type", "application/http");
        messageContent.Headers.Add("Content-Transfer-Encoding", "binary");

        return messageContent;
    }
}