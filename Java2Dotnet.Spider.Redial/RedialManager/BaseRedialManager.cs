using System.Threading;
using Java2Dotnet.Spider.Redial.AtomicExecutor;
using Java2Dotnet.Spider.Redial.NetworkValidater;
using Java2Dotnet.Spider.Redial.Redialer;
using log4net;

namespace Java2Dotnet.Spider.Redial.RedialManager
{
	public abstract class BaseRedialManager : IRedialManager
	{
		protected static readonly ILog Logger = LogManager.GetLogger(typeof(BaseRedialManager));

		public abstract void WaitforRedialFinish();
		public abstract RedialResult Redial();

		public INetworkValidater NetworkValidater { get; set; } = new DefalutNetworkValidater();
		public IRedialer Redialer { get; set; } = new H3CSshAdslRedialer();
		public abstract IAtomicExecutor AtomicExecutor { get; }

		/// <summary>
		/// Redial interval, default is 5 minitues.
		/// </summary>
		public int RedialInterval { get; set; } = 5;

		/// <summary>
		/// 代码还在测试时大部分在Office里是不能也没有必要拨号（企业网）
		/// 可以测试这个值以跳过拨号以免生产环境中拨号引起调试阻塞
		/// </summary>
		public bool Skip { get; set; } = false;

		protected void RedialInternet()
		{
			Redialer?.Redial();

			NetworkValidater?.Wait();

			Thread.Sleep(2000);
		}
	}
}
