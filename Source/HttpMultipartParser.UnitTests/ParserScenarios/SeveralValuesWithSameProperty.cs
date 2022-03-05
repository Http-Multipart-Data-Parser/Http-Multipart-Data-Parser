using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HttpMultipartParser.UnitTests.ParserScenarios
{
	public class SeveralValuesWithSameProperty
	{
		private static readonly string _testData = TestUtil.TrimAllLines(
			@"------B6u9lJxB4ByPiGPZ
            Content-Disposition: form-data; name=""options""

            value0
            ------B6u9lJxB4ByPiGPZ
            Content-Disposition: form-data; name=""options""

            value1
            ------B6u9lJxB4ByPiGPZ
            Content-Disposition: form-data; name=""options""

            value2
            ------B6u9lJxB4ByPiGPZ--"
		);

		private static readonly TestData _testCase = new TestData(
			_testData,
			new List<ParameterPart> {
				new ParameterPart("options", "value0"),
				new ParameterPart("options", "value1"),
				new ParameterPart("options", "value2")
			},
			new List<FilePart>()
		);

		public SeveralValuesWithSameProperty()
		{
			foreach (var filePart in _testCase.ExpectedFileData)
			{
				filePart.Data.Position = 0;
			}
		}

		[Fact]
		public void AcceptSeveralValuesWithSameProperty()
		{
			using (Stream stream = TestUtil.StringToStream(_testCase.Request, Encoding.UTF8))
			{
				var parser = MultipartFormDataParser.Parse(stream, Encoding.UTF8);
				Assert.True(_testCase.Validate(parser));
			}
		}

		[Fact]
		public async Task AcceptSeveralValuesWithSamePropertyAsync()
		{
			using (Stream stream = TestUtil.StringToStream(_testCase.Request, Encoding.UTF8))
			{
				var parser = await MultipartFormDataParser.ParseAsync(stream, Encoding.UTF8).ConfigureAwait(false);
				Assert.True(_testCase.Validate(parser));
			}
		}
	}
}
