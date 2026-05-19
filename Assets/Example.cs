using System;
using System.Collections.Generic;
using UnityEngine;
using UniTyped;
#if UNITY_EDITOR
using UniTyped.Editor;
#endif

[UniTyped]
public class Example : MonoBehaviour
{
#pragma warning disable 0414 // assigned but never used: this is a serialization-only sample class

    // primitive C# types

    [SerializeField] private byte someByte = 0;
    [SerializeField] private sbyte someSbyte = 0;
    [SerializeField] private short someShort = 0;
    [SerializeField] private ushort someUshort = 0;
    [SerializeField] private int someInt = 0;
    [SerializeField] private uint someUint = 0;
    [SerializeField] private long someLong = 0;
    [SerializeField] private ulong someUlong = 0;
    [SerializeField] private float someFloat = 0;
    [SerializeField] private double someDouble = 0;
    [SerializeField] private bool someBool = false;
    [SerializeField] private string someString = string.Empty;
    [SerializeField] private char someChar = '\0';

    // built-in unity types

    [SerializeField] private AnimationCurve someAnimationCurve = null;
    [SerializeField] private BoundsInt someBoundsInt = default;
    [SerializeField] private Bounds someBounds = default;
    [SerializeField] private Color someColor = default;
    [SerializeField] private Hash128 someHash128 = default;
    [SerializeField] private Quaternion someQuaternion = default;
    [SerializeField] private RectInt someRectInt = default;
    [SerializeField] private Rect someRect = default;
    [SerializeField] private Vector2Int someVector2Int = default;
    [SerializeField] private Vector2 someVector2 = default;
    [SerializeField] private Vector3Int someVector3Int = default;
    [SerializeField] private Vector3 someVector3 = default;
    [SerializeField] private Vector4 someVector4 = default;

    // enum

    [SerializeField] private SomeEnum someEnum = default;
    [SerializeField] private SomeEnumSmall someEnumSmall = default;

    public enum SomeEnum
    {
        Option0,
        Option1
    }

    public enum SomeEnumSmall : byte
    {
        Option0,
        Option1
    }

    // array / list

    [SerializeField] private int[] someArray = default;
    [SerializeField] private List<int> someList = default;

    // UnityEngine.Object

    [SerializeField] private UnityEngine.Object someObject = default;
    [SerializeField] private Texture2D someTexture = default;

    // custom serializable

    [SerializeField] private SerializableTest someSerializable = default;
    [SerializeReference] private object[] someManagedReferenceArray = default;
    [SerializeReference] private List<object> someManagedReferenceList = default;

    [Serializable]
    public class SerializableTest
    {
        [SerializeField] private int value;
    }

    // SerializeReference

    [SerializeReference] private object someManagedReference = default;

    // fixed buffer

    [SerializeField] private FixedBufferContainer fixedBufferContainer = default;

    [Serializable]
    public unsafe struct FixedBufferContainer
    {
        [SerializeField] private fixed char fixedBuffer[30];
    }

#pragma warning restore 0414
}


#if UNITY_EDITOR

[UnityEditor.CustomEditor(typeof(Example))]
public class ExampleEditor : UnityEditor.Editor
{

    public override void OnInspectorGUI()
    {
        var view = new UniTyped.Generated.ExampleView()
        {
            Target = serializedObject
        };

        view.someArray[0].Set(0);

        // array access
        for (int i = 0; i < view.someArray.Length; i++)
        {
            Debug.Log(view.someArray[i].Value);

            // set value
            view.someArray[i].Set(100);
            // ... or
            var elementView = view.someArray[i];
            elementView.Value = 100;
        }

        // also accessible with IEnumerator<T>
        foreach (var element in view.someArray)
        {
            Debug.Log(element.Value);
        }

        serializedObject.ApplyModifiedProperties();

        base.OnInspectorGUI();
    }
}

#endif
