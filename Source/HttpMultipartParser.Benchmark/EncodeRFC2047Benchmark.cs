using BenchmarkDotNet.Attributes;
using System;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;

#pragma warning disable IDE0066 // Convert switch statement to expression
#pragma warning disable IDE0018 // Inline variable declaration
#pragma warning disable IDE0090 // Use 'new(...)'

namespace HttpMultipartParser.Benchmark
{
    [MemoryDiagnoser]
    [HtmlExporter]
    [JsonExporter]
    [MarkdownExporter]
    public class EncodeRFC2047Benchmark
    {
        const string sample = "=?iso-8859-1?Q?=A1Hola,_se=F1or!?=";

        public EncodeRFC2047Benchmark()
        {
        }

        [Benchmark]
        public string Grumpydev()
        {
            return From_Grumpydev_GitHub.RFC2047.Decode(sample);
        }

        [Benchmark]
        public string Stratosphere()
        {
            return From_Stratosphere_GitHub.RFC2047Decoder.Parse(sample);
        }

        [Benchmark]
        public string SystemNetMailAttachment()
        {
            var attachment = Attachment.CreateAttachmentFromString(string.Empty, sample);
            return attachment.Name;
        }

        [Benchmark]
        public string DecodeQuotedPrintables()
        {
            return DecodeQuotedPrintables(sample, null);
        }

        // From https://stackoverflow.com/a/8307660/153084
        private static string DecodeQuotedPrintables(string input, string charSet)
        {
            if (string.IsNullOrEmpty(charSet))
            {
                var charSetOccurences = new Regex(@"=\?.*\?Q\?", RegexOptions.IgnoreCase);
                var charSetMatches = charSetOccurences.Matches(input);
                foreach (Match match in charSetMatches)
                {
                    charSet = match.Groups[0].Value.Replace("=?", "").Replace("?Q?", "");
                    input = input.Replace(match.Groups[0].Value, "").Replace("?=", "");
                }
            }

            Encoding enc = new ASCIIEncoding();
            if (!string.IsNullOrEmpty(charSet))
            {
                try
                {
                    enc = Encoding.GetEncoding(charSet);
                }
                catch
                {
                    enc = new ASCIIEncoding();
                }
            }

            //decode iso-8859-[0-9]
            var occurences = new Regex(@"=[0-9A-Z]{2}", RegexOptions.Multiline);
            var matches = occurences.Matches(input);
            foreach (Match match in matches)
            {
                try
                {
                    byte[] b = new byte[] { byte.Parse(match.Groups[0].Value.Substring(1), System.Globalization.NumberStyles.AllowHexSpecifier) };
                    char[] hexChar = enc.GetChars(b);
                    input = input.Replace(match.Groups[0].Value, hexChar[0].ToString());
                }
                catch { }
            }

            //decode base64String (utf-8?B?)
            occurences = new Regex(@"\?utf-8\?B\?.*\?", RegexOptions.IgnoreCase);
            matches = occurences.Matches(input);
            foreach (Match match in matches)
            {
                byte[] b = Convert.FromBase64String(match.Groups[0].Value.Replace("?utf-8?B?", "").Replace("?UTF-8?B?", "").Replace("?", ""));
                string temp = Encoding.UTF8.GetString(b);
                input = input.Replace(match.Groups[0].Value, temp);
            }

            input = input.Replace("=\r\n", "");
            return input;
        }
    }
}

//===============================================================================
// RFC2047 (Encoded Word) Decoder
//
// http://tools.ietf.org/html/rfc2047
//===============================================================================
// Copyright © Steven Robbins.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================
namespace From_Grumpydev_GitHub
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Provides support for decoding RFC2047 (Encoded Word) encoded text
    /// </summary>
    public static class RFC2047
    {
        /// <summary>
        /// Regex for parsing encoded word sections
        /// From http://tools.ietf.org/html/rfc2047#section-3
        /// encoded-word = "=?" charset "?" encoding "?" encoded-text "?="
        /// </summary>
        private static readonly Regex EncodedWordFormatRegEx = new Regex(@"=\?(?<charset>.*?)\?(?<encoding>[qQbB])\?(?<encodedtext>.*?)\?=", RegexOptions.Singleline | RegexOptions.Compiled);

        /// <summary>
        /// Regex for removing CRLF SPACE separators from between encoded words
        /// </summary>
        private static readonly Regex EncodedWordSeparatorRegEx = new Regex(@"\?=\r\n =\?", RegexOptions.Compiled);

        /// <summary>
        /// Replacement string for removing CRLF SPACE separators
        /// </summary>
        private const string SeparatorReplacement = @"?==?";

        /// <summary>
        /// The maximum line length allowed
        /// </summary>
        private const int MaxLineLength = 75;

        /// <summary>
        /// Regex for "Q-Encoding" hex bytes from http://tools.ietf.org/html/rfc2047#section-4.2
        /// </summary>
        private static readonly Regex QEncodingHexCodeRegEx = new Regex(@"(=(?<hexcode>[0-9a-fA-F][0-9a-fA-F]))", RegexOptions.Compiled);

        /// <summary>
        /// Regex for replacing _ with space as declared in http://tools.ietf.org/html/rfc2047#section-4.2
        /// </summary>
        private static readonly Regex QEncodingSpaceRegEx = new Regex("_", RegexOptions.Compiled);

        /// <summary>
        /// Format for an encoded string
        /// </summary>
        private const string EncodedStringFormat = @"=?{0}?{1}?{2}?=";

        /// <summary>
        /// Special characters, as defined by RFC2047
        /// </summary>
        private static readonly char[] SpecialCharacters = { '(', ')', '<', '>', '@', ',', ';', ':', '<', '>', '/', '[', ']', '?', '.', '=', '\t' };

        /// <summary>
        /// Represents a content encoding type defined in RFC2047
        /// </summary>
        public enum ContentEncoding
        {
            /// <summary>
            /// Unknown / invalid encoding
            /// </summary>
            Unknown,

            /// <summary>
            /// "Q Encoding" (reduced character set) encoding
            /// http://tools.ietf.org/html/rfc2047#section-4.2
            /// </summary>
            QEncoding,

            /// <summary>
            /// Base 64 encoding
            /// http://tools.ietf.org/html/rfc2047#section-4.1
            /// </summary>
            Base64
        }

        /// <summary>
        /// Encode a string into RFC2047
        /// </summary>
        /// <param name="plainString">Plain string to encode</param>
        /// <param name="contentEncoding">Content encoding to use</param>
        /// <param name="characterSet">Character set used by plainString</param>
        /// <returns>Encoded string</returns>
        public static string Encode(string plainString, ContentEncoding contentEncoding = ContentEncoding.QEncoding, string characterSet = "iso-8859-1")
        {
            if (String.IsNullOrEmpty(plainString))
            {
                return String.Empty;
            }

            if (contentEncoding == ContentEncoding.Unknown)
            {
                throw new ArgumentException("contentEncoding cannot be unknown for encoding.", "contentEncoding");
            }

            if (!IsSupportedCharacterSet(characterSet))
            {
                throw new ArgumentException("characterSet is not supported", "characterSet");
            }

            var textEncoding = Encoding.GetEncoding(characterSet);

            var encoder = GetContentEncoder(contentEncoding);

            var encodedContent = encoder.Invoke(plainString, textEncoding);

            return BuildEncodedString(characterSet, contentEncoding, encodedContent);
        }

        /// <summary>
        /// Decode a string containing RFC2047 encoded sections
        /// </summary>
        /// <param name="encodedString">String contaning encoded sections</param>
        /// <returns>Decoded string</returns>
        public static string Decode(string encodedString)
        {
            // Remove separators
            var decodedString = EncodedWordSeparatorRegEx.Replace(encodedString, SeparatorReplacement);

            return EncodedWordFormatRegEx.Replace(
                decodedString,
                m =>
                {
                    var contentEncoding = GetContentEncodingType(m.Groups["encoding"].Value);
                    if (contentEncoding == ContentEncoding.Unknown)
                    {
                        // Regex should never match, but return anyway
                        return string.Empty;
                    }

                    var characterSet = m.Groups["charset"].Value;
                    if (!IsSupportedCharacterSet(characterSet))
                    {
                        // Fall back to iso-8859-1 if invalid/unsupported character set found
                        characterSet = @"iso-8859-1";
                    }

                    var textEncoding = Encoding.GetEncoding(characterSet);
                    var contentDecoder = GetContentDecoder(contentEncoding);
                    var encodedText = m.Groups["encodedtext"].Value;

                    return contentDecoder.Invoke(encodedText, textEncoding);
                });
        }

        /// <summary>
        /// Determines if a character set is supported
        /// </summary>
        /// <param name="characterSet">Character set name</param>
        /// <returns>Bool representing whether the character set is supported</returns>
        private static bool IsSupportedCharacterSet(string characterSet)
        {
            return Encoding.GetEncodings()
                           .Where(e => String.Equals(e.Name, characterSet, StringComparison.InvariantCultureIgnoreCase))
                           .Any();
        }

        /// <summary>
        /// Gets the content encoding type from the encoding character
        /// </summary>
        /// <param name="contentEncodingCharacter">Content contentEncodingCharacter character</param>
        /// <returns>ContentEncoding type</returns>
        private static ContentEncoding GetContentEncodingType(string contentEncodingCharacter)
        {
            switch (contentEncodingCharacter)
            {
                case "Q":
                case "q":
                    return ContentEncoding.QEncoding;
                case "B":
                case "b":
                    return ContentEncoding.Base64;
                default:
                    return ContentEncoding.Unknown;
            }
        }

        /// <summary>
        /// Gets the content decoder delegate for the given content encoding type
        /// </summary>
        /// <param name="contentEncoding">Content encoding type</param>
        /// <returns>Decoding delegate</returns>
        private static Func<string, Encoding, string> GetContentDecoder(ContentEncoding contentEncoding)
        {
            switch (contentEncoding)
            {
                case ContentEncoding.Base64:
                    return DecodeBase64;
                case ContentEncoding.QEncoding:
                    return DecodeQEncoding;
                default:
                    // Will never get here, but return a "null" delegate anyway
                    return (s, e) => String.Empty;
            }
        }

        /// <summary>
        /// Gets the content encoder delegate for the given content encoding type
        /// </summary>
        /// <param name="contentEncoding">Content encoding type</param>
        /// <returns>Encoding delegate</returns>
        private static Func<string, Encoding, string> GetContentEncoder(ContentEncoding contentEncoding)
        {
            switch (contentEncoding)
            {
                case ContentEncoding.Base64:
                    return EncodeBase64;
                case ContentEncoding.QEncoding:
                    return EncodeQEncoding;
                default:
                    // Will never get here, but return a "null" delegate anyway
                    return (s, e) => String.Empty;
            }
        }

        /// <summary>
        /// Decodes a base64 encoded string
        /// </summary>
        /// <param name="encodedText">Encoded text</param>
        /// <param name="textEncoder">Encoding instance for the code page required</param>
        /// <returns>Decoded string</returns>
        private static string DecodeBase64(string encodedText, Encoding textEncoder)
        {
            var encodedBytes = Convert.FromBase64String(encodedText);

            return textEncoder.GetString(encodedBytes);
        }

        /// <summary>
        /// Encodes a base64 encoded string
        /// </summary>
        /// <param name="plainText">Plain text</param>
        /// <param name="textEncoder">Encoding instance for the code page required</param>
        /// <returns>Encoded string</returns>
        private static string EncodeBase64(string plainText, Encoding textEncoder)
        {
            var plainTextBytes = textEncoder.GetBytes(plainText);

            return Convert.ToBase64String(plainTextBytes);
        }

        /// <summary>
        /// Decodes a "Q encoded" string
        /// </summary>
        /// <param name="encodedText">Encoded text</param>
        /// <param name="textEncoder">Encoding instance for the code page required</param>
        /// <returns>Decoded string</returns>
        private static string DecodeQEncoding(string encodedText, Encoding textEncoder)
        {
            var decodedText = QEncodingSpaceRegEx.Replace(encodedText, " ");

            decodedText = QEncodingHexCodeRegEx.Replace(
                decodedText,
                m =>
                {
                    var hexString = m.Groups["hexcode"].Value;

                    int characterValue;
                    if (!int.TryParse(hexString, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out characterValue))
                    {
                        return String.Empty;
                    }

                    return textEncoder.GetString(new[] { (byte)characterValue });
                });

            return decodedText;
        }

        /// <summary>
        /// Encodes a "Q encoded" string
        /// </summary>
        /// <param name="plainText">Plain text</param>
        /// <param name="textEncoder">Encoding instance for the code page required</param>
        /// <returns>Encoded string</returns>
        private static string EncodeQEncoding(string plainText, Encoding textEncoder)
        {
            if (textEncoder.GetByteCount(plainText) != plainText.Length)
            {
                throw new ArgumentException("Q encoding only supports single byte encodings", "textEncoder");
            }

            var specialBytes = textEncoder.GetBytes(SpecialCharacters);

            var sb = new StringBuilder(plainText.Length);

            var plainBytes = textEncoder.GetBytes(plainText);

            // Replace "high" values
            for (int i = 0; i < plainBytes.Length; i++)
            {
                if (plainBytes[i] <= 127 && !specialBytes.Contains(plainBytes[i]))
                {
                    sb.Append(Convert.ToChar(plainBytes[i]));
                }
                else
                {
                    sb.Append("=");
                    sb.Append(Convert.ToString(plainBytes[i], 16).ToUpper());
                }
            }

            return sb.ToString().Replace(" ", "_");
        }

        /// <summary>
        /// Builds the full encoded string representation
        /// </summary>
        /// <param name="characterSet">Characterset to use</param>
        /// <param name="contentEncoding">Content encoding to use</param>
        /// <param name="encodedContent">Content, encoded to the above parameters</param>
        /// <returns>Valid RFC2047 string</returns>
        private static string BuildEncodedString(string characterSet, ContentEncoding contentEncoding, string encodedContent)
        {
            var encodingCharacter = String.Empty;

            switch (contentEncoding)
            {
                case ContentEncoding.Base64:
                    encodingCharacter = "B";
                    break;
                case ContentEncoding.QEncoding:
                    encodingCharacter = "Q";
                    break;
            }

            var wrapperLength = string.Format(EncodedStringFormat, characterSet, encodingCharacter, String.Empty).Length;
            var chunkLength = MaxLineLength - wrapperLength;

            if (encodedContent.Length <= chunkLength)
            {
                return string.Format(EncodedStringFormat, characterSet, encodingCharacter, encodedContent);
            }

            var sb = new StringBuilder();
            foreach (var chunk in SplitStringByLength(encodedContent, chunkLength))
            {
                sb.AppendFormat(EncodedStringFormat, characterSet, encodingCharacter, chunk);
                sb.Append("\r\n ");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Splits a string into chunks
        /// </summary>
        /// <param name="inputString">Input string</param>
        /// <param name="chunkSize">Size of each chunk</param>
        /// <returns>String collection of chunked strings</returns>
        public static IEnumerable<string> SplitStringByLength(this string inputString, int chunkSize)
        {
            for (int index = 0; index < inputString.Length; index += chunkSize)
            {
                yield return inputString.Substring(index, Math.Min(chunkSize, inputString.Length - index));
            }
        }
    }
}

/// This class adapted from the original RFC2047Decoder class seen here
/// http://blog.crazybeavers.se/index.php/archive/rfc2047decoder-in-c-sharp/
/// 
/// It has been adapted (following http://www.ietf.org/rfc/rfc2047.txt) to:
/// * handle underscores inside encoded-words
/// * strip whitespace between adjacent encoded-words
/// * do not strip whitespace from "surrounding text"
namespace From_Stratosphere_GitHub
{
    using System;
    using System.Text;

    public static class RFC2047Decoder
    {
        public static string Parse(string input)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder currentWord = new StringBuilder();
            StringBuilder currentSurroundingText = new StringBuilder();
            bool readingWord = false;
            bool hasSeenAtLeastOneWord = false;

            int wordQuestionMarkCount = 0;
            int i = 0;
            while (i < input.Length)
            {
                char currentChar = input[i];
                char peekAhead;
                switch (currentChar)
                {
                    case '=':
                        peekAhead = (i == input.Length - 1) ? ' ' : input[i + 1];

                        if (!readingWord && peekAhead == '?')
                        {
                            if (!hasSeenAtLeastOneWord
                                || (hasSeenAtLeastOneWord && currentSurroundingText.ToString().Trim().Length > 0))
                            {
                                sb.Append(currentSurroundingText.ToString());
                            }

                            currentSurroundingText = new StringBuilder();
                            hasSeenAtLeastOneWord = true;
                            readingWord = true;
                            wordQuestionMarkCount = 0;
                        }
                        break;

                    case '?':
                        if (readingWord)
                        {
                            wordQuestionMarkCount++;

                            peekAhead = (i == input.Length - 1) ? ' ' : input[i + 1];

                            if (wordQuestionMarkCount > 3 && peekAhead == '=')
                            {
                                readingWord = false;

                                currentWord.Append(currentChar);
                                currentWord.Append(peekAhead);

                                sb.Append(ParseEncodedWord(currentWord.ToString()));
                                currentWord = new StringBuilder();

                                i += 2;
                                continue;
                            }
                        }
                        break;
                }

                if (readingWord)
                {
                    currentWord.Append(('_' == currentChar) ? ' ' : currentChar);
                    i++;
                }
                else
                {
                    currentSurroundingText.Append(currentChar);
                    i++;
                }
            }

            sb.Append(currentSurroundingText.ToString());

            return sb.ToString();
        }

        private static string ParseEncodedWord(string input)
        {
            StringBuilder sb = new StringBuilder();

            if (!input.StartsWith("=?"))
                return input;

            if (!input.EndsWith("?="))
                return input;

            // Get the name of the encoding but skip the leading =?
            string encodingName = input.Substring(2, input.IndexOf("?", 2) - 2);
            Encoding enc = ASCIIEncoding.ASCII;
            if (!string.IsNullOrEmpty(encodingName))
            {
                enc = Encoding.GetEncoding(encodingName);
            }

            // Get the type of the encoding
            char type = input[encodingName.Length + 3];

            // Start after the name of the encoding and the other required parts
            int startPosition = encodingName.Length + 5;

            switch (char.ToLowerInvariant(type))
            {
                case 'q':
                    sb.Append(ParseQuotedPrintable(enc, input, startPosition, true));
                    break;
                case 'b':
                    string baseString = input.Substring(startPosition, input.Length - startPosition - 2);
                    byte[] baseDecoded = Convert.FromBase64String(baseString);
                    var intermediate = enc.GetString(baseDecoded);
                    sb.Append(intermediate);
                    break;
            }
            return sb.ToString();
        }

        public static string ParseQuotedPrintable(Encoding enc, string input)
        {
            return ParseQuotedPrintable(enc, input, 0, false);
        }

        public static string ParseQuotedPrintable(Encoding enc, string input, int startPos, bool skipQuestionEquals)
        {
            byte[] workingBytes = ASCIIEncoding.ASCII.GetBytes(input);

            int i = startPos;
            int outputPos = i;

            while (i < workingBytes.Length)
            {
                byte currentByte = workingBytes[i];
                char[] peekAhead = new char[2];
                switch (currentByte)
                {
                    case (byte)'=':
                        bool canPeekAhead = (i < workingBytes.Length - 2);

                        if (!canPeekAhead)
                        {
                            workingBytes[outputPos] = workingBytes[i];
                            ++outputPos;
                            ++i;
                            break;
                        }

                        int skipNewLineCount = 0;
                        for (int j = 0; j < 2; ++j)
                        {
                            char c = (char)workingBytes[i + j + 1];
                            if ('\r' == c || '\n' == c)
                            {
                                ++skipNewLineCount;
                            }
                        }

                        if (skipNewLineCount > 0)
                        {
                            // If we have a lone equals followed by newline chars, then this is an artificial
                            // line break that should be skipped past.
                            i += 1 + skipNewLineCount;
                        }
                        else
                        {
                            try
                            {
                                peekAhead[0] = (char)workingBytes[i + 1];
                                peekAhead[1] = (char)workingBytes[i + 2];

                                byte decodedByte = Convert.ToByte(new string(peekAhead, 0, 2), 16);
                                workingBytes[outputPos] = decodedByte;

                                ++outputPos;
                                i += 3;
                            }
                            catch (Exception)
                            {
                                // could not parse the peek-ahead chars as a hex number... so gobble the un-encoded '='
                                i += 1;
                            }
                        }
                        break;

                    case (byte)'?':
                        if (skipQuestionEquals && workingBytes[i + 1] == (byte)'=')
                        {
                            i += 2;
                        }
                        else
                        {
                            workingBytes[outputPos] = workingBytes[i];
                            ++outputPos;
                            ++i;
                        }
                        break;

                    default:
                        workingBytes[outputPos] = workingBytes[i];
                        ++outputPos;
                        ++i;
                        break;
                }
            }

            string output = string.Empty;

            int numBytes = outputPos - startPos;
            if (numBytes > 0)
            {
                output = enc.GetString(workingBytes, startPos, numBytes);
            }

            return output;
        }
    }
}

#pragma warning restore IDE0090 // Use 'new(...)'
#pragma warning restore IDE0018 // Inline variable declaration
#pragma warning restore IDE0066 // Convert switch statement to expression