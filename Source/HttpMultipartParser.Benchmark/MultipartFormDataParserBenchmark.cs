using BenchmarkDotNet.Attributes;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace HttpMultipartParser.Benchmark
{
	[MemoryDiagnoser]
	[HtmlExporter]
	[JsonExporter]
	[MarkdownExporter]
	public class MultipartFormDataParserBenchmark
	{
		private readonly Stream small;
		private readonly Stream medium;
		private readonly Stream large;

		public MultipartFormDataParserBenchmark()
		{
			small = new BenchmarkData(5, 10, 1, 125000).ToStream();
			medium = new BenchmarkData(25, 50, 5, 250000).ToStream();
			large = new BenchmarkData(100, 500, 50, 500000).ToStream();
		}

		[Benchmark]
		public async Task<MultipartFormDataParser> Small()
		{
			var options = new ParserOptions
			{
				Boundary = "boundary"
			};

			small.Position = 0;
			return await MultipartFormDataParser.ParseAsync(small, options, CancellationToken.None).ConfigureAwait(false);
		}

		[Benchmark]
		public async Task<MultipartFormDataParser> Medium()
		{
			var options = new ParserOptions
			{
				Boundary = "boundary"
			};

			medium.Position = 0;
			return await MultipartFormDataParser.ParseAsync(medium, options, CancellationToken.None).ConfigureAwait(false);
		}

		[Benchmark]
		public async Task<MultipartFormDataParser> Large()
		{
			var options = new ParserOptions
			{
				Boundary = "boundary"
			};

			large.Position = 0;
			return await MultipartFormDataParser.ParseAsync(large, options, CancellationToken.None).ConfigureAwait(false);
		}
	}
}
