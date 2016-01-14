using System;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using Java2Dotnet.Spider.Core.Downloader;
using Java2Dotnet.Spider.Core.Pipeline;
using Java2Dotnet.Spider.Core.Processor;
using Java2Dotnet.Spider.Core.Scheduler;
using Java2Dotnet.Spider.Core.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Java2Dotnet.Spider.Core.Test
{
	[TestClass]
	public class SpiderTest
	{
		[TestMethod]
		public void TestStartAndStop()
		{
			Spider spider = Spider.Create(new SimplePageProcessor("http://www.oschina.net/", "http://www.oschina.net/*")).AddPipeline(new TestPipeline()).SetThreadNum(1);
			spider.Start();
			Thread.Sleep(10000);
			spider.Stop();
			Thread.Sleep(10000);
			spider.Start();
			Thread.Sleep(10000);
		}

		[TestMethod]
		public void TestStreamCopy()
		{
			HttpWebRequest req = (HttpWebRequest)WebRequest.Create("http://www.oschina.net/");
			req.Method = "GET";
			using (WebResponse wr = req.GetResponse())
			{
				MemoryStream stream = new MemoryStream();
				wr.GetResponseStream()?.CopyTo(stream);
			}
		}

		private class TestPipeline : IPipeline
		{
			public void Process(ResultItems resultItems, ISpider spider)
			{
				foreach (var entry in resultItems.Results)
				{
					Console.WriteLine($"{entry.Key}:{entry.Value}");
				}
			}

			public void Dispose()
			{
			}
		}

		[Ignore]
		[TestMethod]
		public void TestWaitAndNotify()
		{
			for (int i = 0; i < 10000; i++)
			{
				Console.WriteLine("round " + i);
				TestRound();
			}
		}

		private void TestRound()
		{
			Spider spider = Spider.Create(new TestPageProcessor(), new TestScheduler()).SetThreadNum(10);
			spider.Run();
		}

		private class TestScheduler : IScheduler
		{
			private AtomicInteger _count = new AtomicInteger();
			private Random _random = new Random();

			public void Init(ISpider spider)
			{
			}

			public void Push(Request request, ISpider spider)
			{
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			public Request Poll(ISpider spider)
			{
				if (_count.Inc() > 1000)
				{
					return null;
				}
				if (_random.Next(100) > 90)
				{
					return null;
				}
				return new Request("test", 1, null);
			}
		}

		private class TestPageProcessor : IPageProcessor
		{
			public void Process(Page page)
			{
				page.IsSkip = true;
			}

			public Site Site => new Site { SleepTime = 0 };
		}

		public class TestDownloader : IDownloader
		{
			public Page Download(Request request, ISpider spider)
			{
				var page = new Page(request);
				page.RawText = "";
				return page;
			}

			public int ThreadNum { get; set; }
		}
	}
}
