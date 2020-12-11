using System;
using System.Threading.Tasks;
using CrmNx.Crm.Toolkit.Testing.Functional;
using CrmNx.Xrm.Toolkit.Messages;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CrmNx.Xrm.Toolkit.FunctionalTests.Messages
{
    public class AddMembersTeamActionTests: IntegrationTest<TestStartup>
    {
        private const string SystemUserEntityName = "systemuser";
        private const string TeamEntityName = "team";
        
        public AddMembersTeamActionTests(TestStartup fixture, ITestOutputHelper outputHelper)
            : base(fixture, outputHelper)
        {
        }
        
        [Fact()]
        public async Task ExecuteAsync_AddMembersTeamAction_When_OneUsersAdded_Then_Ok()
        {
            var unitTestsUserId = CrmClient.GetMyCrmUserId();

            var userRef = new EntityReference(SystemUserEntityName, unitTestsUserId);
            var boundTeamRef = new EntityReference(TeamEntityName, new Guid("a640a425-a463-ea11-aae8-005056b42cd8"));

            var action = new AddMembersTeamAction(boundTeamRef)
            {
                Members = { userRef }
            };
            
            await CrmClient.ExecuteAsync(action);
        }
        
        [Fact()]
        public async Task ExecuteAsync_AddMembersTeamAction_When_MultipleUsersAdded_Then_Ok()
        {
            var unitTestsUserId = CrmClient.GetMyCrmUserId();
            var otherUserId = new Guid("a988c6a2-5236-ea11-aadc-005056b42cd8"); 
            var testTeamId = new Guid("a640a425-a463-ea11-aae8-005056b42cd8");

            var userRef = new EntityReference(SystemUserEntityName, unitTestsUserId);
            var userRef1 = new EntityReference(SystemUserEntityName, otherUserId);
            // var userRef2 = new EntityReference(SystemUserEntityName, unitTestsUserId);
            var boundTeamRef = new EntityReference(TeamEntityName, testTeamId);

            var action = new AddMembersTeamAction(boundTeamRef)
            {
                Members = { userRef, userRef1 }
            };
            
            await CrmClient.ExecuteAsync(action);
        }
        
    }
}