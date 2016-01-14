using System;

namespace Java2Dotnet.Spider.Core.Pipeline
{
	/// <summary>
	/// Pipeline is the persistent and offline process part of crawler. 
	/// The interface Pipeline can be implemented to customize ways of persistent.
	/// </summary>
	public interface IPipeline : IDisposable
	{
		/// <summary>
		/// Process extracted results.
		/// </summary>
		/// <param name="resultItems"></param>
		/// <param name="spider"></param>
		void Process(ResultItems resultItems, ISpider spider);
	}
}