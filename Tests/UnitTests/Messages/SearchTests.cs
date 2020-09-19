using System;
using System.Collections.Generic;
using System.Linq;
using CrmNx.Xrm.Toolkit.Messages;
using FluentAssertions;
using Microsoft.AspNetCore.WebUtilities;
using Xunit;

namespace CrmNx.Xrm.Toolkit.UnitTests.Messages
{
    public class SearchTests
    {
        [Fact]
        public void Search_ToQueryString_When_MinimalCondition_Then_Query_IsCorrect()
        {
            var request = new SearchRequest()
            {
                AppointmentRequest = new AppointmentRequest
                {
                    SearchWindowStart = new DateTime(2020, 09, 19, 9, 0, 0, DateTimeKind.Utc),
                    ServiceId = new Guid("5bdbf8d2-5ad8-e911-aacb-005056b410d8")
                }
            };

            var queryString = request.QueryString();
            var queryParams = queryString.Split("?").Last();


            var value = QueryHelpers.ParseQuery(queryParams).GetValueOrDefault("@p1").ToString();

            value.Should().NotBeNullOrEmpty();
            value.Should()
                .Be(
                    "{\"ServiceId\":\"5bdbf8d2-5ad8-e911-aacb-005056b410d8\",\"SearchWindowStart\":\"2020-09-19T09:00:00Z\",\"NumberOfResults\":1}"
                );
        }

        [Fact]
        public void Search_ToQueryString_When_Direction_Present_Then_Query_IsCorrect()
        {
            var request = new SearchRequest()
            {
                AppointmentRequest = new AppointmentRequest
                {
                    SearchWindowStart = new DateTime(2020, 09, 19, 9, 0, 0, DateTimeKind.Utc),
                    Direction = SearchDirection.Forward
                }
            };

            var queryString = request.QueryString();
            var queryParams = queryString.Split("?").Last();


            var value = QueryHelpers.ParseQuery(queryParams).GetValueOrDefault("@p1").ToString();

            value.Should().NotBeNullOrEmpty();
            value.Should()
                .Be(
                    "{\"ServiceId\":\"00000000-0000-0000-0000-000000000000\",\"SearchWindowStart\":\"2020-09-19T09:00:00Z\",\"Direction\":\"Forward\",\"NumberOfResults\":1}"
                );
        }
    }
}