// ===============================================================================
// RFC5987 Decoder
//
// http://greenbytes.de/tech/webdav/rfc5987.html
// ===============================================================================
// Copyright Steven Robbins.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
// ===============================================================================
namespace HttpMultipartParser
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;

	/// <summary>
	/// Provides a way to decode the value of so-called "star-parameters"
	/// according to RFC 5987 which is superceded by RFC 8187.
	///
	/// <see href="https://www.rfc-editor.org/rfc/rfc5987">RFC 5987</see>
	/// <see href="https://www.rfc-editor.org/rfc/rfc8187">RFC 8187</see>
	/// <see href="https://author-tools.ietf.org/diff?doc_1=5987&amp;doc_2=8187">Handy side-by-side comparison</see> of the two RFCs.
	/// </summary>
	/// <remarks>Taken from <see href="https://github.com/grumpydev/RFC5987-Decoder" />.</remarks>
	public static class RFC5987
	{
		/// <summary>
		/// Regex for the encoded string format detailed in
		/// http://greenbytes.de/tech/webdav/rfc5987.html.
		/// </summary>
		private static readonly Regex EncodedStringRegex = new Regex(@"(?:(?<charset>.*?))'(?<language>.*?)?'(?<encodeddata>.*?)$", RegexOptions.Compiled);

		/// <summary>
		/// Decode a RFC5987 encoded value.
		/// </summary>
		/// <param name="inputString">Encoded input string.</param>
		/// <returns>Decoded string.</returns>
		public static string Decode(string inputString)
		{
			return EncodedStringRegex.Replace(
				inputString,
				m =>
				{
					var characterSet = m.Groups["charset"].Value;
					var language = m.Groups["language"].Value;
					var encodedData = m.Groups["encodeddata"].Value;

					if (!IsSupportedCharacterSet(characterSet))
					{
						// Fall back to iso-8859-1 if invalid/unsupported character set found
						characterSet = @"UTF-8";
					}

					var textEncoding = Encoding.GetEncoding(characterSet);

					return textEncoding.GetString(GetDecodedBytes(encodedData).ToArray());
				});
		}

		/// <summary>
		/// Get the decoded bytes from the encoded data string.
		/// </summary>
		/// <param name="encodedData">Encoded data.</param>
		/// <returns>Decoded bytes.</returns>
		private static IEnumerable<byte> GetDecodedBytes(string encodedData)
		{
			var encodedCharacters = encodedData.ToCharArray();
			for (int i = 0; i < encodedCharacters.Length; i++)
			{
				if (encodedCharacters[i] == '%')
				{
					var hexString = new string(encodedCharacters, i + 1, 2);

					i += 2;

					int characterValue;
					if (int.TryParse(hexString, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out characterValue))
					{
						yield return (byte)characterValue;
					}
				}
				else
				{
					yield return (byte)encodedCharacters[i];
				}
			}
		}

		/// <summary>
		/// Determines if a character set is supported.
		/// </summary>
		/// <param name="characterSet">Character set name.</param>
		/// <returns>Bool representing whether the character set is supported.</returns>
		private static bool IsSupportedCharacterSet(string characterSet)
		{
			return Encoding.GetEncodings()
						   .Where(e => string.Equals(e.Name, characterSet, StringComparison.InvariantCultureIgnoreCase))
						   .Any();
		}
	}
}
