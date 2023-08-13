using unity_yaml_merge;

namespace Tests;

public class DiffTests
{
    private static readonly string[] Empty = Array.Empty<string>();

    [Test]
    public void Test_01_Empty()
    {
        var diff = Diff.Make(Empty, Empty);

        Assert.That(diff.Blocks.Count, Is.EqualTo(0));
    }

    [Test]
    public void Test_02_EqualSingleLine()
    {
        var diff = Diff.Make("A", "A");

        Assert.That(diff.Blocks.Count, Is.EqualTo(1));
        Assert.That(diff.Blocks[0].Type, Is.EqualTo(BlockType.Unchanged));
        Assert.That(diff.Blocks[0].OldValue, Is.EqualTo("A".CharsToStringArray()));
        Assert.That(diff.Blocks[0].NewValue, Is.EqualTo("A".CharsToStringArray()));
    }

    [Test]
    public void Test_02_AddedToEmpty()
    {
        var diff = Diff.Make("", "A");

        Assert.That(diff.Blocks.Count, Is.EqualTo(1));
        Assert.That(diff.Blocks[0].Type, Is.EqualTo(BlockType.Added));
        Assert.That(diff.Blocks[0].OldValue, Is.EqualTo(Empty));
        Assert.That(diff.Blocks[0].NewValue, Is.EqualTo("A".CharsToStringArray()));
    }

    [Test]
    public void Test_03_AddedLine()
    {
        var diff = Diff.Make(@base: "A", modified: "AB");

        Assert.That(diff.Blocks.Count, Is.EqualTo(2));

        Assert.That(diff.Blocks[0].Type, Is.EqualTo(BlockType.Unchanged));
        Assert.That(diff.Blocks[1].Type, Is.EqualTo(BlockType.Added));

        Assert.That(diff.Blocks[0].NewValue, Is.EqualTo("A".CharsToStringArray()));
        Assert.That(diff.Blocks[1].OldValue, Is.EqualTo(Empty));
        Assert.That(diff.Blocks[1].NewValue, Is.EqualTo("B".CharsToStringArray()));
    }

    [Test]
    public void Test_04_AddedLineInTheMiddle()
    {
        var diff = Diff.Make(@base: "AC", modified: "ABC");

        Assert.That(diff.Blocks.Count, Is.EqualTo(3));

        Assert.That(diff.Blocks[0].Type, Is.EqualTo(BlockType.Unchanged));
        Assert.That(diff.Blocks[1].Type, Is.EqualTo(BlockType.Added));
        Assert.That(diff.Blocks[2].Type, Is.EqualTo(BlockType.Unchanged));

        Assert.That(diff.Blocks[0].NewValue, Is.EqualTo("A".CharsToStringArray()));
        Assert.That(diff.Blocks[1].OldValue, Is.EqualTo(Empty));
        Assert.That(diff.Blocks[1].NewValue, Is.EqualTo("B".CharsToStringArray()));
        Assert.That(diff.Blocks[2].NewValue, Is.EqualTo("C".CharsToStringArray()));
    }

    [Test]
    public void Test_05_RemovedLineInTheMiddle()
    {
        var diff = Diff.Make(@base: "ABC", modified: "AC");

        Assert.That(diff.Blocks.Count, Is.EqualTo(3));

        Assert.That(diff.Blocks[0].Type, Is.EqualTo(BlockType.Unchanged));
        Assert.That(diff.Blocks[1].Type, Is.EqualTo(BlockType.Removed));
        Assert.That(diff.Blocks[2].Type, Is.EqualTo(BlockType.Unchanged));

        Assert.That(diff.Blocks[0].NewValue, Is.EqualTo("A".CharsToStringArray()));
        Assert.That(diff.Blocks[1].OldValue, Is.EqualTo("B".CharsToStringArray()));
        Assert.That(diff.Blocks[1].NewValue, Is.EqualTo(Empty));
        Assert.That(diff.Blocks[2].NewValue, Is.EqualTo("C".CharsToStringArray()));
    }

    [Test]
    public void Test_06_ChangedLineInTheMiddle()
    {
        var diff = Diff.Make(@base: "ABC", modified: "ADC");

        Assert.That(diff.Blocks.Count, Is.EqualTo(3));

        Assert.That(diff.Blocks[0].Type, Is.EqualTo(BlockType.Unchanged));
        Assert.That(diff.Blocks[1].Type, Is.EqualTo(BlockType.Changed));
        Assert.That(diff.Blocks[2].Type, Is.EqualTo(BlockType.Unchanged));

        Assert.That(diff.Blocks[0].NewValue, Is.EqualTo("A".CharsToStringArray()));
        Assert.That(diff.Blocks[1].OldValue, Is.EqualTo("B".CharsToStringArray()));
        Assert.That(diff.Blocks[1].NewValue, Is.EqualTo("D".CharsToStringArray()));
        Assert.That(diff.Blocks[2].NewValue, Is.EqualTo("C".CharsToStringArray()));
    }

    [Test]
    public void Test_10_Complex()
    {
        var diff = Diff.Make(@base: "ABABA", modified: "BABAB");

        Assert.That(diff.Blocks.Count, Is.EqualTo(3));

        Assert.That(diff.Blocks[0].Type, Is.EqualTo(BlockType.Removed));
        Assert.That(diff.Blocks[1].Type, Is.EqualTo(BlockType.Unchanged));
        Assert.That(diff.Blocks[2].Type, Is.EqualTo(BlockType.Added));

        Assert.That(diff.Blocks[0].OldValue, Is.EqualTo("A".CharsToStringArray()));
        Assert.That(diff.Blocks[1].OldValue, Is.EqualTo("BABA".CharsToStringArray()));
        Assert.That(diff.Blocks[2].NewValue, Is.EqualTo("A".CharsToStringArray()));
    }

    [Test]
    public void Test_11_Complex()
    {
        var diff = Diff.Make(@base: "C", modified: "ABCDE");

        Assert.That(diff.Blocks.Count, Is.EqualTo(3));

        Assert.That(diff.Blocks[0].Type, Is.EqualTo(BlockType.Added));
        Assert.That(diff.Blocks[1].Type, Is.EqualTo(BlockType.Unchanged));
        Assert.That(diff.Blocks[2].Type, Is.EqualTo(BlockType.Added));

        Assert.That(diff.Blocks[0].NewValue, Is.EqualTo("AB".CharsToStringArray()));
        Assert.That(diff.Blocks[1].OldValue, Is.EqualTo("C".CharsToStringArray()));
        Assert.That(diff.Blocks[2].NewValue, Is.EqualTo("DE".CharsToStringArray()));
    }

    [Test]
    public void Test_12_Complex()
    {
        var diff = Diff.Make(@base: "A", modified: "BCDEF");

        Assert.That(diff.Blocks.Count, Is.EqualTo(1));

        Assert.That(diff.Blocks[0].Type, Is.EqualTo(BlockType.Changed));

        Assert.That(diff.Blocks[0].OldValue, Is.EqualTo("A".CharsToStringArray()));
        Assert.That(diff.Blocks[0].NewValue, Is.EqualTo("BCDEF".CharsToStringArray()));
    }

    [Test]
    public void Test_13_Complex()
    {
        var diff = Diff.Make(@base: "ABC", modified: "ACB");

        Assert.That(diff.Blocks.Count, Is.EqualTo(2));

        Assert.That(diff.Blocks[0].Type, Is.EqualTo(BlockType.Unchanged));
        Assert.That(diff.Blocks[1].Type, Is.EqualTo(BlockType.Changed));

        Assert.That(diff.Blocks[0].OldValue, Is.EqualTo("A".CharsToStringArray()));
        Assert.That(diff.Blocks[1].OldValue, Is.EqualTo("BC".CharsToStringArray()));
        Assert.That(diff.Blocks[1].NewValue, Is.EqualTo("CB".CharsToStringArray()));
    }
}
