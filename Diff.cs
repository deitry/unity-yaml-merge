﻿using System.Diagnostics;
using JetBrains.Annotations;
using Tests;
using YamlDotNet.Serialization;

namespace unity_yaml_merge;

/// <summary>
/// Difference between two files
/// </summary>
[PublicAPI]
public class Diff
{
    public IReadOnlyList<Block> Blocks => _blocks;

    private readonly List<Block> _blocks = new();

    public int OriginalLength => Blocks.Sum(b => b.OriginalLength);

    public int ModifiedLength => Blocks.Sum(b => b.ModifiedLength);

#if DEBUG
    public LocalizationAsset? OriginalYaml { get; private set; }
    public LocalizationAsset? ModifiedYaml { get; private set; }
#endif

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
        var deserializer = new DeserializerBuilder()
            .WithTagMapping(LocalizationAsset.Tag, typeof(LocalizationAsset))
            .Build();

        LocalizationAsset? originalYaml = null;
        LocalizationAsset? modifiedYaml = null;

#if DEBUG
        try
        {
            originalYaml = deserializer.Deserialize<LocalizationAsset>(string.Join('\n', @base).FromUnity());
            modifiedYaml = deserializer.Deserialize<LocalizationAsset>(string.Join('\n', modified).FromUnity());
        }
        catch (Exception)
        {
            // ignore
            // may fail for non-YAML test files
        }
#endif

        var diff = new Diff()
        {
#if DEBUG
            OriginalYaml = originalYaml,
            ModifiedYaml = modifiedYaml,
#endif
        };

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

        // -1 so it will be valid index of the last element
        var length = new Indices(@base.Length - 1, modified.Length - 1);

        while (current != Indices.End)
        {
            var span1 = current.Original != Indices.EndIndex ? @base.AsSpan(current.Original) : Span<string>.Empty;
            var span2 = current.Modified != Indices.EndIndex ? modified.AsSpan(current.Modified) : Span<string>.Empty;

            if (current.Original != Indices.EndIndex && current.Modified != Indices.EndIndex
                && @base[current.Original] == modified[current.Modified])
            {
                var next = current + GetNextDifference(span1, span2);

                diff.Add(new Block(BlockType.Unchanged, current.Fix(length))
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
                    diff.Add(new Block(BlockType.Added, current.Fix(length))
                    {
                        ModifiedLines = newValue,
                    });
                }
                else if (next.Modified == current.Modified)
                {
                    diff.Add(new Block(BlockType.Removed, current.Fix(length))
                    {
                        OriginalLines = oldValue,
                    });
                }
                else if (next == Indices.End && current.Original == Indices.EndIndex)
                {
                    diff.Add(new Block(BlockType.Removed, current.Fix(length))
                    {
                        OriginalLines = oldValue,
                    });
                }
                else if (next == Indices.End && current.Modified == Indices.EndIndex)
                {
                    diff.Add(new Block(BlockType.Added, current.Fix(length))
                    {
                        ModifiedLines = newValue,
                    });
                }
                else //?
                {
                    diff.Add(new Block(BlockType.Changed, current.Fix(length))
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
            var changedBlock = (Block?) null;

            while (i < diff.Blocks.Count)
            {
                var currentBlock = diff.Blocks[i];
                var nextBlock = (i < diff.Blocks.Count - 1) ? diff.Blocks[i + 1] : null;
                var oldValue = currentBlock.OriginalLines;
                var newValue = currentBlock.ModifiedLines;

                var suitableForAppendingToChanged = currentBlock.Type == BlockType.Changed;

                if (currentBlock.Type == BlockType.Added
                    && nextBlock?.Type == BlockType.Removed
                    && currentBlock.ModifiedLines.Count == 1)
                {
                    oldValue.AddRange(nextBlock.OriginalLines);
                    suitableForAppendingToChanged = true;
                }

                if (currentBlock.Type == BlockType.Removed
                    && nextBlock?.Type == BlockType.Added
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
                    changedBlock = null; // clear it
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

            // if (i == diff.Blocks.Count - 1)
            // {
            //     fixedBlocks.Add(diff.Blocks[i]);
            // }
        }

        var result = new Diff(fixedBlocks)
        {
#if DEBUG
            OriginalYaml = originalYaml,
            ModifiedYaml = modifiedYaml,
#endif
        };

        ValidateDiff(@base, result);

        return result;
    }

    /// <summary>
    /// Validate diff result
    /// </summary>
    private static void ValidateDiff(string[] @base, Diff result)
    {
        if (result.OriginalLength != @base.Length)
            throw new Exception("Original length mismatch");

        foreach (var block in result.Blocks)
        {
            if (block.End < block.Start)
                throw new Exception("Block end is less than start");
        }

        for (var i = 1; i < result.Blocks.Count; i++)
        {
            var previousBlock = result.Blocks[i - 1];
            var currentBlock = result.Blocks[i];

            // 1 is typical
            // 0 is when we have additions
            if (currentBlock.Start.Original - previousBlock.End.Original == 0)
            {
                if (previousBlock.Type != BlockType.Added && currentBlock.Type != BlockType.Added)
                    throw new Exception("Expected start of new block on the same line as previous block end");
            }
            else if (currentBlock.Start.Original - previousBlock.End.Original != 1)
            {
                throw new Exception("Missed lines in original file");
            }
        }
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
        {
            // TODO: here some Unity YAML bound logic must reside to correctly get proper diffs of specific fields

            if (i1.Original == i1.Modified)
                return i1;

            if (i2.Original == i2.Modified)
                return i2;

            return i1.Original < i2.Modified ? i1 : new Indices(i2.Modified, i2.Original);
        }

        if (r1)
            return i1;

        if (r2)
            return i2;

        return Indices.End;
    }

    private static bool NextEqualInternal(Span<string> f1, Span<string> f2, out Indices indices)
    {
        // when we start to parse a block of data, it is quite important to parse the block as a whole.
        // in .resx block is between <data></data>
        var blockStart = f1.Length > 0 && f2.Length > 0
            && f1[0].Trim().StartsWith("<data")
            && f2[0].Trim().StartsWith("<data");

        for (var i2 = 0; i2 < f2.Length; i2++)
        {
            for (var i1 = 0; i1 < f1.Length; i1++)
            {
                var l1 = f1[i1];
                var l2 = f2[i2];

                if (l1 == l2)
                {
                    if (blockStart && l1.Trim() == "</data>")
                        continue;

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
