using System;

namespace Java2Dotnet.Spider.Extension.Model.Attribute
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
	public class ExtractBy : System.Attribute
	{
		private ExtractType _type = ExtractType.XPath;

		//public ExtractBy(string value, Type type = Type.XPath, bool notNull = false, Source source = Source.SelectedHtml, bool multi = false)
		//{
		//	this.value = value;
		//	this.type = type;
		//	this.notNull = notNull;
		//	this.source = source;
		//	this.multi = multi;
		//}

		/// <summary>
		/// Extractor expression, support XPath, CSS Selector and regex.
		/// </summary>
		public string Value { get; set; }

		/// <summary>
		/// Extractor type, support XPath, CSS Selector and regex.
		/// </summary>
		public ExtractType Type
		{
			get
			{
				return _type;
			}
			set
			{
				if (_type != value)
				{
					_type = value;
					if (value == ExtractType.Enviroment)
					{
						Source = ExtractSource.Enviroment;
					}
					if (value == ExtractType.JsonPath)
					{
						Source = ExtractSource.Json;
					}
				}
			}
		}

		/// <summary>
		/// Define whether the field can be null. 
		/// If set to 'true' and the extractor get no result, the entire class will be discarded.
		/// </summary>
		public bool NotNull { get; set; }

		/// <summary>
		///The source for extracting. 
		///It works only if you already added 'ExtractBy' to Class.   
		/// </summary>
		public ExtractSource Source { get; set; }

		public long Count { get; set; } = long.MaxValue;

		//public string Expresion { get; set; }
	}
}
