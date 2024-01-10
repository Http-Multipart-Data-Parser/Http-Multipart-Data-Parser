using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HttpMultipartParser.UnitTests.ParserScenarios
{
	public class InvalidPart
	{
		// For details see: https://github.com/Http-Multipart-Data-Parser/Http-Multipart-Data-Parser/issues/110
		// This data is considered invalid because it contains nothing but empty lines
		private static readonly string _testData = @"--KoZIhvcNAQcB

--KoZIhvcNAQcB--";

		private static readonly TestData _testCase = new TestData(
			_testData,
			Enumerable.Empty<ParameterPart>().ToList(),
			Enumerable.Empty<FilePart>().ToList()
		);

		public InvalidPart()
		{
			foreach (var filePart in _testCase.ExpectedFileData)
			{
				filePart.Data.Position = 0;
			}
		}

		[Fact]
		public void Exception_is_thrown_when_attempting_to_parse()
		{
			// The default behavior is to throw an exception when the form contains an invalid section.
			using (Stream stream = TestUtil.StringToStream(_testCase.Request, Encoding.UTF8))
			{
				Assert.Throws<MultipartParseException>(() => MultipartFormDataParser.Parse(stream));
			}
		}

		[Fact]
		public async Task Exception_is_thrown_when_attempting_to_parse_async()
		{
			// The default behavior is to throw an exception when the form contains an invalid section.
			using (Stream stream = TestUtil.StringToStream(_testCase.Request, Encoding.UTF8))
			{
				await Assert.ThrowsAsync<MultipartParseException>(() => MultipartFormDataParser.ParseAsync(stream));
			}
		}

		[Fact]
		public void Invalid_part_is_ignored()
		{
			using (Stream stream = TestUtil.StringToStream(_testCase.Request, Encoding.UTF8))
			{
				var parser = MultipartFormDataParser.Parse(stream, ignoreInvalidParts: true);
				Assert.Equal(0, parser.Files.Count);
				Assert.Equal(0, parser.Parameters.Count);
			}
		}

		[Fact]
		public async Task Invalid_part_is_ignored_async()
		{
			using (Stream stream = TestUtil.StringToStream(_testCase.Request, Encoding.UTF8))
			{
				var parser = await MultipartFormDataParser.ParseAsync(stream, ignoreInvalidParts: true);
				Assert.Equal(0, parser.Files.Count);
				Assert.Equal(0, parser.Parameters.Count);
			}
		}
	}
}
