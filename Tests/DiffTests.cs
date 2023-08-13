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
        Assert.That(diff.Blocks[0].OriginalLines, Is.EqualTo("A".CharsToStringArray()));
        Assert.That(diff.Blocks[0].ModifiedLines, Is.EqualTo("A".CharsToStringArray()));
    }

    [Test]
    public void Test_02_AddedToEmpty()
    {
        var diff = Diff.Make("", "A");

        Assert.That(diff.Blocks.Count, Is.EqualTo(1));
        Assert.That(diff.Blocks[0].Type, Is.EqualTo(BlockType.Added));
        Assert.That(diff.Blocks[0].OriginalLines, Is.EqualTo(Empty));
        Assert.That(diff.Blocks[0].ModifiedLines, Is.EqualTo("A".CharsToStringArray()));
    }

    [Test]
    public void Test_03_AddedLine()
    {
        var diff = Diff.Make(@base: "A", modified: "AB");

        Assert.That(diff.Blocks.Count, Is.EqualTo(2));

        Assert.That(diff.Blocks[0].Type, Is.EqualTo(BlockType.Unchanged));
        Assert.That(diff.Blocks[1].Type, Is.EqualTo(BlockType.Added));

        Assert.That(diff.Blocks[0].ModifiedLines, Is.EqualTo("A".CharsToStringArray()));
        Assert.That(diff.Blocks[1].OriginalLines, Is.EqualTo(Empty));
        Assert.That(diff.Blocks[1].ModifiedLines, Is.EqualTo("B".CharsToStringArray()));
    }

    [Test]
    public void Test_04_AddedLineInTheMiddle()
    {
        var diff = Diff.Make(@base: "AC", modified: "ABC");

        Assert.That(diff.Blocks, Has.Count.EqualTo(3));

        Assert.That(diff.Blocks[0].Type, Is.EqualTo(BlockType.Unchanged));
        Assert.That(diff.Blocks[1].Type, Is.EqualTo(BlockType.Added));
        Assert.That(diff.Blocks[2].Type, Is.EqualTo(BlockType.Unchanged));

        Assert.That(diff.Blocks[0].ModifiedLines, Is.EqualTo("A".CharsToStringArray()));
        Assert.That(diff.Blocks[1].OriginalLines, Is.EqualTo(Empty));
        Assert.That(diff.Blocks[1].ModifiedLines, Is.EqualTo("B".CharsToStringArray()));
        Assert.That(diff.Blocks[2].ModifiedLines, Is.EqualTo("C".CharsToStringArray()));
    }

    [Test]
    public void Test_05_RemovedLineInTheMiddle()
    {
        var diff = Diff.Make(@base: "ABC", modified: "AC");

        Assert.That(diff.Blocks.Count, Is.EqualTo(3));

        Assert.That(diff.Blocks[0].Type, Is.EqualTo(BlockType.Unchanged));
        Assert.That(diff.Blocks[1].Type, Is.EqualTo(BlockType.Removed));
        Assert.That(diff.Blocks[2].Type, Is.EqualTo(BlockType.Unchanged));

        Assert.That(diff.Blocks[0].ModifiedLines, Is.EqualTo("A".CharsToStringArray()));
        Assert.That(diff.Blocks[1].OriginalLines, Is.EqualTo("B".CharsToStringArray()));
        Assert.That(diff.Blocks[1].ModifiedLines, Is.EqualTo(Empty));
        Assert.That(diff.Blocks[2].ModifiedLines, Is.EqualTo("C".CharsToStringArray()));
    }

    [Test]
    public void Test_06_ChangedLineInTheMiddle()
    {
        var diff = Diff.Make(@base: "ABC", modified: "ADC");

        Assert.That(diff.Blocks.Count, Is.EqualTo(3));

        Assert.That(diff.Blocks[0].Type, Is.EqualTo(BlockType.Unchanged));
        Assert.That(diff.Blocks[1].Type, Is.EqualTo(BlockType.Changed));
        Assert.That(diff.Blocks[2].Type, Is.EqualTo(BlockType.Unchanged));

        Assert.That(diff.Blocks[0].ModifiedLines, Is.EqualTo("A".CharsToStringArray()));
        Assert.Multiple(() =>
        {
            Assert.That(diff.Blocks[1].OriginalLines, Is.EqualTo("B".CharsToStringArray()));
            Assert.That(diff.Blocks[1].ModifiedLines, Is.EqualTo("D".CharsToStringArray()));
        });
        Assert.That(diff.Blocks[2].ModifiedLines, Is.EqualTo("C".CharsToStringArray()));
    }

    [Test]
    public void Test_06a_ChangedLineInTheMiddle()
    {
        var diff = Diff.Make(@base: "ABbbC", modified: "ADddC");

        Assert.That(diff.Blocks.Count, Is.EqualTo(3));

        Assert.That(diff.Blocks[0].Type, Is.EqualTo(BlockType.Unchanged));
        Assert.That(diff.Blocks[1].Type, Is.EqualTo(BlockType.Changed));
        Assert.That(diff.Blocks[2].Type, Is.EqualTo(BlockType.Unchanged));

        Assert.That(diff.Blocks[0].ModifiedLines, Is.EqualTo("A".CharsToStringArray()));
        Assert.Multiple(() =>
        {
            Assert.That(diff.Blocks[1].OriginalLines, Is.EqualTo("Bbb".CharsToStringArray()));
            Assert.That(diff.Blocks[1].ModifiedLines, Is.EqualTo("Ddd".CharsToStringArray()));
        });
        Assert.That(diff.Blocks[2].ModifiedLines, Is.EqualTo("C".CharsToStringArray()));
    }

    [Test]
    public void Test_06b_ChangedLineInTheMiddle()
    {
        var diff = Diff.Make(@base: "ABbbC", modified: "ADC");

        Assert.That(diff.Blocks.Count, Is.EqualTo(3));

        Assert.That(diff.Blocks[0].Type, Is.EqualTo(BlockType.Unchanged));
        Assert.That(diff.Blocks[1].Type, Is.EqualTo(BlockType.Changed));
        Assert.That(diff.Blocks[2].Type, Is.EqualTo(BlockType.Unchanged));

        Assert.That(diff.Blocks[0].ModifiedLines, Is.EqualTo("A".CharsToStringArray()));
        Assert.Multiple(() =>
        {
            Assert.That(diff.Blocks[1].OriginalLines, Is.EqualTo("Bbb".CharsToStringArray()));
            Assert.That(diff.Blocks[1].ModifiedLines, Is.EqualTo("D".CharsToStringArray()));
        });
        Assert.That(diff.Blocks[2].ModifiedLines, Is.EqualTo("C".CharsToStringArray()));
    }

    [Test]
    public void Test_06c_ChangedLineInTheMiddle()
    {
        var diff = Diff.Make(@base: "ABC", modified: "ADddC");

        Assert.That(diff.Blocks.Count, Is.EqualTo(3));

        Assert.That(diff.Blocks[0].Type, Is.EqualTo(BlockType.Unchanged));
        Assert.That(diff.Blocks[1].Type, Is.EqualTo(BlockType.Changed));
        Assert.That(diff.Blocks[2].Type, Is.EqualTo(BlockType.Unchanged));

        Assert.That(diff.Blocks[0].ModifiedLines, Is.EqualTo("A".CharsToStringArray()));
        Assert.Multiple(() =>
        {
            Assert.That(diff.Blocks[1].OriginalLines, Is.EqualTo("B".CharsToStringArray()));
            Assert.That(diff.Blocks[1].ModifiedLines, Is.EqualTo("Ddd".CharsToStringArray()));
        });
        Assert.That(diff.Blocks[2].ModifiedLines, Is.EqualTo("C".CharsToStringArray()));
    }

    [Test]
    public void Test_10_Complex()
    {
        var diff = Diff.Make(@base: "ABABA", modified: "BABAB");

        Assert.Multiple(() =>
        {
            Assert.That(diff.Blocks, Has.Count.EqualTo(3));
            Assert.That(diff.Blocks[0].Type, Is.EqualTo(BlockType.Added));
            Assert.That(diff.Blocks[1].Type, Is.EqualTo(BlockType.Unchanged));
            Assert.That(diff.Blocks[2].Type, Is.EqualTo(BlockType.Removed));

            Assert.That(diff.Blocks[0].ModifiedLines, Is.EqualTo("B".CharsToStringArray()));
            Assert.That(diff.Blocks[1].OriginalLines, Is.EqualTo("ABAB".CharsToStringArray()));
            Assert.That(diff.Blocks[2].OriginalLines, Is.EqualTo("A".CharsToStringArray()));
        });
    }

    [Test]
    public void Test_11_Complex()
    {
        var diff = Diff.Make(@base: "C", modified: "ABCDE");

        Assert.That(diff.Blocks.Count, Is.EqualTo(3));

        Assert.That(diff.Blocks[0].Type, Is.EqualTo(BlockType.Added));
        Assert.That(diff.Blocks[1].Type, Is.EqualTo(BlockType.Unchanged));
        Assert.That(diff.Blocks[2].Type, Is.EqualTo(BlockType.Added));

        Assert.That(diff.Blocks[0].ModifiedLines, Is.EqualTo("AB".CharsToStringArray()));
        Assert.That(diff.Blocks[1].OriginalLines, Is.EqualTo("C".CharsToStringArray()));
        Assert.That(diff.Blocks[2].ModifiedLines, Is.EqualTo("DE".CharsToStringArray()));
    }

    [Test]
    public void Test_12_Complex()
    {
        var diff = Diff.Make(@base: "A", modified: "BCDEF");

        Assert.That(diff.Blocks.Count, Is.EqualTo(1));

        Assert.That(diff.Blocks[0].Type, Is.EqualTo(BlockType.Changed));

        Assert.That(diff.Blocks[0].OriginalLines, Is.EqualTo("A".CharsToStringArray()));
        Assert.That(diff.Blocks[0].ModifiedLines, Is.EqualTo("BCDEF".CharsToStringArray()));
    }

    [Test]
    [Ignore("Current implementation treats this case as removal + addition")]
    public void Test_13_ChangedWithSimilarSymbols_Preferable()
    {
        var diff = Diff.Make(@base: "ABC", modified: "ACB");

        Assert.That(diff.Blocks.Count, Is.EqualTo(2));

        Assert.That(diff.Blocks[0].Type, Is.EqualTo(BlockType.Unchanged));
        Assert.That(diff.Blocks[1].Type, Is.EqualTo(BlockType.Changed));

        Assert.That(diff.Blocks[0].OriginalLines, Is.EqualTo("A".CharsToStringArray()));
        Assert.That(diff.Blocks[1].OriginalLines, Is.EqualTo("BC".CharsToStringArray()));
        Assert.That(diff.Blocks[1].ModifiedLines, Is.EqualTo("CB".CharsToStringArray()));
    }

    [Test]
    public void Test_13_ChangedWithSimilarSymbols()
    {
        var diff = Diff.Make(@base: "ABC", modified: "ACB");

        Assert.That(diff.Blocks.Count, Is.EqualTo(4));

        Assert.That(diff.Blocks[0].Type, Is.EqualTo(BlockType.Unchanged));
        Assert.That(diff.Blocks[1].Type, Is.EqualTo(BlockType.Added));
        Assert.That(diff.Blocks[2].Type, Is.EqualTo(BlockType.Unchanged));
        Assert.That(diff.Blocks[3].Type, Is.EqualTo(BlockType.Removed));

        Assert.That(diff.Blocks[0].OriginalLines, Is.EqualTo("A".CharsToStringArray()));
        Assert.That(diff.Blocks[1].ModifiedLines, Is.EqualTo("C".CharsToStringArray()));
        Assert.That(diff.Blocks[2].ModifiedLines, Is.EqualTo("B".CharsToStringArray()));
        Assert.That(diff.Blocks[3].OriginalLines, Is.EqualTo("C".CharsToStringArray()));
    }

    [Test]
    public void Test_13a_ChangedWithSimilarSymbols()
    {
        var diff = Diff.Make(@base: "ABbCc", modified: "ACcBb");

        Assert.Multiple(() =>
        {
            Assert.That(diff.Blocks, Has.Count.EqualTo(4));
            Assert.That(diff.Blocks[0].Type, Is.EqualTo(BlockType.Unchanged));
            Assert.That(diff.Blocks[1].Type, Is.EqualTo(BlockType.Added));
            Assert.That(diff.Blocks[2].Type, Is.EqualTo(BlockType.Unchanged));
            Assert.That(diff.Blocks[3].Type, Is.EqualTo(BlockType.Removed));

            Assert.That(diff.Blocks[0].OriginalLines, Is.EqualTo("A".CharsToStringArray()));
            Assert.That(diff.Blocks[1].ModifiedLines, Is.EqualTo("Cc".CharsToStringArray()));
            Assert.That(diff.Blocks[2].ModifiedLines, Is.EqualTo("Bb".CharsToStringArray()));
            Assert.That(diff.Blocks[3].OriginalLines, Is.EqualTo("Cc".CharsToStringArray()));
        });
    }

    [Test]
    [Ignore("Current implementation cannot handle it in preferable way")]
    public void Test_13b_ChangedWithSimilarSymbols_Preferable()
    {
        var diff = Diff.Make(@base: "ABCc", modified: "ACcB");

        Assert.That(diff.Blocks.Count, Is.EqualTo(4));

        Assert.That(diff.Blocks[0].Type, Is.EqualTo(BlockType.Unchanged));
        Assert.That(diff.Blocks[1].Type, Is.EqualTo(BlockType.Removed));
        Assert.That(diff.Blocks[2].Type, Is.EqualTo(BlockType.Unchanged));
        Assert.That(diff.Blocks[3].Type, Is.EqualTo(BlockType.Added));

        Assert.That(diff.Blocks[0].OriginalLines, Is.EqualTo("A".CharsToStringArray()));
        Assert.That(diff.Blocks[1].OriginalLines, Is.EqualTo("B".CharsToStringArray()));
        Assert.That(diff.Blocks[2].ModifiedLines, Is.EqualTo("Cc".CharsToStringArray()));
        Assert.That(diff.Blocks[3].ModifiedLines, Is.EqualTo("B".CharsToStringArray()));
    }

    [Test]
    public void Test_13b_ChangedWithSimilarSymbols()
    {
        var diff = Diff.Make(@base: "ABCc", modified: "ACcB");

        Assert.That(diff.Blocks.Count, Is.EqualTo(4));

        Assert.That(diff.Blocks[0].Type, Is.EqualTo(BlockType.Unchanged));
        Assert.That(diff.Blocks[1].Type, Is.EqualTo(BlockType.Added));
        Assert.That(diff.Blocks[2].Type, Is.EqualTo(BlockType.Unchanged));
        Assert.That(diff.Blocks[3].Type, Is.EqualTo(BlockType.Removed));

        Assert.That(diff.Blocks[0].OriginalLines, Is.EqualTo("A".CharsToStringArray()));
        Assert.That(diff.Blocks[1].ModifiedLines, Is.EqualTo("Cc".CharsToStringArray()));
        Assert.That(diff.Blocks[2].ModifiedLines, Is.EqualTo("B".CharsToStringArray()));
        Assert.That(diff.Blocks[3].OriginalLines, Is.EqualTo("Cc".CharsToStringArray()));
    }

    [Test]
    public void Test_13c_ChangedWithSimilarSymbols()
    {
        var diff = Diff.Make(@base: "ABbbC", modified: "ACBbb");

        Assert.That(diff.Blocks.Count, Is.EqualTo(4));

        Assert.That(diff.Blocks[0].Type, Is.EqualTo(BlockType.Unchanged));
        Assert.That(diff.Blocks[1].Type, Is.EqualTo(BlockType.Added));
        Assert.That(diff.Blocks[2].Type, Is.EqualTo(BlockType.Unchanged));
        Assert.That(diff.Blocks[3].Type, Is.EqualTo(BlockType.Removed));

        Assert.That(diff.Blocks[0].OriginalLines, Is.EqualTo("A".CharsToStringArray()));
        Assert.That(diff.Blocks[1].ModifiedLines, Is.EqualTo("C".CharsToStringArray()));
        Assert.That(diff.Blocks[2].ModifiedLines, Is.EqualTo("Bbb".CharsToStringArray()));
        Assert.That(diff.Blocks[3].OriginalLines, Is.EqualTo("C".CharsToStringArray()));
    }

    [Test]
    public void Test_14_ChangedWithoutSimilarSymbols()
    {
        var diff = Diff.Make(@base: "ABC", modified: "ADE");

        Assert.That(diff.Blocks.Count, Is.EqualTo(2));

        Assert.That(diff.Blocks[0].Type, Is.EqualTo(BlockType.Unchanged));
        Assert.That(diff.Blocks[1].Type, Is.EqualTo(BlockType.Changed));

        Assert.That(diff.Blocks[0].OriginalLines, Is.EqualTo("A".CharsToStringArray()));
        Assert.That(diff.Blocks[1].OriginalLines, Is.EqualTo("BC".CharsToStringArray()));
        Assert.That(diff.Blocks[1].ModifiedLines, Is.EqualTo("DE".CharsToStringArray()));
    }
}
