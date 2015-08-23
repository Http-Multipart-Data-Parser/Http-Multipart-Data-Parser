What is it?
===========

The Http Multipart Parser does it exactly what it claims on the tin: parses multipart/form-data. This particular
parser is well suited to parsing large data from streams as it doesn't attempt to read the entire stream at once and
procudes a set of streams for file data.

Installation
=============
Simply add the HttpMultipartParser project to your solution and reference it in the projects you want to use it in.

There is also a NuGet package provided.

Dependencies
============
The parser was built and tested for NET 4.0. Versions lower then this may work but are untested.

How do I use it?
================
## Non-Streaming (Simple, don't use on very large files)
1. Create a new MultipartFormDataParser with the stream containing the multipart/form-data.
2. Access the data through the parser.

## Streaming (Handles large files)
1. Create a new StreamingMultipartFormDataParser with the stream containing the multipart/form-data
2. Set up the ParameterHandler and FileHandler delegates
3. Call parser.Run()
4. The delegates will be called as data streams in.

Examples
========

Single file
-----------

    // stream:
    -----------------------------41952539122868
    Content-Disposition: form-data; name="username"

    example
    -----------------------------41952539122868
    Content-Disposition: form-data; name="email"

    example@data.com
    -----------------------------41952539122868
    Content-Disposition: form-data; name="files[]"; filename="photo1.jpg"
    Content-Type: image/jpeg

    ExampleBinaryData012031203
    -----------------------------41952539122868--

    // ===== Simple Parsing ====
    // parse:
    var parser = new MultipartFormDataParser(stream);

    // From this point the data is parsed, we can retrieve the
    // form data using the GetParameterValue method.
    var username = parser.GetParameterValue("username");
    var email = parser.GetParameterValue("email")

    // Files are stored in a list:
    var file = parser.Files.First();
    string filename = file.FileName;
    Stream data = file.Data;

    // ==== Advanced Parsing ====
    // parse:
    var parser = new StreamingMultipartFormDataParser(stream);
    parser.ParameterHandler += parameter => DoSomethingWithParameter(parameter);
    parser.FileHandler += (name, fileName, type, disposition, buffer, bytes) =>
    {
        // Write the part of the file we've recieved to a file stream. (Or do something else)
        filestream.Write(buffer, 0, bytes);
    }

Multiple Parameters
-------------------

    // stream:
    -----------------------------41952539122868
    Content-Disposition: form-data; name="checkbox"

    likes_cake
    -----------------------------41952539122868
    Content-Disposition: form-data; name="checkbox"

    likes_cookies
    -----------------------------41952539122868--

    // ===== Simple Parsing ====
    // parse:
    var parser = new MultipartFormDataParser(stream);

    // From this point the data is parsed, we can retrieve the
    // form data from the GetParameterValues method
    var checkboxResponses = parser.GetParameterValues("checkbox");
    foreach(var parameter in checkboxResponses)
    {
        Console.WriteLine("Parameter {0} is {1}", parameter.Name, parameter.Data)
    }

Multiple Files
-----------

    // stream:
    -----------------------------41111539122868
    Content-Disposition: form-data; name="files[]"; filename="photo1.jpg"
    Content-Type: image/jpeg

    MoreBinaryData
    -----------------------------41111539122868
    Content-Disposition: form-data; name="files[]"; filename="photo2.jpg"
    Content-Type: image/jpeg

    ImagineLotsOfBinaryData
    -----------------------------41111539122868--

    // ===== Simple Parsing ====
    // parse:
    var parser = new MultipartFormDataParser(stream);

    // Loop through all the files
    foreach(var file in parser.Files)
    {
        Stream data = file.Data;

        // Do stuff with the data.
    }

    // ==== Advanced Parsing ====
    // parse:
    var parser = new StreamingMultipartFormDataParser(stream);
    parser.ParameterHandler += parameter => DoSomethingWithParameter(parameter);
    parser.FileHandler += (name, fileName, type, disposition, buffer, bytes) =>
    {
        // Write the part of the file we've recieved to a file stream. (Or do something else)
        // Assume that filesreamsByName is a Dictionary<string, FileStream> of all the files
        // we are writing.
        filestreamsByName[name].Write(buffer, 0, bytes);
    }

Licensing
=========
Please see LICENSE
