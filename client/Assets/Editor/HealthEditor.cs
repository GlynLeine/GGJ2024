using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Health))]
public class HealthEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Health health = (Health)target;
        if (GUILayout.Button("Kill"))
        {
            health.DamageHealth(1000);
        }
        if (GUILayout.Button("Respawn"))
        {
            health.Respawn();
        }


    }
}
