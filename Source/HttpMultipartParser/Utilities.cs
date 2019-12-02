using Microsoft.IO;

namespace HttpMultipartParser
{
    internal static class Utilities
    {
        internal static RecyclableMemoryStreamManager MemoryStreamManager { get; } = new RecyclableMemoryStreamManager();
    }
}