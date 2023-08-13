using System.Diagnostics;

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

        Debug.Assert(oursDiff.OriginalLength == theirsDiff.OriginalLength);

        // iterate over diff blocks
        var oursI = 0;
        var oursLine = 0;
        var theirsI = 0;
        var theirsLine = 0;

        var merged = new List<string>();

        for (var i = 0; i < @base.Length; i++)
        {
            var baseLine = @base[i];

            // we can have more than one block at the same original line because of additions
            var oursBlock = oursDiff.GetBlockAt(i);
            var theirsBlock = theirsDiff.GetBlockAt(i);

            var oursChange = oursBlock.Type;
            var theirsChange = theirsBlock.Type;

            if (oursChange == BlockType.Unchanged && theirsChange == BlockType.Unchanged)
            {
                // if unchanged in both blocks
                merged.Add(baseLine);
            }
            else if (oursChange == BlockType.Changed && theirsChange == BlockType.Changed
                && oursBlock.Start.Original == theirsBlock.Start.Original
                && oursBlock.OriginalLength == theirsBlock.OriginalLength
                && oursBlock.ModifiedLines.SequenceEqual(theirsBlock.ModifiedLines))
            {
                // if both has same changes
                // TODO: improve in the cases when block are not exactly the same

                merged.AddRange(oursBlock.ModifiedLines);

                i = oursBlock.End.Original;
            }
            else if ((oursChange == BlockType.Unchanged && theirsChange == BlockType.Removed)
                     || (oursChange == BlockType.Removed && theirsChange == BlockType.Unchanged))
            {
                // if unchanged in one and removed in second
                // skip
            }
            else if (oursChange == BlockType.Unchanged && theirsChange == BlockType.Added)
            {
                if (i != theirsBlock.Start.Original)
                    throw new InvalidOperationException("Expected start of block");

                // if unchanged in one and has additions in second, merge second first
                merged.AddRange(theirsBlock.ModifiedLines);

                i = theirsBlock.End.Original - 1;
            }
            else if (oursChange == BlockType.Added && theirsChange == BlockType.Unchanged)
            {
                if (i != oursBlock.Start.Original)
                    throw new InvalidOperationException("Expected start of block");

                // if unchanged in one and has additions in second, merge second first
                merged.AddRange(oursBlock.ModifiedLines);

                i = oursBlock.End.Original;
            }
            else if (oursChange == BlockType.Unchanged && theirsChange == BlockType.Changed
                     && theirsBlock.OriginalLength <= oursBlock.OriginalLength)
            {
                if (i != theirsBlock.Start.Original)
                    throw new InvalidOperationException("Expected start of block");

                // if one is changed, apply it
                merged.AddRange(theirsBlock.ModifiedLines);

                i = theirsBlock.End.Original;
            }
            else if (oursChange == BlockType.Changed && theirsChange == BlockType.Unchanged
                     && oursBlock.OriginalLength <= theirsBlock.OriginalLength)
            {
                if (i != oursBlock.Start.Original)
                    throw new InvalidOperationException("Expected start of block");

                // if one is changed, apply it
                merged.AddRange(oursBlock.ModifiedLines);

                i = oursBlock.End.Original;
            }
            else
            {
                // output as conflict
            }
        }

        // special handling for blocks beyond the end of the original file
        var oursTrailingBlocks = oursDiff.Blocks.Where(b => b.Start.Original >= @base.Length).ToList();
        var theirsTrailingBlocks = oursDiff.Blocks.Where(b => b.Start.Original >= @base.Length).ToList();

        if (oursTrailingBlocks.Any())
        {

        }

        if (theirsTrailingBlocks.Any())
        {

        }

        return merged;
    }
}
