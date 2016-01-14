using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Web;
using HtmlAgilityPack;
using Java2Dotnet.Spider.Core.Proxy;
using Java2Dotnet.Spider.Core.Utils;
using Java2Dotnet.Spider.Redial;

namespace Java2Dotnet.Spider.Core.Downloader
{
	/// <summary>
	/// The http downloader based on HttpClient.
	/// </summary>
	[Synchronization]
	public class HttpClientDownloader : BaseDownloader
	{
		//private static AutomicLong _exceptionCount = new AutomicLong(0);

		public override Page Download(Request request, ISpider spider)
		{
			if (spider.Site == null)
			{
				return null;
			}

			Site site = spider.Site;

			ICollection<int> acceptStatCode = site.AcceptStatCode;
			var charset = site.Encoding;
			var headers = site.Headers;

			//Logger.InfoFormat("Downloading page {0}", request.Url);

			int statusCode = 0;

			HttpWebResponse response = null;
			try
			{
				var httpWebRequest = GetHttpWebRequest(request, site, headers);

				response = RedialManagerConfig.RedialManager.AtomicExecutor.Execute("downloader-download", h =>
				{
					HttpWebRequest tmpHttpWebRequest = h as HttpWebRequest;
					return (HttpWebResponse)tmpHttpWebRequest?.GetResponse();
				}, httpWebRequest);

				statusCode = (int)response.StatusCode;
				request.PutExtra(Request.StatusCode, statusCode);
				if (StatusAccept(acceptStatCode, statusCode))
				{
					Page page = HandleResponse(request, charset, response, statusCode);

					//page.SetRawText(File.ReadAllText(@"C:\Users\Lewis\Desktop\taobao.html"));

					// 这里只要是遇上登录的, 则在拨号成功之后, 全部抛异常在Spider中加入Scheduler调度
					// 因此如果使用多线程遇上多个Warning Custom Validate Failed不需要紧张, 可以考虑用自定义Exception分开
					ValidatePage(page);

					// 结束后要置空, 这个值存到Redis会导置无限循环跑单个任务
					request.PutExtra(Request.CycleTriedTimes, null);

					httpWebRequest.ServicePoint.ConnectionLimit = int.MaxValue;

					return page;
				}
				else
				{
					throw new SpiderExceptoin("Download failed.");
				}

				//正常结果在上面已经Return了, 到此处必然是下载失败的值.
				//throw new SpiderExceptoin("Download failed.");
			}
			finally
			{
				// 先Close Response, 避免前面语句异常导致没有关闭.
				try
				{
					//ensure the connection is released back to pool
					//check:
					//EntityUtils.consume(httpResponse.getEntity());
					response?.Close();
				}
				catch (Exception e)
				{
					Logger.Warn("Close response fail.", e);
				}
				request.PutExtra(Request.StatusCode, statusCode);
			}
		}

		private bool StatusAccept(ICollection<int> acceptStatCode, int statusCode)
		{
			return acceptStatCode.Contains(statusCode);
		}

		private HttpWebRequest GeneratorCookie(HttpWebRequest httpWebRequest, Request request, Site site)
		{
			string domain = request.Url.Host;

			CookieContainer cookieContainer = new CookieContainer();

			foreach (var cookie in site.AllCookies)
			{
				cookieContainer.Add(new Cookie(cookie.Key, cookie.Value));
			}
			httpWebRequest.CookieContainer = cookieContainer;

			return httpWebRequest;
		}

		private HttpWebRequest GetHttpWebRequest(Request request, Site site, IDictionary headers)
		{
			if (site == null) return null;

			HttpWebRequest httpWebRequest = SelectRequestMethod(request);

			httpWebRequest.UserAgent = site.UserAgent ?? "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:39.0) Gecko/20100101 Firefox/39.0Mozilla/5.0 (Windows NT 10.0; WOW64; rv:39.0) Gecko/20100101 Firefox/39.0";
			httpWebRequest.Accept = site.Accept ?? "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

			if (site.IsUseGzip)
			{
				httpWebRequest.Headers.Add("Accept-Encoding", "gzip");
			}

			// headers
			if (headers != null)
			{
				var enumerator = headers.GetEnumerator();
				while (enumerator.MoveNext())
				{
					var key = enumerator.Key;
					var value = enumerator.Value;
					httpWebRequest.Headers.Add(key.ToString(), value.ToString());
				}
			}

			// cookie
			httpWebRequest = GeneratorCookie(httpWebRequest, request, site);

			//check:
			httpWebRequest.Timeout = site.Timeout;
			httpWebRequest.ContinueTimeout = site.Timeout;
			httpWebRequest.ReadWriteTimeout = site.Timeout;
			httpWebRequest.AllowAutoRedirect = true;

			if (site.HttpProxyPoolEnable)
			{
				HttpHost host = site.GetHttpProxyFromPool();
				httpWebRequest.Proxy = new WebProxy(host.Host, host.Port);
				request.PutExtra(Request.Proxy, host);
			}
			else
			{
				// 避开Fiddler之类的代理
				httpWebRequest.Proxy = null;
			}

			return httpWebRequest;
		}

		private HttpWebRequest SelectRequestMethod(Request request)
		{
			HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(request.Url);
			if (request.Method == null || request.Method.ToUpper().Equals(HttpConstant.Method.Get))
			{
				//default get
				webrequest.Method = HttpConstant.Method.Get;
				return webrequest;
			}
			if (request.Method.ToUpper().Equals(HttpConstant.Method.Post))
			{
				webrequest.Method = HttpConstant.Method.Post;
				return webrequest;
			}
			if (request.Method.ToUpper().Equals(HttpConstant.Method.Head))
			{
				webrequest.Method = HttpConstant.Method.Head;
				return webrequest;
			}
			if (request.Method.ToUpper().Equals(HttpConstant.Method.Put))
			{
				webrequest.Method = HttpConstant.Method.Put;
				return webrequest;
			}
			if (request.Method.ToUpper().Equals(HttpConstant.Method.Delete))
			{
				webrequest.Method = HttpConstant.Method.Delete;
				return webrequest;
			}
			if (request.Method.ToUpper().Equals(HttpConstant.Method.Trace))
			{
				webrequest.Method = HttpConstant.Method.Trace;
				return webrequest;
			}
			throw new ArgumentException("Illegal HTTP Method " + request.Method);
		}

		private Page HandleResponse(Request request, Encoding charset, HttpWebResponse response, int statusCode)
		{
			string content = GetContent(charset, response);
			if (string.IsNullOrEmpty(content))
			{
				throw new SpiderExceptoin($"Download {request.Url} failed.");
			}
			content = HttpUtility.UrlDecode(HttpUtility.HtmlDecode(content), charset);
			Page page = new Page(request);
			page.RawText = content;
			page.TargetUrl = response.ResponseUri.ToString();
			page.Url = request.Url.ToString();
			page.StatusCode = statusCode;
			return page;
		}

		private string GetContent(Encoding charset, HttpWebResponse response)
		{
			byte[] contentBytes = GetContentBytes(response);

			if (charset == null)
			{
				Encoding htmlCharset = GetHtmlCharset(response.ContentType, contentBytes);
				if (htmlCharset != null)
				{
					return htmlCharset.GetString(contentBytes);
				}

				return Encoding.Default.GetString(contentBytes);
			}
			return charset.GetString(contentBytes);
		}

		private byte[] GetContentBytes(HttpWebResponse response)
		{
			Stream stream = null;

			//GZIIP处理  
			if (response.ContentEncoding.Equals("gzip", StringComparison.InvariantCultureIgnoreCase))
			{
				//开始读取流并设置编码方式
				var tempStream = response.GetResponseStream();
				if (tempStream != null) stream = new GZipStream(tempStream, CompressionMode.Decompress);
			}
			else
			{
				//开始读取流并设置编码方式  
				stream = response.GetResponseStream();
			}

			MemoryStream resultStream = new MemoryStream();
			if (stream != null)
			{
				stream.CopyTo(resultStream);
				return resultStream.StreamToBytes();
			}
			return null;
		}

		private Encoding GetHtmlCharset(string contentType, byte[] contentBytes)
		{
			// charset
			// 1、encoding in http header Content-Type
			string value = contentType;
			var encoding = UrlUtils.GetEncoding(value);
			if (encoding != null)
			{
				Logger.DebugFormat("Auto get charset: {0}", encoding);
				return encoding;
			}
			// use default charset to decode first time
			Encoding defaultCharset = Encoding.Default;
			string content = defaultCharset.GetString(contentBytes);
			string charset = null;
			// 2、charset in meta
			if (!string.IsNullOrEmpty(content))
			{
				HtmlDocument document = new HtmlDocument();
				document.LoadHtml(content);
				HtmlNodeCollection links = document.DocumentNode.SelectNodes("//meta");
				if (links != null)
				{
					foreach (var link in links)
					{
						// 2.1、html4.01 <meta http-equiv="Content-Type" content="text/html; charset=UTF-8" />
						string metaContent = link.Attributes["content"] != null ? link.Attributes["content"].Value : "";
						string metaCharset = link.Attributes["charset"] != null ? link.Attributes["charset"].Value : "";
						if (metaContent.IndexOf("charset", StringComparison.Ordinal) != -1)
						{
							metaContent = metaContent.Substring(metaContent.IndexOf("charset", StringComparison.Ordinal), metaContent.Length - metaContent.IndexOf("charset", StringComparison.Ordinal));
							charset = metaContent.Split('=')[1];
							break;
						}
						// 2.2、html5 <meta charset="UTF-8" />
						if (!string.IsNullOrEmpty(metaCharset))
						{
							charset = metaCharset;
							break;
						}
					}
				}
			}
			Logger.DebugFormat("Auto get charset: {0}", charset);
			// 3、todo use tools as cpdetector for content decode
			try
			{
				return Encoding.GetEncoding(string.IsNullOrEmpty(charset) ? "UTF-8" : charset);
			}
			catch
			{
				return Encoding.Default;
			}
		}
	}
}