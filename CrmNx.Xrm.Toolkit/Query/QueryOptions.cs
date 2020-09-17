using System.Collections.Generic;
using System.Linq;
using CrmNx.Xrm.Toolkit.Infrastructure;
using CrmNx.Xrm.Toolkit.Metadata;
using Microsoft.AspNetCore.WebUtilities;

namespace CrmNx.Xrm.Toolkit.Query
{
    public class QueryOptions
    {
        // Paging?
        public int? TopCount { get; private set; }

        // Paging?
        //public string SkipToken { get; }

        // Paging?
        //public int? Page { get; }
        public List<ExpandOptions> ExpandOptions { get; private set; } = new List<ExpandOptions>();

        public bool ReturnTotalCount { get; private set; }

        public Dictionary<string, OrderType> Order { get; } = new Dictionary<string, OrderType>();

        public string FilterExpression { get; private set; }

        public ColumnSet ColumnSet { get; private set; } = new ColumnSet();

        public QueryOptions(params string[] columns)
        {

            Select(new ColumnSet(columns));
        }

        public QueryOptions Select(ColumnSet columnSet)
        {
            ColumnSet = columnSet;

            return this;
        }

        public QueryOptions Filter(string expression)
        {
            FilterExpression = expression;

            return this;
        }

        public QueryOptions OrderBy(string propertyName)
        {
            Order.Add(propertyName, OrderType.Ascending);
            return this;
        }

        public QueryOptions OrderByDesc(string propertyName)
        {
            Order.Add(propertyName, OrderType.Descending);
            return this;
        }

        public QueryOptions Top(int count)
        {
            TopCount = count;

            return this;
        }

        public QueryOptions Count()
        {
            ReturnTotalCount = true;

            return this;
        }

        /// <summary>
        /// Expand associated entity
        /// </summary>
        /// <param name="propertyName">Referencing attribute logical name</param>
        /// <param name="columns">Referenced entity attributes</param>
        public QueryOptions Expand(string propertyName, params string[] columns)
        {
            ExpandOptions.Add(new ExpandOptions(propertyName, columns));

            return this;
        }

        /// <summary>
        /// Expand associated entity
        /// </summary>
        /// <param name="propertyName">Referencing attribute logical name</param>
        /// <param name="disableNameResolving">Disable navigation property name resolving</param>
        /// <param name="columns">Referenced entity attributes</param>
        public QueryOptions Expand(string propertyName, bool disableNameResolving, params string[] columns)
        {
            ExpandOptions.Add(new ExpandOptions(propertyName, disableNameResolving, columns));

            return this;
        }

        internal string BuildQueryString(IWebApiMetadataService webApiMetadata, in string entityName)
        {
            var query = string.Empty;

            if (BuildSelectOptionValue(webApiMetadata, entityName, ColumnSet, out var selectValue))
            {
                query = QueryHelpers.AddQueryString(query, "$select", selectValue);
            }

            if (!string.IsNullOrEmpty(FilterExpression))
            {
                query = QueryHelpers.AddQueryString(query, "$filter", FilterExpression);
            }

            if (BuildOrderOptionValue(webApiMetadata, entityName, Order, out var orderValue))
            {
                query = QueryHelpers.AddQueryString(query, "$orderby", orderValue);
            }

            if (BuildExpandOptionValue(webApiMetadata, entityName, ExpandOptions, out var expandValue))
            {
                query = QueryHelpers.AddQueryString(query, "$expand", expandValue);
            }

            if (TopCount.GetValueOrDefault(0) > 0)
            {
                query = QueryHelpers.AddQueryString(query, "$top", TopCount.ToString());
            }

            if (ReturnTotalCount)
            {
                query = QueryHelpers.AddQueryString(query, "$count", "true");
            }

            return query;
        }

        private static bool BuildExpandOptionValue(IWebApiMetadataService metadata, string entityName, IEnumerable<ExpandOptions> expandOptions, out string expandValue)
        {
            // Init out Args
            expandValue = string.Empty;

            var terms = new List<string>();

            foreach (var expand in expandOptions)
            {
                var navPropertyName = expand.PropertyName;
                OneToManyRelationshipMetadata relationship = default;

                if (!expand.DisableNameResolving)
                {
                    relationship = metadata.GetRelationshipMetadata(x =>
                        x.ReferencingEntity == entityName
                        && x.ReferencingAttribute == expand.PropertyName);

                    if (relationship != null)
                    {
                        navPropertyName = relationship.ReferencingEntityNavigationPropertyName;
                    }
                }

                if (expand.ColumnSet.Columns.Any() && !expand.ColumnSet.AllColumns)
                {
                    terms.Add($"{navPropertyName}($select={string.Join(",", expand.ColumnSet.Columns)})");
                    continue;
                }
                else if (relationship != null)
                {
                    terms.Add($"{navPropertyName}($select={relationship.ReferencedAttribute})");
                }
                else
                {
                    terms.Add(navPropertyName);
                }
            }

            if (!terms.Any()) return false;

            expandValue = string.Join(",", terms);
            return true;

        }

        private static bool BuildOrderOptionValue(IWebApiMetadataService webApiMetadata, in string entityLogicalName, in Dictionary<string, OrderType> orders, out string orderValue)
        {
            orderValue = string.Empty;

            if (!orders.Any()) return false;

            var list = new List<string>();
            foreach (var (attributeName, orderType) in orders)
            {
                var formattedProperty = webApiMetadata.FormatPropertyToLogicalName(entityLogicalName, attributeName);

                list.Add(orderType == OrderType.Ascending ? formattedProperty : $"{formattedProperty} desc");
            }

            orderValue = string.Join(",", list);
            return true;

        }

        private static bool BuildSelectOptionValue(IWebApiMetadataService webApiMetadata, string entityLogicalName, in ColumnSet columnSet, out string selectOptionValue)
        {
            selectOptionValue = string.Empty;

            var entityMd = webApiMetadata.GetEntityMetadata(entityLogicalName);

            if (columnSet is null && entityMd != null)
            {
                selectOptionValue = entityMd.PrimaryIdAttribute;
                return true;
            }

            if (columnSet != null && columnSet.Columns.Any())
            {
                var list = columnSet.Columns.Select(propertyName => entityMd != null
                        ? webApiMetadata.FormatPropertyToLogicalName(entityLogicalName, propertyName)
                        : propertyName)
                    .ToList();

                selectOptionValue = string.Join(",", list);
                return true;
            }

            if (columnSet != null && columnSet.AllColumns)
            {
                return false;
            }

            selectOptionValue = entityMd?.PrimaryIdAttribute;
            return entityMd?.PrimaryIdAttribute != null;
        }
    }
}
