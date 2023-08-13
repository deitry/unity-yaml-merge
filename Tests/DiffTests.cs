using unity_yaml_merge;

namespace Tests;

public class DiffTests
{
    [Test]
    public void Test_01_Empty()
    {
        var diff = Diff.Make(new string []{}, new string []{});

        Assert.That(diff.Blocks.Count, Is.EqualTo(0));
    }

    [Test]
    public void Test_02_EqualSingleLine()
    {
        var diff = Diff.Make(new []{ "A" }, new [] { "A" });

        Assert.That(diff.Blocks.Count, Is.EqualTo(1));
        Assert.That(diff.Blocks[0].Type, Is.EqualTo(BlockType.Unchanged));
        Assert.That(diff.Blocks[0].OldValue, Is.EqualTo(new []{ "A" }));
        Assert.That(diff.Blocks[0].NewValue, Is.EqualTo(new []{ "A" }));
    }

    [Test]
    public void Test_02_AddedToEmpty()
    {
        var diff = Diff.Make(new string []{ }, new [] { "A" });

        Assert.That(diff.Blocks.Count, Is.EqualTo(1));
        Assert.That(diff.Blocks[0].Type, Is.EqualTo(BlockType.Added));
        Assert.That(diff.Blocks[0].OldValue, Is.EqualTo(new string []{ }));
        Assert.That(diff.Blocks[0].NewValue, Is.EqualTo(new []{ "A" }));
    }

    [Test]
    public void Test_03_AddedLine()
    {
        var diff = Diff.Make(@base: new []{ "A" }, modified: new [] { "A", "B" });

        Assert.That(diff.Blocks.Count, Is.EqualTo(2));

        Assert.That(diff.Blocks[0].Type, Is.EqualTo(BlockType.Unchanged));
        Assert.That(diff.Blocks[1].Type, Is.EqualTo(BlockType.Added));

        Assert.That(diff.Blocks[0].NewValue, Is.EqualTo(new []{ "A" }));
        Assert.That(diff.Blocks[1].OldValue, Is.EqualTo(new string []{ }));
        Assert.That(diff.Blocks[1].NewValue, Is.EqualTo(new []{ "B" }));
    }

    [Test]
    public void Test_04_AddedLineInTheMiddle()
    {
        var diff = Diff.Make(@base: new []{ "A", "C" }, modified: new [] { "A", "B", "C" });

        Assert.That(diff.Blocks.Count, Is.EqualTo(3));

        Assert.That(diff.Blocks[0].Type, Is.EqualTo(BlockType.Unchanged));
        Assert.That(diff.Blocks[1].Type, Is.EqualTo(BlockType.Added));
        Assert.That(diff.Blocks[2].Type, Is.EqualTo(BlockType.Unchanged));

        Assert.That(diff.Blocks[0].NewValue, Is.EqualTo(new []{ "A" }));
        Assert.That(diff.Blocks[1].OldValue, Is.EqualTo(new string []{ }));
        Assert.That(diff.Blocks[1].NewValue, Is.EqualTo(new []{ "B" }));
        Assert.That(diff.Blocks[2].NewValue, Is.EqualTo(new []{ "C" }));
    }

    [Test]
    public void Test_05_RemovedLineInTheMiddle()
    {
        var diff = Diff.Make(@base: new []{ "A", "B", "C" }, modified: new [] { "A", "C" });

        Assert.That(diff.Blocks.Count, Is.EqualTo(3));

        Assert.That(diff.Blocks[0].Type, Is.EqualTo(BlockType.Unchanged));
        Assert.That(diff.Blocks[1].Type, Is.EqualTo(BlockType.Removed));
        Assert.That(diff.Blocks[2].Type, Is.EqualTo(BlockType.Unchanged));

        Assert.That(diff.Blocks[0].NewValue, Is.EqualTo(new []{ "A" }));
        Assert.That(diff.Blocks[1].OldValue, Is.EqualTo(new []{ "B" }));
        Assert.That(diff.Blocks[1].NewValue, Is.EqualTo(new string []{ }));
        Assert.That(diff.Blocks[2].NewValue, Is.EqualTo(new []{ "C" }));
    }

    [Test]
    public void Test_06_ChangedLineInTheMiddle()
    {
        var diff = Diff.Make(@base: new []{ "A", "B", "C" }, modified: new [] { "A", "D", "C" });

        Assert.That(diff.Blocks.Count, Is.EqualTo(3));

        Assert.That(diff.Blocks[0].Type, Is.EqualTo(BlockType.Unchanged));
        Assert.That(diff.Blocks[1].Type, Is.EqualTo(BlockType.Changed));
        Assert.That(diff.Blocks[2].Type, Is.EqualTo(BlockType.Unchanged));

        Assert.That(diff.Blocks[0].NewValue, Is.EqualTo(new []{ "A" }));
        Assert.That(diff.Blocks[1].OldValue, Is.EqualTo(new []{ "B" }));
        Assert.That(diff.Blocks[1].NewValue, Is.EqualTo(new []{ "D" }));
        Assert.That(diff.Blocks[2].NewValue, Is.EqualTo(new []{ "C" }));
    }
}
