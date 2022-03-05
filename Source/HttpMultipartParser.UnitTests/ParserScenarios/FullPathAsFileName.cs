using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HttpMultipartParser.UnitTests.ParserScenarios
{
	public class FullPathAsFileName
	{
		private static readonly string _testData = TestUtil.TrimAllLines(
			@"-----------------------------7de6cc440a46
            Content-Disposition: form-data; name=""file""; filename=""C:\test\test;abc.txt""
            Content-Type: text/plain

            test
            -----------------------------7de6cc440a46--"
		);

		private static readonly TestData _testCase = new TestData(
			_testData,
			new List<ParameterPart>(),
			new List<FilePart> {
					new FilePart("file", "test;abc.txt", TestUtil.StringToStream("test"), "text/plain", "form-data")
			}
		);

		public FullPathAsFileName()
		{
			foreach (var filePart in _testCase.ExpectedFileData)
			{
				filePart.Data.Position = 0;
			}
		}

		[Fact]
		public void HandlesFullPathAsFileNameWithSemicolonCorrectly()
		{
			using (Stream stream = TestUtil.StringToStream(_testCase.Request, Encoding.UTF8))
			{
				var parser = MultipartFormDataParser.Parse(stream, Encoding.UTF8);
				Assert.True(_testCase.Validate(parser));
			}
		}

		[Fact]
		public async Task HandlesFullPathAsFileNameWithSemicolonCorrectlyAsync()
		{
			using (Stream stream = TestUtil.StringToStream(_testCase.Request, Encoding.UTF8))
			{
				var parser = await MultipartFormDataParser.ParseAsync(stream, Encoding.UTF8).ConfigureAwait(false);
				Assert.True(_testCase.Validate(parser));
			}
		}
	}
}
