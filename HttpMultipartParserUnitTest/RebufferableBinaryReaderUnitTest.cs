using System;
using System.Text;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HttpMultipartParserUnitTest
{
    using System.IO;

    using HttpMultipartParser;
    using System.Threading.Tasks;

    /// <summary>
    /// Summary description for RebufferableBinaryReaderUnitTest
    /// </summary>
    [TestClass]
    public class RebufferableBinaryReaderUnitTest
    {
        public RebufferableBinaryReaderUnitTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        #region Read() Tests
        [TestMethod]
        public async Task CanReadSingleCharacterBuffer()
        {
            var stream = new InputStreamImpl(TestUtil.StringToStreamNoBom("abc"));
            var reader = new RebufferableBinaryReader(stream, Encoding.UTF8);

            Assert.AreEqual(await reader.ReadAsync(), 'a');
            Assert.AreEqual(await reader.ReadAsync(), 'b');
            Assert.AreEqual(await reader.ReadAsync(), 'c');
        }

        [TestMethod]
        public async Task CanReadSingleCharacterOverBuffers()
        {
            var stream = new InputStreamImpl(TestUtil.StringToStreamNoBom("def"));
            var reader = new RebufferableBinaryReader(stream, Encoding.UTF8);
            reader.Buffer(TestUtil.StringToByteNoBom("abc"));

            Assert.AreEqual(await reader.ReadAsync(), 'a');
            Assert.AreEqual(await reader.ReadAsync(), 'b');
            Assert.AreEqual(await reader.ReadAsync(), 'c');
            Assert.AreEqual(await reader.ReadAsync(), 'd');
            Assert.AreEqual(await reader.ReadAsync(), 'e');
            Assert.AreEqual(await reader.ReadAsync(), 'f');
        }

        [TestMethod]
        public async Task CanReadMixedAsciiAndUTFCharacters()
        {
            var stream = new InputStreamImpl(TestUtil.StringToStreamNoBom("abcdèfg"));
            var reader = new RebufferableBinaryReader(stream, Encoding.UTF8);

            Assert.AreEqual(await reader.ReadAsync(), 'a');
            Assert.AreEqual(await reader.ReadAsync(), 'b');
            Assert.AreEqual(await reader.ReadAsync(), 'c');
            Assert.AreEqual(await reader.ReadAsync(), 'd');
            Assert.AreEqual(await reader.ReadAsync(), 'è');
            Assert.AreEqual(await reader.ReadAsync(), 'f');
            Assert.AreEqual(await reader.ReadAsync(), 'g');
        }

        [TestMethod]
        public async Task CanReadMixedAsciiAndUTFCharactersOverBuffers()
        {
            var stream = new InputStreamImpl(TestUtil.StringToStreamNoBom("dèfg"));
            var reader = new RebufferableBinaryReader(stream, Encoding.UTF8);
            reader.Buffer(TestUtil.StringToByteNoBom("abc"));

            Assert.AreEqual(await reader.ReadAsync(), 'a');
            Assert.AreEqual(await reader.ReadAsync(), 'b');
            Assert.AreEqual(await reader.ReadAsync(), 'c');
            Assert.AreEqual(await reader.ReadAsync(), 'd');
            Assert.AreEqual(await reader.ReadAsync(), 'è');
            Assert.AreEqual(await reader.ReadAsync(), 'f');
            Assert.AreEqual(await reader.ReadAsync(), 'g');
        }
        #endregion

        #region Read(buffer, index, count) Tests
        [TestMethod]
        public async Task CanReadSingleBuffer()
        {
            var stream = new InputStreamImpl(TestUtil.StringToStreamNoBom("6chars"));
            var reader = new RebufferableBinaryReader(stream, Encoding.UTF8);

            var buffer = new byte[Encoding.UTF8.GetByteCount("6chars")];
            await reader.ReadAsync(buffer, 0, buffer.Length);
            var result = Encoding.UTF8.GetString(buffer);
            Assert.AreEqual(result, "6chars");
        }
        
        [TestMethod]
        public async Task CanReadAcrossMultipleBuffers()
        {
            var stream = new InputStreamImpl(TestUtil.StringToStreamNoBom("ars"));
            var reader = new RebufferableBinaryReader(stream, Encoding.UTF8);
            reader.Buffer(TestUtil.StringToByteNoBom("6ch"));

            var buffer = new byte[6];
            await reader.ReadAsync(buffer, 0, buffer.Length);
            Assert.AreEqual(Encoding.UTF8.GetString(buffer), "6chars");
        }

        [TestMethod]
        public async Task CanReadMixedAsciiAndUTF8()
        {
            var stream = new InputStreamImpl(TestUtil.StringToStreamNoBom("5èats"));
            var reader = new RebufferableBinaryReader(stream, Encoding.UTF8);

            var buffer = new byte[Encoding.UTF8.GetByteCount("5èats")];
            await reader.ReadAsync(buffer, 0, buffer.Length);
            var result = Encoding.UTF8.GetString(buffer);
            Assert.AreEqual(result, "5èats");
        }
        
        [TestMethod]
        public async Task CanReadMixedAsciiAndUTF8AcrossMultipleBuffers()
        {
            var stream = new InputStreamImpl(TestUtil.StringToStreamNoBom("ts"));
            var reader = new RebufferableBinaryReader(stream, Encoding.UTF8);
            reader.Buffer(TestUtil.StringToByteNoBom(("5èa")));

            var buffer = new byte[Encoding.UTF8.GetByteCount("5èats")];
            await reader.ReadAsync(buffer, 0, buffer.Length);
            var result = Encoding.UTF8.GetString(buffer);
            Assert.AreEqual(result, "5èats");
        }

        [TestMethod]
        public async Task ReadCorrectlyHandlesSmallerBufferThenStream()
        {
            var stream = new InputStreamImpl(TestUtil.StringToStreamNoBom("6chars"));
            var reader = new RebufferableBinaryReader(stream, Encoding.UTF8);

            var buffer = new byte[4];
            await reader.ReadAsync(buffer, 0, buffer.Length);
            Assert.AreEqual(Encoding.UTF8.GetString(buffer), "6cha");

            buffer = new byte[2];
            await reader.ReadAsync(buffer, 0, buffer.Length);
            Assert.AreEqual(Encoding.UTF8.GetString(buffer), "rs");
        }

        [TestMethod]
        public async Task ReadCorrectlyHandlesLargerBufferThenStream()
        {
            var stream = new InputStreamImpl(TestUtil.StringToStreamNoBom("6chars"));
            var reader = new RebufferableBinaryReader(stream, Encoding.UTF8);

            var buffer = new byte[10];
            int amountRead = await reader.ReadAsync(buffer, 0, buffer.Length);
            Assert.AreEqual(Encoding.UTF8.GetString(buffer), "6chars\0\0\0\0");
            Assert.AreEqual(amountRead, 6);
        }

        [TestMethod]
        public async Task ReadReturnsZeroOnNoData()
        {
            var stream = new InputStreamImpl(new MemoryStream());
            var reader = new RebufferableBinaryReader(stream, Encoding.UTF8);

            var buffer = new byte[6];
            int amountRead = await reader.ReadAsync(buffer, 0, buffer.Length);
            Assert.AreEqual(Encoding.UTF8.GetString(buffer), "\0\0\0\0\0\0");
            Assert.AreEqual(amountRead, 0);
        }

        [TestMethod]
        public async Task ReadCanResumeInterruptedStream()
        {
            var stream = new InputStreamImpl(TestUtil.StringToStreamNoBom("6chars"));
            var reader = new RebufferableBinaryReader(stream, Encoding.UTF8);

            var buffer = new byte[4];
            int amountRead = await reader.ReadAsync(buffer, 0, buffer.Length);
            Assert.AreEqual(Encoding.UTF8.GetString(buffer), "6cha");
            Assert.AreEqual(amountRead, 4);

            reader.Buffer(TestUtil.StringToByteNoBom("14intermission"));
            buffer = new byte[14];
            amountRead = await reader.ReadAsync(buffer, 0, buffer.Length);
            Assert.AreEqual(Encoding.UTF8.GetString(buffer), "14intermission");
            Assert.AreEqual(amountRead, 14);

            buffer = new byte[2];
            amountRead = await reader.ReadAsync(buffer, 0, buffer.Length);
            Assert.AreEqual(Encoding.UTF8.GetString(buffer), "rs");
            Assert.AreEqual(amountRead, 2);
        }
        #endregion

        #region ReadByteLine() Tests
        [TestMethod]
        public async Task CanReadByteLineOnMixedAsciiAndUTF8Text()
        {
            var stream = new InputStreamImpl(TestUtil.StringToStreamNoBom("Bonjour poignée"));
            var reader = new RebufferableBinaryReader(stream, Encoding.UTF8);
            var bytes = await reader.ReadByteLine();
            var expected = new byte[] {66, 111, 110, 106, 111, 117, 114, 32, 112, 111, 105, 103, 110, 195, 169, 101};

            foreach (var pair in expected.Zip(bytes, Tuple.Create))
            {
                Assert.AreEqual(pair.Item1, pair.Item2);
            }
        }
        #endregion
    }
}
