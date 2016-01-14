using System.Collections.Generic;
using System;

namespace Java2Dotnet.Spider.Core.Processor
{
	/// <summary>
	/// A simple PageProcessor.
	/// </summary>
	public class SimplePageProcessor : IPageProcessor
	{
		private readonly string _urlPattern;

		public SimplePageProcessor(string startUrl, string urlPattern)
		{
			Site = new Site();
			Site.AddStartUrl(startUrl);
			Uri url = new Uri(startUrl);
			Site.Domain = url.Host;
			//compile "*" expression to regex
			_urlPattern = "(" + urlPattern.Replace(".", "\\.").Replace("*", "[^\"'#]*") + ")";
		}

		public void Process(Page page)
		{
			IList<string> requests = page.HtmlDocument.Links().Regex(_urlPattern).GetAll();
			//add urls to fetch
			page.AddTargetRequests(requests);
			//extract by XPath
			page.AddResultItem("title", page.HtmlDocument.XPath("//title"));
			page.AddResultItem("html", page.HtmlDocument.ToString());
			//extract by Readability
			page.AddResultItem("content", page.HtmlDocument.SmartContent());
		}

		/// <summary>
		/// Get the site settings
		/// </summary>
		public Site Site { get; }
	}
}
