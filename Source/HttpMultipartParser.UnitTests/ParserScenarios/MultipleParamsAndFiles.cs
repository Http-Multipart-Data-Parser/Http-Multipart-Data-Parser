using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HttpMultipartParser.UnitTests.ParserScenarios
{
	/// <summary>
	///     Test case for multiple parameters and files.
	/// </summary>
	public class MultipleParamsAndFiles
	{
		private static readonly string _testData = TestUtil.TrimAllLines(
			@"--boundary
            Content-Disposition: form-data; name=""text""
              
            textdata
            --boundary
            Content-Disposition: form-data; name=""after"";TestForTextWithoutSplit
              
            afterdata
            --boundary
            Content-Disposition: form-data; name=""file""; filename=""data.txt""
            Content-Type: text/plain

            I am the first data 
            --boundary
            Content-Disposition: form-data;TestForTextWithoutSplit; name=""newfile""; filename=""superdata.txt""
            Content-Type: text/plain

            I am the second data
            --boundary
            Content-Disposition: form-data; name=""never""

            neverdata 
            --boundary
            Content-Disposition: form-data; name=""waylater""

            waylaterdata 
            --boundary--"
		);

		private static readonly TestData _testCase = new TestData(
			_testData,
			new List<ParameterPart> {
				new ParameterPart("text", "textdata"),
				new ParameterPart("after", "afterdata"),
				new ParameterPart("never", "neverdata"),
				new ParameterPart("waylater", "waylaterdata")
			},
			new List<FilePart> {
				new FilePart( "file", "data.txt", TestUtil.StringToStreamNoBom("I am the first data")),
				new FilePart( "newfile", "superdata.txt", TestUtil.StringToStreamNoBom("I am the second data"))
			}
		);

		public MultipleParamsAndFiles()
		{
		}

		/// <summary>
		///     Checks that multiple files don't get in the way of parsing each other
		///     and that everything parses correctly.
		/// </summary>
		[Fact]
		public void MultipleFilesAndParamsTest()
		{
			using (Stream stream = TestUtil.StringToStream(_testCase.Request, Encoding.UTF8))
			{
				var parser = MultipartFormDataParser.Parse(stream, "boundary", Encoding.UTF8, 16);
				Assert.True(_testCase.Validate(parser));
			}
		}

		[Fact]
		public async Task MultipleFilesAndParamsTestAsync()
		{
			using (Stream stream = TestUtil.StringToStream(_testCase.Request, Encoding.UTF8))
			{
				var parser = await MultipartFormDataParser.ParseAsync(stream, "boundary", Encoding.UTF8, 16);
				Assert.True(_testCase.Validate(parser));
			}
		}
	}
}
