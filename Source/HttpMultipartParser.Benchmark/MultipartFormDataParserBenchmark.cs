using BenchmarkDotNet.Attributes;
using System.IO;
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
			small.Position = 0;
			return await MultipartFormDataParser.ParseAsync(small, "boundary").ConfigureAwait(false);
		}

		[Benchmark]
		public async Task<MultipartFormDataParser> Medium()
		{
			medium.Position = 0;
			return await MultipartFormDataParser.ParseAsync(medium, "boundary").ConfigureAwait(false);
		}

		[Benchmark]
		public async Task<MultipartFormDataParser> Large()
		{
			large.Position = 0;
			return await MultipartFormDataParser.ParseAsync(large, "boundary").ConfigureAwait(false);
		}
	}
}
