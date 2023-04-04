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


        [Fact]
        public async Task RetrieveMultipleAsync_FetchXml_LargeQuery()
        {
            var fetch = new FetchXmlExpression(
                @"<fetch count='2' distinct='true' no-lock='true' returntotalrecordcount='true' page='1'>
  <entity name='resource'>
    <attribute name='resourceid' alias='resourceid_mlkm_SearchResourcesQuery' />
    <attribute name='resourceid' />
    <attribute name='name' />
    <attribute name='isdisabled' />
    <attribute name='objecttypecode' />
    <filter>
      <condition attribute='objecttypecode' operator='in'>
        <value>4000</value>
        <value>8</value>
      </condition>
      <condition attribute='isdisabled' operator='in'>
        <value>0</value>
      </condition>
    </filter>
    <filter type='or' />
    <filter type='or'>
      <filter type='or'>
        <condition entityname='systemuser_groups' attribute='nbnit_servicegroupid' operator='eq-or-under' value='297d9c8e-1df5-eb11-ab0a-005056b42cd8' />
        <condition entityname='systemuser_groups' attribute='nbnit_servicegroupid' operator='eq-or-under' value='693c7cae-1ff5-eb11-ab0a-005056b42cd8' />
        <condition entityname='systemuser_groups' attribute='nbnit_servicegroupid' operator='eq-or-under' value='a7c07d89-f483-ec11-ab15-005056b42cd8' />
        <condition entityname='systemuser_groups' attribute='nbnit_servicegroupid' operator='eq-or-under' value='21484652-22f5-eb11-ab0a-005056b42cd8' />
      </filter>
      <filter type='and'>
        <filter type='or'>
          <condition entityname='equipment_groups' attribute='nbnit_servicegroupid' operator='eq-or-under' value='297d9c8e-1df5-eb11-ab0a-005056b42cd8' />
          <condition entityname='equipment_groups' attribute='nbnit_servicegroupid' operator='eq-or-under' value='693c7cae-1ff5-eb11-ab0a-005056b42cd8' />
          <condition entityname='equipment_groups' attribute='nbnit_servicegroupid' operator='eq-or-under' value='a7c07d89-f483-ec11-ab15-005056b42cd8' />
          <condition entityname='equipment_groups' attribute='nbnit_servicegroupid' operator='eq-or-under' value='21484652-22f5-eb11-ab0a-005056b42cd8' />
        </filter>
        <condition entityname='equipments' attribute='gm_fired' operator='ne' value='1' />
        <condition entityname='equipments' attribute='gm_resourceusetypecode' operator='in'>
          <value>930660000</value>
          <value>930660002</value>
        </condition>
      </filter>
    </filter>
    <link-entity name='nbnit_servicegroup_systemuser' from='systemuserid' to='resourceid' link-type='outer'>
      <link-entity name='nbnit_servicegroup' from='nbnit_servicegroupid' to='nbnit_servicegroupid' link-type='outer' alias='systemuser_groups' />
    </link-entity>
    <link-entity name='nbnit_servicegroup_equipment' from='equipmentid' to='resourceid' link-type='outer'>
      <link-entity name='nbnit_servicegroup' from='nbnit_servicegroupid' to='nbnit_servicegroupid' link-type='outer' alias='equipment_groups' />
      <link-entity name='equipment' from='equipmentid' to='equipmentid' link-type='outer' alias='equipments' />
    </link-entity>
    <order attribute='name' descending='false' />
  </entity>
</fetch>");

            var collection = await CrmClient.RetrieveMultipleAsync(fetch);
            collection.EntityName.Should().Be("resource");
            collection.MoreRecords.Should().BeTrue();
            collection.PagingCookie.Should().NotBeEmpty();

            collection.Entities.Count.Should().BePositive();
        }

        [Fact]
        public async Task RetrieveMultipleAsync_FetchXml_MultiplePages()
        {
            const string fetchXml = @"
            <fetch mapping='logical' distinct='true'>
              <entity name='resource'>
                <attribute name='resourceid' />
                <attribute name='resourceid' />
                <attribute name='name' />
                <attribute name='isdisabled' />
                <attribute name='objecttypecode' />
              </entity>
            </fetch>";
            
            var fetchXmlExpression = new FetchXmlExpression(fetchXml)
            {
                Page = 1,
                Count = 2,
                NoLock = true
            };

            var collection = await CrmClient.RetrieveMultipleAsync(fetchXmlExpression);

            collection.EntityName.Should().Be("resource");
            collection.MoreRecords.Should().BeTrue();
            collection.PagingCookie.Should().NotBeEmpty();

            fetchXmlExpression.Page++;
            fetchXmlExpression.PagingCookie = collection.PagingCookie;

            collection = await CrmClient.RetrieveMultipleAsync(fetchXmlExpression);
            collection.Entities.Count.Should().BePositive();
        }

        [Fact]
        public async void RetrieveMultiple_FetchAggregateCount()
        {
            const string countAlias = "TotalCount";
            var fetchXml = $@"
            <fetch mapping='logical'>
                <entity name='resource'>
                    <attribute name='resourceid' aggregate='count' alias='{countAlias}'/>
                </entity>
            </fetch>
            ";

            var fetchExp = new FetchXmlExpression(fetchXml)
            {
                Aggregate = true,
                IncludeAnnotations = false
            };

            var collection = await CrmClient.RetrieveMultipleAsync(fetchExp);
            collection.Entities.Count.Should().Be(1);
            collection.Entities[0].GetAttributeValue<int>("TotalCount").Should().BeGreaterOrEqualTo(1);
        }
    }
}