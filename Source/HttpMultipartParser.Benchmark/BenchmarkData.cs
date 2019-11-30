using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HttpMultipartParser.Benchmark
{
    public class BenchmarkData
    {
        private readonly IEnumerable<(string Name, string Data)> _parameters;
        private readonly IEnumerable<(string Name, string FileName, string Data)> _files;

        public BenchmarkData(int numberOfParameters, int sizeOfParameter, int numberOfFiles, int sizeOfFile)
        {
            _parameters = Enumerable
                .Range(1, numberOfParameters)
                .Select(index => ($"parameter{index}", new string(Convert.ToChar(index), sizeOfParameter)))
                .ToArray();

            _files = Enumerable
                .Range(1, numberOfFiles)
                .Select(index => ($"file{index}", $"fileName{index}", new string(Convert.ToChar(index), sizeOfFile)))
                .ToArray();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var (Name, Data) in _parameters)
            {
                sb.Append("--boundary\n");
                sb.Append($"Content-Disposition: form-data; name=\"{Name}\"\n");
                sb.Append("\n");
                sb.Append($"{Data}\n");
            }
            foreach (var (Name, FileName, Data) in _files)
            {
                sb.Append("--boundary\n");
                sb.Append($"Content-Disposition: form-data; name=\"{Name}\"; filename=\"{FileName}\"\n");
                sb.Append("Content-Type: text/plain\n");
                sb.Append("\n");
                sb.Append($"{Data}\n");
            }
            sb.Append("--boundary--");

            return sb.ToString();
        }

        public Stream ToStream()
        {
            var content = this.ToString();
            var buffer = Encoding.UTF8.GetBytes(content);
            return new MemoryStream(buffer);
        }
    }
}
