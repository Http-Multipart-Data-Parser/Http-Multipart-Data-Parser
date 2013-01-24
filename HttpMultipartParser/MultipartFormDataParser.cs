// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MultipartFormDataParser.cs" company="Jake Woods">
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
//   Provides methods to parse a multipart/form-data
//   stream into it's parameters and file data.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace HttpMultipartParser
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;

    /// <summary>
    ///     Provides methods to parse a
    ///     <see href="http://www.ietf.org/rfc/rfc2388.txt">
    ///         <c>multipart/form-data</c>
    ///     </see>
    ///     stream into it's parameters and file data.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         A parameter is defined as any non-file data passed in the multipart stream. For example
    ///         any form fields would be considered a parameter.
    ///     </para>
    ///     <para>
    ///         The parser determines if a section is a file or not based on the presence or absence
    ///         of the filename argument for the Content-Type header. If filename is set then the section
    ///         is assumed to be a file, otherwise it is assumed to be parameter data.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code lang="C#"> 
    ///      Stream multipartStream = GetTheMultipartStream();
    ///      var parser = new MultipartFormDataParser(multipartStream, Encoding.UTF8);
    /// 
    ///      // Grab the parameters (non-file data). Key is based on the name field
    ///      var username = parser.Parameters["username"].Data;
    ///      var password = parser.parameters["password"].Data;
    ///      
    ///      // Grab the file data as a stream!
    ///      var filename = parser.FileName["file"].FileName
    ///      var filestream = parser.Files["file"].Data;
    ///  </code>
    /// </example>
    public class MultipartFormDataParser
    {
        #region Fields

        /// <summary>
        ///     The boundary of the multipart message  as a string.
        /// </summary>
        private readonly string boundary;

        /// <summary>
        ///     The boundary of the multipart message as a byte string
        ///     encoded with CurrentEncoding
        /// </summary>
        private readonly byte[] boundaryBinary;

        /// <summary>
        ///     The end boundary of the multipart message as a string.
        /// </summary>
        private readonly string endBoundary;

        /// <summary>
        ///     The end boundary of the multipart message as a byte string
        ///     encoded with CurrentEncoding
        /// </summary>
        private readonly byte[] endBoundaryBinary;

        /// <summary>
        ///     Determines if we have consumed the end boundary binary and determines
        ///     if we are done parsing.
        /// </summary>
        private bool readEndBoundary;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipartFormDataParser"/> class
        ///     with the boundary and input stream.
        /// </summary>
        /// <param name="boundary">
        /// The multipart/form-data boundary. This should be the value
        ///     returned by the request header.
        /// </param>
        /// <param name="stream">
        /// The stream containing the multipart data
        /// </param>
        public MultipartFormDataParser(string boundary, Stream stream)
            : this(boundary, stream, Encoding.UTF8)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipartFormDataParser"/> class
        ///     with the boundary, input stream and stream encoding.
        /// </summary>
        /// <param name="boundary">
        /// The multipart/form-data boundary. This should be the value
        ///     returned by the request header.
        /// </param>
        /// <param name="stream">
        /// The stream containing the multipart data
        /// </param>
        /// <param name="encoding">
        /// The encoding of the multipart data
        /// </param>
        public MultipartFormDataParser(string boundary, Stream stream, Encoding encoding)
            : this(boundary, stream, encoding, 4096)
        {
            // 4096 is the optimal buffer size as it matches the internal buffer of a StreamReader
            // See: http://stackoverflow.com/a/129318/203133
            // See: http://msdn.microsoft.com/en-us/library/9kstw824.aspx (under remarks)
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipartFormDataParser"/> class
        ///     with the boundary, stream, input encoding and buffer size.
        /// </summary>
        /// <param name="boundary">
        /// The multipart/form-data boundary. This should be the value
        ///     returned by the request header.
        /// </param>
        /// <param name="stream">
        /// The stream containing the multipart data
        /// </param>
        /// <param name="encoding">
        /// The encoding of the multipart data
        /// </param>
        /// <param name="binaryBufferSize">
        /// The size of the buffer to use for parsing the multipart form data. This must be larger
        ///     then (size of boundary + 4 + # bytes in newline).
        /// </param>
        public MultipartFormDataParser(string boundary, Stream stream, Encoding encoding, int binaryBufferSize)
        {
            this.Parameters = new Dictionary<string, ParameterPart>();
            this.Files = new Dictionary<string, FilePart>();
            this.Encoding = encoding;
            this.BinaryBufferSize = binaryBufferSize;
            this.readEndBoundary = false;

            // It's important to remember that the boundary given in the header has a -- appended to the start
            // and the last one has a -- appended to the end
            this.boundary = "--" + boundary;
            this.endBoundary = this.boundary + "--";

            // We add newline here because unlike reader.ReadLine() binary reading
            // does not automatically consume the newline, we want to add it to our signature
            // so we can automatically detect and consume newlines after the boundary
            this.boundaryBinary = this.Encoding.GetBytes(this.boundary + Environment.NewLine);
            this.endBoundaryBinary = this.Encoding.GetBytes(this.endBoundary + Environment.NewLine);

            Debug.Assert(
                binaryBufferSize >= this.endBoundaryBinary.Length, "binaryBufferSize must be bigger then the boundary");

            this.Parse(stream);
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the binary buffer size.
        /// </summary>
        public int BinaryBufferSize { get; set; }

        /// <summary>
        ///     Gets the encoding.
        /// </summary>
        public Encoding Encoding { get; private set; }

        /// <summary>
        ///     Gets the mapping of parameters parsed files. The name of a given field
        ///     maps to the parsed file data.
        /// </summary>
        public Dictionary<string, FilePart> Files { get; private set; }

        /// <summary>
        ///     Gets the mapping of the parameters. The name of a given field
        ///     maps to the parameter data.
        /// </summary>
        public Dictionary<string, ParameterPart> Parameters { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Begins the parsing of the stream into objects.
        /// </summary>
        /// <param name="stream">
        /// The multipart/form-data stream to parse
        /// </param>
        /// <exception cref="MultipartParseException">
        /// thrown on finding unexpected data such as a boundary before we are ready for one.
        /// </exception>
        private void Parse(Stream stream)
        {
            // Parsing references include:
            // RFC1341 section 7: http://www.w3.org/Protocols/rfc1341/7_2_Multipart.html
            // RFC2388: http://www.ietf.org/rfc/rfc2388.txt
            using (var reader = new RebufferableBinaryReader(stream, this.Encoding))
            {
                // First we need to read untill we find a boundary
                while (true)
                {
                    string line = reader.ReadLine();
                    if (line == this.boundary)
                    {
                        break;
                    }

                    if (line == null)
                    {
                        throw new MultipartParseException("Could not find expected boundary");
                    }
                }

                // Now that we've found the initial boundary we know where to start. 
                // We need parse each individual section
                while (!this.readEndBoundary)
                {
                    // ParseSection will parse up to and including
                    // the next boundary.
                    this.ParseSection(reader);
                }
            }
        }

        /// <summary>
        /// Parses a section of the stream that is known to be file data.
        /// </summary>
        /// <param name="parameters">
        /// The header parameters of this file, expects "name" and "filename" to be valid keys
        /// </param>
        /// <param name="reader">
        /// The StreamReader to read the data from
        /// </param>
        /// <returns>
        /// The <see cref="FilePart"/> containing the parsed data (name, filename, stream containing file).
        /// </returns>
        private FilePart ParseFilePart(Dictionary<string, string> parameters, RebufferableBinaryReader reader)
        {
            // We want to create a stream and fill it with the data from the
            // file.
            var data = new MemoryStream();
            var curBuffer = new byte[this.BinaryBufferSize];
            var prevBuffer = new byte[this.BinaryBufferSize];
            int curLength = 0;
            int prevLength = 0;

            prevLength = reader.Read(prevBuffer, 0, prevBuffer.Length);
            do
            {
                curLength = reader.Read(curBuffer, 0, curBuffer.Length);

                // Combine both buffers into the fullBuffer
                // See: http://stackoverflow.com/questions/415291/best-way-to-combine-two-or-more-byte-arrays-in-c-sharp
                var fullBuffer = new byte[this.BinaryBufferSize * 2];
                Buffer.BlockCopy(prevBuffer, 0, fullBuffer, 0, prevLength);
                Buffer.BlockCopy(curBuffer, 0, fullBuffer, prevLength, curLength);

                // Now we want to check for a substring within the current buffer.
                int endPos = SubsequenceFinder.Search(fullBuffer, this.boundaryBinary);
                int endPosLength = this.boundaryBinary.Length;

                if (endPos == -1)
                {
                    endPos = SubsequenceFinder.Search(fullBuffer, this.endBoundaryBinary);
                    endPosLength = this.endBoundaryBinary.Length;

                    if (endPos != -1)
                    {
                        this.readEndBoundary = true;
                    }
                }

                if (endPos != -1)
                {
                    // We've found an end. We need to consume all the binary up to it 
                    // and then write the remainder back to the original stream. Then we
                    // need to modify the original streams position to take into account
                    // the new data.
                    // We also want to chop off the newline that is inserted by the protocl.
                    // We can do this by reducing endPos by the length of newline in this environment
                    // and encoding
                    data.Write(fullBuffer, 0, endPos - this.Encoding.GetByteCount(Environment.NewLine));

                    int writeBackOffset = endPos + endPosLength;
                    int writeBackAmount = (prevLength + curLength) - writeBackOffset;
                    var writeBackBuffer = new byte[writeBackAmount];
                    Buffer.BlockCopy(fullBuffer, writeBackOffset, writeBackBuffer, 0, writeBackAmount);
                    reader.Buffer(writeBackBuffer);

                    // stream.Write(fullBuffer, writeBackOffset, writeBackAmount);
                    // stream.Position = stream.Position - writeBackAmount;
                    // stream.Flush();
                    data.Position = 0;
                    data.Flush();
                    break;
                }

                // No end, consume the entire previous buffer    
                data.Write(prevBuffer, 0, prevLength);
                data.Flush();

                // Now we want to swap the two buffers, we don't care
                // what happens to the data from prevBuffer so we set
                // curBuffer to it so it gets overwrited.
                byte[] tempBuffer = curBuffer;
                curBuffer = prevBuffer;
                prevBuffer = tempBuffer;

                // We don't need to swap the lengths because
                // curLength will be overwritten in the next
                // iteration of the loop.
                prevLength = curLength;
            }
            while (prevLength != 0);

            var part = new FilePart(parameters["name"], parameters["filename"], data);

            return part;
        }

        /// <summary>
        /// Parses a section of the stream that is known to be parameter data.
        /// </summary>
        /// <param name="parameters">
        /// The header parameters of this section. "name" must be a valid key.
        /// </param>
        /// <param name="reader">
        /// The StreamReader to read the data from
        /// </param>
        /// <returns>
        /// The <see cref="ParameterPart"/> containing the parsed data (name, value).
        /// </returns>
        /// <exception cref="MultipartParseException">
        /// thrown if unexpected data is found such as running out of stream before hitting the boundary.
        /// </exception>
        private ParameterPart ParseParameterPart(Dictionary<string, string> parameters, RebufferableBinaryReader reader)
        {
            // Our job is to get the actual "data" part of the parameter and construct
            // an actual ParameterPart object with it. All we need to do is read data into a string
            // untill we hit the boundary
            var data = new StringBuilder();
            string line = reader.ReadLine();
            while (line != this.boundary && line != this.endBoundary)
            {
                if (line == null)
                {
                    throw new MultipartParseException("Unexpected end of section");
                }

                data.Append(line);
                line = reader.ReadLine();
            }

            if (line == this.endBoundary)
            {
                this.readEndBoundary = true;
            }

            // If we're here we've hit the boundary and have the data!
            var part = new ParameterPart(parameters["name"], data.ToString());

            return part;
        }

        /// <summary>
        /// Parses the header of the next section of the multipart stream and
        ///     determines if it contains file data or parameter data.
        /// </summary>
        /// <param name="reader">
        /// The StreamReader to read data from.
        /// </param>
        /// <exception cref="MultipartParseException">
        /// thrown if unexpected data is hit such as end of stream.
        /// </exception>
        private void ParseSection(RebufferableBinaryReader reader)
        {
            // Our first job is to determine what type of section this is: form data or file.
            // This is a bit tricky because files can still be encoded with Content-Disposition: form-data
            // in the case of single file uploads. Multi-file uploads have Content-Disposition: file according
            // to the spec however in practise it seems that multiple files will be represented by
            // multiple Content-Disposition: form-data files.
            var parameters = new Dictionary<string, string>();

            string line = reader.ReadLine();
            while (line != string.Empty)
            {
                if (line == null)
                {
                    throw new MultipartParseException("Unexpected end of stream");
                }

                if (line == this.boundary || line == this.endBoundary)
                {
                    throw new MultipartParseException("Unexpected end of section");
                }

                // This line parses the header values into a set of key/value pairs. For example:
                // Content-Disposition: form-data; name="textdata" 
                // ["content-disposition"] = "form-data"
                // ["name"] = "textdata"
                // Content-Disposition: form-data; name="file"; filename="data.txt"
                // ["content-disposition"] = "form-data"
                // ["name"] = "file"
                // ["filename"] = "data.txt"
                // Content-Type: text/plain 
                // ["Content-Type"] = "text/plain"
                Dictionary<string, string> values = line.Split(';') // Split the line into n strings delimited by ;
                                                        .Select(x => x.Split(new[] { ':', '=' }))
                                                        .ToDictionary(
                                                            x => x[0].Trim().Replace("\"", string.Empty).ToLower(), 
                                                            x => x[1].Trim().Replace("\"", string.Empty));

                // Here we just want to push all the values that we just retrieved into the 
                // parameters dictionary.
                try
                {
                    foreach (var pair in values)
                    {
                        parameters.Add(pair.Key, pair.Value);
                    }
                }
                catch (ArgumentException ex)
                {
                    throw new MultipartParseException("Duplicate field in section");
                }

                line = reader.ReadLine();
            }

            // Now that we've consumed all the parameters we're up to the body. We're going to do
            // different things depending on if we're parsing a, relatively small, form value or a
            // potentially large file.
            if (parameters.ContainsKey("filename"))
            {
                // Right now we assume that if a section contains filename then it is a file.
                // This assumption needs to be checked, it holds true in firefox but is untested for other 
                // browsers.
                FilePart part = this.ParseFilePart(parameters, reader);
                this.Files.Add(part.Name, part);
            }
            else
            {
                ParameterPart part = this.ParseParameterPart(parameters, reader);
                this.Parameters.Add(part.Name, part);
            }
        }

        #endregion
    }
}