using System.Threading;
using System.Threading.Tasks;

namespace HttpMultipartParser
{
	/// <summary>
	///     Provides methods to parse a
	///     <see href="http://www.ietf.org/rfc/rfc2388.txt">
	///         <c>multipart/form-data</c>
	///     </see>
	///     stream into it's parameters and file data.
	/// </summary>
	public interface IStreamingBinaryMultipartFormDataParser
	{
		/// <summary>
		/// Gets or sets the FileHandler. Delegates attached to this property will receive sequential file stream data from this parser.
		/// </summary>
		FileStreamDelegate FileHandler { get; set; }

		/// <summary>
		/// Gets or sets the ParameterHandler. Delegates attached to this property will receive parameter data.
		/// </summary>
		BinaryParameterDelegate ParameterHandler { get; set; }

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
