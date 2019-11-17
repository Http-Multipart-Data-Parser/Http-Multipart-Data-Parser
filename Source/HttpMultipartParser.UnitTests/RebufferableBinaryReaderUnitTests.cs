using System;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace HttpMultipartParser.UnitTests
{
    /// <summary>
    ///     Summary description for RebufferableBinaryReaderUnitTest
    /// </summary>
    public class RebufferableBinaryReaderUnitTests
    {
        #region Read() Tests

        [Fact]
        public void CanReadSingleCharacterBuffer()
        {
            var reader = new RebufferableBinaryReader(TestUtil.StringToStreamNoBom("abc"), Encoding.UTF8);

            Assert.Equal('a', reader.Read());
            Assert.Equal('b', reader.Read());
            Assert.Equal('c', reader.Read());
        }

        [Fact]
        public void CanReadSingleCharacterOverBuffers()
        {
            var reader = new RebufferableBinaryReader(TestUtil.StringToStreamNoBom("def"), Encoding.UTF8);
            reader.Buffer(TestUtil.StringToByteNoBom("abc"));

            Assert.Equal('a', reader.Read());
            Assert.Equal('b', reader.Read());
            Assert.Equal('c', reader.Read());
            Assert.Equal('d', reader.Read());
            Assert.Equal('e', reader.Read());
            Assert.Equal('f', reader.Read());
        }

        [Fact]
        public void CanReadMixedAsciiAndUTFCharacters()
        {
            var reader = new RebufferableBinaryReader(TestUtil.StringToStreamNoBom("abcdèfg"), Encoding.UTF8);

            Assert.Equal('a', reader.Read());
            Assert.Equal('b', reader.Read());
            Assert.Equal('c', reader.Read());
            Assert.Equal('d', reader.Read());
            Assert.Equal('è', reader.Read());
            Assert.Equal('f', reader.Read());
            Assert.Equal('g', reader.Read());
        }

        [Fact]
        public void CanReadMixedAsciiAndUTFCharactersOverBuffers()
        {
            var reader = new RebufferableBinaryReader(TestUtil.StringToStreamNoBom("dèfg"), Encoding.UTF8);
            reader.Buffer(TestUtil.StringToByteNoBom("abc"));

            Assert.Equal('a', reader.Read());
            Assert.Equal('b', reader.Read());
            Assert.Equal('c', reader.Read());
            Assert.Equal('d', reader.Read());
            Assert.Equal('è', reader.Read());
            Assert.Equal('f', reader.Read());
            Assert.Equal('g', reader.Read());
        }

        #endregion

        #region Read(buffer, index, count) Tests

        [Fact]
        public void CanReadSingleBuffer()
        {
            var reader = new RebufferableBinaryReader(TestUtil.StringToStreamNoBom("6chars"), Encoding.UTF8);

            var buffer = new byte[Encoding.UTF8.GetByteCount("6chars")];
            reader.Read(buffer, 0, buffer.Length);
            string result = Encoding.UTF8.GetString(buffer);
            Assert.Equal("6chars", result);
        }

        [Fact]
        public void CanReadAcrossMultipleBuffers()
        {
            var reader = new RebufferableBinaryReader(TestUtil.StringToStreamNoBom("ars"), Encoding.UTF8);
            reader.Buffer(TestUtil.StringToByteNoBom("6ch"));

            var buffer = new byte[6];
            reader.Read(buffer, 0, buffer.Length);
            Assert.Equal("6chars", Encoding.UTF8.GetString(buffer));
        }

        [Fact]
        public void CanReadMixedAsciiAndUTF8()
        {
            var reader = new RebufferableBinaryReader(TestUtil.StringToStreamNoBom("5èats"), Encoding.UTF8);

            var buffer = new byte[Encoding.UTF8.GetByteCount("5èats")];
            reader.Read(buffer, 0, buffer.Length);
            string result = Encoding.UTF8.GetString(buffer);
            Assert.Equal("5èats", result);
        }

        [Fact]
        public void CanReadMixedAsciiAndUTF8AcrossMultipleBuffers()
        {
            var reader = new RebufferableBinaryReader(TestUtil.StringToStreamNoBom("ts"), Encoding.UTF8);
            reader.Buffer(TestUtil.StringToByteNoBom(("5èa")));

            var buffer = new byte[Encoding.UTF8.GetByteCount("5èats")];
            reader.Read(buffer, 0, buffer.Length);
            string result = Encoding.UTF8.GetString(buffer);
            Assert.Equal("5èats", result);
        }

        [Fact]
        public void ReadCorrectlyHandlesSmallerBufferThenStream()
        {
            var reader = new RebufferableBinaryReader(TestUtil.StringToStreamNoBom("6chars"), Encoding.UTF8);

            var buffer = new byte[4];
            reader.Read(buffer, 0, buffer.Length);
            Assert.Equal("6cha", Encoding.UTF8.GetString(buffer));

            buffer = new byte[2];
            reader.Read(buffer, 0, buffer.Length);
            Assert.Equal("rs", Encoding.UTF8.GetString(buffer));
        }

        [Fact]
        public void ReadCorrectlyHandlesLargerBufferThenStream()
        {
            var reader = new RebufferableBinaryReader(TestUtil.StringToStreamNoBom("6chars"), Encoding.UTF8);

            var buffer = new byte[10];
            int amountRead = reader.Read(buffer, 0, buffer.Length);
            Assert.Equal("6chars\0\0\0\0", Encoding.UTF8.GetString(buffer));
            Assert.Equal(6, amountRead);
        }

        [Fact]
        public void ReadReturnsZeroOnNoData()
        {
            var reader = new RebufferableBinaryReader(new MemoryStream(), Encoding.UTF8);

            var buffer = new byte[6];
            int amountRead = reader.Read(buffer, 0, buffer.Length);
            Assert.Equal("\0\0\0\0\0\0", Encoding.UTF8.GetString(buffer));
            Assert.Equal(0, amountRead);
        }

        [Fact]
        public void ReadLineReturnsNullOnNoData()
        {
            var reader = new RebufferableBinaryReader(new MemoryStream(new byte[6]), Encoding.UTF8);

            var s = reader.ReadLine();
            Assert.Equal("\0\0\0\0\0\0", s);
            Assert.Null(reader.ReadLine());
        }

        [Fact]
        public void ReadCanResumeInterruptedStream()
        {
            var reader = new RebufferableBinaryReader(TestUtil.StringToStreamNoBom("6chars"), Encoding.UTF8);

            var buffer = new byte[4];
            int amountRead = reader.Read(buffer, 0, buffer.Length);
            Assert.Equal("6cha", Encoding.UTF8.GetString(buffer));
            Assert.Equal(4, amountRead);

            reader.Buffer(TestUtil.StringToByteNoBom("14intermission"));
            buffer = new byte[14];
            amountRead = reader.Read(buffer, 0, buffer.Length);
            Assert.Equal("14intermission", Encoding.UTF8.GetString(buffer));
            Assert.Equal(14, amountRead);

            buffer = new byte[2];
            amountRead = reader.Read(buffer, 0, buffer.Length);
            Assert.Equal("rs", Encoding.UTF8.GetString(buffer));
            Assert.Equal(2, amountRead);
        }

        #endregion

        #region ReadByteLine() Tests

        [Fact]
        public void CanReadByteLineOnMixedAsciiAndUTF8Text()
        {
            var reader = new RebufferableBinaryReader(TestUtil.StringToStreamNoBom("Bonjour poignée"), Encoding.UTF8);
            byte[] bytes = reader.ReadByteLine();
            var expected = new byte[] { 66, 111, 110, 106, 111, 117, 114, 32, 112, 111, 105, 103, 110, 195, 169, 101 };

            foreach (var pair in expected.Zip(bytes, Tuple.Create))
            {
                Assert.Equal(pair.Item1, pair.Item2);
            }
        }

        #endregion
    }
}