using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Core.Utils;
using Java2Dotnet.Spider.Extension.DbSupport.Dapper;
using Java2Dotnet.Spider.Redial;

namespace Java2Dotnet.Spider.Extension.Pipeline
{
	public enum OperateType
	{
		Insert,
		Update
	}

	public sealed class DatabasePipeline : IPageModelPipeline
	{
		private readonly static log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(DatabasePipeline));
		private readonly OperateType _operateType;
		public long TotalCount => _totalCount.Value;
		private readonly ConcurrentDictionary<Type, IDataRepository> _cache = new ConcurrentDictionary<Type, IDataRepository>();

		public DatabasePipeline(OperateType operateType = OperateType.Insert)
		{
			_operateType = operateType;
		}

		private readonly AutomicLong _totalCount = new AutomicLong(0);

		public void Process(Dictionary<Type, List<dynamic>> data, ISpider spider)
		{
			if (data == null)
			{
				return;
			}

			foreach (var pair in data)
			{
				Type type = pair.Key;
				IDataRepository dataRepository = null;

				if (!type.IsGenericType)
				{
					if (_cache.ContainsKey(type))
					{
						dataRepository = _cache[type];
						_totalCount.Inc();
					}
					else
					{
						dataRepository = new DataRepository(type);
						dataRepository.CreateSheme();
						dataRepository.CreateTable();
						_cache.TryAdd(type, dataRepository);
					}
				}
				else
				{
					IList list = pair.Value;
					if (list.Count > 0)
					{
						type = list[0].GetType();

						if (_cache.ContainsKey(type))
						{
							dataRepository = _cache[type];
						}
						else
						{
							dataRepository = new DataRepository(type);
							dataRepository.CreateSheme();
							dataRepository.CreateTable();
							_cache.TryAdd(type, dataRepository);
						}

						for (int i = 0; i < list.Count; ++i)
						{
							_totalCount.Inc();
						}
					}
				}

				SaveOrUpate(dataRepository, pair);
			}
		}

		public void Dispose()
		{
			_cache.Clear();
		}

		private void SaveOrUpate(IDataRepository dataRepository, KeyValuePair<Type, List<dynamic>> data)
		{
			switch (_operateType)
			{
				case OperateType.Insert:
					{
						RedialManagerConfig.RedialManager.AtomicExecutor.Execute("db-insert", () =>
						{
							for (int i = 0; i < 100; i++)
							{
								try
								{
									dataRepository?.Insert(data.Value);
									break;
								}
								catch (Exception e)
								{
									Logger.Warn($"Try to save data to DB failed. Times: {i + 1}", e);
									Thread.Sleep(2000);
									// ignored
								}
							}
						});

						break;
					}
				case OperateType.Update:
					{
						RedialManagerConfig.RedialManager.AtomicExecutor.Execute("db-update", () =>
						{
							dataRepository?.Update(data.Value);
						});

						break;
					}
			}
		}
	}
}
