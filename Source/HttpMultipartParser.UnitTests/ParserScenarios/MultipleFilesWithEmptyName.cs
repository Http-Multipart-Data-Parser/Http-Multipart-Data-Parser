using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HttpMultipartParser.UnitTests.ParserScenarios
{
	/// <summary>
	///     Test case for multiple files with no name.
	/// </summary>
	public class MultipleFilesWithEmptyName
	{
		private static readonly string _testData = TestUtil.TrimAllLines(
			@"--boundary
            Content-Disposition: form-data; name="""";filename=""file1.txt"";
            Content-Type: text/plain

            THIS IS TEXT FILE 1
            --boundary
            Content-Disposition: form-data; name="""";filename=""file2.txt"";
            Content-Type: text/plain

            THIS IS TEXT FILE 2 !!!
            --boundary
            Content-Disposition: form-data; name="""";filename=""file3.txt"";
            Content-Type: text/plain

            This is text file 3 1234567890
            --boundary--"
		);

		private static readonly TestData _testCase = new TestData(
			_testData,
			Enumerable.Empty<ParameterPart>().ToList(),
			new List<FilePart> {
				new FilePart( "", "file1.txt", TestUtil.StringToStreamNoBom("THIS IS TEXT FILE 1")),
				new FilePart( "", "file2.txt", TestUtil.StringToStreamNoBom("THIS IS TEXT FILE 2 !!!")),
				new FilePart( "", "file3.txt", TestUtil.StringToStreamNoBom("This is text file 3 1234567890"))
			}
		);

		public MultipleFilesWithEmptyName()
		{
		}

		/// <summary>
		///     Checks that multiple files don't get in the way of parsing each other
		///     and that everything parses correctly.
		/// </summary>
		[Fact]
		public void MultipleFilesWithNoNameTest()
		{
			using (Stream stream = TestUtil.StringToStream(_testCase.Request, Encoding.UTF8))
			{
				var parser = MultipartFormDataParser.Parse(stream, "boundary", Encoding.UTF8, 16);
				Assert.True(_testCase.Validate(parser));
			}
		}

		[Fact]
		public async Task MultipleFilesWithNoNameAsync()
		{
			using (Stream stream = TestUtil.StringToStream(_testCase.Request, Encoding.UTF8))
			{
				var parser = await MultipartFormDataParser.ParseAsync(stream, "boundary", Encoding.UTF8, 16);
				Assert.True(_testCase.Validate(parser));
			}
		}
	}
}
