using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HttpMultipartParser.UnitTests.ParserScenarios
{
    // These unit tests allow reproducing the "Value cannot be null. (Parameter 'source')" exception discussed here:
    // https://github.com/Http-Multipart-Data-Parser/Http-Multipart-Data-Parser/issues/104
    // https://github.com/Http-Multipart-Data-Parser/Http-Multipart-Data-Parser/issues/102
    // https://github.com/Http-Multipart-Data-Parser/Http-Multipart-Data-Parser/issues/66

    // This problem was caused by the fact that developers would read the content of their stream prior to parsing their content
    // which had the side effect of moving the 'Position' to the end of the stream and therefore we would get a null value when
    // reading the content of the stream in StreamingMultipartFormDataParser.DetectBoundary (and DetectBoundaryAsync as well).
    // As of March 2022, this problem was resolved by throwing a more descriptive exception.
    public class StreamPositionHasBeenMoved
    {
        private static readonly TestData _testCase = new TestData(
            "Hello world",
            Enumerable.Empty<ParameterPart>().ToList(),
            Enumerable.Empty<FilePart>().ToList()
        );

        public StreamPositionHasBeenMoved()
        {
            foreach (var filePart in _testCase.ExpectedFileData)
            {
                filePart.Data.Position = 0;
            }
        }

        [Fact]
        public void End_of_stream_and_boundary_is_known()
        {
            using (Stream stream = TestUtil.StringToStream(_testCase.Request, Encoding.UTF8))
            {
                // Move the Position to the end of the stream
                var sr = new StreamReader(stream);
                var content = sr.ReadToEnd();

                // When the developer provides the boundary, the parser does not have to determine the boundary
                // and therefore the DetectBoundary method is not invoked which avoids the problem altogether.
                // However, the parser is unable to find the provided boundary and throws a meaningful exception.
                Assert.Throws<MultipartParseException>(() => MultipartFormDataParser.Parse(stream, "MyBoundary"));
            }
        }

        [Fact]
        public void End_of_stream_and_boundary_is_unknown()
        {
            using (Stream stream = TestUtil.StringToStream(_testCase.Request, Encoding.UTF8))
            {
                // Move the Position to the end of the stream
                var sr = new StreamReader(stream);
                var content = sr.ReadToEnd();

                // As of March 2022, the problem was resolved by throwing a more descriptive exception
                Assert.Throws<MultipartParseException>(() => MultipartFormDataParser.Parse(stream));
            }
        }

        [Fact]
        public void Middle_of_stream_and_boundary_is_known()
        {
            using (Stream stream = TestUtil.StringToStream(_testCase.Request, Encoding.UTF8))
            {
                // Move the Position to an arbitrary location
                stream.Position = 3;

                // When the developer provides the boundary, the parser does not have to determine the boundary
                // and therefore the DetectBoundary method is not invoked which avoids the problem altogether.
                // However, the parser is unable to find the provided boundary and throws a meaningful exception.
                Assert.Throws<MultipartParseException>(() => MultipartFormDataParser.Parse(stream, "MyBoundary"));
            }
        }

        [Fact]
        public void Middle_of_stream_and_boundary_is_unknown()
        {
            using (Stream stream = TestUtil.StringToStream(_testCase.Request, Encoding.UTF8))
            {
                // Move the Position to an arbitrary location
                stream.Position = 3;

                // As of March 2022, the problem was resolved by throwing a more descriptive exception
                Assert.Throws<MultipartParseException>(() => MultipartFormDataParser.Parse(stream));
            }
        }

        [Fact]
        public async Task End_of_stream_and_boundary_is_known_async()
        {
            using (Stream stream = TestUtil.StringToStream(_testCase.Request, Encoding.UTF8))
            {
                // Move the Position to the end of the stream
                var sr = new StreamReader(stream);
                var content = await sr.ReadToEndAsync().ConfigureAwait(false);

                // When the developer provides the boundary, the parser does not have to determine the boundary
                // and therefore the DetectBoundary method is not invoked which avoids the problem altogether.
                // However, the parser is unable to find the provided boundary and throws a meaningful exception.
                await Assert.ThrowsAsync<MultipartParseException>(() => MultipartFormDataParser.ParseAsync(stream, "MyBoundary")).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task End_of_stream_and_boundary_is_unknown_async()
        {
            using (Stream stream = TestUtil.StringToStream(_testCase.Request, Encoding.UTF8))
            {
                // Move the Position to the end of the stream
                var sr = new StreamReader(stream);
                var content = await sr.ReadToEndAsync().ConfigureAwait(false);

                // As of March 2022, the problem was resolved by throwing a more descriptive exception
                await Assert.ThrowsAsync<MultipartParseException>(() => MultipartFormDataParser.ParseAsync(stream)).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task Middle_of_stream_and_boundary_is_known_async()
        {
            using (Stream stream = TestUtil.StringToStream(_testCase.Request, Encoding.UTF8))
            {
                // Move the Position to an arbitrary location
                stream.Position = 3;

                // When the developer provides the boundary, the parser does not have to determine the boundary
                // and therefore the DetectBoundary method is not invoked which avoids the problem altogether.
                // However, the parser is unable to find the provided boundary and throws a meaningful exception.
                await Assert.ThrowsAsync<MultipartParseException>(() => MultipartFormDataParser.ParseAsync(stream, "MyBoundary")).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task Middle_of_stream_and_boundary_is_unknown_async()
        {
            using (Stream stream = TestUtil.StringToStream(_testCase.Request, Encoding.UTF8))
            {
                // Move the Position to an arbitrary location
                stream.Position = 3;

                // As of March 2022, the problem was resolved by throwing a more descriptive exception
                await Assert.ThrowsAsync<MultipartParseException>(() => MultipartFormDataParser.ParseAsync(stream)).ConfigureAwait(false);
            }
        }
    }
}
