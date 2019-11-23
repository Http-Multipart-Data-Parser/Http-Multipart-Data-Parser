using System.IO;
using System.Linq;
using System.Text;

namespace HttpMultipartParser.UnitTests
{
    internal static class TestUtil
    {
        public static Stream StringToStream(string input)
        {
            return StringToStream(input, Encoding.UTF8);
        }

        public static Stream StringToStream(string input, Encoding encoding)
        {
            var stream = new MemoryStream();
#pragma warning disable IDE0067 // Dispose objects before losing scope
            var writer = new StreamWriter(stream, encoding);
#pragma warning restore IDE0067 // Dispose objects before losing scope
            writer.Write(input);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        // Assumes UTF8
        public static Stream StringToStreamNoBom(string input)
        {
            var stream = new MemoryStream();
#pragma warning disable IDE0067 // Dispose objects before losing scope
            var writer = new StreamWriter(stream, new UTF8Encoding(false));
#pragma warning restore IDE0067 // Dispose objects before losing scope
            writer.Write(input);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static byte[] StringToByteNoBom(string input)
        {
            var encoding = new UTF8Encoding(false);
            return encoding.GetBytes(input);
        }

        public static string TrimAllLines(string input)
        {
            return
                string.Concat(
                    input.Split('\n')
                         .Select(x => x.Trim())
                         .Aggregate((first, second) => first + '\n' + second)
                         .Where(x => x != '\r'));
            /*return Regex.Split(input, "\n")
                        .Select(x => x.Trim())
                        .Where(x => x != "\r")
                        .Aggregate((first, second) => first + System.Environment.NewLine + second);*/
        }
    }
}