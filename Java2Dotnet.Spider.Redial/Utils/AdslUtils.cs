namespace Java2Dotnet.Spider.Redial.Utils
{
	public static class AdslUtil
	{
		public static void Connect(string connectionName, string user, string pass)
		{
			Disconnect(connectionName);
			string arg = $"rasdial \"{connectionName}\" {user} {pass}";
			CmdUtil.InvokeCmd(arg);
		}

		private static void Disconnect(string connectionName)
		{
			string arg = $"rasdial \"{connectionName}\" /disconnect";
			CmdUtil.InvokeCmd(arg);
		}
	}
}
