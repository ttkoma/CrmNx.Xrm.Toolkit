using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CrmNx.Crm.Toolkit.Testing;
using CrmNx.Crm.Toolkit.Testing.CustomActions;
using CrmNx.Xrm.Toolkit.Messages;
using FluentAssertions;
using Microsoft.AspNetCore.WebUtilities;
using Xunit;

namespace CrmNx.Xrm.Toolkit.UnitTests.Messages
{
    public class CustomActionTests
    {
        [Fact]
        public async Task When_CustomAction_Parameter_Is_EntityReference_Then_Body_IsCorrect()
        {
            string body = null;
            
            var httpClient = new HttpClient(new MockedHttpMessageHandler(async (request) =>
            {
                body = await request.Content.ReadAsStringAsync();
                
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }));
            
            var crmClient = FakeCrmWebApiClient.Create(httpClient);
            
            // Preparation
            var userRef = new EntityReference("systemuser", SetupBase.EntityId);
            var crmRequest = new ActionUnboundEntityReferenceIn(userRef);
            
            // Execution
            await crmClient.ExecuteAsync(crmRequest);
            
            // Validation
            body.Should().NotBeNullOrEmpty();

            var requestContent = JsonSerializer.Deserialize<Dictionary<string,Dictionary<string,object>>>(body);
            requestContent.Should().ContainKey("UserRef");

            var userRefDeserialized = requestContent["UserRef"];
            userRefDeserialized.Should().ContainKey("systemuserid");
            (userRefDeserialized["systemuserid"] as string).Should()
                .Equals(SetupBase.EntityId.ToString());
            
            userRefDeserialized.Should().ContainKey("@odata.type");
            (userRefDeserialized["@odata.type"] as string).Should()
                .Equals("Microsoft.Dynamics.CRM.systemuser");
        }
    }
}