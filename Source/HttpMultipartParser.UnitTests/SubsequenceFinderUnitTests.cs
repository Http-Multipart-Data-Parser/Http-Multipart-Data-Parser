using Xunit;

namespace HttpMultipartParser.UnitTests
{
    /// <summary>
    ///     Summary description for SubsequenceFinderUnitTest
    /// </summary>
    public class SubsequenceFinderUnitTests
    {
        [Fact]
        public void SmokeTest()
        {
            var A = new byte[] { 0x1, 0x2, 0x3, 0x4 };
            var B = new byte[] { 0x3, 0x4 };

            Assert.Equal(2, SubsequenceFinder.Search(A, B, A.Length));
        }

        [Fact]
        // This unit test was used to demonstrate the bug in the 'Search' method when the haystack contains
        // a subset of the needle immediately followed by the needle. Specifically, notice the
        // '0x0D' byte in the sample haystack followed by '0x0D, 0x0A' which is the sample needle.
        // This bug was resolved in July 2021.
        // See: https://github.com/Http-Multipart-Data-Parser/Http-Multipart-Data-Parser/issues/98
        public void Haystack_contains_subset_of_needle_followed_by_needle()
        {
            var haystack = new byte[] { 0x96, 0xC7, 0x0D, 0x0D, 0x0A, 0x2D, 0x2D, 0x63, 0x65 };
            var needle = new byte[] { 0x0D, 0x0A };
            var expectedPosition = 3;

            var result = SubsequenceFinder.Search(haystack, needle, haystack.Length);

            Assert.Equal(expectedPosition, result);
        }
    }
}