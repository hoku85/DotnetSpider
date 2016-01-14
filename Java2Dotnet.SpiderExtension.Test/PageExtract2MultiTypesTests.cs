using System.Collections.Generic;
using System.Linq;
using System.Text;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Core.Scheduler;
using Java2Dotnet.Spider.Extension.DbSupport.Dapper;
using Java2Dotnet.Spider.Extension.DbSupport.Dapper.Attributes;
using Java2Dotnet.Spider.Extension.Model;
using Java2Dotnet.Spider.Extension.Model.Attribute;
using Java2Dotnet.Spider.Extension.Pipeline;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Java2Dotnet.Spider.Extension.Test
{
	[TestClass]
	public class PageExtract2MultiTypesTests
	{
		[ExtractBy(Value = "//*[@id='nav_menu']/a[1]")]
		[Scheme("cnblogs", "yuanzi")]
		public class Yuanzi : SpiderEntity
		{
			[ExtractBy(Value = ".")]
			[StoredAs("name", StoredAs.ValueType.Varchar, false, 20)]
			public string Name { get; set; }
		}

		[ExtractBy(Value = "//*[@id='nav_menu']/a[2]")]
		[Scheme("cnblogs", "jinghua")]
		public class Jinghua : SpiderEntity
		{
			[ExtractBy(Value = ".")]
			[StoredAs("name", StoredAs.ValueType.Varchar, false, 20)]
			public string Name { get; set; }
		}

		[TestMethod]
		public void PageExtract2MultiTypes()
		{
			OoSpider ooSpider = OoSpider.Create(new Site { SleepTime = 1000, Encoding = Encoding.UTF8 }, new QueueDuplicateRemovedScheduler(), new CollectorPageModelPipeline(), typeof(Yuanzi), typeof(Jinghua));
			ooSpider.SetEmptySleepTime(15000);
			ooSpider.SetThreadNum(1);
			ooSpider.ModelPipeline.CachedSize = 1;
			ooSpider.AddStartUrls(new List<string> { "http://www.cnblogs.com/" });
			var results = ((CollectorPageModelPipeline)ooSpider.Pipelines[0]).GetCollected();
			Assert.AreEqual("园子", results[typeof(Yuanzi)][0].Name);
			Assert.AreEqual("新闻", results[typeof(Jinghua)][0].Name);
		}

		[TestMethod]
		public void PageExtract2MultiTypes2()
		{
			OoSpider ooSpider = OoSpider.Create(new Site { SleepTime = 1000, Encoding = Encoding.UTF8 }, new QueueDuplicateRemovedScheduler(), new CollectorPageModelPipeline(), typeof(Yuanzi), typeof(Jinghua));
			ooSpider.SetEmptySleepTime(15000);
			ooSpider.SetThreadNum(1);
			ooSpider.ModelPipeline.CachedSize = 1;
			ooSpider.AddStartUrls(new List<string> { "http://www.cnblogs.com/" });
			var collectorPageModelPipeline = (CollectorPageModelPipeline)ooSpider.Pipelines[0];

			var results = collectorPageModelPipeline.GetCollected();
			Assert.AreEqual("新闻", results[typeof(Jinghua)][0].Name);
		}

		[TestMethod]
		public void PageExtract2MultiTypes3()
		{
			OoSpider ooSpider = OoSpider.Create(new Site { SleepTime = 1000, Encoding = Encoding.UTF8 }, new QueueDuplicateRemovedScheduler(), new DatabasePipeline(), typeof(Yuanzi), typeof(Jinghua));
			ooSpider.SetEmptySleepTime(15000);
			ooSpider.SetThreadNum(1);
			ooSpider.ModelPipeline.CachedSize = 1;
			ooSpider.AddStartUrl("http://www.cnblogs.com/");
			ooSpider.Run();
			DataRepository dataRepository = new DataRepository(typeof(Jinghua));
			Assert.AreEqual("新闻", dataRepository.GetWhere("id>0").ToList()[0].Name);
			dataRepository.Execute("drop database cnblogs");
		}
	}
}
