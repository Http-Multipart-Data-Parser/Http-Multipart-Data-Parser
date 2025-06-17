using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HttpMultipartParser.UnitTests.ParserScenarios
{
	public class SingleFile
	{
		private static readonly string _testData = TestUtil.TrimAllLines(
			@"--boundary
            Content-Disposition: form-data; name=""file""; filename=""data.txt"";
            Content-Type: text/plain

            I am the first data1
            --boundary--"
		);

		/// <summary>
		///     Test case for single files.
		/// </summary>
		private static readonly TestData _testCase = new TestData(
			_testData,
			new List<ParameterPart> { },
			new List<FilePart> {
				new FilePart("file", "data.txt", TestUtil.StringToStreamNoBom("I am the first data1"), "text/plain", "form-data")
			}
	   );

		/// <summary>
		///     Initializes the test data before each run, this primarily
		///     consists of resetting data stream positions.
		/// </summary>
		public SingleFile()
		{
			foreach (var filePart in _testCase.ExpectedFileData)
			{
				filePart.Data.Position = 0;
			}
		}

		[Fact]
		public void SingleFileTest()
		{
			using (Stream stream = TestUtil.StringToStream(_testCase.Request, Encoding.UTF8))
			{
				var parser = MultipartFormDataParser.Parse(stream, "boundary", Encoding.UTF8, 16);
				Assert.True(_testCase.Validate(parser));
			}
		}

		[Fact]
		public async Task SingleFileTest_Async()
		{
			using (Stream stream = TestUtil.StringToStream(_testCase.Request, Encoding.UTF8))
			{
				var parser = await MultipartFormDataParser.ParseAsync(stream, "boundary", Encoding.UTF8, 16, cancellationToken: TestContext.Current.CancellationToken);
				Assert.True(_testCase.Validate(parser));
			}
		}
	}
}
