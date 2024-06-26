﻿#nullable enable

/// <summary>
/// Main script called from the Custom Connector
/// </summary>
public partial class Script : ScriptBase
{
	/// <summary>
	/// Handle the different Operations in the Connector. Each operation has is't own processor
	/// </summary>
	/// <returns>The resulting HTTP response send back from the Custom connctor to the client</returns>
	public override async Task<HttpResponseMessage> ExecuteAsync()
	{
		// initialize the Logger in HTML Agility Pack
		Trace.Logger = Context.Logger;

		// Fix doing requests from the Custom Connector test pane
		var authHeader = Context.Request.Headers.Authorization;
		Context.Request.Headers.Clear();
		Context.Request.Headers.Authorization = authHeader;

		try
		{
			switch (Context.OperationId)
			{
				case "QueryDocumentFromString":
					var stringProcessor = new QueryDocumentFromStringProcessor(Context);
					return await stringProcessor.Process(CancellationToken).ConfigureAwait(false);
				case "QueryDocumentFromUrl":
					var urlProcessor = new QueryDocumentFromUrlProcessor(Context);
					return await urlProcessor.Process(CancellationToken).ConfigureAwait(false);
				case "Internal-GetSchema":
					var schemaProcessor = new GetSchemaProcessor(Context);
					return await schemaProcessor.Process(CancellationToken).ConfigureAwait(false);

				default:
					return await Context.SendAsync(Context.Request, CancellationToken).ConfigureAwait(false);
			}
		}
		catch (Exception ex)
		{
			Context.Logger.Log(LogLevel.Critical, ex, "Error while processing Operation ID '{operationId}'", Context.OperationId);
			var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
			response.Content = new StringContent(ex.ToString());
			return response;
		}
	}

	public class QueryDocumentFromUrlProcessor : QueryDocumentProcessor
	{
		public QueryDocumentFromUrlProcessor(IScriptContext context) : base(context)
		{
		}

		protected override async Task LoadHtmlDocumentAsync(HtmlDocument doc, CancellationToken token)
		{
			if (string.IsNullOrWhiteSpace(Request?.Url))
				throw new ArgumentException("Url is not provided", "url");

			var request = new HttpRequestMessage(HttpMethod.Get, Request!.Url);
			var response = await Context.SendAsync(request, token);

			if (!response.Content.Headers.ContentType.MediaType.ToLowerInvariant().StartsWith("text/html"))
				throw new InvalidOperationException("Retrieved document from URL is not an HTML document");

			using (var stream = await response.Content.ReadAsStreamAsync())
				doc.Load(stream);
		}
	}

	public class QueryDocumentFromStringProcessor : QueryDocumentProcessor
	{
		public QueryDocumentFromStringProcessor(IScriptContext context) : base(context)
		{
		}

		protected override Task LoadHtmlDocumentAsync(HtmlDocument doc, CancellationToken token)
		{
			if (string.IsNullOrWhiteSpace(Request?.Html))
				throw new ArgumentException("HTML is not provided", "html");

			doc.LoadHtml(Request!.Html);

			return Task.CompletedTask;
		}
	}

	public abstract class QueryDocumentProcessor
	{
		protected IScriptContext Context { get; }
		protected QueryRequest? Request { get; private set; }

		protected QueryDocumentProcessor(IScriptContext context)
		{
			Context = context;
		}

		protected abstract Task LoadHtmlDocumentAsync(HtmlDocument doc, CancellationToken token);

		public async Task<HttpResponseMessage> Process(CancellationToken token)
		{
			string content = await Context.Request.Content.ReadAsStringAsync() ?? throw new ArgumentException("Request does not include a valid body");
			Request = JsonConvert.DeserializeObject<QueryRequest>(content) ?? throw new ArgumentException("Request does not include a valid body");

			try
			{
				Request.CompileScopeQuery();
			}
			catch (XPathException)
			{
				return new HttpResponseMessage(HttpStatusCode.BadRequest)
				{
					Content = CreateJsonContent(JsonConvert.SerializeObject(new
					{
						error = $"Invalid query for provided for the Scope"
					}))
				};
			}

			foreach (var query in Request.Queries)
			{
				try
				{
					query.CompileQuery();
				}
				catch (XPathException)
				{
					return new HttpResponseMessage(HttpStatusCode.BadRequest)
					{
						Content = CreateJsonContent(JsonConvert.SerializeObject(new
						{
							error = $"Invalid query for {query.Id ?? query.Query}"
						}))
					};
				}
			}

			var doc = new HtmlDocument();
			await LoadHtmlDocumentAsync(doc, token);
			var result = QueryDocument(doc);
			return new HttpResponseMessage()
			{
				Content = CreateJsonContent(JsonConvert.SerializeObject(result))
			};
		}

		private object QueryDocument(HtmlDocument doc)
		{
			if (Request!.CompiledScopeQuery != null)
			{
				var scopeNodes = doc.DocumentNode.SelectNodes(Request.CompiledScopeQuery);

				List<Dictionary<string, object?>> results = new List<Dictionary<string, object?>>();
				foreach (var item in scopeNodes)
				{
					HtmlDocument innerDoc = new HtmlDocument();
					innerDoc.LoadHtml(item.OuterHtml);

					results.Add(QueryNode(innerDoc.DocumentNode));
				}

				return results;
			}
			else
				return QueryNode(doc.DocumentNode);
		}

		private Dictionary<string, object?> QueryNode(HtmlNode scopeNode)
		{
			var result = new Dictionary<string, object?>();

			foreach (var query in Request!.Queries)
			{
				var id = query.Id ?? query.Query;

				if (query.SelectMultiple)
				{
					var nodes = scopeNode.SelectNodes(query.CompiledQuery);
					var multipleResults = new List<string?>();
					result.Add(id, multipleResults);
					if (nodes != null)
					{
						foreach (var node in nodes)
						{
							AddResult(query, node, multipleResults.Add);
						}
					}
				}
				else
				{
					var node = scopeNode.SelectSingleNode(query.CompiledQuery);
					AddResult(query, node, val => result.Add(id, val));
				}
			}

			return result;
		}

		private void AddResult(HtmlQuery query, HtmlNode? node, Action<string?> add)
		{
			if (node is null)
			{
				add(null);
				return;
			}

			switch (query.ResultMode)
			{
				case ResultMode.OuterHtml:
					add(node.OuterHtml.Trim());
					return;
				case ResultMode.Text:
					add(node.InnerText.Trim());
					return;
				case ResultMode.Attribute:
					if (!string.IsNullOrWhiteSpace(query.Attribute))
					{
						var attr = node.Attributes[query.Attribute];
						add(attr?.Value?.Trim());
					}
					else
					{
						add(null);
					}
					return;
				case ResultMode.InnerHtml:
				default:
					add(node.InnerHtml.Trim());
					return;
			}
		}
	}

	public class GetSchemaProcessor
	{
		private IScriptContext _context;

		public GetSchemaProcessor(IScriptContext context)
		{
			_context = context;
		}

		internal async Task<HttpResponseMessage> Process(CancellationToken cancellationToken)
		{
			string content = await _context.Request.Content.ReadAsStringAsync() ?? throw new ArgumentException("Request does not include a valid body");
			var request = JsonConvert.DeserializeObject<QueryRequest>(content) ?? throw new ArgumentException("Request does not include a valid body");

			var properties = new JObject();
			var schema = new JObject
			{
				{ "x-ms-summary", "Query Results"},
				{ "description", "All query results" },
				{ "type", "object" },
				{ "properties", properties },

			};

			foreach (var query in request.Queries)
			{
				string name = query.Id ?? query.Query;

				if (string.IsNullOrEmpty(name))
					continue;

				var querySchema = new JObject
				{
					{ "type", "string" },
					{ "x-ms-summary", "'name'" },
					{ "description", $"Result for '{name}'"},
				};

				if (query.ResultMode == ResultMode.Attribute)
				{
					querySchema.Add("x-nullable", "true");
				}
				else if (query.ResultMode != ResultMode.Text)
				{
					querySchema.Add("format", "html");
				}

				if (query.SelectMultiple)
				{
					var itemSchema = querySchema;
					querySchema = new JObject
					{
						{ "type", "array" },
						{ "items", itemSchema },
						{ "x-ms-summary", $"List of '{name}'"},
						{ "description", $"List of results for '{name}'"},
					};
				}

				properties.Add(name, querySchema);
			}

			return new HttpResponseMessage()
			{
				Content = CreateJsonContent(schema.ToString())
			};

		}
	}

	public class QueryRequest
	{
		public string? Url { get; set; }
		public string? Html { get; set; }

		public string? ScopeQuery { get; set; }
		[JsonIgnore]
		public XPathExpression? CompiledScopeQuery { get; private set; }

		public IEnumerable<HtmlQuery> Queries { get; set; } = null!;

		public void CompileScopeQuery()
		{
			if (ScopeQuery != null)
			{
				CompiledScopeQuery = Helpers.CompileQuery(ScopeQuery);
			}
		}
	}

	public class HtmlQuery
	{
		public string? Id { get; set; }
		public string Query { get; set; } = null!;
		[JsonIgnore]
		public XPathExpression? CompiledQuery { get; private set; }
		public bool SelectMultiple { get; set; }
		[JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
		public ResultMode ResultMode { get; set; }
		public string? Attribute { get; set; }

		public void CompileQuery()
		{
			CompiledQuery = Helpers.CompileQuery(Query);
		}
	}

	public static class Helpers
	{
		public static XPathExpression CompileQuery(string query)
		{
			// Try convert a css selector from Power Automate Desktop to Xpath Query
			if (query.StartsWith("html >"))
			{
				query = "//" + query.Replace(" > ", "/");
				query = Regex.Replace(query, @":eq\((?<no>\d)\)", m => "[" + (int.TryParse(m.Groups["no"].Value, out int no) ? no + 1 : m.Groups["no"].Value) + "]");
			}

			return XPathExpression.Compile(query);
		}
	}

	public enum ResultMode
	{
		Unset,
		InnerHtml,
		OuterHtml,
		Text,
		Attribute
	}
}
#nullable disable