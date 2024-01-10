using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace HttpMultipartParser.UnitTests.ParserScenarios
{
	/// <summary>
	///     The small data test.
	/// </summary>
	public class SmallData
	{
		private static readonly string _testData = TestUtil.TrimAllLines(
			@"-----------------------------265001916915724
            Content-Disposition: form-data; name=""textdata""
                
            Testdata
            -----------------------------265001916915724
            Content-Disposition: form-data; name=""file""; filename=""data.txt""
            Content-Type: application/octet-stream

            This is a small file
            -----------------------------265001916915724
            Content-Disposition: form-data; name=""submit""

            Submit
            -----------------------------265001916915724--"
		);

		/// <summary>
		///     The small data test case with expected outcomes
		/// </summary>
		private static readonly TestData _testCase = new TestData(
			_testData,
			new List<ParameterPart> {
				new ParameterPart("textdata", "Testdata"),
				new ParameterPart("submit", "Submit"),
			},
			new List<FilePart> {
				new FilePart( "file", "data.txt", TestUtil.StringToStreamNoBom("This is a small file"), "application/octet-stream", "form-data")
			}
		);

		/// <summary>
		///     Initializes the test data before each run, this primarily
		///     consists of resetting data stream positions.
		/// </summary>
		public SmallData()
		{
			foreach (var filePart in _testCase.ExpectedFileData)
			{
				filePart.Data.Position = 0;
			}
		}

		[Fact]
		public void SmallDataTest()
		{
			using (Stream stream = TestUtil.StringToStream(_testCase.Request))
			{
				// The boundary is missing the first two -- in accordance with the multipart
				// spec. (A -- is added by the parser, this boundary is what would be sent in the
				// request header)
				var parser = MultipartFormDataParser.Parse(stream, "---------------------------265001916915724");
				Assert.True(_testCase.Validate(parser));
			}
		}

		[Fact]
		public async Task SmallDataTestAsync()
		{
			using (Stream stream = TestUtil.StringToStream(_testCase.Request))
			{
				// The boundary is missing the first two -- in accordance with the multipart
				// spec. (A -- is added by the parser, this boundary is what would be sent in the
				// request header)
				var parser = await MultipartFormDataParser.ParseAsync(stream, "---------------------------265001916915724").ConfigureAwait(false);
				Assert.True(_testCase.Validate(parser));
			}
		}
	}
}
