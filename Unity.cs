using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public static class Fixer
{
    internal const string BadUnitySequence = " #";
    internal const string GoodYamlSequence = " ⋕";

    public static string FromUnity(this string input)
    {
        // `#` is valid symbol in unquoted Unity strings, but is considered as comment start in YamlDotNet
        return input.Replace(BadUnitySequence, GoodYamlSequence);
    }

    public static string ToUnity(this string input)
    {
        return input.Replace(GoodYamlSequence, BadUnitySequence);
    }
}

public class LocalizationAsset
{
    public const string Tag = "tag:unity3d.com,2011:114";

    public MonoBehaviour MonoBehaviour { get; set; }
}

public class MonoBehaviour
{
    [YamlMember(Alias = "m_ObjectHideFlags")]
    public long m_ObjectHideFlags { get; set; }

    [YamlMember(ScalarStyle = YamlDotNet.Core.ScalarStyle.Literal)]
    public FileReference? m_CorrespondingSourceObject { get; set; }

    [YamlMember(ScalarStyle = YamlDotNet.Core.ScalarStyle.Plain)]
    public FileReference? m_PrefabInstance { get; set; }

    [YamlMember(ScalarStyle = YamlDotNet.Core.ScalarStyle.Plain)]
    public FileReference? m_PrefabAsset { get; set; }

    [YamlMember(ScalarStyle = YamlDotNet.Core.ScalarStyle.Plain)]
    public FileReference? m_GameObject { get; set; }

    public string m_TableCollectionName { get; set; }
    public string m_TableCollectionNameGuidString { get; set; }
    public long m_Enabled { get; set; }
    public long m_EditorHideFlags { get; set; }
    public FileReference m_Script { get; set; }
    public string m_Name { get; set; }
    public string m_EditorClassIdentifier { get; set; }
    public Locale m_LocaleId { get; set; }
    public FileReference m_SharedData { get; set; }
    public Metadata m_Metadata { get; set; }

    /// <summary>
    /// List of localizations. Not null for dictionary
    /// </summary>
    /// <value></value>
    [YamlMember(Alias = "m_TableData")]
    public List<LocalizationEntry>? LocalizationValues { get; set; }

    /// <summary>
    /// List of localization keys. Not null for shared keys asset
    /// </summary>
    /// <value></value>
    [YamlMember(Alias = "m_Entries")]
    public List<LocalizationKeyEntry>? LocalizationKeys { get; set; }

    public KeyGenerator m_KeyGenerator { get; set; }
    public References references { get; set; }
}

public class FileReference
{
    [YamlMember(Alias = "fileID")]
    public long FileId { get; set; }


    [YamlMember(Alias = "guid", DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
    public string? Guid { get; set; }

    [YamlMember(Alias = "type", DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
    public long? Type { get; set; }
}

public class Locale
{
    public string m_Code { get; set; }
}

public class Metadata
{
    public List<object> m_Items { get; set; }
}

public class LocalizationEntry
{
    public long m_Id { get; set; }

    [YamlMember(ScalarStyle = YamlDotNet.Core.ScalarStyle.SingleQuoted)]
    public string m_Localized { get; set; }
    public Metadata m_Metadata { get; set; }

    public override string ToString() => m_Localized;
}

public class LocalizationKeyEntry
{
    public long m_Id { get; set; }
    public string m_Key { get; set; }
    public Metadata m_Metadata { get; set; }

    public override string ToString() => m_Key;
}

public class KeyGenerator
{
    public string rid { get; set; }
}

public class References
{
    public int version { get; set; }
    public List<object> RefIds { get; set; }
}