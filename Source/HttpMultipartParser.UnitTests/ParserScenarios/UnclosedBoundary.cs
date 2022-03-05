using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HttpMultipartParser.UnitTests.ParserScenarios
{
	public class UnclosedBoundary
	{
		private static readonly string _testData = TestUtil.TrimAllLines(
			@"------51523
            Content-Disposition: form-data; name=""value""
              
            my value
            ------51523"
		);

		private static readonly TestData _testCase = new TestData(
			_testData,
			new List<ParameterPart> {
				new ParameterPart("value", "my value")
			},
			new List<FilePart>()
		);

		public UnclosedBoundary()
		{
			foreach (var filePart in _testCase.ExpectedFileData)
			{
				filePart.Data.Position = 0;
			}
		}

		[Fact]
		public void DoesntInfiniteLoopOnUnclosedInput()
		{
			using (Stream stream = TestUtil.StringToStream(_testCase.Request, Encoding.UTF8))
			{
				// We expect this to throw!
				Assert.Throws<MultipartParseException>(() => MultipartFormDataParser.Parse(stream, Encoding.UTF8));
			}
		}

		[Fact]
		public async Task DoesntInfiniteLoopOnUnclosedInputAsync()
		{
			using (Stream stream = TestUtil.StringToStream(_testCase.Request, Encoding.UTF8))
			{
				// We expect this to throw!
				await Assert.ThrowsAsync<MultipartParseException>(() => MultipartFormDataParser.ParseAsync(stream, Encoding.UTF8)).ConfigureAwait(false);
			}
		}
	}
}
