﻿using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FlurlGraphQL.Tests
{
    [TestClass]
    public class FlurlGraphQLMutationTests : BaseFlurlGraphQLTest
    {

        [TestMethod]
        public async Task TestMutationWithQueryResultsAsync()
        {
            var mutationResult = await GraphQLApiEndpoint
                .WithGraphQLQuery(@"
                    mutation($reviewInput: CreateReviewInput) {
	                    createReview(input: $reviewInput) {
		                    episode
		                    review {
			                    id
			                    stars
			                    commentary
		                    }
	                    }
                    }
                ")
                .SetGraphQLVariable("reviewInput", new { 
                    episode = "EMPIRE",
                    stars = 5,
                    commentary = "I love this Movie!"
                })
                .PostGraphQLQueryAsync()
                .ReceiveGraphQLMutationResult<CreateReviewPayload>()
                .ConfigureAwait(false);

            Assert.IsNotNull(mutationResult);
            Assert.IsFalse(string.IsNullOrEmpty(mutationResult.Episode));
            Assert.IsNotNull(mutationResult.Review);
            Assert.IsFalse(string.IsNullOrEmpty(mutationResult.Review.Commentary));
            Assert.IsNotNull(mutationResult.Review.Id);
            Assert.AreNotEqual(Guid.Empty, mutationResult.Review.Id);

            var jsonText = JsonConvert.SerializeObject(mutationResult, Formatting.Indented);
            TestContext.WriteLine(jsonText);
        }
    }

    public class CreateReviewPayload {
        public string Episode { get; set; }
        public ReviewResult Review {get; set;}

        public class ReviewResult
        {
            public Guid Id { get; set; }
            public int Stars { get; set; }
            public string Commentary { get; set; }
        }
    }

    public class EventResult
    {
        public Guid? EventUUID { get; set; }
        public int? EventId { get; set; }
    }
}