#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Adds a prominent "Add Page" button at the TOP of the Inspector, above the
/// (potentially very long) pages list. Unity's default list drawer puts its
/// own +/- controls at the very bottom of the fully expanded list, which on
/// a list this size means scrolling past every existing entry just to add
/// one more - easy to miss entirely and mistake for "adding doesn't work".
/// </summary>
[CustomEditor(typeof(LearningContentData))]
public class LearningContentDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        LearningContentData data = (LearningContentData)target;

        // Keep serializedObject in sync with the target and always go through
        // SerializedProperty for edits in this method. Mixing a direct C#
        // mutation of `data.pages` with DrawDefaultInspector() (which does its
        // own serializedObject.Update()/ApplyModifiedProperties() cycle from a
        // separately cached snapshot) let the stale cache silently overwrite
        // the direct change back out - the "+" button appeared to do nothing.
        serializedObject.Update();

        SerializedProperty pagesProp = serializedObject.FindProperty("pages");

        EditorGUILayout.Space(6);

        using (new EditorGUILayout.HorizontalScope())
        {
            GUI.backgroundColor = new Color(0.55f, 0.85f, 0.55f);

            if (GUILayout.Button("+ Add New Page", GUILayout.Height(32)))
            {
                int nextNumber = 1;
                for (int i = 0; i < pagesProp.arraySize; i++)
                {
                    SerializedProperty pageNumProp = pagesProp
                        .GetArrayElementAtIndex(i)
                        .FindPropertyRelative("pageNumber");

                    if (pageNumProp.intValue >= nextNumber)
                        nextNumber = pageNumProp.intValue + 1;
                }

                int newIndex = pagesProp.arraySize;
                pagesProp.arraySize++;

                SerializedProperty newPage = pagesProp.GetArrayElementAtIndex(newIndex);
                // Clear whatever the array-resize duplicated from the previous
                // last element (Unity's default behavior) before setting ours.
                newPage.FindPropertyRelative("pageButtonImage").objectReferenceValue = null;
                newPage.FindPropertyRelative("video").FindPropertyRelative("videoName").stringValue = "";
                newPage.FindPropertyRelative("video").FindPropertyRelative("videoUrl").stringValue = "";
                newPage.FindPropertyRelative("activities").FindPropertyRelative("pages").arraySize = 0;

                newPage.FindPropertyRelative("pageNumber").intValue = nextNumber;
                newPage.FindPropertyRelative("pageName").stringValue = nextNumber.ToString();

                serializedObject.ApplyModifiedProperties();
            }

            GUI.backgroundColor = Color.white;
        }

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField($"Total pages: {pagesProp.arraySize}", EditorStyles.miniLabel);
        EditorGUILayout.Space(4);

        DrawDefaultInspector();
    }
}
#endif
