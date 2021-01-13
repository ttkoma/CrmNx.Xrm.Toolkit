using System;
using System.Threading.Tasks;
using CrmNx.Crm.Toolkit.Testing.Functional;
using CrmNx.Xrm.Toolkit.Messages;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CrmNx.Xrm.Toolkit.FunctionalTests.Messages
{
    public class WinOpportunityActionTests: IntegrationTest<TestStartup>
    {
        private const string OpportunityEntityName = "opportunity";
        private const string OpportunityCloseEntityName = "opportunityclose";
        
        public WinOpportunityActionTests(TestStartup fixture, ITestOutputHelper outputHelper)
            : base(fixture, outputHelper)
        {
        }
        
        [Fact()]
        public async Task ExecuteAsync_WinOpportunityRequest_When_Caller_Is_Null_Then_Ok()
        {
            var opportunityId = new Guid("{5D0E46CA-49F1-EA11-AAF2-005056B42CD8}");
            var opportunityEntityRef = new EntityReference(OpportunityEntityName, opportunityId);
            
            var opportunityCloseEntity = new Entity("opportunityclose");
            opportunityCloseEntity.SetAttributeValue("subject", "Won Opportunity");
            opportunityCloseEntity.SetAttributeValue("opportunityid", opportunityEntityRef);
            
            var action = new WinOpportunityRequest()
            {
                OpportunityClose = opportunityCloseEntity,
                Status = 3
            };
            
            await CrmClient.ExecuteAsync(action);
        }
    }
}