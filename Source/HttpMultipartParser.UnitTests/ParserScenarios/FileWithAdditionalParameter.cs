using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HttpMultipartParser.UnitTests.ParserScenarios
{
    public class FileWithAdditionalParameter
    {
        private static readonly string _testData = TestUtil.TrimAllLines(
            @"-----------------------------41952539122868
            Content-ID: <some string>
            Content-Type: application/octet-stream

            <... binary data ...>
            -----------------------------41952539122868--"
        );

        /// <summary>
        ///     Test case for files with additional parameter.
        /// </summary>
        private static readonly TestData _testCase = new TestData(
            _testData,
            new List<ParameterPart> { },
            new List<FilePart> {
                new FilePart(null, null, TestUtil.StringToStreamNoBom("<... binary data ...>"), (new[] { new KeyValuePair<string, string>("content-id", "<some string>") }).ToDictionary(kvp => kvp.Key, kvp => kvp.Value), "application/octet-stream", "form-data")
            }
       );

        /// <summary>
        ///     Initializes the test data before each run, this primarily
        ///     consists of resetting data stream positions.
        /// </summary>
        public FileWithAdditionalParameter()
        {
            foreach (var filePart in _testCase.ExpectedFileData)
            {
                filePart.Data.Position = 0;
            }
        }

        [Fact]
        public void FileWithAdditionalParameterTest()
        {
            using (Stream stream = TestUtil.StringToStream(_testCase.Request, Encoding.UTF8))
            {
                var parser = MultipartFormDataParser.Parse(stream, Encoding.UTF8);
                Assert.True(_testCase.Validate(parser));
            }
        }

        [Fact]
        public async Task FileWithAdditionalParameterTest_Async()
        {
            using (Stream stream = TestUtil.StringToStream(_testCase.Request, Encoding.UTF8))
            {
                var parser = await MultipartFormDataParser.ParseAsync(stream, Encoding.UTF8).ConfigureAwait(false);
                Assert.True(_testCase.Validate(parser));
            }
        }
    }
}