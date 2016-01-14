using System;
using System.Collections.Generic;
using System.Linq;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Core.Pipeline;
using Java2Dotnet.Spider.Core.Scheduler;
using Java2Dotnet.Spider.Extension.Pipeline;
using Java2Dotnet.Spider.Extension.Processor;

namespace Java2Dotnet.Spider.Extension.Model
{
	/// <summary>
	/// The spider for page model extractor.
	/// </summary>
	public class OoSpider : Core.Spider
	{
		public ModelPipeline ModelPipeline { get; private set; }

		private OoSpider(string identify, ModelPageProcessor modelPageProcessor, IScheduler scheduler)
			: base(identify, modelPageProcessor, scheduler)
		{
			//_modelPageProcessor = modelPageProcessor;
		}

		private OoSpider(string identify, Site site, IScheduler scheduler, IPageModelPipeline[] pageModelPipeline, params Type[] modelTypes)
			: this(identify, ModelPageProcessor.Create(site, modelTypes), scheduler)
		{
			Init(pageModelPipeline, modelTypes);
		}

		private OoSpider(string identify, ModelPageProcessor processor, IScheduler scheduler, IPageModelPipeline[] pageModelPipeline, params Type[] modelTypes)
			: this(identify, processor, scheduler)
		{
			Init(pageModelPipeline, modelTypes);
		}

		public void SetCustomizeTargetUrls(Func<Page, IList<string>> getCustomizeTargetUrls)
		{
			((ModelPageProcessor)PageProcessor).GetCustomizeTargetUrls = getCustomizeTargetUrls;
		}

		//protected override List<ICollectorPipeline> GetCollectorPipeline(params Type[] types)
		//{
		//	return types.Select(type => new PageModelCollectorPipeline(type)).Cast<ICollectorPipeline>().ToList();
		//}

		public static OoSpider Create(Site site, IPageModelPipeline pageModelPipeline, params Type[] pageModels)
		{
			if (pageModelPipeline == null)
			{
				throw new SpiderExceptoin("PageModelPipeline can't be null.");
			}
			return Create(site.Domain, site, new QueueDuplicateRemovedScheduler(), new[] { pageModelPipeline }, pageModels);
		}

		public static OoSpider Create(Site site, IScheduler scheduler, IPageModelPipeline pageModelPipeline, params Type[] pageModels)
		{
			if (pageModelPipeline == null)
			{
				throw new SpiderExceptoin("PageModelPipeline can't be null.");
			}
			return Create(site.Domain, site, scheduler, new[] { pageModelPipeline }, pageModels);
		}

		public static OoSpider Create(string identify, Site site, IScheduler scheduler, IPageModelPipeline pageModelPipeline, params Type[] pageModels)
		{
			if (pageModelPipeline == null)
			{
				throw new SpiderExceptoin("PageModelPipeline can't be null.");
			}
			return Create(identify, site, scheduler, new[] { pageModelPipeline }, pageModels);
		}

		public static OoSpider Create(string identify, Site site, IScheduler scheduler, IPageModelPipeline[] pageModelPipelines, params Type[] pageModels)
		{
			if (pageModelPipelines == null || pageModelPipelines.Length == 0)
			{
				throw new SpiderExceptoin("PageModelPipelines can't be null.");
			}
			return new OoSpider(identify, site, scheduler, pageModelPipelines, pageModels);
		}

		public static OoSpider Create<T>(string identify, Site site, IScheduler scheduler, IPageModelPipeline[] pageModelPipeline, params Type[] pageModels) where T : ModelPageProcessor
		{
			var processor = ModelPageProcessor.Create<T>(site, pageModels);
			return new OoSpider(identify, processor, scheduler, pageModelPipeline, pageModels);
		}

		public OoSpider AddPageModel(IPageModelPipeline pageModelPipeline, params Type[] pageModels)
		{
			var processor = ((ModelPageProcessor)PageProcessor);
			foreach (Type pageModel in pageModels)
			{
				processor.AddPageModel(pageModel);
				ModelPipeline.Put(pageModel, pageModelPipeline);
			}
			return this;
		}

		private void Init(IPageModelPipeline[] pageModelPipeline, Type[] modelTypes)
		{
			ModelPipeline = new ModelPipeline();

			AddPipeline(ModelPipeline);

			foreach (Type modelType in modelTypes)
			{
				if (pageModelPipeline != null)
				{
					foreach (var modelPipeline in pageModelPipeline)
					{
						ModelPipeline.Put(modelType, modelPipeline);
					}
				}
				//_pageModelTypes.Add(modelType);
			}
		}
	}
}