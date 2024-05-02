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

			//var request = new HttpRequestMessage(HttpMethod.Get, Request!.Url);
			Context.Request.Content = null;
			Context.Request.Method = HttpMethod.Get;
			Context.Request.RequestUri = new Uri(Request!.Url);
			var response = await Context.SendAsync(Context.Request, token);

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

			foreach (var query in Request.Queries)
			{
				try
				{
					query.CompileXPath();
				}
				catch (XPathException)
				{
					return new HttpResponseMessage(HttpStatusCode.BadRequest)
					{
						Content = CreateJsonContent(JsonConvert.SerializeObject(new
						{
							error = $"Invalid XPath query for {query.Id ?? query.XPath}"
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

		private Dictionary<string, object?> QueryDocument(HtmlDocument doc)
		{
			var result = new Dictionary<string, object?>();

			foreach (var query in Request!.Queries)
			{
				var id = query.Id ?? query.XPath;

				if (query.SelectMultiple)
				{
					var nodes = doc.DocumentNode.SelectNodes(query.XPath);
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
					var node = doc.DocumentNode.SelectSingleNode(query.XPath);
					AddResult(query, node, val => result.Add(id, val));
				}
			}

			return result;
		}

		private void AddResult(Query query, HtmlNode? node, Action<string?> add)
		{
			if (node is null)
			{
				add(null);
				return;
			}

			if (!string.IsNullOrWhiteSpace(query.Attribute))
			{
				var attr = node.Attributes[query.Attribute];
				add(attr?.Value);
			}
			else
			{
				add(node.InnerHtml);
			}
		}
	}

	public class QueryRequest
	{
		public string? Url { get; set; }
		public string? Html { get; set; }

		public IEnumerable<Query> Queries { get; set; } = null!;
	}

	public class Query
	{
		public string? Id { get; set; }
		public string XPath { get; set; } = null!;
		public XPathExpression? CompiledXPath { get; private set; }
		public bool SelectMultiple { get; set; }
		public string? Attribute { get; set; }

		public void CompileXPath()
		{
			// Try convert a css selector from Power Automate Desktop to Xpath Query
			if (XPath.StartsWith("html >"))
			{
				XPath = "//" + XPath.Replace(" > ", "/");
				XPath = Regex.Replace(XPath, @":eq\((?<no>\d)\)", m => "[" + (int.TryParse(m.Groups["no"].Value, out int no) ? no + 1 : m.Groups["no"].Value) + "]");
			}

			CompiledXPath = XPathExpression.Compile(XPath);
		}
	}
}
#nullable disable