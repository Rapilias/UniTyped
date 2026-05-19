using System.Text;
using Microsoft.CodeAnalysis;

namespace UniTyped.Generator.SerializationViews;

public static class TypedViewGenerator
{
    public static void GenerateViews(UniTypedGeneratorContext context, StringBuilder sourceBuilder)
    {
        foreach (var uniTypedType in context.Collector.UniTypedTypes)
        {
            var semanticModel = context.Compilation.GetSemanticModel(uniTypedType.SyntaxTree);
            var symbol = semanticModel.GetDeclaredSymbol(uniTypedType) as INamedTypeSymbol;
            if (symbol == null) continue;

            context.GetTypedView(context, symbol, ViewUsage.Root);
        }

        for (int i = 0; i < context.RuntimeViews.Count; i++)
        {
            var v = context.RuntimeViews[i];
            v.Resolve(context);
        }

        //Build namespace tree

        var globalNamespace = new TypePathNode();

        TypePathNode GetNode(TypePath path)
        {
            TypePathNode parent;
            if (path.Parent != null)
            {
                parent = GetNode(path.Parent);
            }
            else
            {
                parent = globalNamespace;
            }

            var existing = parent.Children.FirstOrDefault(c => c.Path == path);
            if (existing != null) return existing;

            var node = new TypePathNode(path);
            parent.Children.Add(node);
            return node;
        }

        foreach (var v in context.RuntimeViews.OfType<GeneratedViewDefinition>())
        {
            var path = v.GetFullTypePath(context);
            sourceBuilder.AppendLine($"// {path} ({v.SourceType.ToString()})");
            var node = GetNode(path);
            node.View = v;
        }

        void GenerateFromTree(TypePathNode node)
        {
            if (node.IsNamespace)
            {
                if (!node.IsGlobalNamesapce)
                {
                    var path = node.Path!;
                    if (path.TypeParams.Length > 0)
                    {
                        sourceBuilder.Append($$"""
public static class {{path.Name}}<{{string.Join(", ", path.TypeParams.Select(p => p.Name))}}>
""");

                        foreach (var p in path.TypeParams)
                        {
                            sourceBuilder.Append(
                                $" where {p.Name} : struct, global::UniTyped.Editor.ISerializedPropertyView");
                        }

                        sourceBuilder.AppendLine();

                        sourceBuilder.AppendLine($$"""
{
""");
                    }
                    else
                    {
                        sourceBuilder.AppendLine($$"""
namespace {{path.Name}}
{
""");
                    }
                }

                foreach (var child in node.Children)
                {
                    GenerateFromTree(child);
                    sourceBuilder.AppendLine();
                }

                if (!node.IsGlobalNamesapce)
                {
                    var path = node.Path!;
                    if (path.TypeParams.Length > 0)
                    {
                        sourceBuilder.AppendLine($$"""
} // class {{path.Name}}
""");
                    }
                    else
                    {
                        sourceBuilder.AppendLine($$"""
} // namespace {{path.Name}}
""");
                    }
                }
            }
            else
            {
                var view = node.View!;
                view.GenerateViewTypeOpen(context, sourceBuilder);

                view.GenerateViewTypeContent(context, sourceBuilder);

                foreach (var child in node.Children)
                {
                    GenerateFromTree(child);
                    sourceBuilder.AppendLine();
                }

                view.GenerateViewTypeClose(context, sourceBuilder);
            }
        }

        GenerateFromTree(globalNamespace);
    }
}