using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HttpMultipartParser.UnitTests.ParserScenarios
{
    public class EmptyForm
    {
        private static readonly string _testData = TestUtil.TrimAllLines(
           @"------WebKitFormBoundaryb4SfPlH9Bv7c2PKS--"
        );

        private static readonly TestData _testCase = new TestData(
            _testData,
            Enumerable.Empty<ParameterPart>().ToList(),
            Enumerable.Empty<FilePart>().ToList()
        );

        public EmptyForm()
        {
            foreach (var filePart in _testCase.ExpectedFileData)
            {
                filePart.Data.Position = 0;
            }
        }

        [Fact]
        public void Parse_empty_form_boundary_specified()
        {
            using (Stream stream = TestUtil.StringToStream(_testCase.Request, Encoding.UTF8))
            {
                var parser = MultipartFormDataParser.Parse(stream, "----WebKitFormBoundaryb4SfPlH9Bv7c2PKS");
                Assert.True(_testCase.Validate(parser));
            }
        }

        [Fact]
        public void Parse_empty_form_boundary_omitted()
        {
            using (Stream stream = TestUtil.StringToStream(_testCase.Request, Encoding.UTF8))
            {
                var parser = MultipartFormDataParser.Parse(stream);
                Assert.True(_testCase.Validate(parser));
            }
        }

        [Fact]
        public async Task Parse_empty_form_boundary_specified_async()
        {
            using (Stream stream = TestUtil.StringToStream(_testCase.Request, Encoding.UTF8))
            {
                var parser = await MultipartFormDataParser.ParseAsync(stream, "----WebKitFormBoundaryb4SfPlH9Bv7c2PKS").ConfigureAwait(false);
                Assert.True(_testCase.Validate(parser));
            }
        }

        [Fact]
        public async Task Parse_empty_form_boundary_omitted_async()
        {
            using (Stream stream = TestUtil.StringToStream(_testCase.Request, Encoding.UTF8))
            {
                var parser = await MultipartFormDataParser.ParseAsync(stream).ConfigureAwait(false);
                Assert.True(_testCase.Validate(parser));
            }
        }
    }
}