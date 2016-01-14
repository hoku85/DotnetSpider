using System.Collections.Generic;
using System.Text;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Extension.Model;
using Java2Dotnet.Spider.Extension.Model.Attribute;
using Java2Dotnet.Spider.Extension.Pipeline;

namespace Java2Dotnet.Spider.Samples.Model
{
	[TargetUrl(new[] { "http://www.oschina.net/question/\\d+_\\d+*" })]
	[HelpUrl(Value = new[] { "http://www.oschina.net/question/*" })]
	[ExtractBy(Value = "//*[@id=\"OSC_Content\"]/div[1]/div[1]/div[4]/ul/li")]
	public class OschinaAnswer : IAfterExtractor
	{
		[ExtractBy(Value = "//span[@class='user_info']/a")]
		public string User { get; set; }

		[ExtractBy(Value = "//div[@class='detail']/text()")]
		public string Content { get; set; }

		public static void Run()
		{
			Site site = new Site { Encoding = Encoding.UTF8 };
			site.AddStartUrl("http://www.oschina.net/question/1995445_2136783");
			OoSpider.Create(site, new CollectorPageModelPipeline(), typeof(List<OschinaAnswer>)).Run();
		}

		public void AfterProcess(Page page)
		{
		}
	}
}
