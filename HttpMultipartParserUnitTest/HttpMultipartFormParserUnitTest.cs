using System.IO;
using System.Text;

using HttpMultipartParser;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HttpMultipartParserUnitTest
{
    [TestClass]
    public class HttpMultipartFormParserUnitTest
    {
        [TestMethod]
        public void CorrectlyHandlesCRLF()
        {
            var request = TestUtil.TrimAllLines(@"--boundry
                  Content-Disposition: form-data; name=""text""
                  
                  textdata
                  --boundry
                  Content-Disposition: form-data; name=""file""; filename=""data.txt""
                  Content-Type: text/plain

                  tiny
                  --boundry
                  Content-Disposition: form-data; name=""after""

                  afterdata
                  --boundry--").Replace("\n", "\r\n");

                using (var stream = TestUtil.StringToStream(request, Encoding.UTF8))
                {
                    var parser = new MultipartFormDataParser("boundry", stream, Encoding.UTF8);

                    Assert.AreEqual(parser.Parameters["text"].Data, "textdata");
                    Assert.AreEqual(parser.Parameters["after"].Data, "afterdata");

                    var fileData = parser.Files["file"];
                    Assert.AreEqual(fileData.Name, "file");
                    Assert.AreEqual(fileData.FileName, "data.txt");

                    var reader = new StreamReader(fileData.Data);
                    string data = reader.ReadToEnd();
                    Assert.AreEqual(data, "tiny");
                }
        }

        [TestMethod]
        public void TinyDataTest()
        {
            var request = TestUtil.TrimAllLines(
                @"--boundry
                  Content-Disposition: form-data; name=""text""
                  
                  textdata
                  --boundry
                  Content-Disposition: form-data; name=""file""; filename=""data.txt""
                  Content-Type: text/plain

                  tiny
                  --boundry
                  Content-Disposition: form-data; name=""after""

                  afterdata
                  --boundry--");

                using (var stream = TestUtil.StringToStream(request, Encoding.UTF8))
                {
                    var parser = new MultipartFormDataParser("boundry", stream, Encoding.UTF8);

                    Assert.AreEqual(parser.Parameters["text"].Data, "textdata");
                    Assert.AreEqual(parser.Parameters["after"].Data, "afterdata");

                    var fileData = parser.Files["file"];
                    Assert.AreEqual(fileData.Name, "file");
                    Assert.AreEqual(fileData.FileName, "data.txt");

                    var reader = new StreamReader(fileData.Data);
                    string data = reader.ReadToEnd();
                    Assert.AreEqual(data, "tiny");
                }
        }

        [TestMethod]
        public void SmallDataTest()
        {
            var request = TestUtil.TrimAllLines(
                @"-----------------------------265001916915724
                Content-Disposition: form-data; name=""textdata""
                
                Testdata
                -----------------------------265001916915724
                Content-Disposition: form-data; name=""file""; filename=""data.txt""
                Content-Type: text/plain

                This is a small file
                -----------------------------265001916915724
                Content-Disposition: form-data; name=""submit""

                Submit
                -----------------------------265001916915724--");

            using (var stream = TestUtil.StringToStream(request))
            {
                // The boundry is missing the first two -- in accordance with the multipart
                // spec. (A -- is added by the parser, this boundry is what would be sent in the
                // requset header)
                var parser = new MultipartFormDataParser(
                    "---------------------------265001916915724",stream);

                // Make sure the small parameters are parsed correctly
                Assert.AreEqual(parser.Parameters["textdata"].Data, "Testdata");

                // Make sure we can read the small stream
                var fileData = parser.Files["file"];
                Assert.AreEqual(fileData.Name, "file");
                Assert.AreEqual(fileData.FileName, "data.txt");

                // Try and read the stream into a string, should be fine for small files
                var reader = new StreamReader(fileData.Data);
                string data = reader.ReadToEnd();
                Assert.AreEqual(data, "This is a small file");
            }
        }

        [TestMethod]
        public void CanDetectBoundriesCrossBuffer()
        {
            var request = TestUtil.TrimAllLines(
                @"--boundry
                  Content-Disposition: form-data; name=""text""
                  
                  textdata
                  --boundry
                  Content-Disposition: form-data; name=""file""; filename=""data.txt""
                  Content-Type: text/plain

                  tiny
                  --boundry
                  Content-Disposition: form-data; name=""after""

                  afterdata
                  --boundry--");

            using (var stream = TestUtil.StringToStream(request, Encoding.UTF8))
            {
                var parser = new MultipartFormDataParser("boundry", stream, Encoding.UTF8, 16);

                Assert.AreEqual(parser.Parameters["text"].Data, "textdata");
                Assert.AreEqual(parser.Parameters["after"].Data, "afterdata");

                var fileData = parser.Files["file"];
                Assert.AreEqual(fileData.Name, "file");
                Assert.AreEqual(fileData.FileName, "data.txt");

                var reader = new StreamReader(fileData.Data);
                string data = reader.ReadToEnd();
                Assert.AreEqual(data, "tiny");
            }
        }

        [TestMethod]
        public void MultipleFilesAndParamsTest()
        {
            var request = TestUtil.TrimAllLines(
                @"--boundry
                  Content-Disposition: form-data; name=""text""
                  
                  textdata
                  --boundry
                  Content-Disposition: form-data; name=""after""
                  
                  afterdata
                  --boundry
                  Content-Disposition: form-data; name=""file""; filename=""data.txt""
                  Content-Type: text/plain

                  I am the first data 
                  --boundry
                  Content-Disposition: form-data; name=""newfile""; filename=""superdata.txt""
                  Content-Type: text/plain

                  I am the second data
                  --boundry
                  Content-Disposition: form-data; name=""never""

                  neverdata 
                  --boundry
                  Content-Disposition: form-data; name=""waylater""

                  waylaterdata 
                  --boundry--");

            using (var stream = TestUtil.StringToStream(request, Encoding.UTF8))
            {
                var parser = new MultipartFormDataParser("boundry", stream, Encoding.UTF8, 16);

                var text = parser.Parameters["text"];
                Assert.AreEqual(text.Name, "text");
                Assert.AreEqual(text.Data, "textdata");

                var after = parser.Parameters["after"];
                Assert.AreEqual(after.Name, "after");
                Assert.AreEqual(after.Data, "afterdata");

                var never = parser.Parameters["never"];
                Assert.AreEqual(never.Name, "never");
                Assert.AreEqual(never.Data, "neverdata");

                var waylater = parser.Parameters["waylater"];
                Assert.AreEqual(waylater.Name, "waylater");
                Assert.AreEqual(waylater.Data, "waylaterdata");

                var fileData = parser.Files["file"];
                Assert.AreEqual(fileData.Name, "file");
                Assert.AreEqual(fileData.FileName, "data.txt");

                var reader = new StreamReader(fileData.Data);
                string data = reader.ReadToEnd();
                Assert.AreEqual(data, "I am the first data");

                var newFileData = parser.Files["newfile"];
                Assert.AreEqual(newFileData.Name, "newfile");
                Assert.AreEqual(newFileData.FileName, "superdata.txt");

                reader = new StreamReader(newFileData.Data);
                data = reader.ReadToEnd();
                Assert.AreEqual(data, "I am the second data");
            }
        }
    }
}
