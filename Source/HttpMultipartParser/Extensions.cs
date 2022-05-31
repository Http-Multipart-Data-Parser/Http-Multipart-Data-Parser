using System;
using System.Collections.Generic;
using System.Linq;

namespace HttpMultipartParser
{
	/// <summary>
	/// Class containing various extension methods.
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		/// Returns true if the parameter has any values. False otherwise.
		/// </summary>
		/// <param name="parser">The multipart form parser.</param>
		/// <param name="name">The name of the parameter.</param>
		/// <returns>True if the parameter exists. False otherwise.</returns>
		public static bool HasParameter(this IMultipartFormDataParser parser, string name)
		{
			return parser.Parameters.Any(p => p.Name == name);
		}

		/// <summary>
		/// Returns the value of a parameter or null if it doesn't exist.
		///
		/// You should only use this method if you're sure the parameter has only one value.
		///
		/// If you need to support multiple values use GetParameterValues.
		/// </summary>
		/// <param name="parser">The multipart form parser.</param>
		/// <param name="name">The name of the parameter.</param>
		/// <param name="comparisonType">One of the enumeration values that specifies how the strings will be compared.</param>
		/// <returns>The value of the parameter.</returns>
		public static string GetParameterValue(this IMultipartFormDataParser parser, string name, StringComparison comparisonType = StringComparison.Ordinal)
		{
			return parser.GetParameterValues(name, comparisonType).FirstOrDefault();
		}

		/// <summary>
		/// Returns the values of a parameter or an empty enumerable if the parameter doesn't exist.
		/// </summary>
		/// <param name="parser">The multipart form parser.</param>
		/// <param name="name">The name of the parameter.</param>
		/// <param name="comparisonType">One of the enumeration values that specifies how the strings will be compared.</param>
		/// <returns>The values of the parameter.</returns>
		public static IEnumerable<string> GetParameterValues(this IMultipartFormDataParser parser, string name, StringComparison comparisonType = StringComparison.Ordinal)
		{
			return parser.Parameters
				.Where(p => p.Name.Equals(name, comparisonType))
				.Select(p => p.Data);
		}
	}
}
