using System;
using System.Collections.Generic;
using System.Net.Http;

namespace CrmNx.Xrm.Toolkit.Infrastructure.Batch;

public class ChangeSet
{
    public ChangeSet(List<HttpRequestMessage> requests)
    {
        Requests = requests;
    }

    private readonly List<HttpRequestMessage> _requests = new();

    public List<HttpRequestMessage> Requests
    {
        get => _requests;
        set
        {
            _requests.Clear();
            value.ForEach(x =>
            {
                if (x.Method == HttpMethod.Get)
                {
                    throw new ArgumentException("ChangeSets cannot contain Get requests.");
                }
                
                _requests.Add(x);
            });
        }
    }
}