using System.Text;

namespace HttpMultipartParser
{
	/// <summary>
	/// Options that allow configuring the parserbehavior.
	/// </summary>
	public class ParserOptions
	{
		/// <summary>
		/// Gets or sets the multipart/form-data boundary.
		/// </summary>
		/// <remarks>
		/// If you omit this value, the parser will attempt to automatically determine it's value by inspecting the first line of data in the stream.
		/// </remarks>
		public string Boundary { get; set; } = null;

		/// <summary>
		/// Gets or sets the encoding of the multipart data.
		/// </summary>
		public Encoding Encoding { get; set; } = Constants.DefaultEncoding;

		/// <summary>
		/// Gets or sets the size of the buffer to use for parsing the multipart form data.
		/// </summary>
		/// <remarks>
		/// This must be larger than (size of boundary + 4 + # bytes in newline).
		/// </remarks>
		public int BinaryBufferSize { get; set; } = Constants.DefaultBufferSize;

		/// <summary>
		/// Gets or sets the list of mimetypes that should be detected as file.
		/// </summary>
		public string[] BinaryMimeTypes { get; set; } = Constants.DefaultBinaryMimeTypes;

		/// <summary>
		/// Gets or sets a value indicating whether an exception should be thrown when the
		/// form data contains invalid parts or if these invalid parts should simply be ignored.
		/// </summary>
		public bool IgnoreInvalidParts { get; set; } = false;
	}
}
