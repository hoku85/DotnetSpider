using System;

namespace Java2Dotnet.Spider.Extension.DbSupport.Dapper.Attributes
{
	public enum SchemeSuffix
	{
		Today,
		ThisMonday,
		FirstDayOfMonth,
		Empty
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class Scheme : Attribute
	{
		/// <summary>
		/// 
		/// </summary>
		public string Value { get; }

		public string TableName { get; }

		public SchemeSuffix Suffix { get; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <param name="tableName"></param>
		/// <param name="suffix"></param>
		public Scheme(string value, string tableName, SchemeSuffix suffix = SchemeSuffix.Empty)
		{
			Value = value;
			Suffix = suffix;
			TableName = tableName;
		}
	}
}
