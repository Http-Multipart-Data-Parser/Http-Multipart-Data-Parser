using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HttpMultipartParser.UnitTests.ParserScenarios
{
	public class EmptyForm
	{
		private static readonly string _testData = TestUtil.TrimAllLines(
			@"------WebKitFormBoundaryb4SfPlH9Bv7c2PKS--"
		);

		private static readonly TestData _testCase = new TestData(
			_testData,
			Enumerable.Empty<ParameterPart>().ToList(),
			Enumerable.Empty<FilePart>().ToList()
		);

		public EmptyForm()
		{
			foreach (var filePart in _testCase.ExpectedFileData)
			{
				filePart.Data.Position = 0;
			}
		}

		[Fact]
		public void Parse_empty_form_boundary_specified()
		{
			var options = new ParserOptions
			{
				Boundary = "----WebKitFormBoundaryb4SfPlH9Bv7c2PKS"
			};

			using (Stream stream = TestUtil.StringToStream(_testCase.Request, options.Encoding))
			{
				var parser = MultipartFormDataParser.Parse(stream, options);
				Assert.True(_testCase.Validate(parser));
			}
		}

		[Fact]
		public void Parse_empty_form_boundary_omitted()
		{
			var options = new ParserOptions
			{
				Encoding = Encoding.UTF8,
			};

			using (Stream stream = TestUtil.StringToStream(_testCase.Request, options.Encoding))
			{
				var parser = MultipartFormDataParser.Parse(stream, options);
				Assert.True(_testCase.Validate(parser));
			}
		}

		[Fact]
		public async Task Parse_empty_form_boundary_specified_async()
		{
			var options = new ParserOptions
			{
				Boundary = "----WebKitFormBoundaryb4SfPlH9Bv7c2PKS"
			};

			using (Stream stream = TestUtil.StringToStream(_testCase.Request, options.Encoding))
			{
				var parser = await MultipartFormDataParser.ParseAsync(stream, options, TestContext.Current.CancellationToken);
				Assert.True(_testCase.Validate(parser));
			}
		}

		[Fact]
		public async Task Parse_empty_form_boundary_omitted_async()
		{
			var options = new ParserOptions
			{
				Encoding = Encoding.UTF8,
			};

			using (Stream stream = TestUtil.StringToStream(_testCase.Request, options.Encoding))
			{
				var parser = await MultipartFormDataParser.ParseAsync(stream, options, TestContext.Current.CancellationToken);
				Assert.True(_testCase.Validate(parser));
			}
		}
	}
}
