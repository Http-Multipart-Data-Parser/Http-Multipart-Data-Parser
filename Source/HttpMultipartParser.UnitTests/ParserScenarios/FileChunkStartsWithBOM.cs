using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace HttpMultipartParser.UnitTests.ParserScenarios
{
	// This test attempts to reproduce the bug discussed here:
	// https://github.com/Http-Multipart-Data-Parser/Http-Multipart-Data-Parser/issues/64
	public class FileChunkStartsWithBOM
	{
		private static readonly int _binaryBufferSize = 100;
		private static readonly string _prefixChunk = new string('a', _binaryBufferSize * 2);
		private static readonly byte[] _utf8BOMBinary = new byte[] { 0xef, 0xbb, 0xbf };
		private static readonly string _utf8BOMString = Encoding.UTF8.GetString(_utf8BOMBinary);

		// The reasoning behind this test is that the parser should be able to handle a file chunk that starts with a BOM.
		private static readonly string _fileContent = $"{_prefixChunk}{_utf8BOMString}Hello world";

		private static readonly string _testDataFormat =
			@"-----------------------------41952539122868
            Content-Type: application/octet-stream

            {0}
            -----------------------------41952539122868--";
		private static readonly string _testData = TestUtil.TrimAllLines(string.Format(_testDataFormat, _fileContent));

		/// <summary>
		///     Test case for files with additional parameter.
		/// </summary>
		private static readonly TestData _testCase = new TestData(
			_testData,
			new List<ParameterPart> { },
			new List<FilePart> {
				new FilePart(null, null, TestUtil.StringToStreamNoBom(_fileContent), null, "application/octet-stream", "form-data")
			}
		);

		/// <summary>
		///     Initializes the test data before each run, this primarily
		///     consists of resetting data stream positions.
		/// </summary>
		public FileChunkStartsWithBOM()
		{
			foreach (var filePart in _testCase.ExpectedFileData)
			{
				filePart.Data.Position = 0;
			}
		}

		[Fact]
		public void CanHandleBOM()
		{
			using (Stream stream = TestUtil.StringToStream(_testCase.Request, Encoding.UTF8))
			{
				var parser = MultipartFormDataParser.Parse(stream, Encoding.UTF8, _binaryBufferSize);

				using (var reader = new BinaryReader(parser.Files[0].Data))
				{
					// Read and discard the prefix
					var buffer = new byte[_prefixChunk.Length];
					reader.Read(buffer, 0, buffer.Length);

					// Read the BOM
					buffer = new byte[_utf8BOMBinary.Length];
					var bomChunk = reader.Read(buffer, 0, buffer.Length);

					// Assert that the BOM was read correctly
					for (int i = 0; i < buffer.Length; i++)
					{
						Assert.Equal(_utf8BOMBinary[i], buffer[i]);
					}
				}
			}
		}
	}
}
