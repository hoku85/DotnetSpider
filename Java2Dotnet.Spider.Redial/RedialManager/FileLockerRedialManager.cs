using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using Java2Dotnet.Spider.Redial.AtomicExecutor;

namespace Java2Dotnet.Spider.Redial.RedialManager
{
	/// <summary>
	/// 用于单台电脑
	/// </summary>
	public class FileLockerRedialManager : BaseRedialManager
	{
		private static FileLockerRedialManager _instanse;
		private readonly string _lockerFilePath;
		private readonly int RedialTimeout = 60 * 1000 / 50;

		public override IAtomicExecutor AtomicExecutor { get; }

		public static FileLockerRedialManager Instance
		{
			get
			{
				if (_instanse == null)
				{
					_instanse = new FileLockerRedialManager();
				}
				return _instanse;
			}
		}

		private FileLockerRedialManager()
		{
			var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DotnetSpider");

			DirectoryInfo di = new DirectoryInfo(folder);
			if (!di.Exists)
			{
				di.Create();
			}
			_lockerFilePath = Path.Combine(folder, "redialer.lock");

			AtomicExecutor = new FileLockerAtomicExecutor(this);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public override void WaitforRedialFinish()
		{
			if (Skip)
			{
				return;
			}

			if (File.Exists(_lockerFilePath))
			{
				for (int i = 0; i < RedialTimeout; ++i)
				{
					Thread.Sleep(50);
					if (!File.Exists(_lockerFilePath))
					{
						break;
					}
				}
			}
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public override RedialResult Redial()
		{
			if (Skip)
			{
				return RedialResult.Skip;
			}

			if (File.Exists(_lockerFilePath))
			{
				while (true)
				{
					Thread.Sleep(50);
					if (!File.Exists(_lockerFilePath))
					{
						return RedialResult.OtherRedialed;
					}
				}
			}
			else
			{
				Stream stream = null;

				try
				{
					stream = File.Open(_lockerFilePath, FileMode.Create, FileAccess.Write);

					// wait all operation stop.
					Thread.Sleep(5000);

					Logger.Warn("Wait atomic action to finish...");

					// 等待数据库等操作完成
					AtomicExecutor.WaitAtomicAction();

					Logger.Warn("Try to redial network...");

					RedialInternet();

					Logger.Warn("Redial finished.");
					return RedialResult.Sucess;
				}
				catch (IOException)
				{
					// 有极小可能同时调用File.Open时抛出异常
					return Redial();
				}
				catch (Exception)
				{
					return RedialResult.Failed;
				}
				finally
				{
					stream?.Close();
					File.Delete(_lockerFilePath);
				}
			}
		}
	}
}
