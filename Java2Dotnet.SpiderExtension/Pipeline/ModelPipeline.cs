using System;
using System.Collections;
using System.Collections.Generic;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Core.Pipeline;

namespace Java2Dotnet.Spider.Extension.Pipeline
{
	/// <summary>
	/// The extension to Pipeline for page model extractor.
	/// </summary>
	public class ModelPipeline : CachedPipeline
	{
		private class PageModelPipelineInfo
		{
			public PageModelPipelineInfo(bool isGeneric, IPageModelPipeline pipeline)
			{
				IsGeneric = isGeneric;
				Pipeline = pipeline;
			}

			public bool IsGeneric { get;  }
			public IPageModelPipeline Pipeline { get;  }
		}

		private readonly Dictionary<Type, PageModelPipelineInfo> _pageModelPipelines = new Dictionary<Type, PageModelPipelineInfo>();

		public ModelPipeline Put(Type type, IPageModelPipeline pageModelPipeline)
		{
			bool isGeneric = typeof(IEnumerable).IsAssignableFrom(type);
			var actuallyType = isGeneric ? type.GenericTypeArguments[0] : type;
			_pageModelPipelines.Add(actuallyType, new PageModelPipelineInfo(isGeneric, pageModelPipeline));

			return this;
		}

		protected override void Process(List<ResultItems> resultItemsList, ISpider spider)
		{
			if (resultItemsList == null || resultItemsList.Count == 0)
			{
				return;
			}

			foreach (var pipelineEntry in _pageModelPipelines)
			{
				Dictionary<Type, List<dynamic>> resultDictionary = new Dictionary<Type, List<dynamic>>();
				foreach (var resultItems in resultItemsList)
				{
					dynamic data = resultItems.GetResultItem(pipelineEntry.Key.FullName);
					Type type = data.GetType();

					if (pipelineEntry.Value.IsGeneric)
					{
						if (resultDictionary.ContainsKey(type))
						{
							resultDictionary[type].AddRange(data);
						}
						else
						{
							List<dynamic> list = new List<dynamic>();
							list.AddRange(data);
							resultDictionary.Add(type, list);
						}
					}
					else
					{
						if (resultDictionary.ContainsKey(type))
						{
							resultDictionary[type].Add(data);
						}
						else
						{
							resultDictionary.Add(type, new List<dynamic> { data });
						}
					}
				}
				pipelineEntry.Value.Pipeline.Process(resultDictionary, spider);
			}
		}
	}
}