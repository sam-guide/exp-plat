using UnityEngine;
using UnityEditor;
using Substitution;

#if (UNITY_EDITOR)
[CustomEditor(typeof(TactileSubstitution), true)]
public class TactileSubstitutionCustomEditor : Editor {

    public override void OnInspectorGUI() {
        serializedObject.Update();

        TactileSubstitution tss = (TactileSubstitution)target;
        if(GUILayout.Button("Update")) {
            tss.UpdateBreaks();
        }

        DrawDefaultInspector();

        // Save all changes made on the inspector
        serializedObject.ApplyModifiedProperties();
    }
}
#endif