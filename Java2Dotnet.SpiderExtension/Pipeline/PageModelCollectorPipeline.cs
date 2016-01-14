using System;
using System.Collections;
using System.Collections.Generic;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Core.Pipeline;

namespace Java2Dotnet.Spider.Extension.Pipeline
{
	internal sealed class PageModelCollectorPipeline : ICollectorPipeline
	{
		private readonly CollectorPageModelPipeline _collectorPipeline = new CollectorPageModelPipeline();
		private readonly Type _type;
		private readonly Type _actualType;

		public PageModelCollectorPipeline(Type type)
		{
			_type = type;

			bool isGeneric = typeof(IEnumerable).IsAssignableFrom(type);
			_actualType = isGeneric ? type.GenericTypeArguments[0] : type;
		}

		public ICollection GetCollected()
		{
			return _collectorPipeline.GetCollected();
		}

		//[MethodImplAttribute(MethodImplOptions.Synchronized)]
		public void Process(ResultItems resultItems, ISpider spider)
		{
			if (resultItems == null)
			{
				return;
			}

			Dictionary<Type, List<dynamic>> resultDictionary = new Dictionary<Type, List<dynamic>>();

			dynamic data = resultItems.GetResultItem(_actualType.FullName);

			if (typeof(IEnumerable).IsAssignableFrom(_type))
			{
				if (resultDictionary.ContainsKey(_actualType))
				{
					resultDictionary[_actualType].AddRange(data);
				}
				else
				{
					List<dynamic> list = new List<dynamic>();
					list.AddRange(data);
					resultDictionary.Add(_actualType, list);
				}
			}
			else
			{
				if (resultDictionary.ContainsKey(_actualType))
				{
					resultDictionary[_actualType].Add(data);
				}
				else
				{
					resultDictionary.Add(_actualType, new List<dynamic> { data });
				}
			}

			_collectorPipeline.Process(resultDictionary, spider);
		}

		public void Dispose()
		{
			_collectorPipeline.Dispose();
		}
	}
}
