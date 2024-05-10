using CustomCode.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CustomCode.Tests
{
	[TestClass]
	public class GetSchemaTests
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
				Queries = queries
			};

			IScriptContext context = new UnitTestContext(_loggerFactory, "Internal-GetSchema", null)
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
		public async Task GetSchemaSingleAsync()
		{
			//arrange
			var context = CreateContextWithQueryRequest(new List<Script.HtmlQuery>
				{
					new Script.HtmlQuery {
						Query = "//h1"
					}
				});
			var sut = new Script.GetSchemaProcessor(context);

			//act
			var response = await sut.Process(_testContext.CancellationTokenSource.Token).ConfigureAwait(false);

			//assert
			var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

			JObject result = JObject.Parse(responseBody);
			//Assert.IsTrue(result.ContainsKey("//h1"), "Expected //h1 property");
			//Assert.AreEqual("Example Domain", result["//h1"]!.Value<string>());
		}
	}
}
