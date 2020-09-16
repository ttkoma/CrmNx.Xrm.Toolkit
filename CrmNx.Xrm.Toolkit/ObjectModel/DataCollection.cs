using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CrmNx.Xrm.Toolkit.ObjectModel
{
    public class DataCollection<T>
    {
        [JsonProperty("value")]
        public IEnumerable<T> Items { get; set; }

        [JsonProperty("@odata.context")]
        public string Contex { get; set; }

        [JsonProperty("@odata.count")]
        public int Count { get; set; }

        [JsonProperty("@odata.nextLink")]
        public Uri NextLink { get; set; }

        public bool MoreRecords => NextLink != null;

        public DataCollection() { }

        public DataCollection(IEnumerable<T> items)
        {
            Items = items.ToArray();
        }
    }
}
