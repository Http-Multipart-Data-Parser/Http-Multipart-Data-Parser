using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Xunit;

namespace HttpMultipartParser.UnitTests.ParserScenarios
{
    public class TinyData
    {
        private static readonly string _testData = TestUtil.TrimAllLines(@"--boundry
            Content-Disposition: form-data; name=""text""

            textdata
            --boundry
            Content-Disposition: form-data; name=""file""; filename=""data.txt""
            Content-Type: text/plain

            tiny
            --boundry
            Content-Disposition: form-data; name=""after""

            afterdata
            --boundry--");

        private static readonly TestData _testCase = new TestData(
            _testData,
            new List<ParameterPart> {
                new ParameterPart("text", "textdata"),
                new ParameterPart("after", "afterdata")
            },
            new List<FilePart> {
                new FilePart( "file", "data.txt", TestUtil.StringToStreamNoBom("tiny"))
            }
        );

        public TinyData()
        {
            foreach (var filePart in _testCase.ExpectedFileData)
            {
                filePart.Data.Position = 0;
            }
        }

        /// <summary>
        ///     Tests for correct detection of the boundary in the input stream.
        /// </summary>
        [Fact]
        public void CanAutoDetectBoundary()
        {
            using (Stream stream = TestUtil.StringToStream(_testCase.Request, Encoding.UTF8))
            {
                var parser = MultipartFormDataParser.Parse(stream);
                Assert.True(_testCase.Validate(parser));
            }
        }

        /// <summary>
        ///     Ensures that boundary detection works even when the boundary spans
        ///     two different buffers.
        /// </summary>
        [Fact]
        public void CanDetectBoundariesCrossBuffer()
        {
            using (Stream stream = TestUtil.StringToStream(_testCase.Request, Encoding.UTF8))
            {
                var parser = MultipartFormDataParser.Parse(stream, "boundry", Encoding.UTF8, 16);
                Assert.True(_testCase.Validate(parser));
            }
        }

        /// <summary>
        ///     Ensure that mixed newline formats are correctly handled.
        /// </summary>
        [Fact]
        public void CorrectlyHandleMixedNewlineFormats()
        {
            // Replace the first '\n' with '\r\n'
            var regex = new Regex(Regex.Escape("\n"));
            string request = regex.Replace(_testCase.Request, "\r\n", 1);
            using (Stream stream = TestUtil.StringToStream(request, Encoding.UTF8))
            {
                var parser = MultipartFormDataParser.Parse(stream, "boundry", Encoding.UTF8);
                Assert.True(_testCase.Validate(parser));
            }
        }

        /// <summary>
        ///     Tests for correct handling of <c>crlf (\r\n)</c> in the input stream.
        /// </summary>
        [Fact]
        public void CorrectlyHandlesCRLF()
        {
            string request = _testCase.Request.Replace("\n", "\r\n");
            using (Stream stream = TestUtil.StringToStream(request, Encoding.UTF8))
            {
                var parser = MultipartFormDataParser.Parse(stream, "boundry", Encoding.UTF8);
                Assert.True(_testCase.Validate(parser));
            }
        }

        /// <summary>
        ///     The tiny data test.
        /// </summary>
        [Fact]
        public void TinyDataTest()
        {
            using (Stream stream = TestUtil.StringToStream(_testCase.Request, Encoding.UTF8))
            {
                var parser = MultipartFormDataParser.Parse(stream, "boundry", Encoding.UTF8);
                Assert.True(_testCase.Validate(parser));
            }
        }

        [Fact]
        public void DoesNotCloseTheStream()
        {
            using (Stream stream = TestUtil.StringToStream(_testCase.Request, Encoding.UTF8))
            {
                var parser = MultipartFormDataParser.Parse(stream, "boundry", Encoding.UTF8);
                Assert.True(_testCase.Validate(parser));

                stream.Position = 0;
                Assert.True(true, "A closed stream would throw ObjectDisposedException");
            }
        }

        [Fact]
        public void GetParameterValueReturnsNullIfNoParameterFound()
        {
            using (Stream stream = TestUtil.StringToStream(_testCase.Request, Encoding.UTF8))
            {
                var parser = MultipartFormDataParser.Parse(stream, "boundry", Encoding.UTF8);
                Assert.Null(parser.GetParameterValue("does not exist"));
            }
        }

        [Fact]
        public void CanDetectBoundriesWithNewLineInNextBuffer()
        {
            for (int i = 16; i < _testCase.Request.Length; i++)
            {
                using (Stream stream = TestUtil.StringToStream(_testCase.Request, Encoding.UTF8))
                {
                    var parser = MultipartFormDataParser.Parse(stream, "boundry", Encoding.UTF8, i);
                    Assert.True(_testCase.Validate(parser), $"Failure in buffer length {i}");
                }
            }
        }
    }
}