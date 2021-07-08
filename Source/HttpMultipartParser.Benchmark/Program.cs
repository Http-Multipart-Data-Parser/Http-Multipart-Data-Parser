using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace HttpMultipartParser.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            IConfig config = null;

            // To debug
            // config = new DebugInProcessConfig();

            var types = new[]
            {
                typeof(SubsequenceFinderBenchmark),
                typeof(MultipartFormDataParserBenchmark)
            };

            BenchmarkSwitcher.FromTypes(types).Run(args, config);
        }
    }
}
