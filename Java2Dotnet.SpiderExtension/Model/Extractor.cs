using Java2Dotnet.Spider.Core.Selector;

namespace Java2Dotnet.Spider.Extension.Model
{
	/// <summary>
	/// The object contains 'ExtractBy' information.
	/// </summary>
	public class Extractor
	{
		public Extractor(ISelector selector, ExtractSource source, bool notNull, long count = long.MaxValue)
		{
			Selector = selector;
			Source = source;
			NotNull = notNull;
			Count = count;
		}

		public long Count { get; set; }

		public ISelector Selector { get; set; }

		public ExtractSource Source { get; set; }

		public bool NotNull { get; set; }
	}
}