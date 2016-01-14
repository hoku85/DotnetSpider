using System;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.CompilerServices;
using System.Threading;
using Java2Dotnet.Spider.Redial.AtomicExecutor;
using ServiceStack.Redis;

namespace Java2Dotnet.Spider.Redial.RedialManager
{
	/// <summary>
	/// 用于一台电脑一根拨号线路或多台共用一个交换机一个拨号线路
	/// </summary>
	public class RedisRedialManager : BaseRedialManager, IDisposable
	{
		private static RedisRedialManager _instanse;
		private readonly SafeRedisManagerPool _pool;
		private const string RedialStatusKey = "REDIAL_STATUS";

		public string RedisHost { get; set; } = "localhost";
		private const string RunningRedialStatus = "Running";
		private const string DialingRedialStatus = "Dialing";

		public override IAtomicExecutor AtomicExecutor { get; }

		private RedisRedialManager()
		{
			var tmpRedisHost = ConfigurationManager.AppSettings["redialRedisServer"];
			if (!string.IsNullOrEmpty(tmpRedisHost))
			{
				RedisHost = tmpRedisHost;
			}

			_pool = new SafeRedisManagerPool(new List<string> { RedisHost }, new RedisPoolConfig { MaxPoolSize = 1000 }, "");

			var redialSetting = GetRedialStatus();
			if (redialSetting == null)
			{
				SetRedialStatus(RunningRedialStatus);
			}

			AtomicExecutor = new FileLockerAtomicExecutor(this);
		}

		public static RedisRedialManager Default
		{
			get
			{
				if (_instanse == null)
				{
					_instanse = new RedisRedialManager();
				}
				return _instanse;
			}
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public override void WaitforRedialFinish()
		{
			if (Skip)
			{
				return;
			}

			var redialSetting = GetRedialStatus();

			if (RunningRedialStatus != redialSetting)
			{
				while (true)
				{
					Thread.Sleep(50);
					var redialSetting1 = GetRedialStatus();
					if (redialSetting1 == RunningRedialStatus)
					{
						break;
					}
				}
			}
		}

		private string GetRedialStatus()
		{
			using (var redisClient = _pool.GetClient())
			{
				return redisClient.GetValue(RedialStatusKey);
			}
		}

		private void SetRedialStatus(string value)
		{
			using (var redisClient = _pool.GetClient())
			{
				redisClient.SetValue(RedialStatusKey, value);
			}
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public override RedialResult Redial()
		{
			if (Skip)
			{
				return RedialResult.Skip;
			}

			var redialSetting = GetRedialStatus();
			if (RunningRedialStatus != redialSetting)
			{
				while (true)
				{
					Thread.Sleep(50);
					var redialSetting1 = GetRedialStatus();
					if (redialSetting1 == RunningRedialStatus)
					{
						return RedialResult.OtherRedialed;
					}
				}
			}
			else
			{
				SetRedialStatus(DialingRedialStatus);

				// wait all operation stop.
				Thread.Sleep(5000);

				AtomicExecutor.WaitAtomicAction();

				Logger.Warn("Try to redial network...");

				RedialInternet();

				SetRedialStatus(RunningRedialStatus);

				Logger.Warn("Redial finished.");
				return RedialResult.Sucess;
			}
		}

		public void Dispose()
		{
			_pool?.Dispose();
		}
	}
}
