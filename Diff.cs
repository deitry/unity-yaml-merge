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

    private Diff(List<Block> blocks)
    {
        _blocks = blocks;
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

        var current = Indices.Zero;

        while (current != Indices.End)
        {
            var span1 = current.I1 != Indices.EndIndex ? @base.AsSpan(current.I1) : Span<string>.Empty;
            var span2 = current.I2 != Indices.EndIndex ? modified.AsSpan(current.I2) : Span<string>.Empty;

            if (current.I1 != Indices.EndIndex && current.I2 != Indices.EndIndex
                && @base[current.I1] == modified[current.I2])
            {
                var next = current + GetNextDifference(span1, span2);

                diff.Add(new Block(BlockType.Unchanged, current)
                {
                    OldValue = @base.Take(new Range(current.I1, next.I1 != Indices.EndIndex ? next.I1 : @base.Length))
                        .ToList(),
                    NewValue = modified.Take(new Range(current.I2,
                        next.I2 != Indices.EndIndex ? next.I2 : modified.Length)).ToList(),
                });
                current = next;
            }
            else
            {
                var next = current + GetNextEqual(span1, span2);
                var oldValue = current.I1 != Indices.EndIndex
                    ? @base.Take(new Range(current.I1, next.I1 != Indices.EndIndex ? next.I1 : @base.Length)).ToList()
                    : new List<string>();

                var newValue = current.I2 != Indices.EndIndex
                    ? modified.Take(new Range(current.I2, next.I2 != Indices.EndIndex ? next.I2 : modified.Length))
                        .ToList()
                    : new List<string>();

                if (next.I1 == current.I1)
                {
                    diff.Add(new Block(BlockType.Added, current)
                    {
                        NewValue = newValue,
                    });
                }
                else if (next.I2 == current.I2)
                {
                    diff.Add(new Block(BlockType.Removed, current)
                    {
                        OldValue = oldValue,
                    });
                }
                else if (next == Indices.End && current.I1 == Indices.EndIndex)
                {
                    diff.Add(new Block(BlockType.Removed, current)
                    {
                        OldValue = oldValue,
                    });
                }
                else if (next == Indices.End && current.I2 == Indices.EndIndex)
                {
                    diff.Add(new Block(BlockType.Added, current)
                    {
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

                current = next;
            }
        }

        // replace removed+added as changed
        var fixedBlocks = new List<Block>(diff.Blocks.Count);
        for (var i = 0; i < diff.Blocks.Count; i++)
        {
            if (diff.Blocks[i].Type == BlockType.Unchanged)
            {
                fixedBlocks.Add(diff.Blocks[i]);
                continue;
            }

            var changedBlock = (Block?) null;
            while (true)
            {
                if (i >= diff.Blocks.Count - 1)
                    break;

                var currentBlock = diff.Blocks[i];
                var nextBlock = diff.Blocks[i + 1];
                var oldValue = currentBlock.OldValue;
                var newValue = currentBlock.NewValue;

                var suitableForAppendingToChanged = currentBlock.Type == BlockType.Changed;

                if (currentBlock.Type == BlockType.Added
                    && nextBlock.Type == BlockType.Removed
                    && currentBlock.NewValue.Count == 1)
                {
                    oldValue.AddRange(nextBlock.OldValue);
                    suitableForAppendingToChanged = true;
                }

                if (currentBlock.Type == BlockType.Removed
                    && nextBlock.Type == BlockType.Added
                    && currentBlock.OldValue.Count == 1)
                {
                    newValue.AddRange(nextBlock.NewValue);
                    suitableForAppendingToChanged = true;
                }

                // attempt to improve blocks type detection in hard cases
                // if (i < diff.Blocks.Count - 2)
                // {
                //     var nextNextBlock = diff.Blocks[i + 2];
                //     if (currentBlock.Type == BlockType.Removed
                //         && nextBlock.Type == BlockType.Unchanged
                //         && nextNextBlock.Type == BlockType.Added
                //         && nextNextBlock.NewValue.SequenceEqual(currentBlock.OldValue))
                //     {
                //         oldValue.AddRange(nextBlock.OldValue);
                //         newValue.AddRange(nextBlock.NewValue);
                //         newValue.AddRange(nextNextBlock.NewValue);
                //         suitableForAppendingToChanged = true;
                //         i++;
                //     }
                //
                //     if (currentBlock.Type == BlockType.Added
                //         && nextBlock.Type == BlockType.Unchanged
                //         && nextNextBlock.Type == BlockType.Removed
                //         && nextNextBlock.OldValue.SequenceEqual(currentBlock.NewValue))
                //     {
                //         oldValue.AddRange(nextBlock.OldValue);
                //         oldValue.AddRange(nextNextBlock.OldValue);
                //         newValue.AddRange(nextBlock.NewValue);
                //         suitableForAppendingToChanged = true;
                //         i++;
                //     }
                //     i++;
                // }

                if (suitableForAppendingToChanged)
                {
                    if (changedBlock == null)
                    {
                        changedBlock = new Block(BlockType.Changed, currentBlock.Start)
                        {
                            OldValue = currentBlock.OldValue,
                            NewValue = currentBlock.NewValue,
                        };
                        fixedBlocks.Add(changedBlock);
                    }
                    else
                    {
                        changedBlock.OldValue.AddRange(currentBlock.OldValue);
                        changedBlock.NewValue.AddRange(currentBlock.NewValue);
                    }
                }
                else
                {
                    fixedBlocks.Add(currentBlock);
                }

                i++;
            }

            // while (i < diff.Blocks.Count - 1
            //        && ((diff.Blocks[i].Type == BlockType.Added && diff.Blocks[i + 1].Type == BlockType.Removed)
            //             || (diff.Blocks[i].Type == BlockType.Removed && diff.Blocks[i + 1].Type == BlockType.Unchanged && diff.Blocks[i + 1].NewValue.Count > 1)
            //             || (diff.Blocks[i].Type == BlockType.Added && diff.Blocks[i + 1].Type == BlockType.Unchanged && diff.Blocks[i + 1].NewValue.Count > 1)
            //             || (diff.Blocks[i].Type == BlockType.Changed)))
            // {
            //     var currentBlock = diff.Blocks[i];
            //
            //     changedBlock ??= new Block(BlockType.Changed, currentBlock.Start);
            //     i++;
            // }

            if (i == diff.Blocks.Count - 1)
            {
                fixedBlocks.Add(diff.Blocks[i]);
            }
        }

        return new Diff(fixedBlocks);
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

        var r1 = NextEqualInternal(f1, f2, out var i1);
        var r2 = NextEqualInternal(f2, f1, out var i2);

        if (r1 && r2)
            return i1.I1 < i2.I2 ? i1 : new Indices(i2.I2, i2.I1);

        if (r1)
            return i1;

        if (r2)
            return i2;

        return Indices.End;
    }

    private static bool NextEqualInternal(Span<string> f1, Span<string> f2, out Indices indices)
    {
        for (var i2 = 0; i2 < f2.Length; i2++)
        {
            for (var i1 = 0; i1 < f1.Length; i1++)
            {
                var l1 = f1[i1];
                var l2 = f2[i2];
                if (l1 == l2)
                {
                    indices = new(i1, i2);
                    return true;
                }
            }
        }

        indices = Indices.End;
        return false;
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

    public Block(BlockType blockType, Indices start)
    {
        Type = blockType;
        Start = start;
    }

    public List<string> OldValue { get; init; } = new();
    public List<string> NewValue { get; init; } = new();

    public Indices Start { get; }

    public override string ToString()
    {
        var value = Type switch
        {
            BlockType.Unchanged => string.Join('\n', NewValue),
            BlockType.Changed => $"{string.Join('\n', OldValue)} > {string.Join('\n', NewValue)}",
            BlockType.Added => string.Join('\n', NewValue),
            BlockType.Removed => string.Join('\n', OldValue),
            _ => throw new ArgumentOutOfRangeException(),
        };

        return $"{Type}: {value}";
    }
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
