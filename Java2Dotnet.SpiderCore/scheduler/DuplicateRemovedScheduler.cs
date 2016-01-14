using System.Runtime.CompilerServices;
using Java2Dotnet.Spider.Core.Scheduler.Component;
using Java2Dotnet.Spider.Redial;
using log4net;

namespace Java2Dotnet.Spider.Core.Scheduler
{
	/// <summary>
	/// Remove duplicate urls and only push urls which are not duplicate.
	/// </summary>
	public abstract class DuplicateRemovedScheduler : IScheduler
	{
		protected static readonly ILog Logger = LogManager.GetLogger(typeof(DuplicateRemovedScheduler));

		protected IDuplicateRemover DuplicateRemover { get; set; } = new HashSetDuplicateRemover();

		public void Push(Request request, ISpider spider)
		{
			RedialManagerConfig.RedialManager.AtomicExecutor.Execute("scheduler-push", () =>
			{
				if (!DuplicateRemover.IsDuplicate(request, spider) || ShouldReserved(request))
				{
					PushWhenNoDuplicate(request, spider);
				}
			});
		}

		public virtual void Init(ISpider spider)
		{
		}

		public virtual Request Poll(ISpider spider)
		{
			return null;
		}

		protected virtual void PushWhenNoDuplicate(Request request, ISpider spider)
		{
		}

		/// <summary>
		/// 用于如果URL执行失败, 重新添加回TargetUrls时因Hash而不能重新加入队列的问题
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		private bool ShouldReserved(Request request)
		{
			var cycleTriedTimes = (int?)request.GetExtra(Request.CycleTriedTimes);

			return cycleTriedTimes > 0;
		}
	}
}