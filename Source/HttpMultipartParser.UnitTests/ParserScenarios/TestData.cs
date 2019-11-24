using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HttpMultipartParser.UnitTests.ParserScenarios
{
    /// <summary>
    ///     Represents a single parsing test case and the expected parameter/file outputs
    ///     for a given request.
    /// </summary>
    internal class TestData
    {
        #region Constructors and Destructors

        public TestData(
            string request,
            List<ParameterPart> expectedParams,
            List<FilePart> expectedFileData)
        {
            Request = request;
            ExpectedParams = expectedParams;
            ExpectedFileData = expectedFileData;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the expected file data.
        /// </summary>
        public List<FilePart> ExpectedFileData { get; set; }

        /// <summary>
        ///     Gets or sets the expected parameters.
        /// </summary>
        public List<ParameterPart> ExpectedParams { get; set; }

        /// <summary>
        ///     Gets or sets the request.
        /// </summary>
        public string Request { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Validates the output of the parser against the expected outputs for
        ///     this test
        /// </summary>
        /// <param name="parser">
        ///     The parser to validate.
        /// </param>
        /// <returns>
        ///     The <see cref="bool" /> representing if this test passed.
        /// </returns>
        public bool Validate(MultipartFormDataParser parser)
        {
            // Deal with all the parameters who are only expected to have one value.
            var expectedParametersWithSingleValue = ExpectedParams
                .GroupBy(p => p.Name)
                .Where(g => g.Count() == 1)
                .Select(g => g.Single());

            foreach (var expectedParameter in expectedParametersWithSingleValue)
            {
                if (!parser.HasParameter(expectedParameter.Name))
                {
                    return false;
                }

                var actualValue = parser.GetParameterValue(expectedParameter.Name);
                var actualValueFromValues = parser.GetParameterValues(expectedParameter.Name).Single();

                if (actualValue != actualValueFromValues)
                {
                    return false;
                }

                if (expectedParameter.Data != actualValue)
                {
                    return false;
                }
            }

            // Deal with the parameters who are expected to have more then one value
            var expectedParametersWithMultiValues = ExpectedParams
                .GroupBy(p => p.Name)
                .Where(a => a.Count() > 1);

            foreach (var expectedParameters in expectedParametersWithMultiValues)
            {
                var key = expectedParameters.Key;
                if (!parser.HasParameter(key))
                {
                    return false;
                }

                var actualValues = parser.GetParameterValues(key);

                if (actualValues.Count() != expectedParameters.Count() || actualValues.Zip(expectedParameters, Tuple.Create).Any(t => t.Item1 != t.Item2.Data))
                {
                    return false;
                }
            }

            // Validate files
            foreach (var filePart in ExpectedFileData)
            {
                var foundPairMatch = false;
                foreach (var file in parser.Files)
                {
                    if (filePart.Name == file.Name)
                    {
                        foundPairMatch = true;

                        FilePart expectedFile = filePart;
                        FilePart actualFile = file;

                        if (expectedFile.Name != actualFile.Name || expectedFile.FileName != actualFile.FileName)
                        {
                            return false;
                        }

                        if (expectedFile.ContentType != actualFile.ContentType ||
                            expectedFile.ContentDisposition != actualFile.ContentDisposition)
                        {
                            return false;
                        }

                        // Read the data from the files and see if it's the same
                        if (expectedFile.Data.CanSeek)
                        {
                            expectedFile.Data.Position = 0;
                        }

                        string expectedFileData;
                        // The last boolean parameter MUST be set to true: it ensures the stream is left open
                        using (var reader = new StreamReader(expectedFile.Data, Encoding.UTF8, false, 1024, true))
                        {
                            expectedFileData = reader.ReadToEnd();
                        }

                        string actualFileData;
                        // The last boolean parameter MUST be set to true: it ensures the stream is left open
                        using (var reader = new StreamReader(actualFile.Data, Encoding.UTF8, false, 1024, true))
                        {
                            actualFileData = reader.ReadToEnd();
                        }

                        if (expectedFileData != actualFileData)
                        {
                            return false;
                        }

                        break;
                    }
                }

                if (!foundPairMatch)
                {
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}
