public class Crc32
{
	public int CheckSum { get; }

	internal void AddToCRC32(int _)
	{
	}
}

partial class Trace
{
	public static ILogger Logger { get; set; }
	partial void WriteLineIntern(string message, string category)
	{
		Logger?.LogDebug(message, category);
	}
}
