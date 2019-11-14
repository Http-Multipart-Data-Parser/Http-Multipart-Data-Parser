using HttpMultipartParser;
using Xunit;

namespace HttpMultipartParserUnitTest
{
    /// <summary>
    ///     Summary description for SubsequenceFinderUnitTest
    /// </summary>
    public class SubsequenceFinderUnitTest
    {
        [Fact]
        public void SmokeTest()
        {
            var A = new byte[] {0x1, 0x2, 0x3, 0x4};
            var B = new byte[] {0x3, 0x4};

            Assert.Equal(SubsequenceFinder.Search(A, B, A.Length), 2);
        }
    }
}