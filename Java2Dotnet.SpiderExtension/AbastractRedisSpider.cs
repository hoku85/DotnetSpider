using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Extension.Scheduler;
using ServiceStack.Redis;

namespace Java2Dotnet.Spider.Extension
{
	public abstract class AbastractRedisSpider : IRedisSpider
	{
		private RedisManagerPool _pool;
		private RedisScheduler _scheduler;

		protected RedisManagerPool Pool => _pool ?? (_pool = new RedisManagerPool(new List<string> { RedisHost }, new RedisPoolConfig { MaxPoolSize = 100 }));

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
			using (var redis = Pool.GetClient())
			{
				IDisposable locker = null;
				try
				{
					string key = "locker-" + Name;
					// 取得锁
					redis.Password = RedisPassword;
					locker = redis.AcquireLock(key);

					var lockerValue = redis.GetValue(Name);
					bool needInitStartRequest = lockerValue != "finished";
					Site site = PrepareSite(needInitStartRequest);
					if (needInitStartRequest)
					{
						redis.SetValue(Name, "finished");
					}
					_spider = ExecuteSpider(site);
					_spider.InitComponent();
				}
				catch (Exception)
				{
					//测试是否操时
				}
				finally
				{
					locker?.Dispose();
				}
			}
		}

		protected abstract Site PrepareSite(bool needInitStartRequest);

		protected abstract Core.Spider ExecuteSpider(Site site);
		public abstract string RedisHost { get; }
		public abstract string RedisPassword { get; }
		public abstract string Name { get; }
	}
}
