using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HttpMultipartParser.UnitTests.ParserScenarios
{
	public class MixedUnicodeWidthAndAsciiWidthCharacters
	{
		private static readonly string _testData = TestUtil.TrimAllLines(
			@"--boundary_.oOo._MjQ1NTU=OTk3Ng==MjcxODE=
            Content-Disposition: form-data; name=""psAdTitle""

            Bonjour poignée 
            --boundary_.oOo._MjQ1NTU=OTk3Ng==MjcxODE=--"
		);

		private static readonly TestData _testCase = new TestData(
			_testData,
			new List<ParameterPart> {
				new ParameterPart("psAdTitle", "Bonjour poignée")
			},
			new List<FilePart>()
		);

		public MixedUnicodeWidthAndAsciiWidthCharacters()
		{
			foreach (var filePart in _testCase.ExpectedFileData)
			{
				filePart.Data.Position = 0;
			}
		}

		[Fact]
		public void CanHandleUnicodeWidthAndAsciiWidthCharacters()
		{
			using (
				Stream stream = TestUtil.StringToStream(_testCase.Request, Encoding.UTF8))
			{
				var parser = MultipartFormDataParser.Parse(stream, Encoding.UTF8);
				Assert.True(_testCase.Validate(parser));
			}
		}

		[Fact]
		public async Task CanHandleUnicodeWidthAndAsciiWidthCharactersAsync()
		{
			using (
				Stream stream = TestUtil.StringToStream(_testCase.Request, Encoding.UTF8))
			{
				var parser = await MultipartFormDataParser.ParseAsync(stream, Encoding.UTF8);
				Assert.True(_testCase.Validate(parser));
			}
		}
	}
}
