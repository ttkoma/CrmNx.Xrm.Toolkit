using System;
using System.Collections.Generic;
using System.Linq;
using CrmNx.Crm.Toolkit.Testing;
using CrmNx.Xrm.Toolkit.Messages;
using FluentAssertions;
using Microsoft.AspNetCore.WebUtilities;
using Xunit;

namespace CrmNx.Xrm.Toolkit.UnitTests.Messages
{
    public class QueryScheduleTests
    {
        [Fact]
        public void QuerySchedules_ToQueryString_Start_IsCorrect()
        {
            var request = new QueryScheduleRequest()
            {
                Start = new DateTime(2019, 02, 25, 0, 0, 0, DateTimeKind.Utc),
            };

            var queryString = request.QueryString();
            var queryParams = queryString.Split("?").Last();

            var value = QueryHelpers.ParseQuery(queryParams).GetValueOrDefault("@Start").ToString();

            value.Should().NotBeNullOrEmpty();
            value.Should().Be(request.Start.ToString("yyyy-MM-ddTHH:mm:ssZ"));
        }

        [Fact]
        public void QuerySchedules_ToQueryString_End_IsCorrect()
        {
            var request = new QueryScheduleRequest()
            {
                End = new DateTime(2019, 02, 25, 0, 0, 0, DateTimeKind.Local),
            };

            var queryString = request.QueryString();
            var queryParams = queryString.Split("?").Last();

            var value = QueryHelpers.ParseQuery(queryParams).GetValueOrDefault("@End").ToString();

            value.Should().NotBeNullOrEmpty();
            value.Should().Be(request.End.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"));
        }

        [Fact]
        public void QuerySchedules_ToQueryString_ResourceId_IsCorrect()
        {
            var request = new QueryScheduleRequest()
            {
                ResourceId = SetupBase.EntityId
            };

            var queryString = request.QueryString();
            var queryParams = queryString.Split("?").Last();


            var value = QueryHelpers.ParseQuery(queryParams).GetValueOrDefault("@ResourceId").ToString();

            value.Should().NotBeNullOrEmpty();
            value.Should().Be($"{SetupBase.EntityId}");
        }

        [Fact]
        public void QuerySchedules_ToQueryString_When_Empty_TimeCodes_IsCorrect()
        {
            var request = new QueryScheduleRequest();

            var queryString = request.QueryString();
            var queryParams = queryString.Split("?").Last();


            var value = QueryHelpers.ParseQuery(queryParams).GetValueOrDefault("@TimeCodes").ToString();

            value.Should().NotBeNullOrEmpty();
            value.Should().Be("[]");
        }

        [Fact]
        public void QuerySchedules_ToQueryString_When_One_TimeCodes_IsCorrect()
        {
            var request = new QueryScheduleRequest()
            {
                TimeCodes = new[] {TimeCode.Filter}
            };

            var queryString = request.QueryString();
            var queryParams = queryString.Split("?").Last();


            var value = QueryHelpers.ParseQuery(queryParams).GetValueOrDefault("@TimeCodes").ToString();

            value.Should().NotBeNullOrEmpty();
            value.Should().Be($"[\"{(int) TimeCode.Filter}\"]");
        }

        [Fact]
        public void QuerySchedules_ToQueryString_When_Multiple_TimeCodes_IsCorrect()
        {
            var request = new QueryScheduleRequest()
            {
                TimeCodes = new[] {TimeCode.Filter, TimeCode.Available, TimeCode.Busy}
            };

            var queryString = request.QueryString();
            var queryParams = queryString.Split("?").Last();


            var value = QueryHelpers.ParseQuery(queryParams).GetValueOrDefault("@TimeCodes").ToString();

            value.Should().NotBeNullOrEmpty();
            value.Should().Be($"[\"3\",\"0\",\"1\"]");
        }
    }
}