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
                OriginalLines = @base.ToList(),
                ModifiedLines = modified.ToList(),
            });
            return diff;
        }

        if (!modified.Any())
        {
            diff.Add(new Block(BlockType.Removed, Indices.Zero)
            {
                OriginalLines = @base.ToList(),
                ModifiedLines = modified.ToList(),
            });
            return diff;
        }

        var current = Indices.Zero;

        while (current != Indices.End)
        {
            var span1 = current.Original != Indices.EndIndex ? @base.AsSpan(current.Original) : Span<string>.Empty;
            var span2 = current.Modified != Indices.EndIndex ? modified.AsSpan(current.Modified) : Span<string>.Empty;

            if (current.Original != Indices.EndIndex && current.Modified != Indices.EndIndex
                && @base[current.Original] == modified[current.Modified])
            {
                var next = current + GetNextDifference(span1, span2);

                diff.Add(new Block(BlockType.Unchanged, current)
                {
                    OriginalLines = @base.Take(new Range(current.Original, next.Original != Indices.EndIndex ? next.Original : @base.Length))
                        .ToList(),
                    ModifiedLines = modified.Take(new Range(current.Modified,
                        next.Modified != Indices.EndIndex ? next.Modified : modified.Length)).ToList(),
                });
                current = next;
            }
            else
            {
                var next = current + GetNextEqual(span1, span2);
                var oldValue = current.Original != Indices.EndIndex
                    ? @base.Take(new Range(current.Original, next.Original != Indices.EndIndex ? next.Original : @base.Length)).ToList()
                    : new List<string>();

                var newValue = current.Modified != Indices.EndIndex
                    ? modified.Take(new Range(current.Modified, next.Modified != Indices.EndIndex ? next.Modified : modified.Length))
                        .ToList()
                    : new List<string>();

                if (next.Original == current.Original)
                {
                    diff.Add(new Block(BlockType.Added, current)
                    {
                        ModifiedLines = newValue,
                    });
                }
                else if (next.Modified == current.Modified)
                {
                    diff.Add(new Block(BlockType.Removed, current)
                    {
                        OriginalLines = oldValue,
                    });
                }
                else if (next == Indices.End && current.Original == Indices.EndIndex)
                {
                    diff.Add(new Block(BlockType.Removed, current)
                    {
                        OriginalLines = oldValue,
                    });
                }
                else if (next == Indices.End && current.Modified == Indices.EndIndex)
                {
                    diff.Add(new Block(BlockType.Added, current)
                    {
                        ModifiedLines = newValue,
                    });
                }
                else //?
                {
                    diff.Add(new Block(BlockType.Changed, current)
                    {
                        OriginalLines = oldValue,
                        ModifiedLines = newValue,
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
                var oldValue = currentBlock.OriginalLines;
                var newValue = currentBlock.ModifiedLines;

                var suitableForAppendingToChanged = currentBlock.Type == BlockType.Changed;

                if (currentBlock.Type == BlockType.Added
                    && nextBlock.Type == BlockType.Removed
                    && currentBlock.ModifiedLines.Count == 1)
                {
                    oldValue.AddRange(nextBlock.OriginalLines);
                    suitableForAppendingToChanged = true;
                }

                if (currentBlock.Type == BlockType.Removed
                    && nextBlock.Type == BlockType.Added
                    && currentBlock.OriginalLines.Count == 1)
                {
                    newValue.AddRange(nextBlock.ModifiedLines);
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
                            OriginalLines = currentBlock.OriginalLines,
                            ModifiedLines = currentBlock.ModifiedLines,
                        };
                        fixedBlocks.Add(changedBlock);
                    }
                    else
                    {
                        changedBlock.OriginalLines.AddRange(currentBlock.OriginalLines);
                        changedBlock.ModifiedLines.AddRange(currentBlock.ModifiedLines);
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
            return i1.Original < i2.Modified ? i1 : new Indices(i2.Modified, i2.Original);

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

    /// <summary>
    /// Check if original line was modified
    /// </summary>
    public BlockType CheckLine(int i)
    {
        return GetBlockAt(i).Type;
    }

    /// <summary>
    /// Check if original line was modified
    /// </summary>
    public Block GetBlockAt(int i)
    {
        foreach (var block in Blocks)
        {
            if (block.Start.Original <= i && block.End.Original >= i)
                return block;
        }

        throw new ArgumentOutOfRangeException(nameof(i), "Line number is outside any block");
    }
}

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

    public List<string> OriginalLines { get; init; } = new();
    public List<string> ModifiedLines { get; init; } = new();

    public Indices Start { get; }
    public Indices End => new (Start.Original + OriginalLines.Count, Start.Modified + ModifiedLines.Count);

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
