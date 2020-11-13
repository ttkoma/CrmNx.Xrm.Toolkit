using CrmNx.Xrm.Toolkit.Infrastructure;
using CrmNx.Xrm.Toolkit.Metadata;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections.Generic;
using System.Linq;

namespace CrmNx.Xrm.Toolkit.Query
{
    public class QueryOptions
    {
        public int? TopCount { get; private set; }

        internal int? PageNumber { get; private set; }

        internal List<ExpandOptions> ExpandOptions { get; private set; } = new List<ExpandOptions>();

        internal bool ReturnTotalCount { get; private set; }

        internal Dictionary<string, OrderType> Order { get; } = new Dictionary<string, OrderType>();

        internal string FilterExpression { get; private set; }

        internal ColumnSet ColumnSet { get; private set; } = new ColumnSet();

        public QueryOptions()
        {
        }

        /// <summary>
        /// Select fields
        /// </summary>
        /// <param name="columnSet"></param>
        /// <returns></returns>
        public QueryOptions Select(ColumnSet columnSet)
        {
            ColumnSet = columnSet;

            return this;
        }

        /// <summary>
        /// Select fields
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static QueryOptions Select(params string[] columns)
        {
            return new QueryOptions().Select(new ColumnSet(columns));
        }

        /// <summary>
        /// Set filter expression
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public QueryOptions Filter(string expression)
        {
            FilterExpression = expression;

            return this;
        }

        /// <summary>
        /// Order result asc
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public QueryOptions OrderBy(string propertyName)
        {
            Order.Add(propertyName, OrderType.Ascending);
            return this;
        }

        /// <summary>
        /// Order result desc
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public QueryOptions OrderByDesc(string propertyName)
        {
            Order.Add(propertyName, OrderType.Descending);
            return this;
        }

        /// <summary>
        /// Return First N records. 
        /// Using with option Page for paging result
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public QueryOptions Top(int count)
        {
            TopCount = count;

            return this;
        }

        /// <summary>
        /// Return total count (max. 5000)
        /// </summary>
        /// <returns></returns>
        public QueryOptions Count()
        {
            ReturnTotalCount = true;

            return this;
        }

        /// <summary>
        /// Set page number for pagging result (Use with Top Option)
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <returns></returns>
        public QueryOptions Page(int pageNumber)
        {
            // FULL SkipToken
            //
            //<cookie pagenumber=\"2\" pagingcookie=\"<cookie page="1">
            //<gm_fiasguid lastnull="1" firstnull="1" />
            //<gm_houseid last="{159B3BDC-37DF-4E16-84EC-000C438E231A}" first="{924D6742-7914-456C-9850-0002D5B3A092}" />
            //</cookie>\" istracking=\"False\" />

            PageNumber = pageNumber;
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

            if (PageNumber.HasValue)
            {
                query = QueryHelpers.AddQueryString(query, "$skiptoken", $"<cookie pagenumber=\"{PageNumber}\" />");
            }

            if (ReturnTotalCount)
            {
                query = QueryHelpers.AddQueryString(query, "$count", "true");
            }

            return query;
        }

        private static bool BuildExpandOptionValue(IWebApiMetadataService metadata, string entityName,
            IEnumerable<ExpandOptions> expandOptions, out string expandValue)
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

            if (!terms.Any())
            {
                return false;
            }

            expandValue = string.Join(",", terms);
            return true;
        }

        private static bool BuildOrderOptionValue(IWebApiMetadataService webApiMetadata, in string entityLogicalName,
            in Dictionary<string, OrderType> orders, out string orderValue)
        {
            orderValue = string.Empty;

            if (!orders.Any())
            {
                return false;
            }

            var list = new List<string>();
            foreach (var (attributeName, orderType) in orders)
            {
                var formattedProperty = webApiMetadata.FormatPropertyToLogicalName(entityLogicalName, attributeName);

                list.Add(orderType == OrderType.Ascending ? formattedProperty : $"{formattedProperty} desc");
            }

            orderValue = string.Join(",", list);
            return true;
        }

        private static bool BuildSelectOptionValue(IWebApiMetadataService webApiMetadata, string entityLogicalName,
            in ColumnSet columnSet, out string selectOptionValue)
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