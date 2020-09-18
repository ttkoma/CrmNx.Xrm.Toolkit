using System.Net;
using CrmNx.Crm.Toolkit.Testing;
using CrmNx.Xrm.Toolkit.Infrastructure;
using CrmNx.Xrm.Toolkit.Query;
using FluentAssertions;
using Xunit;

namespace CrmNx.Xrm.Toolkit.UnitTests.Query
{
    public class QueryOptionsTests
    {
        [Fact()]
        public void GetQueryString_When_Create_Then_Select_Eq_PrimaryId_Test()
        {
            WebApiMetadata metadata = MockedWebApiMetadata.CreateD365Ce();

            var options = new QueryOptions();

            var query = options.BuildQueryString(metadata, "account");

            query.Should().Be("?$select=accountid");
        }

        [Fact()]
        public void GetQueryString_When_Select_Lookup_Then_Query_IsValid_Test()
        {
            WebApiMetadata metadata = MockedWebApiMetadata.CreateD365Ce();

            var options = new QueryOptions("createdby", "primarycontactid");

            var query = options.BuildQueryString(metadata, "account");

            query.Should().Be("?$select=_createdby_value,_primarycontactid_value");
        }

        [Fact()]
        public void GetQueryString_When_SetAllColumns_Then_Query_Is_Empty_Test()
        {
            WebApiMetadata metadata = MockedWebApiMetadata.CreateD365Ce();

            var options = new QueryOptions()
                .Select(new ColumnSet()
                {
                    AllColumns = true
                });

            var query = options.BuildQueryString(metadata, "account");

            query.Should().Be("");
        }

        [Fact()]
        public void GetQueryString_When_SetCount_Then_Query_IsValid_Test()
        {
            WebApiMetadata metadata = MockedWebApiMetadata.CreateD365Ce();

            var options = new QueryOptions()
                .Select(new ColumnSet() {AllColumns = true})
                .Count();

            var query = options.BuildQueryString(metadata, "account");

            query.Should().Be("?$count=true");
        }

        [Fact()]
        public void GetQueryString_When_SetTop_Then_Query_IsValid_Test()
        {
            WebApiMetadata metadata = MockedWebApiMetadata.CreateD365Ce();

            var options = new QueryOptions()
                .Select(new ColumnSet() {AllColumns = true})
                .Top(3);

            var query = options.BuildQueryString(metadata, "account");

            query.Should().Be("?$top=3");
        }

        [Fact()]
        public void GetQueryString_When_SetOrder_Then_Query_IsValid_Test()
        {
            WebApiMetadata metadata = MockedWebApiMetadata.CreateD365Ce();

            var options = new QueryOptions()
                .Select(new ColumnSet() {AllColumns = true})
                .OrderBy("createdby")
                .OrderByDesc("name");

            var query = options.BuildQueryString(metadata, "account");

            query.Should().Be("?$orderby=_createdby_value,name%20desc");
        }

        [Fact()]
        public void GetQueryString_When_Expand_Lookup_Then_Query_IsValid_Test()
        {
            WebApiMetadata metadata = MockedWebApiMetadata.CreateD365Ce();

            var options = new QueryOptions()
                .Select(new ColumnSet() {AllColumns = true})
                .Expand("createdby", "domainname", "businessunitid")
                .Expand("primarycontactid");

            var query = options.BuildQueryString(metadata, "account");


            WebUtility.UrlDecode(query).Should()
                .Be("?$expand=createdby($select=domainname,businessunitid),primarycontactid($select=contactid)");

            query.Should()
                .Be("?$expand=createdby($select%3Ddomainname,businessunitid),primarycontactid($select%3Dcontactid)");
        }
    }
}