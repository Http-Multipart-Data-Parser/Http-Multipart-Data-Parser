using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HttpMultipartParser.UnitTests.ParserScenarios
{
	public class MjpegStream
	{
		private static readonly string _testData = TestUtil.TrimAllLines(
			@"--MOBOTIX_Fast_Serverpush
            Content-Type: image/jpeg

            <jpeg bytes>
            --MOBOTIX_Fast_Serverpush
            Content-Type: image/jpeg

            <jpeg bytes>
            --MOBOTIX_Fast_Serverpush
            Content-Type: image/jpeg

            ... and so on ...
            --MOBOTIX_Fast_Serverpush--"
		);

		/// <summary>
		///     Test case for mjpeg stream.
		/// </summary>
		private static readonly TestData _testCase = new TestData(
			_testData,
			new List<ParameterPart> { },
			new List<FilePart> {
				new FilePart(null, null, TestUtil.StringToStreamNoBom("<jpeg bytes>"), "image/jpeg", "form-data"),
				new FilePart(null, null, TestUtil.StringToStreamNoBom("<jpeg bytes>"), "image/jpeg", "form-data"),
				new FilePart(null, null, TestUtil.StringToStreamNoBom("... and so on ..."), "image/jpeg", "form-data")
			}
	   );

		/// <summary>
		///     Initializes the test data before each run, this primarily
		///     consists of resetting data stream positions.
		/// </summary>
		public MjpegStream()
		{
			foreach (var filePart in _testCase.ExpectedFileData)
			{
				filePart.Data.Position = 0;
			}
		}

		[Fact]
		public void MjpegStreamTest()
		{
			using (Stream stream = TestUtil.StringToStream(_testCase.Request, Encoding.UTF8))
			{
				var parser = MultipartFormDataParser.Parse(stream, "MOBOTIX_Fast_Serverpush", Encoding.UTF8, 32);
				Assert.True(_testCase.Validate(parser));
			}
		}

		[Fact]
		public async Task MjpegStreamTest_Async()
		{
			using (Stream stream = TestUtil.StringToStream(_testCase.Request, Encoding.UTF8))
			{
				var parser = await MultipartFormDataParser.ParseAsync(stream, "MOBOTIX_Fast_Serverpush", Encoding.UTF8, 32).ConfigureAwait(false);
				Assert.True(_testCase.Validate(parser));
			}
		}
	}
}
