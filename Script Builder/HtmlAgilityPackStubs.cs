internal static class Utilities
{
	public static TValue GetDictionaryValueOrDefault<TKey, TValue>(Dictionary<TKey, TValue> dict, TKey key, TValue defaultValue = default(TValue)) where TKey : class
	{
		TValue value;
		if (!dict.TryGetValue(key, out value))
			return defaultValue;
		return value;
	}

#if !(METRO || NETSTANDARD1_3 || NETSTANDARD1_6)
	internal static object To(this Object @this, Type type)
	{
		if (@this != null)
		{
			Type targetType = type;
			if (@this.GetType() == targetType)
			{
				return @this;
			}
			if (@this == DBNull.Value)
			{
				return null;
			}
		}
		return @this;
	}
#endif
}

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

