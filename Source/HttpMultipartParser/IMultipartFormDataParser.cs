using System.Collections.Generic;

namespace HttpMultipartParser
{
	/// <summary>
	///     Provides methods to parse a
	///     <see href="http://www.ietf.org/rfc/rfc2388.txt">
	///         <c>multipart/form-data</c>
	///     </see>
	///     stream into it's parameters and file data.
	/// </summary>
	public interface IMultipartFormDataParser
	{
		/// <summary>
		///     Gets the mapping of parameters parsed files. The name of a given field
		///     maps to the parsed file data.
		/// </summary>
		IReadOnlyList<FilePart> Files { get; }

		/// <summary>
		///     Gets the parameters. Several ParameterParts may share the same name.
		/// </summary>
		IReadOnlyList<ParameterPart> Parameters { get; }
	}
}
