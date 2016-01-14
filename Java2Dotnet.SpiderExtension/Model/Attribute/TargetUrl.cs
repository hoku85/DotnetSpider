using System;

namespace Java2Dotnet.Spider.Extension.Model.Attribute
{
	/// <summary>
	/// Define the url patterns for class.
	/// All urls matching the pattern will be crawled and extracted for new objects.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class TargetUrl : System.Attribute
	{
		/// <summary>
		/// The url patterns for class.
		/// Use regex expression with some changes:
		///      "." stand for literal character "." instead of "any character".
		///      "*" stand for any legal character for url in 0-n length ([^"'#]*) instead of "any length".
		/// </summary>
		public string[] Value { get; }

		/// <summary>
		/// Define the region for url extracting.
		/// Only support XPath, JsonPath
		/// </summary>
		public string SourceRegion { get; }

		public ExtractType ExtractType { get; }

		public TargetUrl(string[] value, string sourceRegion = null, ExtractType extractType = ExtractType.XPath)
		{
			Value = value;
			SourceRegion = sourceRegion;
			ExtractType = extractType;
		}
	}
}
