using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;

namespace HttpMultipartParser.Benchmark
{
	[MemoryDiagnoser]
	[HtmlExporter]
	[JsonExporter]
	[MarkdownExporter]
	public class SubsequenceFinderBenchmark
	{
		// This is the default size buffer size defined in StramingMultipartFormDataParser.cs
		private const int DefaultBufferSize = 4096;

		private readonly byte[] haystack = new byte[DefaultBufferSize];
		private readonly byte[] needle = new byte[2];

		public SubsequenceFinderBenchmark()
		{
			// Pupulate the byte arrays with random data
			var random = new Random();
			random.NextBytes(haystack);
			random.NextBytes(needle);
		}

		[Benchmark(Baseline = true)]
		public int OldSearch()
		{
			return OldSearch(haystack, needle, haystack.Length);
		}

		[Benchmark]
		public int FindSequence()
		{
			return FindSequence(haystack, needle);
		}

		[Benchmark]
		public int Locate()
		{
			return Locate(haystack, needle);
		}

		[Benchmark]
		public int BoyerMoore()
		{
			var searcher = new BoyerMooreClass(needle);
			return searcher.Search(haystack);
		}

		[Benchmark]
		public int ByteSearch()
		{
			return ByteSearch(haystack, needle);
		}

		// This is the logic that was in HttpMultipartParser until July 2021.
		// As explained in this [GitHub issue](https://github.com/Http-Multipart-Data-Parser/Http-Multipart-Data-Parser/issues/98),
		// it contains a bug when the haystack contains a subset of the neddle immediately followed by the needle.
		private static int OldSearch(byte[] haystack, byte[] needle, int haystackLength)
		{
			var charactersInNeedle = new HashSet<byte>(needle);

			var length = needle.Length;
			var index = 0;
			while (index + length <= haystackLength)
			{
				// Worst case scenario: Go back to character-by-character parsing until we find a non-match
				// or we find the needle.
				if (charactersInNeedle.Contains(haystack[index + length - 1]))
				{
					var needleIndex = 0;
					while (haystack[index + needleIndex] == needle[needleIndex])
					{
						if (needleIndex == needle.Length - 1)
						{
							// Found our match!
							return index;
						}

						needleIndex += 1;
					}

					index += 1;
					index += needleIndex;
					continue;
				}

				index += length;
			}

			return -1;
		}

		// From: https://stackoverflow.com/a/39021296/153084
		private static int FindSequence(byte[] haystack, byte[] needle)
		{
			int currentIndex = 0;
			int end = haystack.Length - needle.Length; // past here no match is possible
			byte firstByte = needle[0]; // cached to tell compiler there's no aliasing

			while (currentIndex <= end)
			{
				// scan for first byte only. compiler-friendly.
				if (haystack[currentIndex] == firstByte)
				{
					// scan for rest of sequence
					for (int offset = 1; ; ++offset)
					{
						if (offset == needle.Length)
						{ // full sequence matched?
							return currentIndex;
						}
						else if (haystack[currentIndex + offset] != needle[offset])
						{
							break;
						}
					}
				}

				++currentIndex;
			}

			// end of array reached without match
			return -1;
		}

		// This is the solution proposed by @succeun in the GitHub issue
		private static int Locate(byte[] self, byte[] candidate)
		{
			if (IsEmptyLocate(self, candidate))
				return -1;

			for (int i = 0; i < self.Length; i++)
			{
				if (!IsMatch(self, i, candidate))
				{
					continue;
				}
				return i;
			}

			return -1;
		}

		private static bool IsMatch(byte[] array, int position, byte[] candidate)
		{
			if (candidate.Length > (array.Length - position))
				return false;

			for (int i = 0; i < candidate.Length; i++)
				if (array[position + i] != candidate[i])
					return false;

			return true;
		}

		private static bool IsEmptyLocate(byte[] array, byte[] candidate)
		{
			return array == null
				|| candidate == null
				|| array.Length == 0
				|| candidate.Length == 0
				|| candidate.Length > array.Length;
		}

		// From: https://stackoverflow.com/a/37500883/153084
		private sealed class BoyerMooreClass
		{
			readonly byte[] needle;
			readonly int[] charTable;
			readonly int[] offsetTable;

			public BoyerMooreClass(byte[] needle)
			{
				this.needle = needle;
				this.charTable = makeByteTable(needle);
				this.offsetTable = makeOffsetTable(needle);
			}

			public int Search(byte[] haystack)
			{
				if (needle.Length == 0)
					return -1;

				for (int i = needle.Length - 1; i < haystack.Length;)
				{
					int j;

					for (j = needle.Length - 1; needle[j] == haystack[i]; --i, --j)
					{
						if (j != 0)
							continue;

						return i;
					}

					i += Math.Max(offsetTable[needle.Length - 1 - j], charTable[haystack[i]]);
				}

				return -1;
			}

			static int[] makeByteTable(byte[] needle)
			{
				const int ALPHABET_SIZE = 256;
				int[] table = new int[ALPHABET_SIZE];

				for (int i = 0; i < table.Length; ++i)
					table[i] = needle.Length;

				for (int i = 0; i < needle.Length - 1; ++i)
					table[needle[i]] = needle.Length - 1 - i;

				return table;
			}

			static int[] makeOffsetTable(byte[] needle)
			{
				int[] table = new int[needle.Length];
				int lastPrefixPosition = needle.Length;

				for (int i = needle.Length - 1; i >= 0; --i)
				{
					if (isPrefix(needle, i + 1))
						lastPrefixPosition = i + 1;

					table[needle.Length - 1 - i] = lastPrefixPosition - i + needle.Length - 1;
				}

				for (int i = 0; i < needle.Length - 1; ++i)
				{
					int slen = suffixLength(needle, i);
					table[slen] = needle.Length - 1 - i + slen;
				}

				return table;
			}

			static bool isPrefix(byte[] needle, int p)
			{
				for (int i = p, j = 0; i < needle.Length; ++i, ++j)
					if (needle[i] != needle[j])
						return false;

				return true;
			}

			static int suffixLength(byte[] needle, int p)
			{
				int len = 0;

				for (int i = p, j = needle.Length - 1; i >= 0 && needle[i] == needle[j]; --i, --j)
					++len;

				return len;
			}
		}

		// From: https://boncode.blogspot.com/2011/02/net-c-find-pattern-in-byte-array.html
		private static int ByteSearch(byte[] searchIn, byte[] searchBytes, int start = 0)
		{
			int found = -1;
			bool matched = false;
			//only look at this if we have a populated search array and search bytes with a sensible start
			if (searchIn.Length > 0 && searchBytes.Length > 0 && start <= (searchIn.Length - searchBytes.Length) && searchIn.Length >= searchBytes.Length)
			{
				//iterate through the array to be searched
				for (int i = start; i <= searchIn.Length - searchBytes.Length; i++)
				{
					//if the start bytes match we will start comparing all other bytes
					if (searchIn[i] == searchBytes[0])
					{
						if (searchIn.Length > 1)
						{
							//multiple bytes to be searched we have to compare byte by byte
							matched = true;
							for (int y = 1; y <= searchBytes.Length - 1; y++)
							{
								if (searchIn[i + y] != searchBytes[y])
								{
									matched = false;
									break;
								}
							}
							//everything matched up
							if (matched)
							{
								found = i;
								break;
							}

						}
						else
						{
							//search byte is only one bit nothing else to do
							found = i;
							break; //stop the loop
						}

					}
				}

			}
			return found;
		}
	}
}
