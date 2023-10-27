using unity_yaml_merge;

namespace Tests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }


    private static IEnumerable<object[]> TestCases => new List<object[]>
    {
        new object[]
        {
            "examples/01_no_changes/01.ours.yml",
            "examples/01_no_changes/01.base.yml",
            "examples/01_no_changes/01.theirs.yml",
            "examples/01_no_changes/01.expected.yml"
        },
        new object[]
        {
            "examples/02_only_ours_changes/02.ours.yml",
            "examples/02_only_ours_changes/02.base.yml",
            "examples/02_only_ours_changes/02.theirs.yml",
            "examples/02_only_ours_changes/02.expected.yml"
        },
        new object[]
        {
            "examples/03_only_theirs_changes/03.ours.yml",
            "examples/03_only_theirs_changes/03.base.yml",
            "examples/03_only_theirs_changes/03.theirs.yml",
            "examples/03_only_theirs_changes/03.expected.yml"
        },
        new object[]
        {
            "examples/04_ours_and_theirs_non_conflicting_changes/04.ours.yml",
            "examples/04_ours_and_theirs_non_conflicting_changes/04.base.yml",
            "examples/04_ours_and_theirs_non_conflicting_changes/04.theirs.yml",
            "examples/04_ours_and_theirs_non_conflicting_changes/04.expected.yml"
        },
        new object[]
        {
            "examples/05_ours_and_theirs_conflicting_changes/05.ours.yml",
            "examples/05_ours_and_theirs_conflicting_changes/05.base.yml",
            "examples/05_ours_and_theirs_conflicting_changes/05.theirs.yml",
            "examples/05_ours_and_theirs_conflicting_changes/05.expected.yml"
        },
        // now confliction additions are allowed
        // new object[]
        // {
        //     "examples/11_conflicting_additions_with_trailing_unchanged/ours.yml",
        //     "examples/11_conflicting_additions_with_trailing_unchanged/base.yml",
        //     "examples/11_conflicting_additions_with_trailing_unchanged/theirs.yml",
        //     "examples/11_conflicting_additions_with_trailing_unchanged/expected.yml"
        // },
        new object[]
        {
            "examples/12_only_ours/ours.yml",
            "examples/12_only_ours/base.yml",
            "examples/12_only_ours/theirs.yml",
            "examples/12_only_ours/expected.yml"
        },
        new object[]
        {
            "examples/13_only_theirs/ours.yml",
            "examples/13_only_theirs/base.yml",
            "examples/13_only_theirs/theirs.yml",
            "examples/13_only_theirs/expected.yml"
        },
        new object[]
        {
            "examples/14_ours_and_theirs_non_conflicting/ours.yml",
            "examples/14_ours_and_theirs_non_conflicting/base.yml",
            "examples/14_ours_and_theirs_non_conflicting/theirs.yml",
            "examples/14_ours_and_theirs_non_conflicting/expected.yml"
        },
        new object[]
        {
            "examples/15_same_additions_in_the_same_place/ours.yml",
            "examples/15_same_additions_in_the_same_place/base.yml",
            "examples/15_same_additions_in_the_same_place/theirs.yml",
            "examples/15_same_additions_in_the_same_place/expected.yml"
        },
        new object[]
        {
            "examples/16_different_additions_in_the_same_place/ours.yml",
            "examples/16_different_additions_in_the_same_place/base.yml",
            "examples/16_different_additions_in_the_same_place/theirs.yml",
            "examples/16_different_additions_in_the_same_place/expected.yml"
        },
        new object[]
        {
            "examples/17_simple_conflict/ours.yml",
            "examples/17_simple_conflict/base.yml",
            "examples/17_simple_conflict/theirs.yml",
            "examples/17_simple_conflict/expected.yml"
        },
        new object[]
        {
            "examples/18_duplicate_resx_entries/ours.resx",
            "examples/18_duplicate_resx_entries/base.resx",
            "examples/18_duplicate_resx_entries/theirs.resx",
            "examples/18_duplicate_resx_entries/expected.resx"
        },
        new object[]
        {
            "examples/100_real_case/ours.yml",
            "examples/100_real_case/base.yml",
            "examples/100_real_case/theirs.yml",
            "examples/100_real_case/expected.yml"
        },
    };

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void TestAll(string oursPath, string basePath, string theirsPath, string expectedPath)
    {
        var result = Merger.MergeYamls(oursPath, basePath, theirsPath);
        var expected = File.ReadAllLines(expectedPath);

        CollectionAssert.AreEqual(expected: expected, actual: result);
    }
}
