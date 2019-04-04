namespace Search.UnitTests.Helpers
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Text;
	using TechTalk.SpecFlow;


	/// <summary>
	/// 
	/// </summary>
	public static class TableHelper
	{
		public static IEnumerable<T> CreateInstances<T>(this Table table) where T : class, new()
		{
			var props = typeof(T).GetProperties().Where(p => p.CanRead && p.CanWrite).ToDictionary(p => p.Name);
			var instances = new List<T>();
			var columnNames = table.Header.Select(c => c.ToLower()).ToList();
			foreach(TableRow row in table.Rows)
			{
				var instance = Activator.CreateInstance<T>();
				for(var i = 0; i<row.Count;i++)
				{
					var columnName = columnNames[i];
					var prop = props.FirstOrDefault(p => p.Key.Equals(columnName, StringComparison.CurrentCultureIgnoreCase)).Value;
					if (prop != null)
					{
						var value = row[i];
						if (!string.IsNullOrWhiteSpace(value))
						{
							if (prop.PropertyType==typeof(string))
							{
								prop.SetValue(instance, value);
							}
							else
							{
								prop.SetValue(instance, Convert.ChangeType(value, prop.PropertyType));
							}
						}
					}
				}
				instances.Add(instance);
			}

			return instances;
		}
	}
}