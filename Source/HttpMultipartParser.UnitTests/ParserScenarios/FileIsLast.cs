using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HttpMultipartParser.UnitTests.ParserScenarios
{
    /// <summary>
    ///     Test for cases where the file is last with expected outcomes.
    /// </summary>
    public class FileIsLast
    {
        private static readonly string _testData = TestUtil.TrimAllLines(
            @"-----------------------------41952539122868
            Content-Disposition: form-data; name=""adID""

            1425
            -----------------------------41952539122868
            Content-Disposition: form-data; name=""files[]""; filename=""Capture.JPG""
            Content-Type: image/jpeg

             BinaryData
             -----------------------------41952539122868--");

        private static readonly TestData _testCase = new TestData(
            _testData,
            new List<ParameterPart> {
                new ParameterPart("adID", "1425")
            },
            new List<FilePart> {
                new FilePart("files[]", "Capture.JPG", TestUtil.StringToStreamNoBom("BinaryData"), "image/jpeg", "form-data")
            }
        );

        public FileIsLast()
        {
            foreach (var filePart in _testCase.ExpectedFileData)
            {
                filePart.Data.Position = 0;
            }
        }

        [Fact]
        public void CanHandleFileAsLastSection()
        {
            using (Stream stream = TestUtil.StringToStream(_testCase.Request, Encoding.UTF8))
            {
                var parser = MultipartFormDataParser.Parse(stream, Encoding.UTF8);
                Assert.True(_testCase.Validate(parser));
            }
        }

        [Fact]
        public async Task CanHandleFileAsLastSectionAsync()
        {
            using (Stream stream = TestUtil.StringToStream(_testCase.Request, Encoding.UTF8))
            {
                var parser = await MultipartFormDataParser.ParseAsync(stream, Encoding.UTF8).ConfigureAwait(false);
                Assert.True(_testCase.Validate(parser));
            }
        }
    }
}