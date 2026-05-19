using System.Collections.Generic;
using VYaml.Annotations;

namespace UniTyped.Generator.AnimatorViews;

// VYaml's default NamingConvention is LowerCamelCase. Unity YAML keys (`AnimatorController`,
// `m_AnimatorParameters`, ...) do not follow that, so every member is bound by an explicit
// [YamlMember] tag matching the raw key in the .controller file.

[YamlObject]
internal partial class AnimatorControllerYamlDocument
{
    [YamlMember("AnimatorController")] public AnimatorControllerYamlNode? AnimatorController { get; set; }
}

[YamlObject]
internal partial class AnimatorControllerYamlNode
{
    [YamlMember("m_AnimatorParameters")] public List<AnimatorParameterYamlEntry>? m_AnimatorParameters { get; set; }
    [YamlMember("m_AnimatorLayers")] public List<AnimatorLayerYamlEntry>? m_AnimatorLayers { get; set; }
}

[YamlObject]
internal partial class AnimatorParameterYamlEntry
{
    [YamlMember("m_Name")] public string? m_Name { get; set; }
    [YamlMember("m_Type")] public int m_Type { get; set; }
}

[YamlObject]
internal partial class AnimatorLayerYamlEntry
{
    [YamlMember("m_Name")] public string? m_Name { get; set; }
}
