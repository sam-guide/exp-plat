using UnityEngine;
using UnityEditor;
using Substitution;

#if (UNITY_EDITOR)
[CustomEditor(typeof(AudioSubstitution), true)]
public class AudioSubstitutionCustomEditor : Editor {

    public override void OnInspectorGUI() {
        serializedObject.Update();

        AudioSubstitution ass = (AudioSubstitution)target;
        if(GUILayout.Button("Update")) {
            ass.UpdateBreaks();
        }

        DrawDefaultInspector();

        // Save all changes made on the inspector
        serializedObject.ApplyModifiedProperties();
    }
}
#endif