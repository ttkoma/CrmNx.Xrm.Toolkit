using System;
using System.Collections.Generic;
using System.Linq;
using CrmNx.Xrm.Toolkit.Messages;
using FluentAssertions;
using Microsoft.AspNetCore.WebUtilities;
using TestFramework;
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
                Start = new DateTime(2019, 02, 25),
            };

            var queryString = request.ToQueryString();
            var queryParams = queryString.Split("?").Last();

            var value = QueryHelpers.ParseQuery(queryParams).GetValueOrDefault("@Start").ToString();

            value.Should().NotBeNullOrEmpty();
            value.Should().Be("2019-02-24T21:00:00Z");
        }

        [Fact]
        public void QuerySchedules_ToQueryString_End_IsCorrect()
        {
            var request = new QueryScheduleRequest()
            {
                End = new DateTime(2019, 02, 25),
            };

            var queryString = request.ToQueryString();
            var queryParams = queryString.Split("?").Last();

            var value = QueryHelpers.ParseQuery(queryParams).GetValueOrDefault("@End").ToString();

            value.Should().NotBeNullOrEmpty();
            value.Should().Be("2019-02-24T21:00:00Z");
        }

        [Fact]
        public void QuerySchedules_ToQueryString_ResourceId_IsCorrect()
        {
            var request = new QueryScheduleRequest()
            {
                ResourceId = Setup.EntityId
            };

            var queryString = request.ToQueryString();
            var queryParams = queryString.Split("?").Last();


            var value = QueryHelpers.ParseQuery(queryParams).GetValueOrDefault("@ResourceId").ToString();

            value.Should().NotBeNullOrEmpty();
            value.Should().Be($"{Setup.EntityId}");
        }

        [Fact]
        public void QuerySchedules_ToQueryString_When_Empty_TimeCodes_IsCorrect()
        {
            var request = new QueryScheduleRequest();

            var queryString = request.ToQueryString();
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
                TimeCodes = new[] { TimeCode.Filter }
            };

            var queryString = request.ToQueryString();
            var queryParams = queryString.Split("?").Last();


            var value = QueryHelpers.ParseQuery(queryParams).GetValueOrDefault("@TimeCodes").ToString();

            value.Should().NotBeNullOrEmpty();
            value.Should().Be($"[\"{(int)TimeCode.Filter}\"]");
        }

        [Fact]
        public void QuerySchedules_ToQueryString_When_Multiple_TimeCodes_IsCorrect()
        {
            var request = new QueryScheduleRequest()
            {
                TimeCodes = new[] { TimeCode.Filter, TimeCode.Available, TimeCode.Busy }
            };

            var queryString = request.ToQueryString();
            var queryParams = queryString.Split("?").Last();


            var value = QueryHelpers.ParseQuery(queryParams).GetValueOrDefault("@TimeCodes").ToString();

            value.Should().NotBeNullOrEmpty();
            value.Should().Be($"[\"3\",\"0\",\"1\"]");
        }
    }
}
