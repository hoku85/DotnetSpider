using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Core.Selector;
using Java2Dotnet.Spider.Extension.Model.Attribute;
using Java2Dotnet.Spider.Extension.Model.Formatter;
using Java2Dotnet.Spider.Extension.Utils;

namespace Java2Dotnet.Spider.Extension.Model
{
	/// <summary>
	/// The main internal logic of page model extractor.
	/// </summary>
	public class PageModelExtractor
	{
		private readonly IList<Regex> _targetUrlPatterns = new List<Regex>();
		private ISelector _targetUrlRegionSelector;
		private readonly IList<Regex> _helpUrlPatterns = new List<Regex>();
		private ISelector _helpUrlRegionSelector;
		private readonly Type _modelType;
		private readonly Type _actualType;
		private List<FieldExtractor> _fieldExtractors;
		private IObjectFormatter _targetUrlFormatter;
		private Extractor _objectExtractor;
		private readonly static log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(PageModelExtractor));
		private static readonly Regex UrlRegex = new Regex(@"((http|https|ftp):(\/\/|\\\\)((\w)+[.]){1，}(net|com|cn|org|cc|tv|[0-9]{1，3})(((\/[\~]*|\\[\~]*)(\w)+)|[.](\w)+)*(((([?](\w)+){1}[=]*))*((\w)+){1}([\&](\w)+[\=](\w)+)*)*)");
		private readonly bool _isGeneric;
		private readonly MethodInfo _addMethod;

		private PageModelExtractor(Type type)
		{
			_isGeneric = typeof(IEnumerable).IsAssignableFrom(type);
			if (_isGeneric)
			{
				_addMethod = type.GetMethod("Add");
				if (_addMethod == null)
				{
					throw new SpiderExceptoin("Add method is not exist.");
				}
			}
			_modelType = type;
			_actualType = _isGeneric ? type.GenericTypeArguments[0] : type;
		}

		public static PageModelExtractor Create(Type type)
		{
			var pageModelExtractor = new PageModelExtractor(type);
			pageModelExtractor.Init();

			return pageModelExtractor;
		}

		private FieldExtractor GetAttributeExtractBy(PropertyInfo field)
		{
			FieldExtractor fieldExtractor = null;
			ExtractBy extractBy = (ExtractBy)field.GetCustomAttribute(typeof(ExtractBy));
			if (extractBy != null)
			{
				ISelector selector = ExtractorUtils.GetSelector(extractBy);
				// 改为由属性类型决定是否为List, Attribte可能会错, Property可能更可靠
				fieldExtractor = new FieldExtractor(field, selector, extractBy.Source,
					extractBy.NotNull, field.PropertyType.IsGenericType);
			}

			return fieldExtractor;
		}

		private void Init()
		{
			InitTypeExtractors();
			_fieldExtractors = new List<FieldExtractor>();
			foreach (PropertyInfo field in _actualType.GetProperties())
			{
				FieldExtractor fieldExtractor = GetAttributeExtractBy(field);
				FieldExtractor fieldExtractorTmp = GetAttributeExtractCombo(field);

				if (fieldExtractor == null && fieldExtractorTmp != null)
				{
					fieldExtractor = fieldExtractorTmp;
				}
				fieldExtractorTmp = GetAttributeExtractByUrl(field);
				if (fieldExtractor == null && fieldExtractorTmp != null)
				{
					fieldExtractor = fieldExtractorTmp;
				}
				if (fieldExtractor != null)
				{
					CheckFormat(field, fieldExtractor);
					_fieldExtractors.Add(fieldExtractor);
				}
			}
		}

		private FieldExtractor GetAttributeExtractCombo(PropertyInfo field)
		{
			FieldExtractor fieldExtractor = null;
			ComboExtract comboExtract = field.GetCustomAttribute<ComboExtract>();
			if (comboExtract != null)
			{
				ExtractBy[] extractBies = comboExtract.Value;
				ISelector selector;
				switch (comboExtract.Op)
				{
					case ComboExtract.ExtractOp.And:
						selector = new AndSelector(ExtractorUtils.GetSelectors(extractBies));
						break;
					case ComboExtract.ExtractOp.Or:
						selector = new OrSelector(ExtractorUtils.GetSelectors(extractBies));
						break;
					default:
						selector = new AndSelector(ExtractorUtils.GetSelectors(extractBies));
						break;
				}
				fieldExtractor = new FieldExtractor(field, selector, comboExtract.Source,
						comboExtract.NotNull, comboExtract.Multi || field.PropertyType.IsGenericType);
			}

			return fieldExtractor;
		}

		private void CheckFormat(PropertyInfo field, FieldExtractor fieldExtractor)
		{
			//check custom formatter
			Attribute.Formatter formatter = field.GetCustomAttribute<Attribute.Formatter>();
			Stopper stopper = field.GetCustomAttribute<Stopper>();
			fieldExtractor.Stopper = stopper;

			IObjectFormatter objectFormatter;
			if (formatter?.FormatterType != null)
			{
				fieldExtractor.ObjectFormatter = (IObjectFormatter)Activator.CreateInstance(formatter.FormatterType);
				if (formatter.UseDefaultFormatter)
				{
					fieldExtractor.ObjectFormatter.NextFormatter = FormatterFactory.GetFormatter(field.PropertyType);
				}
				fieldExtractor.ObjectFormatter.InitParam(formatter.Value);
				return;
			}
			else
			{
				objectFormatter = FormatterFactory.GetFormatter(field.PropertyType);
			}

			if (!fieldExtractor.Multi)
			{
				if (objectFormatter == null)
				{
					throw new Exception("Can't find formatter for field " + field.Name + " of type " + field.PropertyType.Name);
				}
				fieldExtractor.ObjectFormatter = objectFormatter;
			}
			else
			{
				// 注释掉, 已经改为从属性得IsMulti值了, 不可能两边不一至。
				//if (!field.PropertyType.IsGenericType)
				//{
				//	throw new SpiderExceptoin("Field " + field.Name + " must be generice type.");
				//}

				Type[] genericType = field.PropertyType.GetGenericArguments();
				if (genericType.Length != 1)
				{
					throw new SpiderExceptoin("Field " + field.Name + " must be single generice type like List<T> Hashset<T>. Not support Diction<K,V> etc...");
				}

				MethodInfo method = fieldExtractor.Field.PropertyType.GetMethod("Add", genericType);
				if (method == null)
				{
					throw new SpiderExceptoin("Field " + field.Name + " did not contains Add(T t) method.");
				}

				if (objectFormatter == null)
				{
					if (formatter != null)
						throw new SpiderExceptoin("Can't find formatter for field " + field.Name + " of type " + formatter.SubType);
				}
				else
				{
					fieldExtractor.ObjectFormatter = objectFormatter;
				}
			}
		}

		private FieldExtractor GetAttributeExtractByUrl(PropertyInfo field)
		{
			FieldExtractor fieldExtractor = null;
			ExtractByUrl extractByUrl = field.GetCustomAttribute<ExtractByUrl>();
			if (extractByUrl != null)
			{
				string regexPattern = extractByUrl.Value;
				if (string.IsNullOrEmpty(regexPattern.Trim()))
				{
					regexPattern = ".*";
				}
				fieldExtractor = new FieldExtractor(field,
						new RegexSelector(regexPattern), ExtractSource.Url, extractByUrl.NotNull,
						extractByUrl.Multi || field.PropertyType.IsGenericType);

			}
			return fieldExtractor;
		}

		private void InitTypeExtractors()
		{
			TargetUrl targetUrlAttribute = _actualType.GetCustomAttribute<TargetUrl>();

			if (targetUrlAttribute == null)
			{
				_targetUrlPatterns.Add(new Regex("(.*)"));
			}
			else
			{
				string[] value = targetUrlAttribute.Value;

				if (value != null)
				{
					foreach (string s in value)
					{
						_targetUrlPatterns.Add(new Regex("(" + s.Replace(".", "\\.").Replace("*", "[^\"'#]*") + ")"));
					}
				}
				else
				{
					_targetUrlPatterns.Add(new Regex("(.*)"));
				}

				if (targetUrlAttribute.ExtractType == ExtractType.XPath)
				{
					_targetUrlRegionSelector = new XPathSelector(string.IsNullOrEmpty(targetUrlAttribute.SourceRegion) ? "." : targetUrlAttribute.SourceRegion);
				}
				else if (targetUrlAttribute.ExtractType == ExtractType.JsonPath)
				{
					_targetUrlRegionSelector = new JsonPathSelector(string.IsNullOrEmpty(targetUrlAttribute.SourceRegion) ? "$." : targetUrlAttribute.SourceRegion);
				}

				TargetUrlFormatter formatter = _actualType.GetCustomAttribute<TargetUrlFormatter>();

				if (formatter?.FormatterType != null)
				{
					_targetUrlFormatter = (IObjectFormatter)Activator.CreateInstance(formatter.FormatterType);
					_targetUrlFormatter.InitParam(formatter.Value);
				}
			}
			HelpUrl helpAnnotation = _actualType.GetCustomAttribute<HelpUrl>();
			if (helpAnnotation != null)
			{
				string[] value = helpAnnotation.Value;
				foreach (string s in value)
				{
					_helpUrlPatterns.Add(new Regex("(" + s.Replace(".", "\\.").Replace("*", "[^\"'#]*") + ")"));
				}
				if (!string.IsNullOrEmpty(helpAnnotation.SourceRegion))
				{
					_helpUrlRegionSelector = new XPathSelector(helpAnnotation.SourceRegion);
				}
			}
			ExtractBy extractByAttribute = _actualType.GetCustomAttribute<ExtractBy>();
			if (extractByAttribute != null)
			{
				_objectExtractor = ExtractorUtils.GetExtractor(extractByAttribute);
			}
		}

		public dynamic Process(Page page)
		{
			bool matched = false;
			if (page.Url != null)
			{
				foreach (Regex targetPattern in _targetUrlPatterns)
				{
					string url = page.Url;
					//check
					if (targetPattern.IsMatch(url))
					{
						matched = true;
					}
					else
					{
						Logger.Warn($"Url {url} is not match your TargetUrl attribute. Cause select 0 element.");
					}
				}
			}
			if (!matched)
			{
				return null;
			}
			if (_objectExtractor == null)
			{
				return ProcessSingle(page, null, true);
			}
			else
			{
				if (_isGeneric)
				{
					IList<string> list = _objectExtractor.Selector.SelectList(page.RawText);
					if (_objectExtractor.Count < long.MaxValue)
					{
						list = list.Take((int)_objectExtractor.Count).ToList();
					}

					dynamic result = Activator.CreateInstance(_modelType);
					foreach (var item in list)
					{
						var obj = ProcessSingle(page, item, false);
						if (obj != null)
						{
							//result.Add(obj);
							_addMethod.Invoke(result, new object[] { obj });
						}
					}
					return result;
				}
				else
				{
					string select = _objectExtractor.Selector.Select(page.RawText);
					object o = ProcessSingle(page, select, false);
					return o;
				}
			}
		}

		private dynamic ProcessSingle(Page page, string content, bool isEntire)
		{
			var instance = Activator.CreateInstance(_actualType);
			foreach (FieldExtractor fieldExtractor in _fieldExtractors)
			{
				if (fieldExtractor.Multi)
				{
					IList<string> value = null;
					switch (fieldExtractor.Source)
					{
						case ExtractSource.RawHtml:
							value = page.HtmlDocument.SelectDocumentForList(fieldExtractor.Selector);
							break;
						case ExtractSource.Html:
							value = isEntire ? page.HtmlDocument.SelectDocumentForList(fieldExtractor.Selector) : fieldExtractor.Selector.SelectList(content);
							break;
						case ExtractSource.Json:
							value = isEntire ? page.GetJson().SelectList(fieldExtractor.Selector).GetAll() : fieldExtractor.Selector.SelectList(content);
							break;
						case ExtractSource.Url:
							value = fieldExtractor.Selector.SelectList(page.Url);
							break;
						case ExtractSource.Enviroment:
							{
								EnviromentSelector selector = fieldExtractor.Selector as EnviromentSelector;
								if (selector != null) value = selector.GetValueList(page);
								break;
							}
						default:
							value = fieldExtractor.Selector.SelectList(content);
							break;
					}
					if ((value == null || value.Count == 0) && fieldExtractor.NotNull)
					{
						return null;
					}

					if (fieldExtractor.ObjectFormatter != null)
					{
						//if (!string.IsNullOrEmpty(fieldExtractor.Expresion))
						//{
						//	MemoryStream stream = new MemoryStream();
						//	StreamWriter writer = new StreamWriter(stream);
						//	writer.Write(fieldExtractor.Expresion.EndsWith(";") ? fieldExtractor.Expresion : fieldExtractor.Expresion + ";");
						//	writer.Flush();

						//	// convert stream to string  
						//	stream.Position = 0;
						//	AntlrInputStream input = new AntlrInputStream(stream);
						//	ModifyScriptLexer lexer = new ModifyScriptLexer(input);
						//	CommonTokenStream tokens = new CommonTokenStream(lexer);
						//	// implement custom expresion
						//	IList<string> tmp = new List<string>();
						//	bool missTargetUrls = false;
						//	// ReSharper disable once PossibleNullReferenceException
						//	foreach (string d in value)
						//	{
						//		lexer.Reset();
						//		tokens.Reset();

						//		ModifyScriptVisitor modifyScriptVisitor = new ModifyScriptVisitor(d, fieldExtractor.Field);
						//		ModifyScriptParser parser = new ModifyScriptParser(tokens);
						//		modifyScriptVisitor.Visit(parser.stats());
						//		if (!string.IsNullOrEmpty(modifyScriptVisitor.Value))
						//		{
						//			tmp.Add(modifyScriptVisitor.Value);
						//		}
						//		if (!missTargetUrls && modifyScriptVisitor.NeedStop)
						//		{
						//			missTargetUrls = true;
						//		}
						//	}
						//	page.MissTargetUrls = missTargetUrls;
						//	value = tmp;
						//}

						IList<dynamic> converted = Convert(value, fieldExtractor.ObjectFormatter);

						if (fieldExtractor.Stopper != null)
						{
							foreach (string d in converted)
							{
								if (fieldExtractor.Stopper.NeedStop(d) && !page.MissTargetUrls)
								{
									page.MissTargetUrls = true;
									break;
								}
							}
						}

						dynamic field = fieldExtractor.Field.GetValue(instance) ?? Activator.CreateInstance(fieldExtractor.Field.PropertyType);

						Type[] genericType = fieldExtractor.Field.PropertyType.GetGenericArguments();
						MethodInfo method = fieldExtractor.Field.PropertyType.GetMethod("Add", genericType);

						if (fieldExtractor.Download)
						{
							List<string> urlList = new List<string>();
							foreach (var url in converted)
							{
								// 不需要判断为空, 前面已经判断过了
								if (UrlRegex.IsMatch(url))
								{
									urlList.Add(url);
								}
							}
							page.AddResultItem(Page.Images, urlList);
						}

						foreach (var v in converted)
						{
							method.Invoke(field, new object[] { v });
						}

						fieldExtractor.Field.SetValue(instance, field);
					}
					else
					{
						fieldExtractor.Field.SetValue(instance, value);

						if (fieldExtractor.Download)
						{
							List<string> urlList = new List<string>();
							foreach (var url in value)
							{
								// 不需要判断为空, 前面已经判断过了
								if (UrlRegex.IsMatch(url))
								{
									urlList.Add(url);
								}
							}
							page.AddResultItem(Page.Images, urlList);
						}
					}
				}
				else
				{
					string value = null;
					switch (fieldExtractor.Source)
					{
						case ExtractSource.RawHtml:
							value = page.HtmlDocument.SelectDocument(fieldExtractor.Selector);
							break;
						case ExtractSource.Html:
							value = isEntire ? page.HtmlDocument.SelectDocument(fieldExtractor.Selector) : fieldExtractor.Selector.Select(content);
							break;
						case ExtractSource.Json:
							value = isEntire ? page.GetJson().SelectList(fieldExtractor.Selector).Value : fieldExtractor.Selector.Select(content);
							break;
						case ExtractSource.Url:
							value = fieldExtractor.Selector.Select(page.Url);
							break;
						case ExtractSource.Enviroment:
							{
								EnviromentSelector selector = fieldExtractor.Selector as EnviromentSelector;
								if (selector != null)
								{
									value = selector.GetValue(page)?.ToString();
								}
								break;
							}
						default:
							value = fieldExtractor.Selector.Select(content);
							break;
					}
					if (value == null && fieldExtractor.NotNull)
					{
						return null;
					}
					if (fieldExtractor.ObjectFormatter != null)
					{
						//if (!string.IsNullOrEmpty(fieldExtractor.Expresion))
						//{
						//	MemoryStream stream = new MemoryStream();
						//	StreamWriter writer = new StreamWriter(stream);
						//	writer.Write(fieldExtractor.Expresion.EndsWith(";") ? fieldExtractor.Expresion : fieldExtractor.Expresion + ";");
						//	writer.Flush();

						//	// convert stream to string  
						//	stream.Position = 0;
						//	AntlrInputStream input = new AntlrInputStream(stream);

						//	ModifyScriptLexer lexer = new ModifyScriptLexer(input);
						//	CommonTokenStream tokens = new CommonTokenStream(lexer);

						//	ModifyScriptVisitor modifyScriptVisitor = new ModifyScriptVisitor(value, fieldExtractor.Field);
						//	ModifyScriptParser parser = new ModifyScriptParser(tokens);

						//	modifyScriptVisitor.Visit(parser.stats());
						//	value = modifyScriptVisitor.Value;
						//	page.MissTargetUrls = modifyScriptVisitor.NeedStop;
						//}

						dynamic converted = Convert(value, fieldExtractor.ObjectFormatter);

						if (converted == null && fieldExtractor.NotNull)
						{
							return null;
						}

						if (fieldExtractor.Stopper != null && !page.MissTargetUrls)
						{
							page.MissTargetUrls = fieldExtractor.Stopper.NeedStop(converted);
						}

						fieldExtractor.Field.SetValue(instance, converted);
					}
					else
					{
						fieldExtractor.Field.SetValue(instance, value);
					}

					if (fieldExtractor.Download)
					{
						// 不需要判断为空, 前面已经判断过了
						if (UrlRegex.IsMatch(value))
						{
							page.AddResultItem(Page.Images, value);
						}
					}
				}
			}

			IAfterExtractor afterExtractor = instance as IAfterExtractor;
			afterExtractor?.AfterProcess(page);

			var customize = instance as ICustomize;
			customize?.Customize();

			return instance;
		}

		private dynamic Convert(string value, IObjectFormatter objectFormatter)
		{
			try
			{
				object format = objectFormatter.Format(value);
				Logger.DebugFormat("String {0} is converted to {1}", value, format);
				return format;
			}
			catch (Exception e)
			{
				Logger.Error("convert " + value + " to " + objectFormatter.GetType() + " error!", e);
			}
			return null;
		}

		private IList<dynamic> Convert(IList<string> values, IObjectFormatter objectFormatter)
		{
			return values.Select(value => Convert(value, objectFormatter)).ToList();
		}

		public Type GetActualType()
		{
			return _actualType;
		}

		public IList<Regex> GetTargetUrlPatterns()
		{
			return _targetUrlPatterns;
		}

		public IObjectFormatter GetTargetUrlFormatter()
		{
			return _targetUrlFormatter;
		}

		public IList<Regex> GetHelpUrlPatterns()
		{
			return _helpUrlPatterns;
		}

		public ISelector GetTargetUrlRegionSelector()
		{
			return _targetUrlRegionSelector;
		}

		public ISelector GetHelpUrlRegionSelector()
		{
			return _helpUrlRegionSelector;
		}
	}
}