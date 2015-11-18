﻿using System.Text.RegularExpressions;

namespace Java2Dotnet.Spider.Core.Utils
{
	public class NetUtils
	{
		public static Regex IpAddressRegex=new Regex(@"((?:(?:25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d)))\.){3}(?:25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d))))");
	}
}
