using System;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Extension.Monitor;
using Java2Dotnet.Spider.Extension.Scheduler;
using Java2Dotnet.Spider.Extension.Utils;

namespace Java2Dotnet.Spider.Extension
{
	public abstract class AbastractRedisSpider : IRedisSpider
	{
		private SafeRedisManagerPool _pool;
		private RedisScheduler _scheduler;

		protected SafeRedisManagerPool Pool => _pool ?? (_pool = new SafeRedisManagerPool(RedisHost, RedisPassword));

		protected RedisScheduler Scheduler
		{
			get
			{
				if (_scheduler == null)
				{
					_scheduler = new RedisScheduler(RedisHost, RedisPassword);
				}
				return _scheduler;
			}
		}

		protected bool IsInited { get; set; }

		private Core.Spider _spider;

		public void Run()
		{
			Prepare();

			_spider?.Run();
		}

		private void Prepare()
		{
			using (var redis = Pool.GetSafeGetClient())
			{
				IDisposable locker = null;
				try
				{
					string key = "locker-" + Name;
					// 取得锁
					Console.WriteLine("Lock: " + key);
					locker = redis.AcquireLock(key, TimeSpan.FromMinutes(10));

					var lockerValue = redis.GetValue(Name);
					bool needInitStartRequest = lockerValue != "init finished";

					Console.WriteLine("Prepare site with paramete: " + needInitStartRequest);

					if (needInitStartRequest)
					{
						PrepareSite();
					}

					Console.WriteLine("Init spider with site.");
					_spider = InitSpider(Site);
					_spider.SaveStatusToRedis = true;
					SpiderMonitor.Instance.Register(_spider);
					_spider.InitComponent();

					if (needInitStartRequest)
					{
						redis.SetValue(Name, "init finished");
					}
				}
				catch (Exception e)
				{
					//测试是否操时
				}
				finally
				{
					Console.WriteLine("Release lock.");
					locker?.Dispose();
				}
			}
		}

		protected abstract void PrepareSite();

		protected abstract Site Site { get; }

		protected abstract Core.Spider InitSpider(Site site);
		public virtual string RedisHost { get; } = "localhost";
		public virtual string RedisPassword { get; } = null;
		public abstract string Name { get; }
	}
}
