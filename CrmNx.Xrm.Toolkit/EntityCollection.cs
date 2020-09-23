using System;
using System.Collections.Generic;

namespace CrmNx.Xrm.Toolkit
{
    public class EntityCollection
    {
        public List<Entity> Entities { get; } = new List<Entity>();

        public Uri NextLink { get; set; }

        public int Count { get; set; }

        public string EntityName { get; set; }

        public bool MoreRecords
        {
            get => NextLink != null || !string.IsNullOrEmpty(PagingCookie);
        }

        public string PagingCookie { get; set; }
    }
}