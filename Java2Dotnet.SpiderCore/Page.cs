using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Java2Dotnet.Spider.Core.Selector;
using Java2Dotnet.Spider.Core.Utils;

namespace Java2Dotnet.Spider.Core
{
	/// <summary>
	/// Object storing extracted result and urls to fetch. 
	/// </summary>
	public class Page
	{
		public const string Images = "580c9065-0f44-47e9-94ea-b172d5a730c0";

		private Html _html;
		private Json _json;

		/// <summary>
		/// Url of current page
		/// </summary>
		/// <returns></returns>
		public string Url { get; set; }

		/// <summary>
		/// Get url of current page
		/// </summary>
		/// <returns></returns>
		public string TargetUrl { get; set; }

		public string Title { get; set; }

		/// <summary>
		/// Get request of current page
		/// </summary>
		/// <returns></returns>
		public Request Request { get; }

		public bool IsNeedCycleRetry { get; set; }

		public ResultItems ResultItems { get; } = new ResultItems();

		public int StatusCode { get; set; }

		public string RawText { get; set; }

		public bool MissTargetUrls { get; set; }

		public bool IsSkip
		{
			get { return ResultItems.IsSkip; }
			set { ResultItems.IsSkip = value; }
		}

		public HashSet<Request> TargetRequests { get; } = new HashSet<Request>();

		public Page(Request request)
		{
			Request = request;
			ResultItems.Request = request;
		}

		/// <summary>
		/// Store extract results
		/// </summary>
		/// <param name="key"></param>
		/// <param name="field"></param>
		public void AddResultItem(string key, dynamic field)
		{
			ResultItems.AddResultItem(key, field);
		}

		/// <summary>
		/// Get html content of page
		/// </summary>
		/// <returns></returns>
		public Html HtmlDocument
		{
			get
			{
				return _html ?? (_html = new Html(RawText, Request.Url));
			}
			set { _html = value; }
		}

		/// <summary>
		/// Get json content of page
		/// </summary>
		/// <returns></returns>
		public Json GetJson()
		{
			return _json ?? (_json = new Json(RawText));
		}

		/// <summary>
		/// Add urls to fetch
		/// </summary>
		/// <param name="requests"></param>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public void AddTargetRequests(IList<string> requests)
		{
			foreach (string s in requests)
			{
				if (string.IsNullOrEmpty(s) || s.Equals("#") || s.StartsWith("javascript:"))
				{
					continue;
				}
				string s1 = UrlUtils.CanonicalizeUrl(s, Url);
				TargetRequests.Add(new Request(s1, Request.NextDepth, Request.Extras));
			}
		}

		/// <summary>
		/// Add urls to fetch
		/// </summary>
		/// <param name="requests"></param>
		/// <param name="priority"></param>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public void AddTargetRequests(IList<string> requests, int priority)
		{
			foreach (string s in requests)
			{
				if (string.IsNullOrEmpty(s) || s.Equals("#") || s.StartsWith("javascript:"))
				{
					continue;
				}
				string s1 = UrlUtils.CanonicalizeUrl(s, Url);
				Request request = new Request(s1, Request.NextDepth, Request.Extras) { Priority = priority };
				TargetRequests.Add(request);
			}
		}

		/// <summary>
		/// Add url to fetch
		/// </summary>
		/// <param name="requestString"></param>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public void AddTargetRequest(string requestString)
		{
			if (string.IsNullOrEmpty(requestString) || requestString.Equals("#"))
			{
				return;
			}

			requestString = UrlUtils.CanonicalizeUrl(requestString, Url);
			TargetRequests.Add(new Request(requestString, Request.NextDepth, Request.Extras));
		}

		/// <summary>
		/// Add requests to fetch
		/// </summary>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public void AddTargetRequest(Request request)
		{
			TargetRequests.Add(request);
		}

		public override string ToString()
		{
			return $"Page{{request='{Request}', resultItems='{ResultItems}', rawText='{RawText}', url={Url}, statusCode={StatusCode}, targetRequests={TargetRequests}}}";
		}
	}
}
