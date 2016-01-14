using System;
using System.Collections.Generic;
using System.Threading;
using ServiceStack.Redis;

namespace Java2Dotnet.Spider.Extension.Utils
{
	public class SafeRedisManagerPool : RedisManagerPool
	{
		public string Password { get; set; }

		public SafeRedisManagerPool(string host, string password) : base(host, new RedisPoolConfig() { MaxPoolSize = 100 })
		{
			Password = password;
			SetRedisResolver();
		}

		public SafeRedisManagerPool(IEnumerable<string> hosts, RedisPoolConfig config, string password) : base(hosts, config)
		{
			Password = password;

			SetRedisResolver();
		}

		public IRedisClient GetSafeGetClient()
		{
			while (true)
			{
				try
				{
					return GetClient();
				}
				catch (Exception e)
				{
					Console.WriteLine("Error: Get redis client failed: " + e);
					Thread.Sleep(500);
				}
			}
		}

		private void SetRedisResolver()
		{
			RedisResolver.ClientFactory = endpoint =>
			{
				var client = new RedisClient(endpoint);
				client.Password = Password;
				return client;
			};
		}
	}
}
