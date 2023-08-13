namespace unity_yaml_merge;

public static class Merger
{
    private static bool IsAssetEnd(IEnumerator<string> line) => line.Current is "  m_Metadata:" or "  references:";

    private static bool IsNewEntry(IEnumerator<string> line) => line.Current.StartsWith("  - m_Id");

    public static IEnumerable<string> MergeYamls(string oursPath, string basePath, string theirsPath)
        => MergeYamls(File.ReadAllLines(oursPath), File.ReadAllLines(basePath), File.ReadAllLines(theirsPath));


    public static IEnumerable<string> MergeYamls(string[] ours, string[] @base, string[] theirs)
    {
        var oursDiff = Diff.Make(@base, ours);
        var theirsDiff = Diff.Make(@base, theirs);

        // iterate over diff blocks
        var oursI = 0;
        var oursLine = 0;
        var theirsI = 0;
        var theirsLine = 0;

        var merged = new List<string>();

        for (var i = 0; i < @base.Length; i++)
        {
            var baseLine = @base[i];

            var oursChange = oursDiff.CheckLine(i);
            var theirsChange = theirsDiff.CheckLine(i);

            if (oursChange == BlockType.Unchanged && theirsChange == BlockType.Unchanged)
            {
                // if unchanged in both blocks
                merged.Add(baseLine);
            }
            else if (oursChange == BlockType.Unchanged && theirsChange == BlockType.Removed)
            {
                // if unchanged in one and removed in second
                // skip
            }
            else if (oursChange == BlockType.Removed && theirsChange == BlockType.Unchanged)
            {
                // if unchanged in one and removed in second
                // skip
            }
            else if (oursChange == BlockType.Unchanged && theirsChange == BlockType.Added)
            {
                // if unchanged in one and has additions in second, merge second first
                merged.AddRange(theirsDiff.GetBlockAt(i).ModifiedLines);
            }
            else if (oursChange == BlockType.Added && theirsChange == BlockType.Unchanged)
            {
                // if unchanged in one and has additions in second, merge second first
                merged.AddRange(oursDiff.GetBlockAt(i).ModifiedLines);
            }
            else if (oursChange == BlockType.Unchanged && theirsChange == BlockType.Changed)
            {
                // if changed,
            }
        }

        return merged;
    }
}
