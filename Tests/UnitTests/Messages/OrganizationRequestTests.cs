using CrmNx.Crm.Toolkit.Testing;
using CrmNx.Xrm.Toolkit.Messages;
using CrmNx.Xrm.Toolkit.Metadata;
using CrmNx.Xrm.Toolkit.ObjectModel;
using FluentAssertions;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CrmNx.Xrm.Toolkit.UnitTests.Messages
{
    public class OrganizationRequestTests
    {
        [Fact]
        public async void WhenNotWebApiAction_And_NotRequestBindingPath_ThenUrlValid()
        {
            Uri requestUri = default;

            var httpClient = new HttpClient(new MockedHttpMessageHandler((request) =>
            {
                requestUri = request.RequestUri;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
            }));


            const string accessType = "Default";
            string msDynamicsAccessTypeValue = $"Microsoft.Dynamics.CRM.EndpointAccessType'{accessType}'";

            var orgRequest = new OrganizationRequest<JObject>("RetrieveCurrentOrganization");
            orgRequest.Parameters.Add("AccessType", msDynamicsAccessTypeValue);


            var crmClient = FakeCrmWebApiClient.Create(httpClient);

            await crmClient.ExecuteAsync(orgRequest);


            var lastPathSegment = requestUri.Segments[^1];

            var accessTypeQueryParameterValue = QueryHelpers.ParseQuery(requestUri.Query)
                 .GetValueOrDefault("@AccessType").ToString();

            lastPathSegment.Should().Be("RetrieveCurrentOrganization(AccessType=@AccessType)");
            accessTypeQueryParameterValue.Should().Be(msDynamicsAccessTypeValue);
        }

        [Fact]
        public async void WhenNotWebApiAction_And_RequestBindingPath_ThenUrlValid()
        {
            Uri requestUri = default;

            var httpClient = new HttpClient(new MockedHttpMessageHandler((request) =>
            {
                requestUri = request.RequestUri;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
            }));

            var fieldNames = string.Join(",", new[]
                        {
                            "SchemaName", "LogicalName", "EntitySetName", "PrimaryIdAttribute",
                            "PrimaryNameAttribute", "ObjectTypeCode"
                        });

            var orgRequest = new OrganizationRequest<DataCollection<EntityMetadata>>
            {
                RequestBindingPath = "EntityDefinitions",
                Parameters =
                {
                    {
                        "$select",
                        fieldNames
                    }
                }
            };

            var crmClient = FakeCrmWebApiClient.Create(httpClient);

            await crmClient.ExecuteAsync(orgRequest);

            var lastPathSegment = requestUri.Segments[^1];

            var selectTermValue = QueryHelpers.ParseQuery(requestUri.Query)
                 .GetValueOrDefault("$select").ToString();

            lastPathSegment.Should().Be(orgRequest.RequestBindingPath);
            selectTermValue.Should().Be(fieldNames);
        }
    }
}
