using unity_yaml_merge;

namespace Tests;

public class TestIndices
{
    private static IEnumerable<object[]> TestEqualCases => new []
    {
        new object[] { "".CharsToStringArray(),      "".CharsToStringArray(),     Indices.End },
        new object[] { "A".CharsToStringArray(),     "A".CharsToStringArray(),    new Indices(0, 0) },
        new object[] { "B".CharsToStringArray(),     "AB".CharsToStringArray(),   new Indices(0, 1) },
        new object[] { "AB".CharsToStringArray(),    "B".CharsToStringArray(),    new Indices(1, 0) },
        new object[] { "BCA".CharsToStringArray(),   "DEFGA".CharsToStringArray(), new Indices(2, 4) },
        new object[] { "A".CharsToStringArray(),     "B".CharsToStringArray(),    Indices.End },
    };

    [Test]
    [TestCaseSource(nameof(TestEqualCases))]
    public void TestNextEqual(string[] i1, string[] i2, Indices expected)
    {
        var actual = Diff.GetNextEqual(i1, i2);

        Assert.That(actual, Is.EqualTo(expected));
    }

    private static IEnumerable<object[]> TestDifferenceCases => new []
    {
        new object[] { "".CharsToStringArray(),      "".CharsToStringArray(),     Indices.End },
        new object[] { "A".CharsToStringArray(),     "B".CharsToStringArray(),    Indices.Zero },
        new object[] { "ABC".CharsToStringArray(),   "ABD".CharsToStringArray(),  new Indices(2, 2) },
        new object[] { "B".CharsToStringArray(),     "AB".CharsToStringArray(),   Indices.Zero },
        new object[] { "AB".CharsToStringArray(),    "B".CharsToStringArray(),    Indices.Zero },
        new object[] { "BCA".CharsToStringArray(),   "DEFGA".CharsToStringArray(), Indices.Zero },
    };

    [Test]
    [TestCaseSource(nameof(TestDifferenceCases))]
    public void TestNextDifference(string[] i1, string[] i2, Indices expected)
    {
        var actual = Diff.GetNextDifference(i1, i2);

        Assert.That(actual, Is.EqualTo(expected));
    }
}
