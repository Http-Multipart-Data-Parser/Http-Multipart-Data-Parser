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

            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
        }
    }
}
