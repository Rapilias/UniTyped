using System.Collections.Generic;
using VYaml.Annotations;

namespace UniTyped.Generator.ProjectReflections;

// VYaml's default NamingConvention is LowerCamelCase. TagManager.asset mixes PascalCase
// (`TagManager`), lower-case (`tags`, `layers`), and `m_*` keys, so every member is bound by an
// explicit [YamlMember] tag matching the raw key in the asset file.

[YamlObject]
internal partial class TagManagerYamlDocument
{
    [YamlMember("TagManager")] public TagManagerYamlNode? TagManager { get; set; }
}

[YamlObject]
internal partial class TagManagerYamlNode
{
    [YamlMember("tags")] public List<string>? tags { get; set; }
    [YamlMember("layers")] public List<string?>? layers { get; set; }
    [YamlMember("m_SortingLayers")] public List<SortingLayerYamlEntry>? m_SortingLayers { get; set; }
}

[YamlObject]
internal partial class SortingLayerYamlEntry
{
    [YamlMember("name")] public string? name { get; set; }
    [YamlMember("uniqueID")] public uint uniqueID { get; set; }
}
