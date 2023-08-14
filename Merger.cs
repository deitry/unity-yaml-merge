namespace unity_yaml_merge;

public static class Merger
{
    private static bool IsAssetEnd(IEnumerator<string> line) => line.Current is "  m_Metadata:" or "  references:";

    private static bool IsNewEntry(IEnumerator<string> line) => line.Current.StartsWith("  - m_Id");

    public static IEnumerable<string> MergeYamls(string ours, string @base, string theirs)
    {
        var oursYml = new UnityYamlIterator(ours);
        var baseYml = new UnityYamlIterator(@base);
        var theirsYml = new UnityYamlIterator(theirs);

        while (true)
        {
            if (oursYml.EndOfFile && baseYml.EndOfFile && theirsYml.EndOfFile)
                break;

            if (oursYml.CurrentBlock == theirsYml.CurrentBlock && oursYml.CurrentBlock == baseYml.CurrentBlock)
                yield return oursYml.CurrentBlock;

            // ours has new or changed entry
            if (oursYml.CurrentBlock != theirsYml.CurrentBlock && theirsYml.CurrentBlock == baseYml.CurrentBlock)
            {
                // get lines from ours until we find
            }
        }
    }
}
