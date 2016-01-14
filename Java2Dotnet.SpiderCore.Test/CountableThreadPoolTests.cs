using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Java2Dotnet.Spider.Core.Test
{
	[TestClass]
	public class CountableThreadPoolTests
	{
		[TestMethod]
		public void CountableThreadPoolTest()
		{
			/*构建一个threadPool*/
			var threadPool = new CountableThreadPool();
			for (int i = 0; i <= 10; i++)
			{
				threadPool.Push((obj) =>
				{
					Thread.Sleep(1000 * 30);
					return true;
				}, "");
			}
			Thread.Sleep(1000 * 10);
			Assert.AreEqual(threadPool.ThreadAlive, 5);
			Thread.Sleep(1000 * 60);
			Assert.IsTrue(threadPool.ThreadAlive == 0);
			threadPool.WaitToExit();
		}
	}
}
