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

            Assert.Equal(stack.Read(), 'a');
            Assert.Equal(stack.Read(), 'b');
            Assert.Equal(stack.Read(), 'c');
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

            Assert.Equal(stack.Read(), 'a');
            Assert.Equal(stack.Read(), 'b');
            Assert.Equal(stack.Read(), 'c');
            Assert.Equal(stack.Read(), 'd');
            Assert.Equal(stack.Read(), 'e');
            Assert.Equal(stack.Read(), 'f');
        }

        [Fact]
        public void CanReadSingleUnicodeCharacter()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("é"));

            Assert.Equal(stack.Read(), 'é');
        }

        [Fact]
        public void CanReadMultipleUnicodeCharacters()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("تست"));

            Assert.Equal(stack.Read(), 'ت');
            Assert.Equal(stack.Read(), 'س');
            Assert.Equal(stack.Read(), 'ت');
        }

        [Fact]
        public void CanReadMultipleUnicodeCharactersOverBuffers()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("ست"));
            stack.Push(TestUtil.StringToByteNoBom("ت"));

            Assert.Equal(stack.Read(), 'ت');
            Assert.Equal(stack.Read(), 'س');
            Assert.Equal(stack.Read(), 'ت');
        }

        [Fact]
        public void CanReadMixedUnicodeAndAsciiCharacters()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("تست.jpg"));

            Assert.Equal(stack.Read(), 'ت');
            Assert.Equal(stack.Read(), 'س');
            Assert.Equal(stack.Read(), 'ت');
            Assert.Equal(stack.Read(), '.');
            Assert.Equal(stack.Read(), 'j');
            Assert.Equal(stack.Read(), 'p');
            Assert.Equal(stack.Read(), 'g');
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
            Assert.Equal(result, "6chars");
        }

        [Fact]
        public void CanReadAcrossMultipleBuffers()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("ars"));
            stack.Push(TestUtil.StringToByteNoBom("6ch"));

            var buffer = new byte[6];
            stack.Read(buffer, 0, buffer.Length);
            Assert.Equal(Encoding.UTF8.GetString(buffer), "6chars");
        }

        [Fact]
        public void ReadCorrectlyHandlesSmallerBufferThenStream()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("6chars"));

            var buffer = new byte[4];
            stack.Read(buffer, 0, buffer.Length);
            Assert.Equal(Encoding.UTF8.GetString(buffer), "6cha");

            buffer = new byte[2];
            stack.Read(buffer, 0, buffer.Length);
            Assert.Equal(Encoding.UTF8.GetString(buffer), "rs");
        }

        [Fact]
        public void ReadCorrectlyHandlesLargerBufferThenStream()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("6chars"));

            var buffer = new byte[10];
            int amountRead = stack.Read(buffer, 0, buffer.Length);
            Assert.Equal(Encoding.UTF8.GetString(buffer), "6chars\0\0\0\0");
            Assert.Equal(amountRead, 6);
        }

        [Fact]
        public void ReadReturnsZeroOnNoData()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);

            var buffer = new byte[6];
            int amountRead = stack.Read(buffer, 0, buffer.Length);
            Assert.Equal(Encoding.UTF8.GetString(buffer), "\0\0\0\0\0\0");
            Assert.Equal(amountRead, 0);
        }

        [Fact]
        public void ReadCanResumeInterruptedStream()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("6chars"));

            var buffer = new byte[4];
            int amountRead = stack.Read(buffer, 0, buffer.Length);
            Assert.Equal(Encoding.UTF8.GetString(buffer), "6cha");
            Assert.Equal(amountRead, 4);

            stack.Push(TestUtil.StringToByteNoBom("14intermission"));
            buffer = new byte[14];
            amountRead = stack.Read(buffer, 0, buffer.Length);
            Assert.Equal(Encoding.UTF8.GetString(buffer), "14intermission");
            Assert.Equal(amountRead, 14);

            buffer = new byte[2];
            amountRead = stack.Read(buffer, 0, buffer.Length);
            Assert.Equal(Encoding.UTF8.GetString(buffer), "rs");
            Assert.Equal(amountRead, 2);
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
            Assert.Equal(result, "6chars");
        }

        [Fact]
        public void CanReadLineMultiplesLineInSingleBuffer()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("al" + Environment.NewLine));
            stack.Push(
                TestUtil.StringToByteNoBom("6chars" + Environment.NewLine + "5char" + Environment.NewLine + "Parti"));

            Assert.Equal(stack.ReadLine(), "6chars");
            Assert.Equal(stack.ReadLine(), "5char");
            Assert.Equal(stack.ReadLine(), "Partial");
        }

        [Fact]
        public void CanReadLineAcrossMultipleBuffers()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("13anotherline" + Environment.NewLine));
            stack.Push(TestUtil.StringToByteNoBom("ars" + Environment.NewLine));
            stack.Push(TestUtil.StringToByteNoBom("6ch"));

            string line = stack.ReadLine();
            Assert.Equal(line, "6chars");

            line = stack.ReadLine();
            Assert.Equal(line, "13anotherline");
        }

        [Fact]
        public void ReadLineCanResumeInterruptedStream()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("6chars" + Environment.NewLine + "Resume" + Environment.NewLine));

            Assert.Equal(stack.ReadLine(), "6chars");

            stack.Push(TestUtil.StringToByteNoBom("Interrupt" + Environment.NewLine));

            Assert.Equal(stack.ReadLine(), "Interrupt");
            Assert.Equal(stack.ReadLine(), "Resume");
        }

        [Fact]
        public void ReadLineCanReadAcrossInterruption()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("6chars" + Environment.NewLine + "Resume" + Environment.NewLine));

            Assert.Equal(stack.ReadLine(), "6chars");

            stack.Push(TestUtil.StringToByteNoBom("Interrupt "));

            Assert.Equal(stack.ReadLine(), "Interrupt Resume");
        }

        [Fact]
        public void ReturnsRemainderOnNoNewline()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("noline"));

            string noline = stack.ReadLine();
            Assert.Equal(noline, "noline");
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
            Assert.Equal(stack.ReadLine(), "--endboundary--");
        }

        #endregion

        #region Mixed Execution Tests

        [Fact]
        public void MixReadAndReadLineWithInterrupt()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("6chars" + Environment.NewLine + "Resume" + Environment.NewLine));

            Assert.Equal(stack.Read(), '6');

            Assert.Equal(stack.ReadLine(), "chars");

            stack.Push(TestUtil.StringToByteNoBom("Interrupt" + Environment.NewLine));

            Assert.Equal(stack.ReadLine(), "Interrupt");
            Assert.Equal(stack.Read(), 'R');
            Assert.Equal(stack.ReadLine(), "esume");
        }

        [Fact]
        public void MixReadAndReadBufferWithMultipleStreams()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("7inners"));
            stack.Push(TestUtil.StringToByteNoBom("6chars"));

            var buffer = new byte[2];

            Assert.Equal(stack.Read(), '6');

            int amountRead = stack.Read(buffer, 0, buffer.Length);
            Assert.Equal(Encoding.UTF8.GetString(buffer), "ch");
            Assert.Equal(amountRead, 2);

            Assert.Equal(stack.Read(), 'a');
            Assert.Equal(stack.Read(), 'r');

            amountRead = stack.Read(buffer, 0, buffer.Length);
            Assert.Equal(Encoding.UTF8.GetString(buffer), "s7");
            Assert.Equal(amountRead, 2);

            Assert.Equal(stack.Read(), 'i');
            Assert.Equal(stack.Read(), 'n');
            Assert.Equal(stack.Read(), 'n');
            Assert.Equal(stack.Read(), 'e');

            amountRead = stack.Read(buffer, 0, buffer.Length);
            Assert.Equal(Encoding.UTF8.GetString(buffer), "rs");
            Assert.Equal(amountRead, 2);
        }

        #endregion
    }
}