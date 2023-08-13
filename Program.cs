using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;


// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

var ours = args[0];
var @base = args[1];
var theirs = args[2];
var outputPath = $"{ours}.converted";

var oursYml = File.ReadAllLines(ours);
var theirsYml = File.ReadAllLines(theirs);

var oursI = 0;
var theirsI = 0;

using var output = File.CreateText(outputPath);
var isNewEntry = (string line) => line.StartsWith("  - m_Id");

// first is for shared asset with keys, second for localizations
var isAssetEnd = (string line) => line == "  m_Metadata:" || line == "  references:";

while (true)
{
    var oursLine = oursYml[oursI];
    var theirsLine = theirsYml[theirsI];

    if (oursLine == theirsLine)
    {
        oursI++;
        theirsI++;

        output.WriteLine(oursLine);
        continue;
    }

    // if not equal

    // write all new keys from ours file
    if (isNewEntry(oursLine) && (isNewEntry(theirsLine) || isAssetEnd(theirsLine)))
    {
        while (isNewEntry(oursLine))
        {
            do
            {
                // write all lines until we find the next key
                output.WriteLine(oursLine);
                oursLine = oursYml[++oursI];
            } while (!isNewEntry(oursLine) && !isAssetEnd(oursLine));
        }

        continue;
    }

    if (isNewEntry(theirsLine) && isAssetEnd(oursLine))
    {
        // write all new keys from theirs file
        while (isNewEntry(theirsLine))
        {
            do
            {
                // write all lines until we find the next key
                output.WriteLine(theirsLine);
                theirsLine = theirsYml[++theirsI];
            } while (!isNewEntry(theirsLine) && !isAssetEnd(theirsLine));
        }

        continue;
    }

    throw new Exception($"Lines are not equal:\n{oursLine}\n{theirsLine}");
}
