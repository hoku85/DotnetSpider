using Java2Dotnet.Spider.Extension.DbSupport.Dapper.Attributes;
using Java2Dotnet.Spider.Extension.Model.Attribute;

namespace Java2Dotnet.Spider.Extension.Model
{
	public class SpiderEntity: ICustomize
	{
		[StoredAs("id", StoredAs.ValueType.Long, true)]
		[KeyProperty(Identity = true)]
		[ExtractBy(Value = "Id", Type = ExtractType.Enviroment)]
		public long Id { get; set; }

		public virtual void Customize()
		{
		}
	}
}
