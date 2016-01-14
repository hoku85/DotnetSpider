namespace Java2Dotnet.Spider.Extension
{
	public interface IRedisSpider
	{
		void Run();
		string Name { get; }
	}
}
