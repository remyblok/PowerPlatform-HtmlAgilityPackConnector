namespace CustomCode.Tests.Helpers
{
	internal class UnitTestContext : IScriptContext
	{
		private readonly HttpResponseMessage? _sendAsyncResult;

		public UnitTestContext(MsTestLoggerFactory loggerFactory, string operationId, HttpResponseMessage? sendAsyncResult)
		{
			OperationId = operationId;
			_sendAsyncResult = sendAsyncResult;

			Logger = loggerFactory.CreateLogger(operationId);
		}

		public string CorrelationId { get; } = Guid.NewGuid().ToString();

		public string OperationId { get; }

		public HttpRequestMessage Request { get; set; } = new HttpRequestMessage();

		public ILogger Logger { get; }

		public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			if (request != Request)
			{
				HttpClient client = new HttpClient();
				return await client.SendAsync(request, cancellationToken);
			}

			return _sendAsyncResult!;
		}
	}
}
