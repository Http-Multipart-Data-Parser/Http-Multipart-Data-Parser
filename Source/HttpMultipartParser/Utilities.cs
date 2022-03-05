using Microsoft.IO;
using System.Buffers;

namespace HttpMultipartParser
{
	internal static class Utilities
	{
		internal static RecyclableMemoryStreamManager MemoryStreamManager { get; } = new RecyclableMemoryStreamManager();

		internal static ArrayPool<byte> ArrayPool { get; } = ArrayPool<byte>.Shared;
	}
}
