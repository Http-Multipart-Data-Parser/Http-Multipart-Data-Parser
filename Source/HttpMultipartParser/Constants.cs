using System.Text;

namespace HttpMultipartParser
{
	internal static class Constants
	{
		/// <summary>
		///     The default buffer size.
		/// </summary>
		/// <remarks>
		///     4096 is the optimal buffer size as it matches the internal buffer of a StreamReader
		///     See: http://stackoverflow.com/a/129318/203133
		///     See: http://msdn.microsoft.com/en-us/library/9kstw824.aspx (under remarks).
		/// </remarks>
		internal const int DefaultBufferSize = 4096;

		/// <summary>
		/// The mimetypes that are considered a file by default.
		/// </summary>
		internal static readonly string[] DefaultBinaryMimeTypes = { "application/octet-stream" };

		/// <summary>
		/// The default encoding used by the parser when developer does not specify the encoding.
		/// </summary>
		internal static readonly Encoding DefaultEncoding = Encoding.UTF8;
	}
}
