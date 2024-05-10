#nullable enable
using CustomCode.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CustomCode.Tests
{
	[TestClass]
	public class ScriptTests
	{
		private static TestContext _testContext = null!;
		private static MsTestLoggerFactory _loggerFactory = null!;

		[ClassInitialize]
		public static void ClassInit(TestContext testContext)
		{
			_testContext = testContext;
			_loggerFactory = new MsTestLoggerFactory(testContext);
		}


		[ClassCleanup]
		public static void ClassCleanup()
		{
			_loggerFactory?.Dispose();
		}

		private IScriptContext CreateContextWithQueryRequest(IEnumerable<Script.HtmlQuery> queries)
		{
			var request = new Script.QueryRequest()
			{
				Html = File.ReadAllText("Resources\\Test.html"),
				Queries = queries
			};

			IScriptContext context = new UnitTestContext(_loggerFactory, "QueryDocumentFromString", null)
			{
				Request = new HttpRequestMessage()
				{
					Method = HttpMethod.Post,
					Content = ScriptBase.CreateJsonContent(JsonConvert.SerializeObject(request))
				}
			};
			return context;
		}

		[TestMethod]
		public async Task TestSingleSelector()
		{
			//arrange
			var context = CreateContextWithQueryRequest(new List<Script.HtmlQuery>
				{
					new Script.HtmlQuery {
						Query = "//h1"
					}
				});
			var sut = new Script.QueryDocumentFromStringProcessor(context);

			//act
			var response = await sut.Process(_testContext.CancellationTokenSource.Token).ConfigureAwait(false);

			//assert
			var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

			JObject result = JObject.Parse(responseBody);
			Assert.IsTrue(result.ContainsKey("//h1"), "Expected //h1 property");
			Assert.AreEqual("Example Domain", result["//h1"]!.Value<string>());
		}

		[TestMethod]
		public async Task TestSingleSelectorWithId()
		{
			//arrange
			var context = CreateContextWithQueryRequest(new List<Script.HtmlQuery>
				{
					new Script.HtmlQuery {
						Query = "//h1",
						Id = "header"
					}
				});
			var sut = new Script.QueryDocumentFromStringProcessor(context);

			//act
			var response = await sut.Process(_testContext.CancellationTokenSource.Token).ConfigureAwait(false);

			//assert
			var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

			JObject result = JObject.Parse(responseBody);
			Assert.IsTrue(result.ContainsKey("header"), "Expected header property");
			Assert.AreEqual("Example Domain", result["header"]!.Value<string>());
		}

		[TestMethod]
		public async Task TestSingleSelectorWithAttribute()
		{
			//arrange
			var context = CreateContextWithQueryRequest(new List<Script.HtmlQuery>
				{
					new Script.HtmlQuery {
						Query = "//a",
						Id = "href",
						ResultMode = Script.ResultMode.Attribute,
						Attribute = "href"
					}
				});
			var sut = new Script.QueryDocumentFromStringProcessor(context);

			//act
			var response = await sut.Process(_testContext.CancellationTokenSource.Token).ConfigureAwait(false);

			//assert
			var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

			JObject result = JObject.Parse(responseBody);
			Assert.IsTrue(result.ContainsKey("href"), "Expected href property");
			Assert.AreEqual("https://www.iana.org/domains/example", result["href"]!.Value<string>());
		}

		[TestMethod]
		public async Task TestSingleSelectorWithNoResult()
		{
			//arrange
			var context = CreateContextWithQueryRequest(new List<Script.HtmlQuery>
				{
					new Script.HtmlQuery {
						Query = "//h2"
					}
				});
			var sut = new Script.QueryDocumentFromStringProcessor(context);

			//act
			var response = await sut.Process(_testContext.CancellationTokenSource.Token).ConfigureAwait(false);

			//assert
			var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

			JObject result = JObject.Parse(responseBody);
			Assert.IsTrue(result.ContainsKey("//h2"), "Expected //h2 property");
			Assert.AreEqual(null, result["//h2"]!.Value<string>());
		}

		[TestMethod]
		public async Task TestMultiSelector()
		{
			//arrange
			var context = CreateContextWithQueryRequest(new List<Script.HtmlQuery>
				{
					new Script.HtmlQuery {
						Query = "//p",
						SelectMultiple = true
					}
				});
			var sut = new Script.QueryDocumentFromStringProcessor(context);

			//act
			var response = await sut.Process(_testContext.CancellationTokenSource.Token).ConfigureAwait(false);

			//assert
			var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

			JObject result = JObject.Parse(responseBody);
			Assert.IsTrue(result.ContainsKey("//p"), "Expected //p property");
			Assert.IsTrue(result["//p"] is JArray, "Expected property to be array");
			if (result["//p"] is JArray array)
			{
				Assert.AreEqual(2, array.Count);
			}
		}

		[TestMethod]
		public async Task TestMultiSelectorWithAttribute()
		{
			//arrange
			var context = CreateContextWithQueryRequest(new List<Script.HtmlQuery>
				{
					new Script.HtmlQuery {
						Id = "meta",
						Query = "//meta[@content]",
						SelectMultiple = true,
						ResultMode = Script.ResultMode.Attribute,
						Attribute = "content"
					}
				});
			var sut = new Script.QueryDocumentFromStringProcessor(context);

			//act
			var response = await sut.Process(_testContext.CancellationTokenSource.Token).ConfigureAwait(false);

			//assert
			var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

			JObject result = JObject.Parse(responseBody);
			Assert.IsTrue(result.ContainsKey("meta"), "Expected meta property");
			Assert.IsTrue(result["meta"] is JArray, "Expected property to be array");
			if (result["meta"] is JArray array)
			{
				Assert.AreEqual(2, array.Count);
				Assert.AreEqual("text/html; charset=utf-8", array[0].Value<string>());
				Assert.AreEqual("width=device-width, initial-scale=1", array[1].Value<string>());
			}
		}

		[TestMethod]
		public async Task TestMultiSelectorWithNoResult()
		{
			//arrange
			var context = CreateContextWithQueryRequest(new List<Script.HtmlQuery>
				{
					new Script.HtmlQuery {
						Query = "//ul",
						SelectMultiple = true
					}
				});
			var sut = new Script.QueryDocumentFromStringProcessor(context);

			//act
			var response = await sut.Process(_testContext.CancellationTokenSource.Token).ConfigureAwait(false);

			//assert
			var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

			JObject result = JObject.Parse(responseBody);
			Assert.IsTrue(result.ContainsKey("//ul"), "Expected //ul property");
			Assert.IsTrue(result["//ul"] is JArray, "Expected property to be array");
			if (result["//ul"] is JArray array)
			{
				Assert.AreEqual(0, array.Count);
			}
		}

		[TestMethod]
		public async Task TestMultiSelectorWithHtmlFragmentResult()
		{
			//arrange
			var context = CreateContextWithQueryRequest(new List<Script.HtmlQuery>
				{
					new Script.HtmlQuery {
						Id="query",
						Query = "//div",
						SelectMultiple = true
					}
				});
			var sut = new Script.QueryDocumentFromStringProcessor(context);

			//act
			var response = await sut.Process(_testContext.CancellationTokenSource.Token).ConfigureAwait(false);

			//assert
			var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

			JObject result = JObject.Parse(responseBody);
			Assert.IsTrue(result.ContainsKey("query"), "Expected query property");
			Assert.IsTrue(result["query"] is JArray, "Expected property to be array");
			if (result["query"] is JArray array)
			{
				Assert.AreEqual(1, array.Count);
			}
		}

		[TestMethod]
		public async Task TestInvalidXpath()
		{
			//arrange
			var context = CreateContextWithQueryRequest(new List<Script.HtmlQuery>
				{
					new Script.HtmlQuery {
						Query = "[@id='x']",
						Id = "x"
					}
				});
			var sut = new Script.QueryDocumentFromStringProcessor(context);

			//act
			var response = await sut.Process(_testContext.CancellationTokenSource.Token).ConfigureAwait(false);

			//assert
			Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

			var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

			JObject result = JObject.Parse(responseBody);
			Assert.IsTrue(result.ContainsKey("error"), "Expected error property");
			Assert.AreEqual("Invalid query for x", result["error"]!.Value<string>());
		}

		//

		[TestMethod]
		public async Task TestPowerAutomateDesktopQuery()
		{
			//arrange
			var context = CreateContextWithQueryRequest(new List<Script.HtmlQuery>
				{
					new Script.HtmlQuery {
						Query = "html > body > div > p:eq(1) > a",
						Id = "a"
					}
				});
			var sut = new Script.QueryDocumentFromStringProcessor(context);

			//act
			var response = await sut.Process(_testContext.CancellationTokenSource.Token).ConfigureAwait(false);

			//assert
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

			var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

			JObject result = JObject.Parse(responseBody);
			Assert.IsTrue(result.ContainsKey("a"), "Expected a property");
			Assert.AreEqual("More information...", result["a"]!.Value<string>());
		}

		[TestMethod]
		public async Task TestSingleSelectorFromUrl()
		{
			//arrange
			HttpResponseMessage sendAsyncResult = new HttpResponseMessage(HttpStatusCode.OK);
			sendAsyncResult.Content = new StringContent(File.ReadAllText("Resources\\Test.html"), Encoding.UTF8, "text/html");

			var request = new Script.QueryRequest()
			{
				Url = "https://example.com",
				Queries = new List<Script.HtmlQuery>
				{
					new Script.HtmlQuery {
						Query = "//h1"
					}
				}
			};

			IScriptContext context = new UnitTestContext(_loggerFactory, "QueryDocumentFromUrl", sendAsyncResult)
			{
				Request = new HttpRequestMessage()
				{
					Method = HttpMethod.Post,
					Content = ScriptBase.CreateJsonContent(JsonConvert.SerializeObject(request))
				}
			};

			var sut = new Script.QueryDocumentFromUrlProcessor(context);

			//act
			var response = await sut.Process(_testContext.CancellationTokenSource.Token).ConfigureAwait(false);

			//assert
			Assert.AreEqual(HttpMethod.Get, context.Request.Method);
			Assert.AreEqual("https://example.com/", context.Request.RequestUri.ToString());

			var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

			JObject result = JObject.Parse(responseBody);
			Assert.IsTrue(result.ContainsKey("//h1"), "Expected //h1 property");
			Assert.AreEqual("Example Domain", result["//h1"]!.Value<string>());
		}
	}
}
