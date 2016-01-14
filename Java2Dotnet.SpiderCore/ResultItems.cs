using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Java2Dotnet.Spider.Core
{
	/// <summary>
	/// Object contains extract results. 
	/// It is contained in Page and will be processed in pipeline.
	/// </summary>
	public class ResultItems
	{
		private readonly Dictionary<string, dynamic> _fields = new Dictionary<string, dynamic>();

		[MethodImpl(MethodImplOptions.Synchronized)]
		public dynamic GetResultItem(string key)
		{
			return _fields.ContainsKey(key) ? _fields[key] : null;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public ResultItems AddResultItem(string key, dynamic value)
		{
			_fields.Add(key, value);
			return this;
		}

		public Dictionary<string, dynamic> Results => _fields;

		public Request Request { get; set; }

		/// <summary>
		/// Whether to skip the result. 
		/// Result which is skipped will not be processed by Pipeline.
		/// </summary>
		public bool IsSkip { get; set; }

		public override string ToString()
		{
			return "ResultItems{ fields=" + _fields + ", request=" + Request + ", skip=" + IsSkip + '}';
		}
	}
}