// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SubsequenceFinder.cs" company="Jake Woods">
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
//   Provides methods to find a subsequence within a
//   sequence.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace HttpMultipartParser
{
    /// <summary>
    ///     Provides methods to find a subsequence within a
    ///     sequence.
    /// </summary>
    public class SubsequenceFinder
    {
        #region Public Methods and Operators

        /// <summary>
        ///     Finds if a sequence exists within another sequence.
        /// </summary>
        /// <param name="haystack">
        ///     The sequence to search.
        /// </param>
        /// <param name="needle">
        ///     The sequence to look for.
        /// </param>
        /// <returns>
        ///     The start position of the found sequence or -1 if nothing was found.
        /// </returns>
        public static int Search(byte[] haystack, byte[] needle)
        {
            return Search(haystack, needle, haystack.Length);
        }

        /// <summary>Finds if a sequence exists within another sequence.</summary>
        /// <param name="haystack">The sequence to search.</param>
        /// <param name="needle">The sequence to look for.</param>
        /// <param name="haystackLength">The length of the haystack to consider for searching.</param>
        /// <returns>The start position of the found sequence or -1 if nothing was found.</returns>
        /// <remarks>Inspired by https://stackoverflow.com/a/39021296/153084 .</remarks>
        public static int Search(byte[] haystack, byte[] needle, int haystackLength)
        {
            const int SEQUENCE_NOT_FOUND = -1;

            // Validate the parameters
            if (haystack == null || haystack.Length == 0) return SEQUENCE_NOT_FOUND;
            if (needle == null || needle.Length == 0) return SEQUENCE_NOT_FOUND;
            if (needle.Length > haystack.Length) return SEQUENCE_NOT_FOUND;
            if (haystackLength > haystack.Length || haystackLength < 1) throw new ArgumentException("Length must be between 1 and the length of the haystack.");

            int currentIndex = 0;
            int end = haystackLength - needle.Length; // past here no match is possible
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
            return SEQUENCE_NOT_FOUND;
        }

        #endregion
    }
}