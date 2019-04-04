using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Search.IndexStrategy;
using Search.SearchIndex;
using Search.UnitTests.Helpers;
using TechTalk.SpecFlow;

namespace Search.UnitTests.Steps
{
    [Binding]
    public class SearchIndexSteps
    {
        [Given(@"uid field ""(.*)""")]
		public void GivenUidField(string uidField)
        {
			ScenarioContext.Current.Set(uidField, "uidField");
			var searcher = new Searcher(uidField);
			ScenarioContext.Current.Set(searcher);
        }

		[Given(@"using substring index strategy")]
		public void GivenUsingSubstringIndexStrategy()
		{
			var searcher = ScenarioContext.Current.Get<Searcher>();
			var uidField = ScenarioContext.Current.Get<string>("uidField");
			var newSearcher = new Searcher(uidField).WithIndexStrategy(new AllSubstringsIndexStrategy()); ;
			newSearcher.SetDocuments(searcher.Documents);
			newSearcher.AddIndex(searcher.SearchableFields.ToArray());
			ScenarioContext.Current.Set(newSearcher);
		}

		[Given(@"A set of documents")]
		public void GivenASetOfDocuments(Table table)
        {
			var books = table.CreateInstances<Book>();
			var searcher = ScenarioContext.Current.Get<Searcher>();
			searcher.SetDocuments(JArray.FromObject(books));
        }

		[Given(@"add the following extra documents")]
		public void GivenAddTheFollowingExtraDocuments(Table table)
		{
			var books = table.CreateInstances<Book>();
			var searcher = ScenarioContext.Current.Get<Searcher>();
			foreach(var book in books)
			{
				searcher.AddDocument(JToken.FromObject(book));
			}
		}

		[Given(@"add the following complex documents")]
		public void GivenAddTheFollowingComplexDocuments(string multilineText)
		{
			var searcher = ScenarioContext.Current.Get<Searcher>();
			var documents = JArray.Parse(multilineText);
			foreach(var doc in documents)
			{
				searcher.AddDocument(doc);
			}
		}


		[Given(@"searchable fields")]
		public void GivenSearchableFields(Table table)
        {
			var searchableFields = new List<string>();
			foreach(TableRow row in table.Rows)
			{
				searchableFields.Add(row["field"]);
			}

			var searcher = ScenarioContext.Current.Get<Searcher>();
			searcher.AddIndex(searchableFields.ToArray());
        }

		[Given(@"add extra searchable fields")]
		public void GivenAddExtraSearchableFields(Table table)
		{
			var searcher = ScenarioContext.Current.Get<Searcher>();
			foreach (TableRow row in table.Rows)
			{
				searcher.AddIndex(row["field"]);
			}
		}


		[When(@"I index the documents")]
		public void WhenIIndexTheDocuments()
        {
			var searcher = ScenarioContext.Current.Get<Searcher>();
			var tfidfIndex = searcher.SearchIndex as TfIdfSearchIndex;
			tfidfIndex.Should().NotBeNull();
			Dictionary<string, double> termFrequencies = new Dictionary<string, double>();
			foreach(var token in tfidfIndex.Tokens)
			{
				var frequency = tfidfIndex.CalculateIdf(token, searcher.Documents);
				termFrequencies.Add(token, frequency);
			}
			ScenarioContext.Current.Set(termFrequencies, "termFrequencies");

			var uidField = ScenarioContext.Current.Get<string>("uidField");
			Dictionary<string, Dictionary<string, double>> tokenDocTfIdf = new Dictionary<string, Dictionary<string, double>>();
			foreach(var token in tfidfIndex.Tokens)
			{
				var docTfIdf = new Dictionary<string, double>();
				foreach(var doc in searcher.Documents)
				{
					var tfidf = tfidfIndex.CalculateTfIdf(
						new List<string> { token }, doc, searcher.Documents);
					var uid = doc.Value<string>(uidField);
					docTfIdf.Add(uid, tfidf);
				}
				tokenDocTfIdf.Add(token, docTfIdf);
			}
			ScenarioContext.Current.Set(tokenDocTfIdf, "tfidf");
        }

		[When(@"I search the documents with query ""(.*)""")]
		public void WhenISearchTheDocumentsWithQuery(string query)
		{
			var searcher = ScenarioContext.Current.Get<Searcher>();
			var results = searcher.Search(query);
			ScenarioContext.Current.Set(results, "searchResults");
		}

		[Then(@"I should get the following IDF values")]
		public void ThenIShouldGetTheFollowingIDFValues(Table table)
        {
			var searcher = ScenarioContext.Current.Get<Searcher>();
			var termFrequencies = ScenarioContext.Current.Get<Dictionary<string, double>>("termFrequencies");
            foreach(TableRow row in table.Rows)
			{
				var token = row[0];
				var numDocsWithToken = int.Parse(row[1]);
				if (numDocsWithToken > 0)
				{
					var expectedIdf = 1 + Math.Log((double)searcher.Documents.Count / (1 + numDocsWithToken));
					termFrequencies.Should().ContainKey(token);
					var actualFrequency = termFrequencies[token];
					actualFrequency.Should().BeInRange(expectedIdf - 0.1, expectedIdf + 0.1);
				}
				else
				{
					termFrequencies.Should().NotContainKey(token);
				}
			}
        }

		[Then(@"I should get the following TfIdf values")]
		public void ThenIShouldGetTheFollowingTfIdfValues(Table table)
		{
			var searcher = ScenarioContext.Current.Get<Searcher>();
			var tokenDocTfIdf = ScenarioContext.Current.Get<Dictionary<string, Dictionary<string, double>>>("tfidf");
			foreach (TableRow row in table.Rows)
			{
				var token = row[0];
				var docId = row[1];
				var numDocsWithToken = double.Parse(row[2]);
				var tokenCountInDoc = int.Parse(row[3]);
				var expectedIdf = 1 + Math.Log((double)searcher.Documents.Count / (1 + numDocsWithToken));
				var expectedTfIdf = expectedIdf * tokenCountInDoc;
				if (numDocsWithToken > 0)
				{
					tokenDocTfIdf.Should().ContainKey(token);
					if (tokenCountInDoc > 0)
					{
						tokenDocTfIdf[token].Should().ContainKey(docId);
						var actualTfIdf = tokenDocTfIdf[token][docId];
						actualTfIdf.Should().BeInRange(expectedTfIdf - 0.1, expectedTfIdf + 0.1);
					}
					else
					{
						if (tokenDocTfIdf[token].ContainsKey(docId))
						{
							var actualTfIdf = tokenDocTfIdf[token][docId];
							actualTfIdf.Should().BeInRange(-0.1, 0.1);
						}
					}
				}
				else
				{
					tokenDocTfIdf.Should().NotContainKey(token);
				}
			}
		}

		[Then(@"I should get the following TfIdf values given no document")]
		public void ThenIShouldGetTheFollowingTfIdfValuesGivenNoDocument(Table table)
		{
			var tokenDocTfIdf = ScenarioContext.Current.Get<Dictionary<string, Dictionary<string, double>>>("tfidf");
			foreach (TableRow row in table.Rows)
			{
				var token = row[0];
				var expectedTfIdf = double.Parse(row[1]);
				tokenDocTfIdf.Should().NotContainKey(token);
			}
		}

		[Then(@"I should get the following score for document with id ""(.*)""")]
		public void ThenIShouldGetTheFollowingScoreForDocumentWithId(string docId, string multilineText)
		{
			var uidField = ScenarioContext.Current.Get<string>("uidField");
			var searcher = ScenarioContext.Current.Get<Searcher>();
			var expectedResults = JArray.Parse(multilineText);
			double expectedTfIdf = 0;
			var tokens = new List<string>();
			var doc = searcher.Documents.First(d => d.Value<string>(uidField) == docId);
			foreach (var expected in expectedResults)
			{
				var token = expected.Value<string>("token");
				var numDocsWithToken = expected.Value<int>("numDocsWithToken");
				var tokenCountInDoc = expected.Value<int>("tokenCountInDoc");
				var expectedIdf = 1 + Math.Log((double)searcher.Documents.Count / (1 + numDocsWithToken));
				expectedTfIdf += expectedIdf * tokenCountInDoc;

				tokens.Add(token);
			}
			var actualTfidf = (searcher.SearchIndex as TfIdfSearchIndex).CalculateTfIdf(tokens, doc, searcher.Documents);

			actualTfidf.Should().BeInRange(expectedTfIdf - 0.01, expectedTfIdf + 0.01);
		}

		[Then(@"I should get the following search results")]
		public void ThenIShouldGetTheFollowingSearchResults(Table table)
		{
			var searchResults = ScenarioContext.Current.Get<JArray>("searchResults");
			searchResults.Should().NotBeNull();
			searchResults.Count.Should().Be(table.Rows.Count);

			var uidField = ScenarioContext.Current.Get<string>("uidField");
			for(var i = 0; i < table.Rows.Count; i++)
			{
				var expectedDocId = table.Rows[i][0];
				var actualDoc = searchResults[i];
				var actualDocId = actualDoc.Value<string>(uidField);
				actualDocId.Should().Be(expectedDocId);
			}
		}

		[Then(@"I should get the following (.*) in output")]
		public void ThenIShouldGetTheFollowingInOutput(string docIds)
		{
			var expectedDocIds = docIds.Split(",", StringSplitOptions.RemoveEmptyEntries)
				.Select(id => id.Trim('"')).ToList();
			var searchResults = ScenarioContext.Current.Get<JArray>("searchResults");
			searchResults.Should().NotBeNull();
			searchResults.Count.Should().Be(expectedDocIds.Count);

			var uidField = ScenarioContext.Current.Get<string>("uidField");
			for (var i = 0; i < expectedDocIds.Count;i++)
			{
				var actualDoc = searchResults[i];
				var actualDocId = actualDoc.Value<string>(uidField);
				actualDocId.Should().Be(expectedDocIds[i]);
			}
		}

	}

	internal class Book
	{
		public string Id { get; set; }
		public string Title { get; set; }
	}
}
