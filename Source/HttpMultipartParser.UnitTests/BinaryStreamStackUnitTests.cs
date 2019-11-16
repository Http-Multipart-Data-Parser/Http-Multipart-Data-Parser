// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BinaryStreamStackUnitTest.cs" company="Jake Woods">
//   Copyright (c) Jake Woods. All rights reserved.
// </copyright>
// <summary>
//   Unit Tests for <see cref="BinaryStreamStack" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Text;
using Xunit;

namespace HttpMultipartParser.UnitTests
{
    /// <summary>
    ///     Unit Tests for <see cref="BinaryStreamStack" />
    /// </summary>
    public class BinaryStreamStackUnitTests
    {
        #region Read() Tests

        /// <summary>
        ///     Tests that reading single characters work from a single
        ///     buffer.
        /// </summary>
        /// <seealso cref="BinaryStreamStack.Read()" />
        [Fact]
        public void CanReadSingleCharacterBuffer()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("abc"));

            Assert.Equal('a', stack.Read());
            Assert.Equal('b', stack.Read());
            Assert.Equal('c', stack.Read());
        }

        /// <summary>
        ///     Tests that reading single characters work across multiple
        ///     buffers.
        /// </summary>
        /// <seealso cref="BinaryStreamStack.Read()" />
        [Fact]
        public void CanReadSingleCharacterOverBuffers()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("def"));
            stack.Push(TestUtil.StringToByteNoBom("abc"));

            Assert.Equal('a', stack.Read());
            Assert.Equal('b', stack.Read());
            Assert.Equal('c', stack.Read());
            Assert.Equal('d', stack.Read());
            Assert.Equal('e', stack.Read());
            Assert.Equal('f', stack.Read());
        }

        [Fact]
        public void CanReadSingleUnicodeCharacter()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("é"));

            Assert.Equal('é', stack.Read());
        }

        [Fact]
        public void CanReadMultipleUnicodeCharacters()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("تست"));

            Assert.Equal('ت', stack.Read());
            Assert.Equal('س', stack.Read());
            Assert.Equal('ت', stack.Read());
        }

        [Fact]
        public void CanReadMultipleUnicodeCharactersOverBuffers()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("ست"));
            stack.Push(TestUtil.StringToByteNoBom("ت"));

            Assert.Equal('ت', stack.Read());
            Assert.Equal('س', stack.Read());
            Assert.Equal('ت', stack.Read());
        }

        [Fact]
        public void CanReadMixedUnicodeAndAsciiCharacters()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("تست.jpg"));

            Assert.Equal('ت', stack.Read());
            Assert.Equal('س', stack.Read());
            Assert.Equal('ت', stack.Read());
            Assert.Equal('.', stack.Read());
            Assert.Equal('j', stack.Read());
            Assert.Equal('p', stack.Read());
            Assert.Equal('g', stack.Read());
        }

        #endregion

        #region Read(buffer, index, count) Tests

        /// <summary>
        ///     Tests that reading multiple characters into a buffer works on
        ///     a single buffer.
        /// </summary>
        /// <seealso cref="BinaryStreamStack.Read(byte[], int, int)" />
        [Fact]
        public void CanReadSingleBuffer()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("6chars"));

            var buffer = new byte[Encoding.UTF8.GetByteCount("6chars")];
            stack.Read(buffer, 0, buffer.Length);
            string result = Encoding.UTF8.GetString(buffer);
            Assert.Equal("6chars", result);
        }

        [Fact]
        public void CanReadAcrossMultipleBuffers()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("ars"));
            stack.Push(TestUtil.StringToByteNoBom("6ch"));

            var buffer = new byte[6];
            stack.Read(buffer, 0, buffer.Length);
            Assert.Equal("6chars", Encoding.UTF8.GetString(buffer));
        }

        [Fact]
        public void ReadCorrectlyHandlesSmallerBufferThenStream()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("6chars"));

            var buffer = new byte[4];
            stack.Read(buffer, 0, buffer.Length);
            Assert.Equal("6cha", Encoding.UTF8.GetString(buffer));

            buffer = new byte[2];
            stack.Read(buffer, 0, buffer.Length);
            Assert.Equal("rs", Encoding.UTF8.GetString(buffer));
        }

        [Fact]
        public void ReadCorrectlyHandlesLargerBufferThenStream()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("6chars"));

            var buffer = new byte[10];
            int amountRead = stack.Read(buffer, 0, buffer.Length);
            Assert.Equal("6chars\0\0\0\0", Encoding.UTF8.GetString(buffer));
            Assert.Equal(6, amountRead);
        }

        [Fact]
        public void ReadReturnsZeroOnNoData()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);

            var buffer = new byte[6];
            int amountRead = stack.Read(buffer, 0, buffer.Length);
            Assert.Equal("\0\0\0\0\0\0", Encoding.UTF8.GetString(buffer));
            Assert.Equal(0, amountRead);
        }

        [Fact]
        public void ReadCanResumeInterruptedStream()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("6chars"));

            var buffer = new byte[4];
            int amountRead = stack.Read(buffer, 0, buffer.Length);
            Assert.Equal("6cha", Encoding.UTF8.GetString(buffer));
            Assert.Equal(4, amountRead);

            stack.Push(TestUtil.StringToByteNoBom("14intermission"));
            buffer = new byte[14];
            amountRead = stack.Read(buffer, 0, buffer.Length);
            Assert.Equal("14intermission", Encoding.UTF8.GetString(buffer));
            Assert.Equal(14, amountRead);

            buffer = new byte[2];
            amountRead = stack.Read(buffer, 0, buffer.Length);
            Assert.Equal("rs", Encoding.UTF8.GetString(buffer));
            Assert.Equal(2, amountRead);
        }

        #endregion

        #region ReadLine() Tests

        [Fact]
        public void CanReadLineSingleBuffer()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("6chars" + Environment.NewLine));

            var buffer = new byte[Encoding.UTF8.GetByteCount("6chars" + Environment.NewLine)];
            string result = stack.ReadLine();
            Assert.Equal("6chars", result);
        }

        [Fact]
        public void CanReadLineMultiplesLineInSingleBuffer()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("al" + Environment.NewLine));
            stack.Push(
                TestUtil.StringToByteNoBom("6chars" + Environment.NewLine + "5char" + Environment.NewLine + "Parti"));

            Assert.Equal("6chars", stack.ReadLine());
            Assert.Equal("5char", stack.ReadLine());
            Assert.Equal("Partial", stack.ReadLine());
        }

        [Fact]
        public void CanReadLineAcrossMultipleBuffers()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("13anotherline" + Environment.NewLine));
            stack.Push(TestUtil.StringToByteNoBom("ars" + Environment.NewLine));
            stack.Push(TestUtil.StringToByteNoBom("6ch"));

            string line = stack.ReadLine();
            Assert.Equal("6chars", line);

            line = stack.ReadLine();
            Assert.Equal("13anotherline", line);
        }

        [Fact]
        public void ReadLineCanResumeInterruptedStream()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("6chars" + Environment.NewLine + "Resume" + Environment.NewLine));

            Assert.Equal("6chars", stack.ReadLine());

            stack.Push(TestUtil.StringToByteNoBom("Interrupt" + Environment.NewLine));

            Assert.Equal("Interrupt", stack.ReadLine());
            Assert.Equal("Resume", stack.ReadLine());
        }

        [Fact]
        public void ReadLineCanReadAcrossInterruption()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("6chars" + Environment.NewLine + "Resume" + Environment.NewLine));

            Assert.Equal("6chars", stack.ReadLine());

            stack.Push(TestUtil.StringToByteNoBom("Interrupt "));

            Assert.Equal("Interrupt Resume", stack.ReadLine());
        }

        [Fact]
        public void ReturnsRemainderOnNoNewline()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("noline"));

            string noline = stack.ReadLine();
            Assert.Equal("noline", noline);
        }

        [Fact]
        public void ReturnsNullOnNoStreams()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);

            string noline = stack.ReadLine();
            Assert.Null(noline);
        }

        [Fact]
        public void CanReadLineNearEnd()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("\r\n--endboundary--"));

            Assert.Equal(stack.ReadLine(), string.Empty);
            Assert.Equal("--endboundary--", stack.ReadLine());
        }

        #endregion

        #region Mixed Execution Tests

        [Fact]
        public void MixReadAndReadLineWithInterrupt()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("6chars" + Environment.NewLine + "Resume" + Environment.NewLine));

            Assert.Equal('6', stack.Read());

            Assert.Equal("chars", stack.ReadLine());

            stack.Push(TestUtil.StringToByteNoBom("Interrupt" + Environment.NewLine));

            Assert.Equal("Interrupt", stack.ReadLine());
            Assert.Equal('R', stack.Read());
            Assert.Equal("esume", stack.ReadLine());
        }

        [Fact]
        public void MixReadAndReadBufferWithMultipleStreams()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("7inners"));
            stack.Push(TestUtil.StringToByteNoBom("6chars"));

            var buffer = new byte[2];

            Assert.Equal('6', stack.Read());

            int amountRead = stack.Read(buffer, 0, buffer.Length);
            Assert.Equal("ch", Encoding.UTF8.GetString(buffer));
            Assert.Equal(2, amountRead);

            Assert.Equal('a', stack.Read());
            Assert.Equal('r', stack.Read());

            amountRead = stack.Read(buffer, 0, buffer.Length);
            Assert.Equal("s7", Encoding.UTF8.GetString(buffer));
            Assert.Equal(2, amountRead);

            Assert.Equal('i', stack.Read());
            Assert.Equal('n', stack.Read());
            Assert.Equal('n', stack.Read());
            Assert.Equal('e', stack.Read());

            amountRead = stack.Read(buffer, 0, buffer.Length);
            Assert.Equal("rs", Encoding.UTF8.GetString(buffer));
            Assert.Equal(2, amountRead);
        }

        #endregion
    }
}