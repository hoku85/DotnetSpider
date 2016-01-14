using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Extension.Scheduler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Java2Dotnet.Spider.Extension.Test
{
	[TestClass]
	public class RedisSchedulerTest
	{
		[TestMethod]
		public void RedisTest()
		{
			RedisScheduler redisScheduler = new RedisScheduler("localhost", "");

			ISpider spider = new TestSpider();
			Request request = new Request("http://www.ibm.com/developerworks/cn/java/j-javadev2-22/", 1, null);
			request.PutExtra("1", "2");
			redisScheduler.Push(request, spider);
			redisScheduler.Poll(spider);
			//System.out.println(poll);
		}
	}

	internal class TestSpider : ISpider
	{
		public string Identify => "1";

		public Site Site => null;
		public void Start()
		{
		}

		public void Run()
		{
		}

		public void Stop()
		{
		}

		public void Exit()
		{

		}
	};
}
