namespace unity_yaml_merge;

public static class Merger
{
    private static bool IsAssetEnd(IEnumerator<string> line) => line.Current is "  m_Metadata:" or "  references:";

    private static bool IsNewEntry(IEnumerator<string> line) => line.Current.StartsWith("  - m_Id");

    public static List<string> MergeYamls(string ours, string @base, string theirs)
    {
        var oursYml = File.ReadAllLines(ours);
        var baseYml = File.ReadAllLines(@base);
        var theirsYml = File.ReadAllLines(theirs);

        var output = new List<string>();

        using var oursEnumerator = oursYml.Cast<string>().GetEnumerator();
        using var baseEnumerator = baseYml.Cast<string>().GetEnumerator();
        using var theirsEnumerator = theirsYml.Cast<string>().GetEnumerator();

        while (true)
        {
            var oursHasNext = oursEnumerator.MoveNext();
            var baseHasNext = baseEnumerator.MoveNext();
            var theirsHasNext = theirsEnumerator.MoveNext();

            if (!oursHasNext && !baseHasNext && !theirsHasNext)
            {
                // expecting that we end interation at the same time
                break;
            }

            if (!oursHasNext)
                throw new Exception("Unexpecting end of ours file");

            if (!baseHasNext)
                throw new Exception("Unexpecting end of base file");

            if (!theirsHasNext)
                throw new Exception("Unexpecting end of theirs file");


            if (oursEnumerator.Current == theirsEnumerator.Current)
            {
                // expecting that baseLine also the same
                if (oursEnumerator.Current != baseEnumerator.Current)
                    throw new Exception("Unexpected difference between ours and base");

                output.Add(oursEnumerator.Current);
                continue;
            }

            // if ours has new entries
            if (IsNewEntry(oursEnumerator) && (IsNewEntry(theirsEnumerator) || IsAssetEnd(theirsEnumerator)))
            {
                while (IsNewEntry(oursEnumerator))
                {
                    do
                    {
                        // write all lines until we find the next key
                        output.Add(oursEnumerator.Current);
                        oursEnumerator.MoveNext();
                    } while (!IsNewEntry(oursEnumerator) && !IsAssetEnd(oursEnumerator));
                }

                continue;
            }

            // if theirs has new entries. Ours should be in the end atm
            if (IsNewEntry(theirsEnumerator) && IsAssetEnd(oursEnumerator))
            {
                // write all new keys from theirs file
                while (IsNewEntry(theirsEnumerator))
                {
                    do
                    {
                        // write all lines until we find the next key
                        output.Add(theirsEnumerator.Current);
                        theirsEnumerator.MoveNext();
                    } while (!IsNewEntry(theirsEnumerator) && !IsNewEntry(theirsEnumerator));
                }

                continue;
            }

            // // here we go if line is not a new key and not end of asset
            // // - if ours is changed, write ours
            // if (oursEnumerator.Current.StartsWith("    m_Localized:") && theirsEnumerator.Current == baseEnumerator.Current)
            // {
            //     // write new value from ours
            //     output.Add(oursEnumerator.Current);
            //
            //
            //     continue;
            // }
            //
            // // - if theirs is changed, write theirs
            // if (theirsLine.StartsWith("    m_Localized:") && oursLine == baseLine)
            // {
            //     output.WriteLine(theirsLine);
            //     oursI++;
            //     baseI++;
            //     theirsI++;
            //
            //     continue;
            // }
            //
            // // print as conflict
            // output.WriteLine($"<<<<<<< ours");
            // output.WriteLine(oursLine);
            // output.WriteLine($"=======");
            // output.WriteLine(baseLine);
            // output.WriteLine($">>>>>>> theirs");
            //
            // oursI++;
            // baseI++;
            // theirsI++;
        }

        return output;
    }
}
