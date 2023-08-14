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

        for (var i = 0; i < @base.Length || oursHasNext || theirsHasNext; i++)
        // for (var i = 0; i < @base.Length || oursHasNext || theirsHasNext; i = Math.Min(i + 1, @base.Length - 1))
        {
            if (oursEnumerator.Current?.End.Original < i)
            {
                if (!oursHasNext)
                    throw new Exception("Expected more blocks in ours diff");

                oursHasNext = oursEnumerator.MoveNext();
            }

            if (theirsEnumerator.Current?.End.Original < i)
            {
                if (!theirsHasNext)
                    throw new Exception("Expected more blocks in theirs diff");

                theirsHasNext = theirsEnumerator.MoveNext();
            }

            if (!oursHasNext && !theirsHasNext)
                break;

            // fix i for trailing blocks
            i = Math.Min(i, @base.Length - 1);

            var baseLine = @base[i];
            var oursBlock = oursHasNext ? oursEnumerator.Current : null;
            var theirsBlock = theirsHasNext ? theirsEnumerator.Current : null;

            var oursChange = oursBlock?.Type;
            var theirsChange = theirsBlock?.Type;

            if (oursChange == BlockType.Unchanged && theirsChange == BlockType.Unchanged)
            {
                // if unchanged in both blocks
                merged.Add(baseLine);
            }
            else if (oursChange == BlockType.Changed && theirsChange == BlockType.Changed
                && oursBlock!.Start.Original == theirsBlock!.Start.Original
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
            else if ((oursChange == BlockType.Unchanged || oursBlock == null) && theirsChange == BlockType.Added)
            {
                if (i != theirsBlock!.Start.Original)
                    throw new InvalidOperationException("Expected start of block");

                // if unchanged in one and has additions in second, merge second first
                merged.AddRange(theirsBlock.ModifiedLines);

                // -1 so it will hit same line again
                i = theirsBlock.End.Original - 1;

                // force move to next block
                theirsHasNext = theirsEnumerator.MoveNext();
            }
            else if (oursChange == BlockType.Added && (theirsChange == BlockType.Unchanged || theirsBlock == null))
            {
                if (i != oursBlock!.Start.Original)
                    throw new InvalidOperationException("Expected start of block");

                // if unchanged in one and has additions in second, merge second first
                merged.AddRange(oursBlock.ModifiedLines);

                // -1 so it will hit same line again
                i = oursBlock.End.Original - 1;

                // force move to next block
                oursHasNext = oursEnumerator.MoveNext();
            }
            else if (oursChange == BlockType.Unchanged && theirsChange == BlockType.Changed
                     && theirsBlock!.OriginalLength <= oursBlock!.OriginalLength)
            {
                if (i != theirsBlock.Start.Original)
                    throw new InvalidOperationException("Expected start of block");

                // if one is changed, apply it
                merged.AddRange(theirsBlock.ModifiedLines);

                i = theirsBlock.End.Original;
            }
            else if (oursChange == BlockType.Changed && theirsChange == BlockType.Unchanged
                     && oursBlock!.OriginalLength <= theirsBlock!.OriginalLength)
            {
                if (i != oursBlock.Start.Original)
                    throw new InvalidOperationException("Expected start of block");

                // if one is changed, apply it
                merged.AddRange(oursBlock.ModifiedLines);

                i = oursBlock.End.Original;
            }
            else if (oursBlock != null && theirsBlock != null && oursChange == theirsChange
                     && oursBlock.OriginalLines.SequenceEqual(theirsBlock.OriginalLines)
                     && oursBlock.ModifiedLines.SequenceEqual(theirsBlock.ModifiedLines))
            {
                // if both has same changes
                switch (oursChange)
                {
                    case BlockType.Unchanged:
                        merged.AddRange(oursBlock.ModifiedLines);
                        i = oursBlock.End.Original;
                        break;
                    case BlockType.Changed:
                        merged.AddRange(oursBlock.ModifiedLines);
                        i = oursBlock.End.Original;
                        break;
                    case BlockType.Added:
                        merged.AddRange(oursBlock.ModifiedLines);

                        Debug.Assert(i == oursBlock.End.Original);
                        i = oursBlock.End.Original - 1;

                        // force move to next block
                        oursHasNext = oursEnumerator.MoveNext();
                        theirsHasNext = theirsEnumerator.MoveNext();
                        break;
                    case BlockType.Removed:
                        // skip
                        i = oursBlock.End.Original;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                // output as conflict
                throw new NotImplementedException();
            }
        }

        Debug.Assert(oursHasNext == false);
        Debug.Assert(theirsHasNext == false);

        return merged;
    }
}
