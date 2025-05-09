using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SwarmSpawner))]
public class SwarmSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SwarmSpawner spawner = (SwarmSpawner)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Spawn Swarm (Editor)"))
        {
            spawner.swarmManager.insects = new List<SwarmInsect>();
            Undo.RegisterFullObjectHierarchyUndo(spawner.gameObject, "Spawn Swarm");
            spawner.SpawnInsects();
        }

        if (GUILayout.Button("Register Children as Swarm"))
        {
            spawner.swarmManager.insects = new List<SwarmInsect>();
            Undo.RegisterFullObjectHierarchyUndo(spawner.gameObject, "Register Swarm");
            spawner.RegisterAllChildren();
        }
    }
}
