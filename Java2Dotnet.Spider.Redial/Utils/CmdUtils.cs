using System.Diagnostics;
using System.Threading;

namespace Java2Dotnet.Spider.Redial.Utils
{
	public static class CmdUtil
	{
		public static Process InvokeCmd(string cmdArgs)
		{
			Process p = new Process
			{
				StartInfo =
				{
					FileName = "cmd.exe",
					UseShellExecute = false,
					RedirectStandardInput = true,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					CreateNoWindow = true
				}
			};
			p.Start();
			p.StandardInput.WriteLine(cmdArgs);
			Thread.Sleep(2000);
			p.StandardInput.WriteLine("exit");
			return p;
		}

		public static void InvokeCmdAndWaitToExit(string cmdArgs)
		{
			Process p = InvokeCmd(cmdArgs);
			p.WaitForExit();
		}
	}
}
