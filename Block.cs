namespace unity_yaml_merge;

public class Block
{
    public BlockType Type { get; }

    public Block(BlockType blockType, Indices start)
    {
        Type = blockType;
        Start = start;
    }

    public List<string> OriginalLines { get; init; } = new();
    public List<string> ModifiedLines { get; init; } = new();

    public Indices Start { get; }
    public Indices End => new (Start.Original + OriginalLength, Start.Modified + ModifiedLength);

    public int OriginalLength => OriginalLines.Count;
    public int ModifiedLength => OriginalLines.Count;

    public override string ToString()
    {
        var value = Type switch
        {
            BlockType.Unchanged => string.Join('\n', ModifiedLines),
            BlockType.Changed => $"{string.Join('\n', OriginalLines)} > {string.Join('\n', ModifiedLines)}",
            BlockType.Added => string.Join('\n', ModifiedLines),
            BlockType.Removed => string.Join('\n', OriginalLines),
            _ => throw new ArgumentOutOfRangeException(),
        };

        return $"{Type}: {value}";
    }
}