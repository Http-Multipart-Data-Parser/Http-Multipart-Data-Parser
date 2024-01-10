using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HttpMultipartParser.UnitTests.ParserScenarios
{
	public class BlankLinesBeforeFirstBoundary
	{
		private static readonly string _testData = TestUtil.TrimAllLines(
			@"--boundary
            Content-Disposition: form-data; name=""text""

            textdata
            --boundary--"
		);

		// This test case has a few blank lines before the first boundary marker
		// This unusual scenario is described in GH-116
		// https://github.com/Http-Multipart-Data-Parser/Http-Multipart-Data-Parser/issues/116
		private static readonly TestData _testCase = new TestData(
			$"\n\n\n{_testData}", // Intentionally add a few blank lines before the data. These blank lines should be ignored by the parser when attempting to detect the boundary marker
			new List<ParameterPart> {
				new ParameterPart("text", "textdata"),
			},
			new List<FilePart>()
		);

		public BlankLinesBeforeFirstBoundary()
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
		///     Tests for correct detection of the boundary in the input stream.
		/// </summary>
		[Fact]
		public async Task CanAutoDetectBoundaryAsync()
		{
			using (Stream stream = TestUtil.StringToStream(_testCase.Request, Encoding.UTF8))
			{
				var parser = await MultipartFormDataParser.ParseAsync(stream, Encoding.UTF8);
				Assert.True(_testCase.Validate(parser));
			}
		}
	}
}
