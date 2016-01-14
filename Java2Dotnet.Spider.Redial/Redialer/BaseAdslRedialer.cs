using System.Configuration;

namespace Java2Dotnet.Spider.Redial.Redialer
{
	public abstract class BaseAdslRedialer : IRedialer
	{
		protected readonly string Interface;
		protected readonly string User;
		protected readonly string Password;

		protected BaseAdslRedialer()
		{
			var interface1 = ConfigurationManager.AppSettings["redialInterface"];
			Interface = string.IsNullOrEmpty(interface1) ? "宽带连接" : interface1;

			User = ConfigurationManager.AppSettings["redialUser"];
			Password = ConfigurationManager.AppSettings["redialPassword"];
		}

		protected BaseAdslRedialer(string interface1, string user, string password)
		{
			Interface = interface1;
			User = user;
			Password = password;
		}

		public abstract void Redial();
	}
}
