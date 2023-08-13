using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public class LocalizationAsset
{
    public MonoBehaviour MonoBehaviour { get; set; }
}

public class MonoBehaviour
{
    [YamlMember(Alias = "m_ObjectHideFlags")]
    public long m_ObjectHideFlags { get; set; }

    public FileReference m_CorrespondingSourceObject { get; set; }
    public FileReference m_PrefabInstance { get; set; }
    public FileReference m_PrefabAsset { get; set; }
    public FileReference m_GameObject { get; set; }
    public long m_Enabled { get; set; }
    public long m_EditorHideFlags { get; set; }
    public FileReference m_Script { get; set; }
    public string m_Name { get; set; }
    public string m_EditorClassIdentifier { get; set; }
    public Locale m_LocaleId {get;set;}
    public FileReference m_SharedData { get; set; }
    public Metadata m_Metadata { get; set; }
    public List<LocalizationEntry> m_TableData { get; set; }
    public References references { get; set; }
}

public class FileReference
{
    [YamlMember(Alias = "fileID")]
    public long FileId { get; set; }


    [YamlMember(Alias = "guid")]
    public string? Guid { get; set; }

    [YamlMember(Alias = "type")]
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
    public string m_Localized { get; set; }
    public Metadata m_Metadata { get; set; }
}

public class References
{
    public int version { get; set; }
    public List<object> RefIds { get; set; }
}