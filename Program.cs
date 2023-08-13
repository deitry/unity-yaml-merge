using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;


// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

var deserializer = new DeserializerBuilder()
    .WithTagMapping(new YamlDotNet.Core.TagName("tag:unity3d.com,2011:114"), typeof(LocalizationAsset))
    .Build();

var ours = args[0];
var @base = args[1];
var theirs = args[2];

//yml contains a string containing your YAML
var yml = File.ReadAllText(ours);
var p = deserializer.Deserialize<LocalizationAsset>(yml);

foreach (var item in p.MonoBehaviour.m_TableData)
{
    Console.WriteLine(item.m_Localized);
}

// serialize back
var serializer = new SerializerBuilder()
    .WithTagMapping(new YamlDotNet.Core.TagName("tag:unity3d.com,2011:114"), typeof(LocalizationAsset))
    .Build();

var yaml = serializer.Serialize(p);
File.WriteAllText($"{ours}.converted", yaml);