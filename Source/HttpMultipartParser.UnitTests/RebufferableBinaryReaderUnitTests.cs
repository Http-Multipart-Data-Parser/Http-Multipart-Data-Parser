using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

		#region ReadAsync() Tests

		[Fact]
		public async Task CanReadSingleCharacterBufferAsync()
		{
			var reader = new RebufferableBinaryReader(TestUtil.StringToStreamNoBom("abc"), Encoding.UTF8);

			Assert.Equal('a', await reader.ReadAsync());
			Assert.Equal('b', await reader.ReadAsync());
			Assert.Equal('c', await reader.ReadAsync());
		}

		[Fact]
		public async Task CanReadSingleCharacterOverBuffersAsync()
		{
			var reader = new RebufferableBinaryReader(TestUtil.StringToStreamNoBom("def"), Encoding.UTF8);
			reader.Buffer(TestUtil.StringToByteNoBom("abc"));

			Assert.Equal('a', await reader.ReadAsync());
			Assert.Equal('b', await reader.ReadAsync());
			Assert.Equal('c', await reader.ReadAsync());
			Assert.Equal('d', await reader.ReadAsync());
			Assert.Equal('e', await reader.ReadAsync());
			Assert.Equal('f', await reader.ReadAsync());
		}

		[Fact]
		public async Task CanReadMixedAsciiAndUTFCharactersAsync()
		{
			var reader = new RebufferableBinaryReader(TestUtil.StringToStreamNoBom("abcdèfg"), Encoding.UTF8);

			Assert.Equal('a', await reader.ReadAsync());
			Assert.Equal('b', await reader.ReadAsync());
			Assert.Equal('c', await reader.ReadAsync());
			Assert.Equal('d', await reader.ReadAsync());
			Assert.Equal('è', await reader.ReadAsync());
			Assert.Equal('f', await reader.ReadAsync());
			Assert.Equal('g', await reader.ReadAsync());
		}

		[Fact]
		public async Task CanReadMixedAsciiAndUTFCharactersOverBuffersAsync()
		{
			var reader = new RebufferableBinaryReader(TestUtil.StringToStreamNoBom("dèfg"), Encoding.UTF8);
			reader.Buffer(TestUtil.StringToByteNoBom("abc"));

			Assert.Equal('a', await reader.ReadAsync());
			Assert.Equal('b', await reader.ReadAsync());
			Assert.Equal('c', await reader.ReadAsync());
			Assert.Equal('d', await reader.ReadAsync());
			Assert.Equal('è', await reader.ReadAsync());
			Assert.Equal('f', await reader.ReadAsync());
			Assert.Equal('g', await reader.ReadAsync());
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

		#region ReadAsync(buffer, index, count) Tests

		[Fact]
		public async Task CanReadSingleBufferAsync()
		{
			var reader = new RebufferableBinaryReader(TestUtil.StringToStreamNoBom("6chars"), Encoding.UTF8);

			var buffer = new byte[Encoding.UTF8.GetByteCount("6chars")];
			await reader.ReadAsync(buffer, 0, buffer.Length);
			string result = Encoding.UTF8.GetString(buffer);
			Assert.Equal("6chars", result);
		}

		[Fact]
		public async Task CanReadAcrossMultipleBuffersAsync()
		{
			var reader = new RebufferableBinaryReader(TestUtil.StringToStreamNoBom("ars"), Encoding.UTF8);
			reader.Buffer(TestUtil.StringToByteNoBom("6ch"));

			var buffer = new byte[6];
			await reader.ReadAsync(buffer, 0, buffer.Length);
			Assert.Equal("6chars", Encoding.UTF8.GetString(buffer));
		}

		[Fact]
		public async Task CanReadMixedAsciiAndUTF8Async()
		{
			var reader = new RebufferableBinaryReader(TestUtil.StringToStreamNoBom("5èats"), Encoding.UTF8);

			var buffer = new byte[Encoding.UTF8.GetByteCount("5èats")];
			await reader.ReadAsync(buffer, 0, buffer.Length);
			string result = Encoding.UTF8.GetString(buffer);
			Assert.Equal("5èats", result);
		}

		[Fact]
		public async Task CanReadMixedAsciiAndUTF8AcrossMultipleBuffersAsync()
		{
			var reader = new RebufferableBinaryReader(TestUtil.StringToStreamNoBom("ts"), Encoding.UTF8);
			reader.Buffer(TestUtil.StringToByteNoBom(("5èa")));

			var buffer = new byte[Encoding.UTF8.GetByteCount("5èats")];
			await reader.ReadAsync(buffer, 0, buffer.Length);
			string result = Encoding.UTF8.GetString(buffer);
			Assert.Equal("5èats", result);
		}

		[Fact]
		public async Task ReadCorrectlyHandlesSmallerBufferThenStreamAsync()
		{
			var reader = new RebufferableBinaryReader(TestUtil.StringToStreamNoBom("6chars"), Encoding.UTF8);

			var buffer = new byte[4];
			await reader.ReadAsync(buffer, 0, buffer.Length);
			Assert.Equal("6cha", Encoding.UTF8.GetString(buffer));

			buffer = new byte[2];
			await reader.ReadAsync(buffer, 0, buffer.Length);
			Assert.Equal("rs", Encoding.UTF8.GetString(buffer));
		}

		[Fact]
		public async Task ReadCorrectlyHandlesLargerBufferThenStreamAsync()
		{
			var reader = new RebufferableBinaryReader(TestUtil.StringToStreamNoBom("6chars"), Encoding.UTF8);

			var buffer = new byte[10];
			int amountRead = await reader.ReadAsync(buffer, 0, buffer.Length);
			Assert.Equal("6chars\0\0\0\0", Encoding.UTF8.GetString(buffer));
			Assert.Equal(6, amountRead);
		}

		[Fact]
		public async Task ReadReturnsZeroOnNoDataAsync()
		{
			var reader = new RebufferableBinaryReader(new MemoryStream(), Encoding.UTF8);

			var buffer = new byte[6];
			int amountRead = await reader.ReadAsync(buffer, 0, buffer.Length);
			Assert.Equal("\0\0\0\0\0\0", Encoding.UTF8.GetString(buffer));
			Assert.Equal(0, amountRead);
		}

		[Fact]
		public async Task ReadLineReturnsNullOnNoDataAsync()
		{
			var reader = new RebufferableBinaryReader(new MemoryStream(new byte[6]), Encoding.UTF8);

			var s = await reader.ReadLineAsync();
			Assert.Equal("\0\0\0\0\0\0", s);
			Assert.Null(await reader.ReadLineAsync());
		}

		[Fact]
		public async Task ReadCanResumeInterruptedStreamAsync()
		{
			var reader = new RebufferableBinaryReader(TestUtil.StringToStreamNoBom("6chars"), Encoding.UTF8);

			var buffer = new byte[4];
			int amountRead = await reader.ReadAsync(buffer, 0, buffer.Length);
			Assert.Equal("6cha", Encoding.UTF8.GetString(buffer));
			Assert.Equal(4, amountRead);

			reader.Buffer(TestUtil.StringToByteNoBom("14intermission"));
			buffer = new byte[14];
			amountRead = await reader.ReadAsync(buffer, 0, buffer.Length);
			Assert.Equal("14intermission", Encoding.UTF8.GetString(buffer));
			Assert.Equal(14, amountRead);

			buffer = new byte[2];
			amountRead = await reader.ReadAsync(buffer, 0, buffer.Length);
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

		#region ReadByteLineAsync() Tests

		[Fact]
		public async Task CanReadByteLineOnMixedAsciiAndUTF8TextAsync()
		{
			var reader = new RebufferableBinaryReader(TestUtil.StringToStreamNoBom("Bonjour poignée"), Encoding.UTF8);
			byte[] bytes = await reader.ReadByteLineAsync();
			var expected = new byte[] { 66, 111, 110, 106, 111, 117, 114, 32, 112, 111, 105, 103, 110, 195, 169, 101 };

			foreach (var pair in expected.Zip(bytes, Tuple.Create))
			{
				Assert.Equal(pair.Item1, pair.Item2);
			}
		}

		#endregion
	}
}
