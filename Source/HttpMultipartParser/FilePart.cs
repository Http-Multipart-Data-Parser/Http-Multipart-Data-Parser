// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FilePart.cs" company="Jake Woods">
//   Copyright (c) 2013 Jake Woods
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
// <author>Jake Woods</author>
// <summary>
//   Represents a single file extracted from a multipart/form-data
//   stream.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace HttpMultipartParser
{
	/// <summary>
	///     Represents a single file extracted from a multipart/form-data
	///     stream.
	/// </summary>
	public class FilePart
	{
		#region Fields

		private const string DefaultContentType = "text/plain";
		private const string DontentDisposition = "form-data";

		#endregion

		#region Constructors and Destructors

		/// <summary>
		///     Initializes a new instance of the <see cref="FilePart" /> class.
		/// </summary>
		/// <param name="name">
		///     The name of the input field used for the upload.
		/// </param>
		/// <param name="fileName">
		///     The name of the file.
		/// </param>
		/// <param name="data">
		///     The file data.
		/// </param>
		/// <param name="contentType">
		///     The content type.
		/// </param>
		/// <param name="contentDisposition">
		///     The content disposition.
		/// </param>
		public FilePart(string name, string fileName, Stream data, string contentType = DefaultContentType, string contentDisposition = DontentDisposition)
			: this(name, fileName, data, null, contentType, contentDisposition)
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="FilePart" /> class.
		/// </summary>
		/// <param name="name">
		///     The name of the input field used for the upload.
		/// </param>
		/// <param name="fileName">
		///     The name of the file.
		/// </param>
		/// <param name="data">
		///     The file data.
		/// </param>
		/// <param name="additionalProperties">
		///     Additional properties associated with this file.
		/// </param>
		/// <param name="contentType">
		///     The content type.
		/// </param>
		/// <param name="contentDisposition">
		///     The content disposition.
		/// </param>
		public FilePart(string name, string fileName, Stream data, IDictionary<string, string> additionalProperties, string contentType = DefaultContentType, string contentDisposition = DontentDisposition)
		{
			Name = name;
			FileName = fileName?.Split(GetInvalidFileNameChars()).Last();
			Data = data;
			ContentType = contentType;
			ContentDisposition = contentDisposition;
			AdditionalProperties = new ReadOnlyDictionary<string, string>(additionalProperties ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
		}

		#endregion

		#region Public Properties

		/// <summary>
		///     Gets the data.
		/// </summary>
		public Stream Data { get; }

		/// <summary>
		///     Gets the file name.
		/// </summary>
		public string FileName { get; }

		/// <summary>
		///     Gets the name.
		/// </summary>
		public string Name { get; }

		/// <summary>
		///     Gets the content-type. Defaults to text/plain if unspecified.
		/// </summary>
		public string ContentType { get; }

		/// <summary>
		///     Gets the content-disposition. Defaults to form-data if unspecified.
		/// </summary>
		public string ContentDisposition { get; }

		/// <summary>
		/// Gets the additional properties associated with this file.
		/// An additional property is any property other than the "well known" ones such as name, filename, content-type, etc.
		/// </summary>
		public IReadOnlyDictionary<string, string> AdditionalProperties { get; private set; }

		#endregion

		#region Private methods

		// This is a cross-platform version of Path.GetInvalidFileNameChars() that returns the same array of characters regadless of the operating system.
		// We use this method rather than the built-in Path.GetInvalidFileNameChars() because some unit tests were behaving differently on Ubuntu compared to Windows.
		private static char[] GetInvalidFileNameChars() => new char[]
		{
			'\"', '<', '>', '|', '\0',
			(char)1, (char)2, (char)3, (char)4, (char)5, (char)6, (char)7, (char)8, (char)9, (char)10,
			(char)11, (char)12, (char)13, (char)14, (char)15, (char)16, (char)17, (char)18, (char)19, (char)20,
			(char)21, (char)22, (char)23, (char)24, (char)25, (char)26, (char)27, (char)28, (char)29, (char)30,
			(char)31, ':', '*', '?', '\\', '/'
		};

		#endregion
	}
}
