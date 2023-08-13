namespace unity_yaml_merge;

/// <summary>
/// Difference between two files
/// </summary>
public class Diff
{
    public IReadOnlyList<Block> Blocks => _blocks;

    private List<Block> _blocks = new ();

    private BlockType? LastBlockType => _blocks.Any() ? _blocks.Last().Type : null;
    private Diff()
    {
    }

    /// <summary>
    /// Create a diff between two files
    /// </summary>
    public static Diff Make(string[] @base, string[] modified)
    {
        var diff = new Diff();

        if (!@base.Any() && !modified.Any())
            return diff;

        using var baseEnumerator = @base.GetTextEnumerator();
        using var modifiedEnumerator = modified.GetTextEnumerator();

        var baseEnded = false;
        var modifiedEnded = false;

        while (true)
        {
            if (!baseEnded)
                baseEnded = !baseEnumerator.MoveNext();

            if (!modifiedEnded)
                modifiedEnded = !modifiedEnumerator.MoveNext();

            if (baseEnded && modifiedEnded)
                break;

            BlockType? currentBlockType = null;
            if (baseEnded && !modifiedEnded)
                currentBlockType = BlockType.Added;
            else if (baseEnumerator.Current == modifiedEnumerator.Current)
                currentBlockType = BlockType.Unchanged;
            else if (!baseEnded && modifiedEnded)
                currentBlockType = BlockType.Removed;
            // else
            //     currentBlockType = BlockType.Changed;

            if (currentBlockType.HasValue && (!diff._blocks.Any() || currentBlockType != diff.LastBlockType))
            {
                var newBlock = new Block(currentBlockType.Value, baseEnumerator.LineNumber, modifiedEnumerator.LineNumber);
                diff._blocks.Add(newBlock);
            }

            if (!baseEnded && !modifiedEnded
                && baseEnumerator.Current == modifiedEnumerator.Current)
            {
                diff._blocks.Last().OldValue.Add(baseEnumerator.Current);
                diff._blocks.Last().NewValue.Add(modifiedEnumerator.Current);
            }

            if (baseEnded && !modifiedEnded)
                diff._blocks.Last().NewValue.Add(modifiedEnumerator.Current);

            if (!baseEnded && modifiedEnded)
                diff._blocks.Last().OldValue.Add(baseEnumerator.Current);
        }

        return diff;
    }
}


public enum BlockType
{
    Unchanged,
    Changed,
    Added,
    Removed,
}

public class Block
{
    public BlockType Type { get; private set; }

    public Block(BlockType currentBlockType, int startingOldLineNumber, int startingNewLineNumber)
    {
        Type = currentBlockType;
        StartingOldLineNumber = startingOldLineNumber;
        StartingNewLineNumber = startingNewLineNumber;
    }

    public List<string> OldValue { get; } = new ();
    public List<string> NewValue { get; } = new ();

    public int StartingOldLineNumber { get; private set; }
    public int StartingNewLineNumber { get; private set; }
}

public static class EnumerableExtensions
{
    public static TextEnumerator GetTextEnumerator(this IEnumerable<string> enumerator)
    {
        return new TextEnumerator(enumerator);
    }
}

public class TextEnumerator : IDisposable
{
    private readonly IEnumerator<string> _enumerator;
    private int _lineNumber = 0;
    private bool _hasNext = true;

    public TextEnumerator(IEnumerable<string> enumerable)
    {
        _enumerator = enumerable.GetEnumerator();
    }

    public bool MoveNext()
    {
        _lineNumber++;
        return _hasNext = _enumerator.MoveNext();
    }

    public string Current => _enumerator.Current;
    public int LineNumber => _lineNumber;

    public void Dispose()
    {
        _enumerator.Dispose();
    }
}
