using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Java2Dotnet.Spider.Core;

namespace Java2Dotnet.Spider.Extension
{
	public interface IRedisSpider
	{
		void Run();
		string Name { get; }
	}
}
