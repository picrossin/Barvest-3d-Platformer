using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Platform))]
public class PlatformEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Platform myPlatform = (Platform)target;
        if (GUILayout.Button("Add Position To Path"))
        {
            myPlatform.AddPositionToPath();
            EditorUtility.SetDirty(target);
        }
    }
}

