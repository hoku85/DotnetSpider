using System;
using Java2Dotnet.Spider.Redial;
using log4net;

namespace Java2Dotnet.Spider.Core.Downloader
{
	public class BaseDownloader : IDownloader, IDisposable
	{
		public DownloadValidation DownloadValidation;
		public int ThreadNum { set; get; }

		protected static readonly ILog Logger = LogManager.GetLogger(typeof(BaseDownloader));

		public virtual Page Download(Request request, ISpider spider)
		{
			return null;
		}

		public virtual void Dispose()
		{
		}

		protected void ValidatePage(Page page)
		{
			//customer verify
			if (DownloadValidation != null)
			{
				var validatResult = DownloadValidation(page);

				switch (validatResult)
				{
					case DownloadValidationResult.Failed:
						{
							throw new SpiderExceptoin("Customize validate failed.");
						}
					case DownloadValidationResult.FailedAndNeedRedial:
						{
							RedialManagerConfig.RedialManager?.Redial();
							throw new RedialException("Download failed and need Redial.");
						}
					case DownloadValidationResult.Success:
						{
							break;
						}
					case DownloadValidationResult.Miss:
						{
							page.IsSkip = true;
							break;
						}
				}
			}
		}
	}
}
