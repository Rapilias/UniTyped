using System.Text;
using VYaml.Serialization;

namespace UniTyped.Generator.ProjectReflections;

public static class ProjectReflectionGenerator
{
    public static void GenerateViews(UniTypedGeneratorContext context, StringBuilder sourceBuilder)
    {
        sourceBuilder.AppendLine($$"""
namespace UniTyped.Reflection
{
""");

        List<string> tags = new();
        List<(int index, string name)> layers = new();
        List<(int id, string name)> sortingLayers = new();

        try
        {
            var projectPath = GetProjectPathFromAnchor(context);
            var projectSettingsPath = Path.Combine(projectPath, "ProjectSettings");

            // tags and layers
            var tagManagerPath = Path.Combine(projectSettingsPath, "TagManager.asset");

            var tagManagerBytes = File.ReadAllBytes(tagManagerPath);
            var docs = YamlSerializer.DeserializeMultipleDocuments<TagManagerYamlDocument>(tagManagerBytes);

            foreach (var doc in docs)
            {
                var node = doc?.TagManager;
                if (node == null) continue;

                if (node.tags is { } docTags)
                {
                    foreach (var t in docTags)
                    {
                        if (t == null) continue;
                        tags.Add(t);
                    }
                }

                if (node.layers is { } docLayers)
                {
                    int i = 0;
                    foreach (var name in docLayers)
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(name)) continue;
                            layers.Add((i, name!));
                        }
                        finally
                        {
                            i++;
                        }
                    }
                }

                if (node.m_SortingLayers is { } docSortingLayers)
                {
                    foreach (var entry in docSortingLayers)
                    {
                        if (string.IsNullOrEmpty(entry.name)) continue;
                        sortingLayers.Add((unchecked((int)entry.uniqueID), entry.name!));
                    }
                }
            }
        }
        finally
        {
            // tags
            {
                sourceBuilder.AppendLine($$"""
    public enum Tags
    {
""");

                for (var i = 0; i < tags.Count; i++)
                {
                    var tag = tags[i];
                    string identifierName = Utils.ToIdentifierCompatible(tag);
                    sourceBuilder.AppendLine($$"""
        /// <summary>
        /// {{tag}}
        /// </summary>
        @{{identifierName}} = {{i.ToString()}},

""");
                }

                sourceBuilder.AppendLine($$"""
    } // enum Tags

""");
            }

            // tag data
            {
                sourceBuilder.AppendLine($$"""
    internal static class TagData
    {
        public static readonly string[] tagNames =
        {
""");

                for (var i = 0; i < tags.Count; i++)
                {
                    var tag = tags[i];
                    string literalName = Utils.ToCSharpEscapedVerbatimLiteral(tag);
                    sourceBuilder.AppendLine($$"""
            @"{{literalName}}",
""");
                }

                sourceBuilder.AppendLine($$"""
        };
    } // class TagData

    public static class TagUtility
    {
        private static global::System.Collections.ObjectModel.ReadOnlyCollection<string> tagNames;
        public static global::System.Collections.ObjectModel.ReadOnlyCollection<string> TagNames => tagNames ??= global::System.Array.AsReadOnly(TagData.tagNames);

        public static string GetTagName(Tags tag)
        {
            return TagData.tagNames[(int)tag];
        }

        public static string ToTagName(this Tags tag)
        {
            return TagData.tagNames[(int)tag];
        }

        public static bool TryGetTagValue(string tagName, out Tags result)
        {
            int index = global::System.Array.IndexOf(TagData.tagNames, tagName);
            result = (Tags)index;
            return index >= 0;
        }
    } // class TagUtility

""");
            }

            // layers
            {
                sourceBuilder.AppendLine($$"""
    public enum Layers
    {
""");

                foreach (var layer in layers)
                {
                    string identifierName = Utils.ToIdentifierCompatible(layer.name);
                    sourceBuilder.AppendLine($$"""
        /// <summary>
        /// {{layer.name}}
        /// </summary>
        @{{identifierName}} = {{layer.index.ToString()}},

""");
                }

                sourceBuilder.AppendLine($$"""
    } // enum Layers

""");
            }

            // sorting layers
            {
                sourceBuilder.AppendLine($$"""
    public enum SortingLayers
    {
""");

                foreach (var sortingLayer in sortingLayers)
                {
                    string identifierName = Utils.ToIdentifierCompatible(sortingLayer.name);
                    sourceBuilder.AppendLine($$"""
        /// <summary>
        /// {{sortingLayer.name}}
        /// </summary>
        @{{identifierName}} = {{sortingLayer.id.ToString()}},

""");
                }

                sourceBuilder.AppendLine($$"""
    } // enum SortingLayers

""");
            }

            sourceBuilder.AppendLine($$"""
} //namespace UniTyped.Reflection

""");
        }
    }

    private static string GetProjectPathFromAnchor(UniTypedGeneratorContext context)
    {
        var compilation = context.Compilation;

        if (compilation.AssemblyName != "UniTyped")
            throw new InvalidOperationException(
                "Project path is only available in UniTyped runtime assembly compilation.");

        var projectAnchorSyntax = context.UniTypedProjectAnchor.DeclaringSyntaxReferences[0];
        var projectAnchorPath = projectAnchorSyntax.SyntaxTree.FilePath;

        // ProjectAnchor is located in
        //  - Packages/com.ruccho.unityped/Runtime/Scripts/ProjectAnchor.cs (in dev)
        //  - Library/PackageCache/com.ruccho.unityped/Runtime/Scripts/ProjectAnchor.cs (imported from git or package repository)
        // when imported as local dependency, correct path is not obtained with this approach.
        var packageDirectory = new DirectoryInfo(Path.GetDirectoryName(projectAnchorPath)).Parent?.Parent?.Parent;

        if (packageDirectory == null)
            throw new NullReferenceException("Project path cannot be determined from source.");

        DirectoryInfo? projectDirectory = null;
        switch (packageDirectory.Name)
        {
            case "Packages":
                projectDirectory = packageDirectory.Parent;
                break;
            case "PackageCache":
                projectDirectory = packageDirectory.Parent?.Parent;
                break;
        }

        if (projectDirectory == null)
            throw new NullReferenceException("Project path cannot be determined from source.");

        return projectDirectory.FullName;
    }
}
