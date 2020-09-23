using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace CrmNx.Xrm.Toolkit.ObjectModel
{
    public class DataCollection<T>
    {
        [JsonProperty("value")] public IEnumerable<T> Items { get; set; }

        [JsonProperty("@odata.context", NullValueHandling = NullValueHandling.Ignore)]
        public string Contex { get; set; }

        [JsonProperty("@odata.count", NullValueHandling = NullValueHandling.Ignore)]
        public int Count { get; set; }

        [JsonProperty("@odata.nextLink", NullValueHandling = NullValueHandling.Ignore)]
        public Uri NextLink { get; set; }

        [JsonIgnore] public bool MoreRecords => NextLink != null;

        public DataCollection()
        {
        }

        public DataCollection(IEnumerable<T> items)
        {
            Items = items.ToArray();
        }
    }
}