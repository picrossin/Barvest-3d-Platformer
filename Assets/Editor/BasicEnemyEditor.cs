using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BasicEnemy))]
public class BasicEnemyEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BasicEnemy myBasicEnemy = (BasicEnemy)target;
        if (GUILayout.Button("Add Position To Path"))
        {
            myBasicEnemy.AddPositionToPath();
        }
    }
}
