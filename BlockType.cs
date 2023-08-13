namespace unity_yaml_merge;

public enum BlockType
{
    Unchanged,
    Changed,
    Added,
    Removed,
    //TODO: Replaced, // Original and Modified length are equal
}