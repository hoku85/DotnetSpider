using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Core.Processor;
using Java2Dotnet.Spider.Core.Selector;
using Java2Dotnet.Spider.Extension.Model;
using Java2Dotnet.Spider.Extension.Model.Formatter;

namespace Java2Dotnet.Spider.Extension.Processor
{
	/// <summary>
	/// The extension to PageProcessor for page model extractor.
	/// </summary>
	public class ModelPageProcessor : IPageProcessor
	{
		private readonly IList<PageModelExtractor> _pageModelExtractorList = new List<PageModelExtractor>();

		public Func<Page, IList<string>> GetCustomizeTargetUrls;

		public static ModelPageProcessor Create(Site site, params Type[] types)
		{
			ModelPageProcessor modelPageProcessor = new ModelPageProcessor(site);
			foreach (Type type in types)
			{
				modelPageProcessor.AddPageModel(type);
			}
			return modelPageProcessor;
		}

		public static T Create<T>(Site site, params Type[] types) where T : ModelPageProcessor
		{
			T t = (T)Activator.CreateInstance(typeof(T), new object[] { site });

			foreach (Type type in types)
			{
				t.AddPageModel(type);
			}
			return t;
		}

		public ModelPageProcessor AddPageModel(Type type)
		{
			PageModelExtractor pageModelExtractor = PageModelExtractor.Create(type);
			_pageModelExtractorList.Add(pageModelExtractor);
			return this;
		}

		protected ModelPageProcessor(Site site)
		{
			Site = site;
		}

		public virtual void Process(Page page)
		{
			foreach (PageModelExtractor pageModelExtractor in _pageModelExtractorList)
			{
				ExtractLinks(page, pageModelExtractor.GetHelpUrlRegionSelector(), pageModelExtractor.GetHelpUrlPatterns());

				dynamic process = pageModelExtractor.Process(page);
				if (process == null || (process is IEnumerable && !((IEnumerable)process).GetEnumerator().MoveNext()))
				{
					continue;
				}
				PostProcessPageModel(process);
				page.AddResultItem(pageModelExtractor.GetActualType().FullName, process);

				if (GetCustomizeTargetUrls == null)
				{
					ExtractLinks(page, pageModelExtractor.GetTargetUrlRegionSelector(), pageModelExtractor.GetTargetUrlPatterns(), pageModelExtractor.GetTargetUrlFormatter());
				}
				else
				{
					page.AddTargetRequests(GetCustomizeTargetUrls(page));
				}
			}
			if (page.ResultItems.Results.Count == 0)
			{
				page.ResultItems.IsSkip = true;
			}
		}

		/// <summary>
		/// 如果找不到则不返回URL, 不然返回的URL太多
		/// </summary>
		/// <param name="page"></param>
		/// <param name="urlRegionSelector"></param>
		/// <param name="urlPatterns"></param>
		/// <param name="formatter"></param>
		private void ExtractLinks(Page page, ISelector urlRegionSelector, IList<Regex> urlPatterns, IObjectFormatter formatter = null)
		{
			var links = urlRegionSelector == null ? new List<string>() : page.HtmlDocument.SelectList(urlRegionSelector).Links().GetAll();

			// check: 仔细考虑是放在前面, 还是在后面做 formatter, 我倾向于在前面. 对targetUrl做formatter则表示Start Url也应该是要符合这个规则的。
			if (formatter != null)
			{
				List<string> tmp = new List<string>();
				foreach (var link in links)
				{
					tmp.Add(formatter.Format(link));
				}
				links = tmp;
			}

			if (urlPatterns == null || urlPatterns.Count == 0)
			{
				page.AddTargetRequests(links);
				return;
			}

			foreach (Regex targetUrlPattern in urlPatterns)
			{
				foreach (string link in links)
				{
					if (targetUrlPattern.IsMatch(link))
					{
						page.AddTargetRequest(new Request(link, page.Request.NextDepth, page.Request.Extras));
					}
				}
			}
		}

		protected virtual void PostProcessPageModel(dynamic obj)
		{
		}

		public Site Site { get; }
	}
}