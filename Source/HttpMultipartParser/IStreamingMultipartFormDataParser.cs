using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HttpMultipartParser
{
    /// <summary>
    /// The FileStreamDelegate defining functions that can handle file stream data from this parser.
    ///
    /// Delegates can assume that the data is sequential i.e. the data received by any delegates will be
    /// the data immediately following any previously received data.
    /// </summary>
    /// <param name="name">The name of the multipart data.</param>
    /// <param name="fileName">The name of the file.</param>
    /// <param name="contentType">The content type of the multipart data.</param>
    /// <param name="contentDisposition">The content disposition of the multipart data.</param>
    /// <param name="buffer">Some of the data from the file (not necessarily all of the data).</param>
    /// <param name="bytes">The length of data in buffer.</param>
    /// <param name="partNumber">Each chunk (or "part") in a given file is sequentially numbered, starting at zero.</param>
    /// <param name="additionalProperties">Properties other than the "well known" ones (such as name, filename, content-type, etc.) associated with a file stream.</param>
    public delegate void FileStreamDelegate(string name, string fileName, string contentType, string contentDisposition, byte[] buffer, int bytes, int partNumber, IDictionary<string, string> additionalProperties);

    /// <summary>
    /// The StreamClosedDelegate defining functions that can handle stream being closed.
    /// </summary>
    public delegate void StreamClosedDelegate();

    /// <summary>
    /// The ParameterDelegate defining functions that can handle multipart parameter data.
    /// </summary>
    /// <param name="part">The parsed parameter part.</param>
    public delegate void ParameterDelegate(ParameterPart part);

    /// <summary>
    ///     Provides methods to parse a
    ///     <see href="http://www.ietf.org/rfc/rfc2388.txt">
    ///         <c>multipart/form-data</c>
    ///     </see>
    ///     stream into it's parameters and file data.
    /// </summary>
    public interface IStreamingMultipartFormDataParser
    {
        /// <summary>
        /// Gets or sets the FileHandler. Delegates attached to this property will receive sequential file stream data from this parser.
        /// </summary>
        FileStreamDelegate FileHandler { get; set; }

        /// <summary>
        /// Gets or sets the ParameterHandler. Delegates attached to this property will receive parameter data.
        /// </summary>
        ParameterDelegate ParameterHandler { get; set; }

        /// <summary>
        /// Gets or sets the StreamClosedHandler. Delegates attached to this property will be notified when the source stream is exhausted.
        /// </summary>
        StreamClosedDelegate StreamClosedHandler { get; set; }

        /// <summary>
        /// Execute the parser. This should be called after all handlers have been set.
        /// </summary>
        void Run();

        /// <summary>
        /// Execute the parser asynchronously. This should be called after all handlers have been set.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The asynchronous task.</returns>
        Task RunAsync(CancellationToken cancellationToken = default);
    }
}