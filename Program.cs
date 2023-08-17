namespace unity_yaml_merge;

public static class Program
{
    public static void Main(string[] args)
    {
        // while (!System.Diagnostics.Debugger.IsAttached)
        //     System.Threading.Thread.Sleep(500);

        var ours = args[0];
        var @base = args[1];
        var theirs = args[2];

        var merged = Merger.MergeYamls(ours, @base, theirs);

        // we should overwrite ours file
        // https://git-scm.com/docs/gitattributes#_defining_a_custom_merge_driver
        using var output = File.CreateText(ours);
        foreach (var line in merged)
        {
            output.Write(line + '\n');
        }
    }
}
