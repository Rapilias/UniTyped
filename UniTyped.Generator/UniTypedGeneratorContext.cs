﻿using Microsoft.CodeAnalysis;

namespace UniTyped.Generator;

public class UniTypedGeneratorContext
{
    public UniTypedGenerator.SyntaxContextReceiver Receiver { get; }
    public GeneratorExecutionContext RoslynContext { get; }
    public INamedTypeSymbol List { get; }
    public INamedTypeSymbol Serializable { get; }
    public INamedTypeSymbol SerializeField { get; }
    public INamedTypeSymbol UnityEngineObject { get; }
    public INamedTypeSymbol AnimationCurve { get; }
    public INamedTypeSymbol BoundsInt { get; }
    public INamedTypeSymbol Bounds { get; }
    public INamedTypeSymbol Color { get; }
    public INamedTypeSymbol Hash128 { get; }
    public INamedTypeSymbol Quaternion { get; }
    public INamedTypeSymbol RectInt { get; }
    public INamedTypeSymbol Rect { get; }
    public INamedTypeSymbol Vector2Int { get; }
    public INamedTypeSymbol Vector2 { get; }
    public INamedTypeSymbol Vector3Int { get; }
    public INamedTypeSymbol Vector3 { get; }
    public INamedTypeSymbol Vector4 { get; }
    public INamedTypeSymbol UniTypedField { get; }

    private readonly TypedViewDefinition[] builtinViews;

    public IReadOnlyList<TypedViewDefinition> CustomValueViews => customValueViews;

    private List<TypedViewDefinition> customValueViews = new List<TypedViewDefinition>();

    private static readonly UnsuuportedViewDefinition unsuuportedView = new UnsuuportedViewDefinition();

    private readonly HashSet<INamespaceSymbol> targetNamespaces = new HashSet<INamespaceSymbol>(SymbolEqualityComparer.Default);

    public IReadOnlyCollection<INamespaceSymbol> TargetNamespaces => targetNamespaces;

    public void AddTargetNamespace(INamespaceSymbol namespaceSymbol) => targetNamespaces.Add(namespaceSymbol);

    public TypedViewDefinition GetTypedView(UniTypedGeneratorContext context, ITypeSymbol type)
    {
        foreach (var v in builtinViews)
        {
            if (v.Match(this, type)) return v;
        }

        //original generics
        if (type is INamedTypeSymbol namedType)
        {
            if (namedType.IsGenericType) namedType = namedType.OriginalDefinition;
            var custom = GetOrAddObjectView(context, namedType);
            if (custom.Match(this, type)) return custom;
        }

        return unsuuportedView;
    }

    public TypedViewDefinition GetOrAddObjectView(UniTypedGeneratorContext context, INamedTypeSymbol type,
        bool rootObject = false)
    {
        foreach (var v in customValueViews)
        {
            if (v.Match(this, type)) return v;
        }

        TypedViewDefinition newView;

        if (!rootObject && Utils.IsDerivedFrom(type, context.UnityEngineObject))
        {
            newView = new UnityEngineObjectReferenceValueViewDefinition(context, type);
        }
        else
        {
            switch (type.TypeKind)
            {
                case TypeKind.Class:
                case TypeKind.Struct:
                    newView = new CustomValueViewDefinition(this, type);
                    break;
                case TypeKind.Enum:
                    newView = new EnumValueViewDefinition(type);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        customValueViews.Add(newView);

        return newView;
    }

    public UniTypedGeneratorContext(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not UniTypedGenerator.SyntaxContextReceiver receiver)
            throw new InvalidOperationException();

        Receiver = receiver;

        RoslynContext = context;

        INamedTypeSymbol GetSymbol(string metadataName) =>
            context.Compilation.GetTypeByMetadataName(metadataName) ??
            throw new NullReferenceException($"Symbol not found: {metadataName}");

        List = GetSymbol("System.Collections.Generic.List`1");
        Serializable = GetSymbol("System.SerializableAttribute");
        SerializeField = GetSymbol("UnityEngine.SerializeField");
        UnityEngineObject = GetSymbol("UnityEngine.Object");
        AnimationCurve = GetSymbol("UnityEngine.AnimationCurve");
        BoundsInt = GetSymbol("UnityEngine.BoundsInt");
        Bounds = GetSymbol("UnityEngine.Bounds");
        Color = GetSymbol("UnityEngine.Color");
        Hash128 = GetSymbol("UnityEngine.Hash128");
        Quaternion = GetSymbol("UnityEngine.Quaternion");
        RectInt = GetSymbol("UnityEngine.RectInt");
        Rect = GetSymbol("UnityEngine.Rect");
        Vector2Int = GetSymbol("UnityEngine.Vector2Int");
        Vector2 = GetSymbol("UnityEngine.Vector2");
        Vector3Int = GetSymbol("UnityEngine.Vector3Int");
        Vector3 = GetSymbol("UnityEngine.Vector3");
        Vector4 = GetSymbol("UnityEngine.Vector4");

        UniTypedField = GetSymbol("UniTyped.UniTypedFieldAttribute");

        builtinViews = new TypedViewDefinition[]
        {
            new TypeParameterViewDefinition(),
            new ArrayViewDefinition(),
            new DirectValueViewDefinition("global::UniTyped.Editor.SerializedPropertyViewByte",
                GetSymbol("System.Byte")),
            new DirectValueViewDefinition("global::UniTyped.Editor.SerializedPropertyViewSByte",
                GetSymbol("System.SByte")),
            new DirectValueViewDefinition("global::UniTyped.Editor.SerializedPropertyViewShort",
                GetSymbol("System.Int16")),
            new DirectValueViewDefinition("global::UniTyped.Editor.SerializedPropertyViewUShort",
                GetSymbol("System.UInt16")),
            new DirectValueViewDefinition("global::UniTyped.Editor.SerializedPropertyViewInt",
                GetSymbol("System.Int32")),
            new DirectValueViewDefinition("global::UniTyped.Editor.SerializedPropertyViewUInt",
                GetSymbol("System.UInt32")),
            new DirectValueViewDefinition("global::UniTyped.Editor.SerializedPropertyViewLong",
                GetSymbol("System.Int64")),
            new DirectValueViewDefinition("global::UniTyped.Editor.SerializedPropertyViewULong",
                GetSymbol("System.UInt64")),
            new DirectValueViewDefinition("global::UniTyped.Editor.SerializedPropertyViewFloat",
                GetSymbol("System.Single")),
            new DirectValueViewDefinition("global::UniTyped.Editor.SerializedPropertyViewDouble",
                GetSymbol("System.Double")),
            new DirectValueViewDefinition("global::UniTyped.Editor.SerializedPropertyViewBool",
                GetSymbol("System.Boolean")),
            new DirectValueViewDefinition("global::UniTyped.Editor.SerializedPropertyViewString",
                GetSymbol("System.String")),
            new DirectValueViewDefinition("global::UniTyped.Editor.SerializedPropertyViewChar",
                GetSymbol("System.Char")),

            new DirectValueViewDefinition("global::UniTyped.Editor.SerializedPropertyViewAnimationCurve",
                AnimationCurve),
            new DirectValueViewDefinition("global::UniTyped.Editor.SerializedPropertyViewBoundsInt", BoundsInt),
            new DirectValueViewDefinition("global::UniTyped.Editor.SerializedPropertyViewBounds", Bounds),
            new DirectValueViewDefinition("global::UniTyped.Editor.SerializedPropertyViewColor", Color),
            new DirectValueViewDefinition("global::UniTyped.Editor.SerializedPropertyViewHash128", Hash128),
            new DirectValueViewDefinition("global::UniTyped.Editor.SerializedPropertyViewQuaternion",
                Quaternion),
            new DirectValueViewDefinition("global::UniTyped.Editor.SerializedPropertyViewRectInt", RectInt),
            new DirectValueViewDefinition("global::UniTyped.Editor.SerializedPropertyViewRect", Rect),
            new DirectValueViewDefinition("global::UniTyped.Editor.SerializedPropertyViewVector2Int",
                Vector2Int),
            new DirectValueViewDefinition("global::UniTyped.Editor.SerializedPropertyViewVector2", Vector2),
            new DirectValueViewDefinition("global::UniTyped.Editor.SerializedPropertyViewVector3Int",
                Vector3Int),
            new DirectValueViewDefinition("global::UniTyped.Editor.SerializedPropertyViewVector3", Vector3),
            new DirectValueViewDefinition("global::UniTyped.Editor.SerializedPropertyViewVector4", Vector4),
        };
    }
}