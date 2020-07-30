using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace Facebook.Unity.Editor
{
	public static class Utility
	{
		private static string BundleIdentifier = "bundleIdentifier";

		private static string ApplicationIdentifier = "applicationIdentifier";

		public static T Pop<T>(this IList<T> list)
		{
			if (!list.Any())
			{
				throw new InvalidOperationException("Attempting to pop item on empty list.");
			}
			int index = list.Count - 1;
			T result = list[index];
			list.RemoveAt(index);
			return result;
		}

		public static bool TryGetValue<T>(this IDictionary<string, object> dictionary, string key, out T value)
		{
			if (dictionary.TryGetValue(key, out object value2) && value2 is T)
			{
				value = (T)value2;
				return true;
			}
			value = default(T);
			return false;
		}

		public static string GetApplicationIdentifier()
		{
			Type typeFromHandle = typeof(PlayerSettings);
			PropertyInfo propertyInfo = typeFromHandle.GetProperty(ApplicationIdentifier) ?? typeFromHandle.GetProperty(BundleIdentifier);
			if (propertyInfo != null)
			{
				string text = (string)propertyInfo.GetValue(typeFromHandle, null);
				if (text != null)
				{
					return text;
				}
			}
			return null;
		}
	}
}
