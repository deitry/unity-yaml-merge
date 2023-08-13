namespace unity_yaml_merge;

public record Indices(int Original, int Modified)
{
    public const int EndIndex = -1;

    public static readonly Indices Zero = new(0, 0);
    public static readonly Indices End = new(EndIndex, EndIndex);

    public static Indices operator +(Indices a, Indices b)
    {
        var i1 = a.Original == EndIndex || b.Original == EndIndex ? EndIndex : a.Original + b.Original;
        var i2 = a.Modified == EndIndex || b.Modified == EndIndex ? EndIndex : a.Modified + b.Modified;

        return new Indices(i1, i2);
    }

    public static bool operator <(Indices a, Indices b)
    {
        if (a.Original < b.Original)
            return true;

        if (a.Original == b.Original && a.Modified < b.Modified)
            return true;

        return false;
    }

    public static bool operator >(Indices a, Indices b) => !(a < b);
}