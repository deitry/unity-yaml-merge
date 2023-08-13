using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;


// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

var deserializer = new DeserializerBuilder()
    .WithTagMapping(new YamlDotNet.Core.TagName("tag:unity3d.com,2011:114"), typeof(LocalizationAsset))
    .Build();

//yml contains a string containing your YAML
var yml = File.ReadAllText(args[1]);
var p = deserializer.Deserialize<LocalizationAsset>(yml);

foreach (var item in p.MonoBehaviour.m_TableData)
{
    Console.WriteLine(item.m_Localized);
}