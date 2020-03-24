using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(ReplaySystem))]
public class ReplaySystemEditor : Editor
{
    private ReplaySystem replayer = null;
    private SerializedProperty loadedRecordings;
    private ReorderableList reorderable;

    private void OnEnable()
    {
        replayer = (ReplaySystem)target;
        loadedRecordings = serializedObject.FindProperty("loadedRecordings");
        reorderable = new ReorderableList(serializedObject, loadedRecordings, false, true, false, false);

        reorderable.drawHeaderCallback += DrawReplayListHeader;
        reorderable.drawElementCallback += DrawLoadedReplayElement;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Replay system controls are only available when the game is playing", MessageType.Info);
            return;
        }

        GUILayout.BeginHorizontal("box");

        if(GUILayout.Button("Load single file"))
        {
            replayer.LoadSingleFile();
        }
        if(GUILayout.Button("Load multiple files"))
        {
            replayer.LoadMultipleFiles();
        }

        GUILayout.EndHorizontal();

        using (new EditorGUI.DisabledScope(loadedRecordings.arraySize <= 0))
        {
            GUILayout.BeginVertical("box");
            GUILayout.BeginHorizontal("box");

            if (GUILayout.Button("Previous node"))
            {
                replayer.GoToPreviousNode();
            }
            if (GUILayout.Button("Reverse"))
            {
                replayer.StartReversePlayback();
            }
            if (GUILayout.Button("Pause"))
            {
                replayer.PausePlayback();
            }
            if (GUILayout.Button("Play"))
            {
                replayer.StartPlayback();
            }
            if (GUILayout.Button("Next node"))
            {
                replayer.GoToNextNode();
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("box");
            if (GUILayout.Button("Jump to first node"))
            {
                replayer.GoToFirstNode();
            }
            if (GUILayout.Button("Jump to final node"))
            {
                replayer.GoToFinalNode();
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        //EditorGUILayout.PropertyField(loadedRecordings, /*new GUIContent("test"),*/ true);

        reorderable.DoLayoutList();
        //EditorGUILayout.HelpBox(loadedRecordings.isArray ? "yes" : "no", MessageType.Info);
    }

    private void DrawReplayListHeader(Rect rect)
    {
        GUI.Label(rect, "Loaded recordings");
    }

    private void DrawLoadedReplayElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        SerializedProperty element = loadedRecordings.GetArrayElementAtIndex(index);
        Rect position = rect;
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), new GUIContent(element.displayName));
        //EditorGUI.Toggle(position, false);

        EditorGUI.BeginChangeCheck();

        GUIStyle elementButtonStyle = new GUIStyle(GUI.skin.button);
        elementButtonStyle.alignment = TextAnchor.MiddleCenter;
        GUIStyle toggledElementButtonStyle = new GUIStyle(elementButtonStyle);
        float buttonRightPlacementPadding = 3.0f;

        float remButtonWidth = 60.0f;
        Rect remButtonRect = new Rect(position.x + position.width - remButtonWidth - buttonRightPlacementPadding,
                                      position.y, remButtonWidth, position.height);
        if(GUI.Button(remButtonRect, "Unload", elementButtonStyle))
        {
            replayer.UnloadRecording(index);
        }
        //position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), new GUIContent(element.FindPropertyRelative("name").stringValue));


        float hideButtonWidth = 50.0f;
        Rect visibleButtonRect = new Rect(remButtonRect.x - hideButtonWidth - buttonRightPlacementPadding * 2,
                                       remButtonRect.y, hideButtonWidth, remButtonRect.height);
        if(GUI.Button(visibleButtonRect, "Hide", elementButtonStyle))
        {
            replayer.HideRecordingByIndex(index);
        }


        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(target);
        }
    }
    
}
