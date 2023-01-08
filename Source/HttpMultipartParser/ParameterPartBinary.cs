// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterPartBinary.cs" company="Jake Woods">
//   Copyright (c) 2022 Jake Woods, Jeremie Desautels
//
//   Permission is hereby granted, free of charge, to any person obtaining a copy of this software
//   and associated documentation files (the "Software"), to deal in the Software without restriction,
//   including without limitation the rights to use, copy, modify, merge, publish, distribute,
//   sublicense, and/or sell copies of the Software, and to permit persons to whom the Software
//   is furnished to do so, subject to the following conditions:
//
//   The above copyright notice and this permission notice shall be included in all copies
//   or substantial portions of the Software.
//
//   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
//   INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
//   PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR
//   ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
//   ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
// <author>Jeremie Desautels</author>
// <summary>
//   Represents the binary data of a single parameter extracted from a multipart/form-data stream.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HttpMultipartParser
{
	/// <summary>
	///     Represents the binary data of a single parameter extracted from a multipart/form-data stream.
	/// </summary>
	/// <remarks>
	///     For our purposes a "parameter" is defined as any non-file data
	///     in the multipart/form-data stream.
	/// </remarks>
	public class ParameterPartBinary
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="ParameterPartBinary" /> class.
		/// </summary>
		/// <param name="name">
		///     The name.
		/// </param>
		/// <param name="data">
		///     The data.
		/// </param>
		public ParameterPartBinary(string name, IEnumerable<byte[]> data)
		{
			Name = name;
			Data = data;
		}

		/// <summary>
		///     Gets the data.
		/// </summary>
		public IEnumerable<byte[]> Data { get; }

		/// <summary>
		///     Gets the name.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Get the binary data expressed as a string.
		/// </summary>
		/// <returns>The binary data expressed as a string.</returns>
		public override string ToString()
		{
			return ToString(Constants.DefaultEncoding);
		}

		/// <summary>
		/// Get the binary data expressed as a string.
		/// </summary>
		/// <param name="encoding">The encoding used to convert the binary data into a string.</param>
		/// <returns>The binary data expressed as a string.</returns>
		public string ToString(Encoding encoding)
		{
			if (encoding == null) { throw new ArgumentNullException(nameof(encoding)); }

			return string.Join(Environment.NewLine, Data.Select(line => encoding.GetString(line)));
		}
	}
}
