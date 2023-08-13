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

        using var oursEnumerator = oursDiff.Blocks.GetEnumerator();
        using var theirsEnumerator = theirsDiff.Blocks.GetEnumerator();

        // start enumerators
        var oursHasNext = oursEnumerator.MoveNext();
        var theirsHasNext = theirsEnumerator.MoveNext();

        for (var i = 0; i < @base.Length; i++)
        {
            var baseLine = @base[i];

            if (oursEnumerator.Current.End.Original < i)
            {
                if (!oursHasNext)
                    throw new Exception("Expected more blocks in ours diff");

                oursHasNext = oursEnumerator.MoveNext();
            }

            if (theirsEnumerator.Current.End.Original < i)
            {
                if (!theirsHasNext)
                    throw new Exception("Expected more blocks in theirs diff");

                theirsHasNext = theirsEnumerator.MoveNext();
            }

            var oursBlock = oursEnumerator.Current;
            var theirsBlock = theirsEnumerator.Current;

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

                // -1 so it will hit same line again
                i = theirsBlock.End.Original - 1;

                // force move to next block
                theirsEnumerator.MoveNext();
            }
            else if (oursChange == BlockType.Added && theirsChange == BlockType.Unchanged)
            {
                if (i != oursBlock.Start.Original)
                    throw new InvalidOperationException("Expected start of block");

                // if unchanged in one and has additions in second, merge second first
                merged.AddRange(oursBlock.ModifiedLines);

                // -1 so it will hit same line again
                i = oursBlock.End.Original - 1;

                // force move to next block
                oursEnumerator.MoveNext();
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
        // they are tied to the last index of the original file
        var oursTrailingBlocks = oursDiff.Blocks.Where(b => b.Start.Original >= @base.Length - 1).ToList();
        var theirsTrailingBlocks = theirsDiff.Blocks.Where(b => b.Start.Original >= @base.Length - 1).ToList();

        if (oursTrailingBlocks.Any())
        {
            if (oursTrailingBlocks.Count > 1)
                throw new Exception("Expected only one trailing block");

            merged.AddRange(oursTrailingBlocks.First().ModifiedLines);
        }

        if (theirsTrailingBlocks.Any())
        {
            if (theirsTrailingBlocks.Count > 1)
                throw new Exception("Expected only one trailing block");

            merged.AddRange(theirsTrailingBlocks.First().ModifiedLines);

        }

        return merged;
    }
}
