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

    private static Color32 toggleButtonTextColor = new Color32(0, 88, 133, 255);

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
            //return;
        }

        //if(GUILayout.Button("reset solo number"))
        //{
        //    replayer.SoloNumberReset();
        //}

        EditorGUI.BeginChangeCheck();

        GUILayout.BeginHorizontal("box");

        if(GUILayout.Button("Load single file"))
        {
            string filepath = EditorUtility.OpenFilePanel("Load recorded data", "", RecordedDataList.GetFileExtension());
            if (filepath.Length > 0)
            {
                replayer.LoadSingleFile(filepath);
            }
        }
        if (GUILayout.Button("Load multiple files"))
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
        replayer.UnloadRemovedRecordings();
        replayer.UpdateActorsActualVisibility();

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(target);
        }
    }

    private void DrawReplayListHeader(Rect rect)
    {
        GUI.Label(rect, "Loaded recordings");
    }

    private void DrawLoadedReplayElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        SerializedProperty element = loadedRecordings.GetArrayElementAtIndex(index);
        Rect position = rect;
        string displayName = element.displayName + (replayer.GetActualVisibility(index) ? " Visible" : " Hidden");
        position = EditorGUI.PrefixLabel(position,
                                         GUIUtility.GetControlID(FocusType.Passive),
                                         new GUIContent(displayName));
        //EditorGUI.Toggle(position, false);

        EditorGUI.BeginChangeCheck();

        GUIStyle elementButtonStyle = new GUIStyle(GUI.skin.button);
        elementButtonStyle.alignment = TextAnchor.MiddleCenter;
        GUIStyle toggledElementButtonStyle = new GUIStyle(elementButtonStyle);
        toggledElementButtonStyle.normal.textColor = toggleButtonTextColor;
        toggledElementButtonStyle.active.textColor = toggledElementButtonStyle.normal.textColor;
        float buttonRightPlacementPadding = 6.0f;

        float remButtonWidth = 60.0f;
        Rect remButtonRect = new Rect(position.x + position.width - remButtonWidth - buttonRightPlacementPadding * 0.5f,
                                      position.y, remButtonWidth, position.height);
        if(GUI.Button(remButtonRect, "Unload", elementButtonStyle))
        {
            replayer.RemoveRecording(index);
        }
        //position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), new GUIContent(element.FindPropertyRelative("name").stringValue));


        float hideButtonWidth = 50.0f;
        Rect visibilityButtonRect = new Rect(remButtonRect.x - hideButtonWidth - buttonRightPlacementPadding,
                                             remButtonRect.y, hideButtonWidth, remButtonRect.height);

        using (new EditorGUI.DisabledScope(replayer.IsAnyActorSoloVisible()))
        {
            if (replayer.IsRecordingActorVisible(index))
            {
                if (GUI.Button(visibilityButtonRect, "Hide", elementButtonStyle))
                {
                    replayer.SetRecordingActorVisibility(index, false);
                }
            }
            else
            {
                if (GUI.Button(visibilityButtonRect, "Hidden", toggledElementButtonStyle))
                {
                    replayer.SetRecordingActorVisibility(index, true);
                }
            }
        }

        float soloButtonWidth = 50.0f;
        Rect soloButtonRect = new Rect(visibilityButtonRect.x - soloButtonWidth - buttonRightPlacementPadding,
                                       visibilityButtonRect.y, soloButtonWidth, visibilityButtonRect.height);
        if (replayer.IsRecordingActorSolo(index))
        {
            if(GUI.Button(soloButtonRect, "Solo", toggledElementButtonStyle))
            {
                replayer.SetRecordingActorSolo(index, false);
            }
        }
        else
        {
            if(GUI.Button(soloButtonRect, "Solo", elementButtonStyle))
            {
                replayer.SetRecordingActorSolo(index, true);
            }
        }


        


        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(target);
        }
    }
    
}
