namespace unity_yaml_merge;

public static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        var ours = args[0];
        var @base = args[1];
        var theirs = args[2];

        var merged = Merger.MergeYamls(ours, @base, theirs);

        using var output = File.CreateText($"{ours}.converted");
        foreach (var line in merged)
        {
            output.WriteLine(line);
        }
    }
}
