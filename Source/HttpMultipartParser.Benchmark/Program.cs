using BenchmarkDotNet.Running;

namespace HttpMultipartParse.Benchmark
{
    class Program
    {
        static void Main()
        {
            BenchmarkRunner.Run(typeof(Program).Assembly);

            // To debug:
            // BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, new DebugInProcessConfig());
        }
    }
}
