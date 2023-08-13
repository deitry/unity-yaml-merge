using Tests;

namespace unity_yaml_merge;

/// <summary>
/// Difference between two files
/// </summary>
public class Diff
{
    public IReadOnlyList<Block> Blocks => _blocks;

    private readonly List<Block> _blocks = new();

    private BlockType? LastBlockType => _blocks.Any() ? _blocks.Last().Type : null;

    private Diff()
    {
    }

    private void Add(Block block)
    {
        _blocks.Add(block);
    }

    public static Diff Make(string @base, string modified) =>
        Make(@base.CharsToStringArray(), modified.CharsToStringArray());

    /// <summary>
    /// Create a diff between two files
    /// </summary>
    public static Diff Make(string[] @base, string[] modified)
    {
        var diff = new Diff();

        if (!@base.Any() && !modified.Any())
            return diff;

        if (!@base.Any())
        {
            diff.Add(new Block(BlockType.Added, Indices.Zero)
            {
                OldValue = @base.ToList(),
                NewValue = modified.ToList(),
            });
            return diff;
        }

        if (!modified.Any())
        {
            diff.Add(new Block(BlockType.Removed, Indices.Zero)
            {
                OldValue = @base.ToList(),
                NewValue = modified.ToList(),
            });
            return diff;
        }

        // using var baseEnumerator = @base.GetTextEnumerator();
        // using var modifiedEnumerator = modified.GetTextEnumerator();

        var current = Indices.Zero;
        // var end = new Indices(@base.Length, modified.Length);

        while (current != Indices.End)
        {
            var span1 = current.I1 != Indices.EndIndex ? @base.AsSpan(current.I1) : Span<string>.Empty;
            var span2 = current.I2 != Indices.EndIndex ? modified.AsSpan(current.I2) : Span<string>.Empty;

            if (current.I1 != Indices.EndIndex && current.I2 != Indices.EndIndex
                && @base[current.I1] == modified[current.I2])
            {
                var next = GetNextDifference(span1, span2);
                diff.Add(new Block(BlockType.Unchanged, current)
                {
                    OldValue = @base.Take(new Range(current.I1,
                        next.I1 != Indices.EndIndex ? next.I1 : @base.Length - current.I1)).ToList(),
                    NewValue = modified.Take(new Range(current.I2,
                        next.I2 != Indices.EndIndex ? next.I2 : modified.Length - current.I2)).ToList(),
                });
                current += next;
            }
            else
            {
                var next = GetNextEqual(span1, span2);
                var oldValue = current.I1 != Indices.EndIndex
                    ? @base.Take(new Range(current.I1,
                        next.I1 != Indices.EndIndex ? next.I1 : @base.Length)).ToList()
                    : new List<string>();

                var newValue = current.I2 != Indices.EndIndex
                    ? modified.Take(new Range(current.I2,
                        next.I2 != Indices.EndIndex ? next.I2 : modified.Length)).ToList()
                    : new List<string>();

                if (next.I1 == current.I1)
                {
                    diff.Add(new Block(BlockType.Added, current)
                    {
                        OldValue = oldValue,
                        NewValue = newValue,
                    });
                }
                else if (next.I2 == current.I2)
                {
                    diff.Add(new Block(BlockType.Removed, current)
                    {
                        OldValue = oldValue,
                        NewValue = newValue,
                    });
                }
                else if (next == Indices.End && current.I1 == Indices.EndIndex)
                {
                    diff.Add(new Block(BlockType.Removed, current)
                    {
                        OldValue = oldValue,
                        NewValue = newValue,
                    });
                }
                else if (next == Indices.End && current.I2 == Indices.EndIndex)
                {
                    diff.Add(new Block(BlockType.Added, current)
                    {
                        OldValue = oldValue,
                        NewValue = newValue,
                    });
                }
                else //?
                {
                    diff.Add(new Block(BlockType.Changed, current)
                    {
                        OldValue = oldValue,
                        NewValue = newValue,
                    });
                }

                current += next;
            }
        }

        // while (true)
        // {
        //     if (!baseEnded)
        //         baseEnded = !baseEnumerator.MoveNext();
        //
        //     if (!modifiedEnded)
        //         modifiedEnded = !modifiedEnumerator.MoveNext();
        //
        //     if (baseEnded && modifiedEnded)
        //         break;
        //
        //     BlockType? currentBlockType = null;
        //     if (baseEnded && !modifiedEnded)
        //         currentBlockType = BlockType.Added;
        //     else if (baseEnumerator.Current == modifiedEnumerator.Current)
        //         currentBlockType = BlockType.Unchanged;
        //     else if (!baseEnded && modifiedEnded)
        //         currentBlockType = BlockType.Removed;
        //     // else
        //     //     currentBlockType = BlockType.Changed;
        //
        //     if (currentBlockType.HasValue && (!diff._blocks.Any() || currentBlockType != diff.LastBlockType))
        //     {
        //         var newBlock = new Block(currentBlockType.Value, baseEnumerator.LineNumber, modifiedEnumerator.LineNumber);
        //         diff._blocks.Add(newBlock);
        //     }
        //
        //     if (!baseEnded && !modifiedEnded
        //         && baseEnumerator.Current == modifiedEnumerator.Current)
        //     {
        //         diff._blocks.Last().OldValue.Add(baseEnumerator.Current);
        //         diff._blocks.Last().NewValue.Add(modifiedEnumerator.Current);
        //     }
        //
        //     if (baseEnded && !modifiedEnded)
        //         diff._blocks.Last().NewValue.Add(modifiedEnumerator.Current);
        //
        //     if (!baseEnded && modifiedEnded)
        //         diff._blocks.Last().OldValue.Add(baseEnumerator.Current);
        // }

        return diff;
    }


    public static Indices GetNextDifference(Span<string> f1, Span<string> f2)
    {
        if (f1.IsEmpty && f2.IsEmpty)
        {
            return Indices.End;
        }

        var i1 = 0;
        while (i1 < f1.Length && i1 < f2.Length)
        {
            if (f1[i1] != f2[i1])
            {
                return new(i1, i1);
            }

            i1++;
        }

        if (i1 < f1.Length)
            return new(i1, Indices.EndIndex);

        if (i1 < f2.Length)
            return new(Indices.EndIndex, i1);

        return Indices.End;
    }

    public static Indices GetNextEqual(Span<string> f1, Span<string> f2)
    {
        if (f1.IsEmpty && f2.IsEmpty)
        {
            return Indices.End;
        }

        for (var i1 = 0; i1 < f1.Length; i1++)
        {
            for (var i2 = 0; i2 < f2.Length; i2++)
            {
                var l1 = f1[i1];
                var l2 = f2[i2];
                if (l1 == l2)
                {
                    return new(i1, i2);
                }
            }
        }

        return Indices.End;
    }
}

public record Indices(int I1, int I2)
{
    public const int EndIndex = -1;

    public static readonly Indices Zero = new(0, 0);
    public static readonly Indices End = new(EndIndex, EndIndex);

    public static Indices operator +(Indices a, Indices b)
    {
        var i1 = a.I1 == EndIndex || b.I1 == EndIndex ? EndIndex : a.I1 + b.I1;
        var i2 = a.I2 == EndIndex || b.I2 == EndIndex ? EndIndex : a.I2 + b.I2;

        return new Indices(i1, i2);
    }

    public static bool operator <(Indices a, Indices b)
    {
        if (a.I1 < b.I1)
            return true;

        if (a.I1 == b.I1 && a.I2 < b.I2)
            return true;

        return false;
    }

    public static bool operator >(Indices a, Indices b) => !(a < b);
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

    public Block(BlockType currentBlockType, Indices start)
    {
        Type = currentBlockType;
        Start = start;
    }

    public List<string> OldValue { get; init; } = new();
    public List<string> NewValue { get; init; } = new();

    public Indices Start { get; }
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
