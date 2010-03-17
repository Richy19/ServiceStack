using System;
using System.Reflection;

namespace ServiceStack.Text.Jsv
{
	internal delegate object ParseDelegate(string value);

	public static class StaticParseMethod<T>
	{
		const string ParseMethod = "Parse";

		private static readonly Func<string, object> CacheFn;

		public static Func<string, object> Parse
		{
			get { return CacheFn; }
		}

		static StaticParseMethod()
		{
			CacheFn = GetParseFn();
		}

		public static Func<string, object> GetParseFn()
		{
			// Get the static Parse(string) method on the type supplied
			var parseMethodInfo = typeof(T).GetMethod(
				ParseMethod, BindingFlags.Public | BindingFlags.Static, null,
				new[] { typeof(string) }, null);

			if (parseMethodInfo == null) return null;

			ParseDelegate parseDelegate;
			try
			{
				parseDelegate = (ParseDelegate)Delegate.CreateDelegate(typeof(ParseDelegate), parseMethodInfo);
			}
			catch (ArgumentException bindingException)
			{
				//Try wrapping strongly-typed return with wrapper fn.
				var typedParseDelegate = (Func<string,T>)Delegate.CreateDelegate(typeof(Func<string,T>), parseMethodInfo);
				parseDelegate = x => typedParseDelegate(x);
			}
			if (parseDelegate != null)
				return value => parseDelegate(value);

			return null;
		}

	}
}