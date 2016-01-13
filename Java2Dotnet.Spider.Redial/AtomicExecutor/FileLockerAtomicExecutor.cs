﻿using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Java2Dotnet.Spider.Redial.RedialManager;

namespace Java2Dotnet.Spider.Redial.AtomicExecutor
{
	public class FileLockerAtomicExecutor : IAtomicExecutor
	{
		private static readonly string AtomicActionFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DotnetSpider", "AtomicAction");

		public IWaitforRedial WaitforRedial { get; }

		public void Execute(string name, Action action)
		{
			WaitforRedial.WaitforRedialFinish();

			Stream stream = null;
			string id = Path.Combine(AtomicActionFolder, name + "-" + Guid.NewGuid().ToString("N"));
			while (File.Exists(id))
			{
				id = Path.Combine(AtomicActionFolder, name + "-" + Guid.NewGuid().ToString("N"));
				Thread.Sleep(50);
			}
			try
			{
				stream = File.Open(id, FileMode.Create, FileAccess.Write);

				action();
			}
			finally
			{
				stream?.Close();
				SafeDeleteFile(id);
			}
		}

		public void Execute(string name, Action<object> action, object obj)
		{
			WaitforRedial.WaitforRedialFinish();
			Stream stream = null;
			string id = Path.Combine(AtomicActionFolder, name + "-" + Guid.NewGuid().ToString("N"));
			while (File.Exists(id))
			{
				id = Path.Combine(AtomicActionFolder, name + "-" + Guid.NewGuid().ToString("N"));
				Thread.Sleep(50);
			}
			try
			{
				stream = File.Open(id, FileMode.Create, FileAccess.Write);

				action(obj);
			}
			finally
			{
				stream?.Close();
				SafeDeleteFile(id);
			}
		}

		public T Execute<T>(string name, Func<object, T> func, object obj)
		{
			WaitforRedial.WaitforRedialFinish();
			Stream stream = null;
			string id = Path.Combine(AtomicActionFolder, name + "-" + Guid.NewGuid().ToString("N"));
			while (File.Exists(id))
			{
				id = Path.Combine(AtomicActionFolder, name + "-" + Guid.NewGuid().ToString("N"));
				Thread.Sleep(50);
			}
			try
			{
				stream = File.Open(id, FileMode.Create, FileAccess.Write);

				return func(obj);
			}
			finally
			{
				stream?.Close();
				SafeDeleteFile(id);
			}
		}

		public T Execute<T>(string name, Func<T> func)
		{
			Stream stream = null;
			WaitforRedial.WaitforRedialFinish();
			string id = Path.Combine(AtomicActionFolder, name + "-" + Guid.NewGuid().ToString("N"));
			while (File.Exists(id))
			{
				id = Path.Combine(AtomicActionFolder, name + "-" + Guid.NewGuid().ToString("N"));
				Thread.Sleep(50);
			}
			try
			{
				stream = File.Open(id, FileMode.Create, FileAccess.Write);

				return func();
			}
			finally
			{
				stream?.Close();
				while (File.Exists(id))
				{
					try
					{
						File.Delete(id);
						break;
					}
					catch (Exception)
					{
						// ignored
					}
				}
			}
		}

		public void WaitAtomicAction()
		{
			// 等待数据库等操作完成
			while (true)
			{
				if (!Directory.GetFiles(AtomicActionFolder).Any())
				{
					break;
				}

				Thread.Sleep(1);
			}
		}

		internal FileLockerAtomicExecutor(IWaitforRedial waitforRedial)
		{
			WaitforRedial = waitforRedial;
			var di = new DirectoryInfo(AtomicActionFolder);
			if (!di.Exists)
			{
				di.Create();
			}
			Task.Factory.StartNew(() =>
			{
				// 用于删除异常关机残留的文件. 在使用的都是被锁住的, 无法删除
				while (true)
				{
					foreach (var action in Directory.GetFiles(AtomicActionFolder))
					{
						//IntPtr vHandle = _lopen(action, OfReadwrite | OfShareDenyNone);
						//if (vHandle != HfileError)
						//{
						try
						{
							File.Delete(action);
						}
						catch (Exception)
						{
							// ignored
						}
						//}
					}
					Thread.Sleep(1000);
				}
				// ReSharper disable once FunctionNeverReturns
			});
		}

		private void SafeDeleteFile(string path)
		{
			while (File.Exists(path))
			{
				try
				{
					File.Delete(path);
					break;
				}
				catch (Exception)
				{
					// ignored
				}
			}
		}
	}
}
