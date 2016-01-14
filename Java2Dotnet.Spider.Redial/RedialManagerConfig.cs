using Java2Dotnet.Spider.Redial.RedialManager;

namespace Java2Dotnet.Spider.Redial
{
	public static class RedialManagerConfig
	{
		public static IRedialManager RedialManager = ZookeeperRedialManager.Instance;
	}
}
