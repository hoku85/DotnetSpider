using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Java2Dotnet.Spider.Core;

namespace Java2Dotnet.Spider.Extension.Pipeline
{
	public class MySqlFilePipeline : IPageModelPipeline
	{
		private static string _folderPath;

		public MySqlFilePipeline()
		{
			_folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DotnetSpider", "Data");

			if (!Directory.Exists(_folderPath))
			{
				Directory.CreateDirectory(_folderPath);
			}
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void Process(Dictionary<Type, List<dynamic>> data, ISpider spider)
		{
			foreach (var pair in data)
			{
				Type type = pair.Key;
				bool isGeneric = typeof(IEnumerable).IsAssignableFrom(type);
				Type actualType = isGeneric ? type.GenericTypeArguments[0] : type;
				foreach (var r in pair.Value)
				{
					string filePath = Path.Combine(_folderPath, actualType.Name + ".sql");

					var properties = actualType.GetProperties();

					StringBuilder builder = new StringBuilder();
					foreach (var propertyInfo in properties)
					{
						builder.Append("'").Append(propertyInfo.GetValue(r).ToString()).Append("'").Append(",");
					}
					builder.Remove(builder.Length - 1, 1);
					builder.Append(Environment.NewLine);

					File.AppendAllText(filePath, builder.ToString(), Encoding.UTF8);
				}
			}
		}

		public void Dispose()
		{
		}
	}
}
