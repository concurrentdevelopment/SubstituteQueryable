namespace SubstituteQueryable
{
	using System;
	using System.Collections.Generic;

	internal static class IDictionaryExtensions
	{
		public static T ToObject<T>(this IDictionary<string, object> dictionary)
		{
			var type = typeof(T);
			var obj = Activator.CreateInstance<T>();

			foreach (var keyValue in dictionary)
			{
				type.GetProperty(keyValue.Key).SetValue(obj, keyValue.Value, null);
			}

			return obj;
		}
	}
}
