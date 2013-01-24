namespace HttpMultipartParserUnitTest
{
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    internal static class TestUtil
    {        public static Stream StringToStream(string input)
        {
            return StringToStream(input, Encoding.UTF8);
        }

        public static Stream StringToStream(string input, Encoding encoding)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream, encoding);
            writer.Write(input);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        // Assumes UTF8
        public static Stream StringToStreamNoBom(string input)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream, new UTF8Encoding(false));
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
            return Regex.Split(input, System.Environment.NewLine)
                        .Select(x => x.Trim())
                        .Aggregate((first, second) => first + System.Environment.NewLine + second);
        }
    }
}
