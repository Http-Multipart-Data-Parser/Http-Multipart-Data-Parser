using System.IO;
using System.Threading.Tasks;

namespace HttpMultipartParser
{
    /// <summary>
    /// Read forward-only binary asynchronous stream
    /// </summary>
    public interface IInputStreamAsync
    {
        /// <summary>
        /// Asynchronously reads a number of bytes determined by buffer capacity and writes them to the buffer,
        /// advancing the reading cursor.
        /// </summary>
        /// <param name="bytes">The buffer where to read the bytes from.</param>
        /// <returns>The number of bytes actually read.</returns>
        Task<int> ReadBytesAsync(byte[] bytes);
    }

    /// <summary>
    /// Simple implementation of IInputStreamAsync using .NET streams.
    /// </summary>
    public class InputStreamImpl : IInputStreamAsync
    {
        private readonly Stream Stream;

        /// <summary>
        /// Constructor for InputStreamImpl.
        /// </summary>
        /// <param name="stream">A .NET stream which must at least allow read forward operations.</param>
        public InputStreamImpl(Stream stream)
        {
            this.Stream = stream;
        }

        /// <summary>
        /// Asynchronously reads a number of bytes determined by buffer capacity and writes them to the buffer,
        /// advancing the reading cursor.
        /// </summary>
        /// <param name="bytes">The buffer where to read the bytes from.</param>
        /// <returns>The number of bytes actually read.</returns>
        public async Task<int> ReadBytesAsync(byte[] bytes)
        {
            return await this.Stream.ReadAsync(bytes, 0, bytes.Length);
        }
    }
}
