using System;
using System.Collections.Generic;
using System.Linq;
using Flurl.Http.Configuration;
using Flurl.Http.Newtonsoft;
using FlurlGraphQL.CustomExtensions;
using FlurlGraphQL.Tests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlurlGraphQL.Tests
{
    [TestClass]
    public class FlurlGraphQLParsingTests : BaseFlurlGraphQLTest
    {
        public FlurlGraphQLParsingTests() : base()
        {
            NestedJsonStructureFlattenedWithForceEnum = LoadTestData("NestedPreFlattened.StarWarsDataWithForceEnum.json");
        }
        #region Sample Json Strings

        private string NestedJsonStructureFlattenedWithForceEnum { get; }

        #endregion

        [TestMethod]
        public void TestSimpleSystemTextJsonParsingOfErrors()
        {
            //NOTE: We leverage Internal Methods and Classes here to get lower level access for Unit Testing and Quicker Debugging...
            var graphqlSerializer = FlurlGraphQLSystemTextJsonSerializer.FromFlurlSerializer(new DefaultJsonSerializer());

            var errorJsonText = LoadTestData("Errors.SimpleTestData.json");
            var graphqlErrors = graphqlSerializer.ParseErrorsFromGraphQLExceptionErrorContent(errorJsonText);

            AssertSimpleErrorTestDataIsValid(graphqlErrors);
        }

        [TestMethod]
        public void TestSimpleNewtonsoftJsonParsingOfErrors()
        {
            //NOTE: We leverage Internal Methods and Classes here to get lower level access for Unit Testing and Quicker Debugging...
            var graphqlSerializer = FlurlGraphQLSystemTextJsonSerializer.FromFlurlSerializer(new NewtonsoftJsonSerializer());

            var errorJsonText = LoadTestData("Errors.SimpleTestData.json");
            var graphqlErrors = graphqlSerializer.ParseErrorsFromGraphQLExceptionErrorContent(errorJsonText);

            AssertSimpleErrorTestDataIsValid(graphqlErrors);
        }

        private void AssertSimpleErrorTestDataIsValid(IReadOnlyList<GraphQLError> graphqlErrors)
        {
            Assert.IsNotNull(graphqlErrors);
            Assert.IsNotNull(graphqlErrors);
            Assert.AreEqual(1, graphqlErrors.Count);

            var error = graphqlErrors[0];
            Assert.IsNotNull(error);
            Assert.IsFalse(string.IsNullOrWhiteSpace(error.Message));
            Assert.AreEqual(1, error.Locations.Count);
            Assert.AreEqual(1, error.Extensions.Count);

            var location = error.Locations[0];
            Assert.AreEqual((uint)2, location.Line);
            Assert.AreEqual((uint)32, location.Column);

            var extension = error.Extensions.First();
            Assert.IsNotNull(extension);
            Assert.AreEqual("code", extension.Key);
            Assert.AreEqual("HC0011", extension.Value);
        }

        [TestMethod]
        public void TestSimpleSystemTextJsonParsingOfPreFlattenedJsonWithStringEnumWithAnnotationJsonConverter()
        {
            //NOTE: We leverage Internal Methods and Classes here to get lower level access for Unit Testing and Quicker Debugging...
            var graphqlSerializer = FlurlGraphQLSystemTextJsonSerializer.FromFlurlSerializer(new DefaultJsonSerializer());

            var characterResults = graphqlSerializer.Deserialize<List<StarWarsCharacterWithEnum>>(NestedJsonStructureFlattenedWithForceEnum);

            Assert.IsNotNull(characterResults);
            Assert.AreEqual(2, characterResults.Count);
            Assert.AreEqual(TheForce.LightSide, characterResults.First(c => c.Name.StartsWith("Luke")).TheForce);
            Assert.AreEqual(TheForce.DarkSide, characterResults.First(c => c.Name.StartsWith("Darth")).TheForce);

            foreach (var result in characterResults)
            {
                TestContext.WriteLine($"Character [{result.PersonalIdentifier}] [{result.Name}] [{result.Height}]");

                foreach (var friend in result.Friends)
                {
                    Assert.IsNotNull(friend);
                    Assert.AreEqual(TheForce.None, friend.TheForce);
                    TestContext.WriteLine($"   Friend [{friend.PersonalIdentifier}] [{friend.Name}] [{friend.Height}]");
                }
            }
        }

        [TestMethod]
        public void TestSystemTextJsonParsingOfNestedPaginatedStarWarsGraphQLResults()
        {
            var systemTextJsonGraphQLProcessor = CreateDefaultSystemTextJsonGraphQLResponseProcessor(this.NestedPaginatedStarWarsJsonText);

            var characterResults = systemTextJsonGraphQLProcessor.LoadTypedResults<StarWarsCharacter>().ToGraphQLConnectionResultsInternal();

            AssertCursorPaginatedResultsAreValid(characterResults);
        }

        //[TestMethod]
        //public void TestSystemTextJsonParsingOfNestedPaginatedBooksAndAuthorsGraphQLResults()
        //{
        //    var systemTextJsonGraphQLProcessor = CreateDefaultSystemTextJsonGraphQLResponseProcessor(this.NestedPaginatedBooksAndAuthorsJsonText);

        //    var bookResults = systemTextJsonGraphQLProcessor.LoadTypedResults<Book>().ToGraphQLConnectionResultsInternal();

        //    //AssertCursorPaginatedResultsAreValid(bookResults);
        //}

        [TestMethod]
        public void TestSystemTextJsonParsingOfNestedPaginatedGraphQLResultsWithJsonPropertyNameMappings()
        {
            var systemTextJsonGraphQLProcessor = CreateDefaultSystemTextJsonGraphQLResponseProcessor(this.NestedPaginatedStarWarsJsonText);

            //NOTE: This will explicitly test/exercise (also for Debugging) the Internal Extension method and by extension also
            //			the custom Json Converter [FlurlGraphQLNewtonsoftJsonPaginatedResultsConverter]!
            var characterResults = systemTextJsonGraphQLProcessor.LoadTypedResults<StarWarsCharacterWithJsonMappings>().ToGraphQLConnectionResultsInternal();

            Assert.AreEqual(2, characterResults.Count);
            foreach (var result in characterResults)
            {
                Assert.IsNotNull(result);
                Assert.IsFalse(string.IsNullOrWhiteSpace(result.MyCursor));
                TestContext.WriteLine($"Character [{result.MyPersonalIdentifier}] [{result.MyName}] [{result.MyHeight}]");

                foreach (var friend in result.MyFriends)
                {
                    Assert.IsNotNull(friend);
                    Assert.IsTrue(friend.MyPersonalIdentifier > 0);
                    Assert.IsFalse(string.IsNullOrWhiteSpace(friend.MyName));
                    Assert.IsFalse(string.IsNullOrWhiteSpace(friend.MyCursor));
                    TestContext.WriteLine($"   Friend [{friend.MyPersonalIdentifier}] [{friend.MyName}] [{friend.MyHeight}]");
                }
            }
        }

        [TestMethod]
        public void TestNewtonsoftJsonParsingOfNestedPaginatedStarWarsGraphQLResults()
        {
            var newtonsoftJsonGraphQLProcessor = CreateDefaultNewtonsoftJsonGraphQLResponseProcessor(this.NestedPaginatedStarWarsJsonText);

            var characterResults = newtonsoftJsonGraphQLProcessor.LoadTypedResults<StarWarsCharacter>().ToGraphQLConnectionResultsInternal();

            AssertCursorPaginatedResultsAreValid(characterResults);
        }

        [TestMethod]
        public void TestNewtonsoftJsonParsingOfNestedPaginatedBooksAndAuthorsGraphQLResults()
        {
            //var newtonsoftJsonGraphQLProcessor = CreateDefaultNewtonsoftJsonGraphQLResponseProcessor(this.NestedPaginatedStarWarsJsonText);
            //NOTE: We leverage Internal Methods and Classes here to get lower level access for Unit Testing and Quicker Debugging...
            var graphqlSerializer = FlurlGraphQLNewtonsoftJsonSerializer.FromFlurlSerializer(new NewtonsoftJsonSerializer());
            var graphqlResult = graphqlSerializer.Deserialize<NewtonsoftGraphQLResult>(this.NestedPaginatedBooksAndAuthorsJsonText);

            var newtonsoftJsonGraphQLProcessor = new FlurlGraphQLNewtonsoftJsonResponseRewriteProcessor(
                graphqlResult.Data,
                graphqlResult.Errors,
                graphqlSerializer as FlurlGraphQLNewtonsoftJsonSerializer
            );

            var bookResults = newtonsoftJsonGraphQLProcessor.LoadTypedResults<Book>().ToGraphQLConnectionResultsInternal();

            Assert.IsNotNull(bookResults);
            Assert.AreEqual(2, bookResults.Count);

            var firstBook = bookResults[0];
            Assert.IsNotNull(firstBook);
            Assert.IsFalse(firstBook?.Name?.IsNullOrEmpty());
            
            var firstAuthor = firstBook.Authors.FirstOrDefault();
            Assert.IsNotNull(firstAuthor);
            Assert.IsFalse(firstAuthor?.FirstName?.IsNullOrEmpty());
            Assert.AreEqual(8, firstAuthor.AuthoredBooks.Length);
            Assert.AreEqual(3, firstAuthor.EditedBooks.Length);
        }

        [TestMethod]
        public void TestNewtonsoftJsonParsingOfNestedPaginatedGraphQLResultsUsingRewriteProcessor()
        {
            var jsonText = this.NestedPaginatedStarWarsJsonText;

            //NOTE: We leverage Internal Methods and Classes here to get lower level access for Unit Testing and Quicker Debugging...
            var graphqlSerializer = FlurlGraphQLNewtonsoftJsonSerializer.FromFlurlSerializer(new NewtonsoftJsonSerializer());
            var graphqlResult = graphqlSerializer.Deserialize<NewtonsoftGraphQLResult>(jsonText);

            var newtonsoftJsonGraphQLProcessor = new FlurlGraphQLNewtonsoftJsonResponseRewriteProcessor(
                graphqlResult.Data,
                graphqlResult.Errors,
                graphqlSerializer as FlurlGraphQLNewtonsoftJsonSerializer
            );

            var characterResults = newtonsoftJsonGraphQLProcessor.LoadTypedResults<StarWarsCharacter>().ToGraphQLConnectionResultsInternal();

            AssertCursorPaginatedResultsAreValid(characterResults);
        }

        private void AssertCursorPaginatedResultsAreValid(IGraphQLConnectionResults<StarWarsCharacter> characterResults)
        {
            Assert.IsNotNull(characterResults);
            Assert.IsTrue(characterResults is IGraphQLConnectionResults<StarWarsCharacter>);
            Assert.AreEqual(2, characterResults.Count);

            Assert.IsNotNull(characterResults.TotalCount);
            Assert.IsTrue(characterResults.TotalCount > characterResults.Count);
            Assert.IsNotNull(characterResults.PageInfo);
            Assert.IsTrue(characterResults.PageInfo.HasNextPage);
            Assert.IsFalse(characterResults.PageInfo.HasPreviousPage);
            Assert.IsFalse(string.IsNullOrWhiteSpace(characterResults.PageInfo.StartCursor));
            Assert.IsFalse(string.IsNullOrWhiteSpace(characterResults.PageInfo.EndCursor));

            foreach (var result in characterResults)
            {
                Assert.IsNotNull(result);
                Assert.IsFalse(string.IsNullOrWhiteSpace(result.Cursor));
                TestContext.WriteLine($"Character [{result.PersonalIdentifier}] [{result.Name}] [{result.Height}]");

                foreach (var friend in result.Friends)
                {
                    Assert.IsNotNull(friend);
                    Assert.IsTrue(friend.PersonalIdentifier > 0);
                    Assert.IsFalse(string.IsNullOrWhiteSpace(friend.Name));
                    Assert.IsFalse(string.IsNullOrWhiteSpace(friend.Cursor));
                    TestContext.WriteLine($"   Friend [{friend.PersonalIdentifier}] [{friend.Name}] [{friend.Height}]");
                }
            }
        }

        #region Test Helpers

        private IFlurlGraphQLResponseProcessor CreateDefaultSystemTextJsonGraphQLResponseProcessor(string jsonText)
        {
            //NOTE: We leverage Internal Methods and Classes here to get lower level access for Unit Testing and Quicker Debugging...
            var graphqlSerializer = FlurlGraphQLSystemTextJsonSerializer.FromFlurlSerializer(new DefaultJsonSerializer());
            var graphqlResult = graphqlSerializer.Deserialize<SystemTextJsonGraphQLResult>(jsonText);

            return new FlurlGraphQLSystemTextJsonResponseProcessor(
                graphqlResult.Data,
                graphqlResult.Errors,
                graphqlSerializer as FlurlGraphQLSystemTextJsonSerializer
            );
        }

        private IFlurlGraphQLResponseProcessor CreateDefaultNewtonsoftJsonGraphQLResponseProcessor(string jsonText)
        {
            //NOTE: We leverage Internal Methods and Classes here to get lower level access for Unit Testing and Quicker Debugging...
            var graphqlSerializer = FlurlGraphQLNewtonsoftJsonSerializer.FromFlurlSerializer(new NewtonsoftJsonSerializer());
            var graphqlResult = graphqlSerializer.Deserialize<NewtonsoftGraphQLResult>(jsonText);

            return new FlurlGraphQLNewtonsoftJsonResponseConverterProcessor(
                graphqlResult.Data,
                graphqlResult.Errors,
                graphqlSerializer as FlurlGraphQLNewtonsoftJsonSerializer
            );
        }

        #endregion
    }
}