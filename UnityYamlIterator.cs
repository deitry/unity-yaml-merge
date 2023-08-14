namespace unity_yaml_merge;

public class UnityYamlIterator
{
    public bool EndOfFile
    {
        get
        {
            return false;
        }
    }

    public UnityYamlIterator(string path)
    {
        Lines = File.ReadAllLines(path);
    }

    public string[] Lines { get; set; }

    public bool MoveNext()
    {
        return false;
    }

    /// <summary>
    /// Is a line for unknown properties and multiple lines for localization entries
    /// </summary>
    public string CurrentBlock
    {
        get
        {

            return string.Empty;
        }
    }

    /// <summary>
    /// Is a line for unknown properties and multiple lines for localization entries
    /// </summary>
    public string CurrentProperty
    {
        get
        {

            return string.Empty;
        }
    }

    public IEnumerable<string> ReadHeader()
    {
        foreach (var line in Lines)
        {
            yield return line;

            // check after yield so this one will be yielded as well
            if (line == "  m_TableData:")
                break;
        }
    }

    LocalizationEntry CurrentKey()
    {
        return null;
    }
}

public class LocalizationEntry
{
    public string Key { get; set; }
    public string Value { get; set; }
}
