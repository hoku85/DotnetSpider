using System;
using System.Collections.Generic;
using System.Data;
using Dapper;

namespace Java2Dotnet.Spider.Extension.DbSupport.Dapper
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class DataRepository : IDataRepository
	{
		public IDbConnection CreateConnection()
		{
			return GetConnection();
		}

		/// <summary>
		/// 
		/// </summary>
		public DataRepository(Type type)
		{
			SqlGenerator = new MySqlGenerator(type);

			if (DbProviderUtil.Provider == null)
			{
				DbProviderUtil.Provider = new DataProviderManager().LoadDataProvider();
			}
			GetConnection = DbProviderUtil.GetProvider;
		}

		private ISqlGenerator SqlGenerator { get; }

		private Func<IDbConnection> GetConnection { get; }

		#region Repository sync base actions

		/// <summary>
		/// 
		/// </summary>
		/// <param name="instance"></param>
		/// <returns></returns>
		public void Insert(object instance)
		{
			if (instance == null)
			{
				return;
			}
			var sql = SqlGenerator.GetInsert(false);

			//AtomicRedialExecutor.Execute("db-insert", () =>
			//{
				using (IDbConnection conn = GetConnection())
				{
					conn.Execute(sql, instance, null, 99999, CommandType.Text);
				}
			//});
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="filters"></param>
		/// <returns></returns>
		public IEnumerable<dynamic> GetWhere(object filters)
		{
			if (filters == null)
			{
				return null;
			}

			var sql = SqlGenerator.GetSelect(filters);

			//return AtomicRedialExecutor.Execute("getwhere-insert", () =>
			//{
				using (IDbConnection conn = GetConnection())
				{
					return conn.Query(sql, filters, null, false, 99999, CommandType.Text);
				}
			//});
		}

		//public dynamic GetWhere

		/// <summary>
		/// 
		/// </summary>
		/// <param name="instance"></param>
		/// <returns></returns>
		public void Update(object instance)
		{
			if (instance == null)
			{
				return;
			}
			var sql = SqlGenerator.GetUpdate();

			//AtomicRedialExecutor.Execute("db-update", () =>
			//{
				using (IDbConnection conn = GetConnection())
				{
					conn.Execute(sql, instance, null, 99999, CommandType.Text);
				}
			//});
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sql"></param>
		/// <returns></returns>
		public int Execute(string sql)
		{
			//return AtomicRedialExecutor.Execute("db-execute", () =>
			//{
				using (IDbConnection conn = GetConnection())
				{
					return conn.Execute(sql, null, null, 99999, CommandType.Text);
				}
			//});
		}

		#endregion


		public void CreateTable()
		{
			//AtomicRedialExecutor.Execute("db-createtable", () =>
			//{
				using (IDbConnection conn = GetConnection())
				{
					conn.Execute(SqlGenerator.GetCreateTable(), null, null, 99999, CommandType.Text);
				}
			//});
		}

		public void CreateSheme()
		{
			//AtomicRedialExecutor.Execute("db-createsheme", () =>
			//{
				using (IDbConnection conn = GetConnection())
				{
					conn.Execute(SqlGenerator.GetCreateSheme(), null, null, 99999, CommandType.Text);
				}
			//});
		}
	}
}
