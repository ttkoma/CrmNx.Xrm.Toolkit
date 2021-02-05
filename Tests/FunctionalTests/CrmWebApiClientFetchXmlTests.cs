using System;
using System.Linq;
using System.Threading.Tasks;
using CrmNx.Crm.Toolkit.Testing.Functional;
using CrmNx.Xrm.Toolkit.Query;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CrmNx.Xrm.Toolkit.FunctionalTests
{
    public class CrmWebApiClientFetchXmlTests : IntegrationTest<TestStartup>
    {
        public CrmWebApiClientFetchXmlTests(TestStartup fixture, ITestOutputHelper outputHelper) :
            base(fixture, outputHelper)
        {
        }

        [Theory]
        [InlineData(1, "")]
        [InlineData(2,
            "<cookie page=\"1\"><incidentid last=\"{C1F086A3-6598-E611-80C5-005056B40C72}\" first=\"{AD3BABB3-A997-E611-80C4-005056B40C72}\" /></cookie>")]
        [InlineData(3,
            "<cookie page=\"2\"><incidentid last=\"{A9F60827-FE98-E611-80C5-005056B40C72}\" first=\"{D1D2B888-EC98-E611-80C5-005056B40C72}\" /></cookie>")]
        [InlineData(4,
            "<cookie page=\"3\"><incidentid last=\"{05D15F68-6798-E611-80C5-005056B40C72}\" first=\"{810198B6-6698-E611-80C5-005056B40C72}\" /></cookie>")]
        public async Task RetrieveMultipleAsync_FetchXml_Pagination_Correct(int page, string pagingCookie)
        {
            var pageNumber = page;
            var nextPage = (pageNumber + 1);
            const int pageSize = 3;

            var fetchData = new
            {
                caseorigincode = "899090003",
                caseorigincode2 = "930660005",
                caseorigincode3 = "930660000"
            };

            var cookie = string.Empty;
            if (!string.IsNullOrEmpty(pagingCookie))
            {
                cookie = $"paging-cookie='{System.Net.WebUtility.HtmlEncode(pagingCookie)}'";
            }

            //output-format='xml-platform' mapping='logical'
            var fetchXml = $@"
            <fetch no-lock='true' distinct='true' count='{pageSize}' page='{pageNumber}' {cookie}>
                <entity name='incident'>
                <order attribute='incidentid' />
                <attribute name='createdon' />
                <attribute name='statuscode' />
                <attribute name='customerid' />
                <attribute name='incidentid' />
                <link-entity name='account' from='accountid' to='customerid' link-type='outer'>
                  <attribute name='telephone2' />
                  <attribute name='address1_telephone1' />
                  <attribute name='telephone3' />
                  <attribute name='telephone1' />
                  <attribute name='createdby' />
                  <attribute name='modifiedby' />
                  <attribute name='accountnumber' />
                  <attribute name='accountid' />
                </link-entity>
                <filter type='and'>
                  <condition attribute='caseorigincode' operator='in'>
                    <value>{fetchData.caseorigincode /*899090003*/}</value>
                    <value>{fetchData.caseorigincode2 /*930660005*/}</value>
                    <value>{fetchData.caseorigincode3 /*930660000*/}</value>
                  </condition>
                </filter>
              </entity>
            </fetch>";


            var collection = await CrmClient.RetrieveMultipleAsync(new FetchXmlExpression(fetchXml));

            collection.PagingCookie.Should().StartWith($"<cookie page=\"{pageNumber}\"><incidentid last=\"");
            collection.EntityName.Should().Be("incident");
            collection.MoreRecords.Should().BeTrue();

            collection.Entities.Count.Should().Be(pageSize);
            collection.Entities.First().GetAttributeValue<Guid>("account1.createdby").Should().NotBeEmpty();
            collection.Entities.First().LogicalName.Should().Be("incident");
        }
    }
}