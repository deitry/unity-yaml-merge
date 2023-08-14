using unity_yaml_merge;

namespace Tests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test_01_NoChanges()
    {
        var ours = "examples/01_no_changes/01.ours.yml";
        var @base = "examples/01_no_changes/01.base.yml";
        var theirs = "examples/01_no_changes/01.theirs.yml";

        var result = Merger.MergeYamls(ours, @base, theirs);

        var expected = File.ReadAllLines("examples/01_no_changes/01.expected.yml");

        CollectionAssert.AreEqual(result, expected);
    }

    [Test]
    public void Test_02_OnlyOursChanges()
    {
        var ours = "examples/02_only_ours_changes/02.ours.yml";
        var @base = "examples/02_only_ours_changes/02.base.yml";
        var theirs = "examples/02_only_ours_changes/02.theirs.yml";

        var result = Merger.MergeYamls(ours, @base, theirs);

        var expected = File.ReadAllLines("examples/02_only_ours_changes/02.expected.yml");

        CollectionAssert.AreEqual(result, expected);
    }

    [Test]
    public void Test_03_OnlyTheirsChanges()
    {
        var ours = "examples/03_only_theirs_changes/03.ours.yml";
        var @base = "examples/03_only_theirs_changes/03.base.yml";
        var theirs = "examples/03_only_theirs_changes/03.theirs.yml";

        var result = Merger.MergeYamls(ours, @base, theirs);

        var expected = File.ReadAllLines("examples/03_only_theirs_changes/03.expected.yml");

        CollectionAssert.AreEqual(result, expected);
    }

    [Test]
    public void Test_04_OursAndTheirsNonConflictingChanges()
    {
        var ours = "examples/04_ours_and_theirs_non_conflicting_changes/04.ours.yml";
        var @base = "examples/04_ours_and_theirs_non_conflicting_changes/04.base.yml";
        var theirs = "examples/04_ours_and_theirs_non_conflicting_changes/04.theirs.yml";

        var result = Merger.MergeYamls(ours, @base, theirs);

        var expected = File.ReadAllLines("examples/04_ours_and_theirs_non_conflicting_changes/04.expected.yml");

        CollectionAssert.AreEqual(result, expected);
    }

    [Test]
    public void Test_05_OursAndTheirsConflictingChanges()
    {
        var ours = "examples/05_ours_and_theirs_conflicting_changes/05.ours.yml";
        var @base = "examples/05_ours_and_theirs_conflicting_changes/05.base.yml";
        var theirs = "examples/05_ours_and_theirs_conflicting_changes/05.theirs.yml";

        var result = Merger.MergeYamls(ours, @base, theirs);

        var expected = File.ReadAllLines("examples/05_ours_and_theirs_conflicting_changes/05.expected.yml");

        CollectionAssert.AreEqual(result, expected);
    }
}
