using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HttpMultipartParser.UnitTests.ParserScenarios
{
	/// <summary>
	///     Test case for multiple files with same name.
	/// </summary>
	public class MultipleFilesWithSameName
	{
		private static readonly string _testData = TestUtil.TrimAllLines(
			@"--boundry
            Content-Disposition: form-data; name=""file1.txt"";filename=""file1.txt"";
            Content-Type: text/plain

            THIS IS TEXT FILE 1
            --boundry
            Content-Disposition: form-data; name=""file2.txt"";filename=""file2.txt"";
            Content-Type: text/plain

            THIS IS TEXT FILE 2 !!!
            --boundry
            Content-Disposition: form-data; name=""file2.txt"";filename=""file2.txt"";
            Content-Type: text/plain

            This is text file 3 1234567890
            --boundry--"
		);

		private static readonly TestData _testCase = new TestData(
			_testData,
			Enumerable.Empty<ParameterPart>().ToList(),
			new List<FilePart> {
				new FilePart( "file1.txt", "file1.txt", TestUtil.StringToStreamNoBom("THIS IS TEXT FILE 1")),
				new FilePart( "file2.txt", "file2.txt", TestUtil.StringToStreamNoBom("THIS IS TEXT FILE 2 !!!")),
				new FilePart( "file2.txt", "file2.txt", TestUtil.StringToStreamNoBom("This is text file 3 1234567890"))
			}
		);

		public MultipleFilesWithSameName()
		{
		}

		/// <summary>
		///     Checks that multiple files don't get in the way of parsing each other
		///     and that everything parses correctly.
		/// </summary>
		[Fact]
		public void MultipleFilesWithSameNameTest()
		{
			using (Stream stream = TestUtil.StringToStream(_testCase.Request, Encoding.UTF8))
			{
				var parser = MultipartFormDataParser.Parse(stream, "boundry", Encoding.UTF8, 16);
				Assert.True(_testCase.Validate(parser));
			}
		}

		[Fact]
		public async Task MultipleFilesWithSameNameTestAsync()
		{
			using (Stream stream = TestUtil.StringToStream(_testCase.Request, Encoding.UTF8))
			{
				var parser = await MultipartFormDataParser.ParseAsync(stream, "boundry", Encoding.UTF8, 16).ConfigureAwait(false);
				Assert.True(_testCase.Validate(parser));
			}
		}
	}
}
