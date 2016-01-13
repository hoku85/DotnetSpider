﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Java2Dotnet.Spider.Core;
using System.Threading;

namespace Java2Dotnet.Spider.Extension.Test
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
				threadPool.Push((obj, cts) =>
				{
					Thread.Sleep(1000 * 30);
					return 1;
				}, "");
			}
			Thread.Sleep(1000 * 10);
			Assert.AreEqual(threadPool.ThreadAlive, 5);
			Thread.Sleep(1000 * 60);
			Assert.IsTrue(threadPool.ThreadAlive == 0);
			threadPool.WaitToExit();
			threadPool.Shutdown();
		}
	}
}
