using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Java2Dotnet.Spider.Extension.Scheduler;
using Java2Dotnet.Spider.Extension.Utils;
using Newtonsoft.Json;

namespace Java2Dotnet.Spider.Extension.Monitor
{
	public class RedisStatusUpdater
	{
		private static SafeRedisManagerPool _pool;
		private readonly ISpiderStatus _spiderStatus;
		private readonly Core.Spider _spider;

		public RedisStatusUpdater(Core.Spider spider, ISpiderStatus spiderStatus)
		{
			_spider = spider;
			_spiderStatus = spiderStatus;
			string host = ConfigurationManager.AppSettings["redisServer"];
			var password = ConfigurationManager.AppSettings["redisPassword"];
			if (!string.IsNullOrEmpty(host))
			{
				_pool = new SafeRedisManagerPool(host, password);
			}
		}

		public void Run()
		{
			Task.Factory.StartNew(() =>
			{
				while (true)
				{
					try
					{
						UpdateStatus();
					}
					catch (Exception)
					{
						// ignored
					}

					Thread.Sleep(1000);
				}
				// ReSharper disable once FunctionNeverReturns
			});
		}

		public void UpdateStatus()
		{
			try
			{
				using (var redis = _pool?.GetSafeGetClient())
				{
					if (redis == null)
					{
						return;
					}

					object status = new
					{
						_spiderStatus.Name,
						_spiderStatus.ErrorPageCount,
						_spiderStatus.LeftPageCount,
						_spiderStatus.PagePerSecond,
						_spiderStatus.StartTime,
						_spiderStatus.EndTime,
						_spiderStatus.Status,
						_spiderStatus.SuccessPageCount,
						_spiderStatus.ThreadCount,
						_spiderStatus.TotalPageCount,
						_spiderStatus.AliveThreadCount
					};
					redis.SetEntryInHash(RedisScheduler.TaskStatus, _spider.Identify, JsonConvert.SerializeObject(status));
				}
			}
			catch (Exception)
			{
				// ignored
			}
		}
	}
}
