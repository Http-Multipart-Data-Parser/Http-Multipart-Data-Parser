// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HttpMultipartFormParserUnitTest.cs" company="Jake Woods">
//   Copyright (c) 2013 Jake Woods
//   
//   Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
//   and associated documentation files (the "Software"), to deal in the Software without restriction, 
//   including without limitation the rights to use, copy, modify, merge, publish, distribute, 
//   sublicense, and/or sell copies of the Software, and to permit persons to whom the Software 
//   is furnished to do so, subject to the following conditions:
//   
//   The above copyright notice and this permission notice shall be included in all copies 
//   or substantial portions of the Software.
//   
//   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
//   INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR 
//   PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR 
//   ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, 
//   ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
// <author>Jake Woods</author>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HttpMultipartParser.UnitTests
{
    /// <summary>
    ///     The http multipart form parser unit test.
    /// </summary>
    public class HttpMultipartFormParserUnitTests
    {
        [Fact]
        public void ConstructingWithNullStreamFails()
        {
            Assert.Throws<ArgumentNullException>(() => MultipartFormDataParser.Parse(Stream.Null));
        }

        [Fact]
        public async Task ConstructingWithNullStreamFailsAsync()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => MultipartFormDataParser.ParseAsync(Stream.Null)).ConfigureAwait(false);
        }

        /// <summary>
        ///     Tests for correct handling of a multiline parameter.
        /// </summary>
        [Fact]
        public void CorrectlyHandlesMultilineParameter()
        {
            string request = TestUtil.TrimAllLines(
                @"-----------------------------41952539122868
                Content-Disposition: form-data; name=""multilined""

                line 1
                line 2
                line 3
                -----------------------------41952539122868--");

            using (Stream stream = TestUtil.StringToStream(request, Encoding.UTF8))
            {
                var parser = MultipartFormDataParser.Parse(stream, Encoding.UTF8);
                Assert.Equal($"line 1{Environment.NewLine}line 2{Environment.NewLine}line 3", parser.GetParameterValue("multilined"));
                Assert.Equal($"line 1{Environment.NewLine}line 2{Environment.NewLine}line 3", parser.GetParameterValues("multilined").First());
            }
        }

        /// <summary>
        ///     Tests for correct handling of a multiline parameter.
        /// </summary>
        [Fact]
        public async Task CorrectlyHandlesMultilineParameterAsync()
        {
            string request = TestUtil.TrimAllLines(
                @"-----------------------------41952539122868
                Content-Disposition: form-data; name=""multilined""

                line 1
                line 2
                line 3
                -----------------------------41952539122868--");

            using (Stream stream = TestUtil.StringToStream(request, Encoding.UTF8))
            {
                var parser = await MultipartFormDataParser.ParseAsync(stream, Encoding.UTF8).ConfigureAwait(false);
                Assert.Equal($"line 1{Environment.NewLine}line 2{Environment.NewLine}line 3", parser.GetParameterValue("multilined"));
                Assert.Equal($"line 1{Environment.NewLine}line 2{Environment.NewLine}line 3", parser.GetParameterValues("multilined").First());
            }
        }

        [Fact]
        public void HandlesFileWithLastCrLfAtBufferLength()
        {
            string request =
@"------WebKitFormBoundaryphElSb1aBJGfLyAP
Content-Disposition: form-data; name=""fileName""

Testfile
------WebKitFormBoundaryphElSb1aBJGfLyAP
Content-Disposition: form-data; name=""file""; filename=""Testfile""
Content-Type: application/pdf

"
+ new string('\0', 8147)
+ @"
------WebKitFormBoundaryphElSb1aBJGfLyAP--
";

            using (Stream stream = TestUtil.StringToStream(request, Encoding.UTF8))
            {
                var parser = MultipartFormDataParser.Parse(stream, Encoding.UTF8);
            }
        }

        [Fact]
        public async Task HandlesFileWithLastCrLfAtBufferLengthAsync()
        {
            string request =
@"------WebKitFormBoundaryphElSb1aBJGfLyAP
Content-Disposition: form-data; name=""fileName""

Testfile
------WebKitFormBoundaryphElSb1aBJGfLyAP
Content-Disposition: form-data; name=""file""; filename=""Testfile""
Content-Type: application/pdf

"
+ new string('\0', 8147)
+ @"
------WebKitFormBoundaryphElSb1aBJGfLyAP--
";

            using (Stream stream = TestUtil.StringToStream(request, Encoding.UTF8))
            {
                var parser = await MultipartFormDataParser.ParseAsync(stream, Encoding.UTF8).ConfigureAwait(false);
            }
        }

        [Fact]
        public void HandlesFileWithLastCrLfImmediatlyAfterBufferLength()
        {
            string request =
@"------WebKitFormBoundaryphElSb1aBJGfLyAP
Content-Disposition: form-data; name=""fileName""

Testfile
------WebKitFormBoundaryphElSb1aBJGfLyAP
Content-Disposition: form-data; name=""file""; filename=""Testfile""
Content-Type: application/pdf

"
+ new string('\0', 8149)
+ @"
------WebKitFormBoundaryphElSb1aBJGfLyAP--
";

            using (Stream stream = TestUtil.StringToStream(request, Encoding.UTF8))
            {
                var parser = MultipartFormDataParser.Parse(stream, Encoding.UTF8);
            }
        }

        [Fact]
        public async Task HandlesFileWithLastCrLfImmediatlyAfterBufferLengthAsync()
        {
            string request =
@"------WebKitFormBoundaryphElSb1aBJGfLyAP
Content-Disposition: form-data; name=""fileName""

Testfile
------WebKitFormBoundaryphElSb1aBJGfLyAP
Content-Disposition: form-data; name=""file""; filename=""Testfile""
Content-Type: application/pdf

"
+ new string('\0', 8149)
+ @"
------WebKitFormBoundaryphElSb1aBJGfLyAP--
";

            using (Stream stream = TestUtil.StringToStream(request, Encoding.UTF8))
            {
                var parser = await MultipartFormDataParser.ParseAsync(stream, Encoding.UTF8).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task HandlesFileWithoutFilename()
        {
            string request =
                @"------WebKitFormBoundaryphElSb1aBJGfLyAP
Content-Disposition: form-data; name=""fileName""

Testfile
------WebKitFormBoundaryphElSb1aBJGfLyAP
Content-Disposition: form-data; name=""file""
Content-Type: application/octet-stream

"
                + new string('\0', 8147)
                + @"
------WebKitFormBoundaryphElSb1aBJGfLyAP--
";

            using (Stream stream = TestUtil.StringToStream(request, Encoding.UTF8))
            {
                var parser = await MultipartFormDataParser.ParseAsync(stream, Encoding.UTF8).ConfigureAwait(false);
                Assert.Single(parser.Files);
            }
        }
    }
}