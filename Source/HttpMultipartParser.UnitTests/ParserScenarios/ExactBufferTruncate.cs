using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HttpMultipartParser.UnitTests.ParserScenarios
{
	/// <summary>
	///     Tests that the final '--' ending up in a seperate chunk doesn't break everything.
	/// </summary>
	public class ExactBufferTruncate
	{
		private static readonly string _testData = TestUtil.TrimAllLines(
			@"--boundary
            Content-Disposition: form-data; name=""text""

            textdata
            --boundary
            Content-Disposition: form-data; name=""file""; filename=""data.txt""
            Content-Type: text/plain

            1234567890123456789012
            --boundary--"
		);

		/// <summary>
		///     This test has the buffer split such that the final '--' of the end boundary
		///     falls into the next buffer.
		/// </summary>
		private static readonly TestData _testCase = new TestData(
			_testData,
			new List<ParameterPart> {
				new ParameterPart("text", "textdata")
			},
			new List<FilePart> {
				new FilePart( "file", "data.txt", TestUtil.StringToStreamNoBom("1234567890123456789012"))
			}
		);

		public ExactBufferTruncate()
		{
			foreach (var filePart in _testCase.ExpectedFileData)
			{
				filePart.Data.Position = 0;
			}
		}

		/// <summary>
		///     Tests that the final '--' ending up in a seperate chunk doesn't break everything.
		/// </summary>
		[Fact]
		public void CanHandleFinalDashesInSeperateBufferFromEndBinary()
		{
			using (Stream stream = TestUtil.StringToStream(_testCase.Request, Encoding.UTF8))
			{
				var parser = MultipartFormDataParser.Parse(stream, "boundary", Encoding.UTF8, 16);
				Assert.True(_testCase.Validate(parser));
			}
		}

		/// <summary>
		///     Tests that the final '--' ending up in a seperate chunk doesn't break everything.
		/// </summary>
		[Fact]
		public async Task CanHandleFinalDashesInSeperateBufferFromEndBinaryAsync()
		{
			using (Stream stream = TestUtil.StringToStream(_testCase.Request, Encoding.UTF8))
			{
				var parser = await MultipartFormDataParser.ParseAsync(stream, "boundary", Encoding.UTF8, 16);
				Assert.True(_testCase.Validate(parser));
			}
		}
	}
}
