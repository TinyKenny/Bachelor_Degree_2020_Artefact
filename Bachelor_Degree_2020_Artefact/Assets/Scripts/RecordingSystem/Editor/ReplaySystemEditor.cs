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
    private int selectionIndexChangeByRemove = 0;

    private GUIStyle elementButtonStyle;
    private GUIStyle toggledElementButtonStyle;
    private GUIStyle notImplementedButtonStyle;
    private static Color32 toggleButtonTextColor = new Color32(0, 88, 133, 255);
    private static Color32 notImplementedButtonTextColor = new Color32(179, 4, 4, 255);

    private bool displayControlsForAll = true;
    private int targetNodeIndexAllRecordings = 0;
    private float targetTimeAllRecordings = 0.0f;
    private float targetTimePercentAllRecordings = 0.0f;

    private bool displayControlsForSelected = true;
    private int targetNodeIndexSelectedRecording = 0;
    private float targetTimeSelectedRecording = 0.0f;
    private float targetTimePercentSelectedRecording = 0.0f;


    private void OnEnable()
    {
        replayer = (ReplaySystem)target;
        loadedRecordings = serializedObject.FindProperty("loadedRecordings");
        reorderable = new ReorderableList(serializedObject, loadedRecordings, false, true, false, false);

        //reorderable.elementHeight = 50.0f;

        reorderable.drawHeaderCallback += DrawReplayListHeader;
        reorderable.drawElementCallback += DrawLoadedReplayElement;
        selectionIndexChangeByRemove = 0;

        toggleButtonTextColor = new Color32(0, 88, 133, 255);
        notImplementedButtonTextColor = new Color32(179, 4, 4, 255);

        displayControlsForAll = true;
        displayControlsForSelected = true;

        //elementButtonStyle = new GUIStyle(GUI.skin.button);
        //elementButtonStyle.alignment = TextAnchor.MiddleCenter;
        //toggledElementButtonStyle = new GUIStyle(elementButtonStyle);
        //toggledElementButtonStyle.normal.textColor = toggleButtonTextColor;
        //toggledElementButtonStyle.active.textColor = toggledElementButtonStyle.normal.textColor;

    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Replay system controls are disabled outside of play mode", MessageType.Info);
            //return;
        }

        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {

            if (elementButtonStyle == null)
            {
                elementButtonStyle = new GUIStyle(GUI.skin.button);
                elementButtonStyle.alignment = TextAnchor.MiddleCenter;
            }
            if (toggledElementButtonStyle == null)
            {
                toggledElementButtonStyle = new GUIStyle(elementButtonStyle);
                toggledElementButtonStyle.normal.textColor = toggleButtonTextColor;
                toggledElementButtonStyle.active.textColor = toggledElementButtonStyle.normal.textColor;
            }
            if (notImplementedButtonStyle == null)
            {
                notImplementedButtonStyle = new GUIStyle(elementButtonStyle);
                notImplementedButtonStyle.normal.textColor = notImplementedButtonTextColor;
                notImplementedButtonStyle.active.textColor = notImplementedButtonStyle.normal.textColor;
            }

            if (GUILayout.Button("Reset recordins list and variables"))
            {
                replayer.ResetValues();
            }

            EditorGUI.BeginChangeCheck();

            if (GUILayout.Button("Load recording file"))
            {
                string filepath = EditorUtility.OpenFilePanel("Load recorded data", "", RecordedDataList.GetFileExtension());
                if (filepath.Length > 0)
                {
                    replayer.LoadSingleFile(filepath);
                }
            }

            using (new EditorGUI.DisabledScope(loadedRecordings.arraySize <= 0))
            {
                GUILayout.BeginVertical("box");
                GUILayout.BeginVertical("box");

                displayControlsForAll = EditorGUILayout.BeginToggleGroup("Controls for all loaded recordings", displayControlsForAll);
                EditorGUILayout.EndToggleGroup();

                if (displayControlsForAll)
                {
                    GUILayout.BeginHorizontal();

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

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Jump to first node"))
                    {
                        replayer.GoToFirstNode();
                    }
                    if (GUILayout.Button("Jump to final node"))
                    {
                        replayer.GoToFinalNode();
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginVertical();

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Jump to node"))
                    {
                        replayer.GoToNode(targetNodeIndexAllRecordings);
                    }
                    targetNodeIndexAllRecordings = EditorGUILayout.IntField(targetNodeIndexAllRecordings);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Jump to time"))
                    {
                        replayer.GoToTime(targetTimeAllRecordings);
                    }
                    targetTimeAllRecordings = EditorGUILayout.FloatField(targetTimeAllRecordings);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Jump to time %"))
                    {
                        replayer.GoToTimePercent(targetTimePercentAllRecordings);
                    }
                    targetTimePercentAllRecordings = EditorGUILayout.Slider(targetTimePercentAllRecordings, 0.0f, 1.0f);
                    GUILayout.EndHorizontal();

                    GUILayout.EndVertical();
                }

                GUILayout.EndVertical();


                GUILayout.BeginVertical("box");

                displayControlsForSelected = EditorGUILayout.BeginToggleGroup("Controls for selected recording", displayControlsForSelected);
                EditorGUILayout.EndToggleGroup();

                if (displayControlsForSelected)
                {
                    if (reorderable.index < 0)
                    {
                        GUILayout.Label("Selected: None");
                    }
                    else
                    {
                        string toPrint = "Selected: ";

                        toPrint += replayer.RecordingByIndexGetName(reorderable.index);
                        toPrint += " (T = " + replayer.RecordingByIndexGetTime(reorderable.index).ToString();
                        toPrint += " N = " + replayer.RecordingByIndexGetNodeIndex(reorderable.index).ToString();
                        toPrint += ")";

                        GUILayout.Label(toPrint);
                    }

                    //GUILayout.Label(reorderable.index.ToString());

                    using (new EditorGUI.DisabledScope(reorderable.index < 0))
                    {
                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button("Previous node", notImplementedButtonStyle))
                        {
                            Debug.Log("Replay System Editor, Selected recording: previous node");
                        }
                        if (GUILayout.Button("Reverse", notImplementedButtonStyle))
                        {
                            Debug.Log("Replay System Editor, Selected recording: reverse");
                        }
                        if (GUILayout.Button("Pause", notImplementedButtonStyle))
                        {
                            Debug.Log("Replay System Editor, Selected recording: pause");
                        }
                        if (GUILayout.Button("Play", notImplementedButtonStyle))
                        {
                            Debug.Log("Replay System Editor, Selected recording: play");
                        }
                        if(GUILayout.Button("Next node", notImplementedButtonStyle))
                        {
                            Debug.Log("Replay System Editor, Selected recording: next node");
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button("Jump to first node", notImplementedButtonStyle))
                        {
                            Debug.Log("Replay System Editor, Selected recording: jump to first node");
                        }
                        if(GUILayout.Button("Jump to final node", notImplementedButtonStyle))
                        {
                            Debug.Log("Replay System Editor, Selected recording: jump to final node");
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.BeginVertical();

                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button("Jump to node", notImplementedButtonStyle))
                        {
                            //replayer.GoToNode(targetNodeIndexSelectedRecording);
                            Debug.Log("Replay System Editor, Selected recording: jump to node");
                        }
                        targetNodeIndexSelectedRecording = EditorGUILayout.IntField(targetNodeIndexSelectedRecording);
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button("Jump to time", notImplementedButtonStyle))
                        {
                            //replayer.GoToTime(targetTimeAllRecordings);
                            Debug.Log("Replay System Editor, Selected recording: jump to time");
                        }
                        targetTimeSelectedRecording = EditorGUILayout.FloatField(targetTimeSelectedRecording);
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button("Jump to time %", notImplementedButtonStyle))
                        {
                            //replayer.GoToTimePercent(targetTimePercentAllRecordings);
                            Debug.Log("Replay System Editor, Selected recording: jump to time percent");
                        }
                        targetTimePercentSelectedRecording = EditorGUILayout.Slider(targetTimePercentSelectedRecording, 0.0f, 1.0f);
                        GUILayout.EndHorizontal();

                        GUILayout.EndVertical();
                    }
                }

                GUILayout.EndVertical();

                GUILayout.EndVertical();
            }

            //EditorGUILayout.PropertyField(loadedRecordings, /*new GUIContent("test"),*/ true);
        }
        

        reorderable.DoLayoutList();
        //EditorGUILayout.HelpBox(loadedRecordings.isArray ? "yes" : "no", MessageType.Info);
        replayer.UnloadRemovedRecordings();
        if (reorderable.index > -1)
        {
            reorderable.index -= selectionIndexChangeByRemove;
        }
        selectionIndexChangeByRemove = 0;
        replayer.UpdateActorsActualVisibility();

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(target);
        }
    }

    private void DrawReplayListHeader(Rect rect)
    {
        Rect labelPos = new Rect(rect.x, rect.y, 115.0f, rect.height);
        float deselectButtonWidth = 120.0f;
        Rect deselectButtonRect = new Rect(rect.x + rect.width - deselectButtonWidth, rect.y + 1.0f, deselectButtonWidth, rect.height - 2.0f);

        using (new EditorGUI.DisabledScope(reorderable.index < 0))
        {
            if (GUI.Button(deselectButtonRect, "Deselect recording"))
            {
                reorderable.index = -1;
            }
        }

        GUI.Label(labelPos, "Loaded recordings");
    }

    private void DrawLoadedReplayElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        SerializedProperty element = loadedRecordings.GetArrayElementAtIndex(index);
        Rect position = rect;

        position.height -= 2.0f;

        string displayName = element.displayName /*+ (replayer.GetActualVisibility(index) ? " Visible" : " Hidden")*/;
        position = EditorGUI.PrefixLabel(position,
                                         GUIUtility.GetControlID(FocusType.Passive),
                                         new GUIContent(displayName));
        //EditorGUI.Toggle(position, false);


        //GUIStyle elementButtonStyle = new GUIStyle(GUI.skin.button);
        //elementButtonStyle.alignment = TextAnchor.MiddleCenter;
        //GUIStyle toggledElementButtonStyle = new GUIStyle(elementButtonStyle);
        //toggledElementButtonStyle.normal.textColor = toggleButtonTextColor;
        //toggledElementButtonStyle.active.textColor = toggledElementButtonStyle.normal.textColor;

        EditorGUI.BeginChangeCheck();

        float buttonRightPlacementPadding = 2.0f;

        float remButtonWidth = 53.0f;
        Rect remButtonRect = new Rect(position.x + position.width - remButtonWidth - buttonRightPlacementPadding * 0.5f,
                                      position.y, remButtonWidth, position.height);
        if(GUI.Button(remButtonRect, "Unload", elementButtonStyle))
        {
            replayer.RecordingByIndexRemove(index);
            if (index == reorderable.index)
            {
                reorderable.index = -1;
            }
            else if (reorderable.index > index)
            {
                selectionIndexChangeByRemove++;
            }
        }
        //position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), new GUIContent(element.FindPropertyRelative("name").stringValue));


        float hideButtonWidth = 50.0f;
        Rect visibilityButtonRect = new Rect(remButtonRect.x - hideButtonWidth - buttonRightPlacementPadding,
                                             remButtonRect.y, hideButtonWidth, remButtonRect.height);

        using (new EditorGUI.DisabledScope(replayer.IsAnyActorSoloVisible()))
        {
            if (replayer.RecordingByIndexIsActorVisible(index))
            {
                if (GUI.Button(visibilityButtonRect, "Hide", elementButtonStyle))
                {
                    replayer.RecordingByIndexSetActorVisibility(index, false);
                }
            }
            else
            {
                if (GUI.Button(visibilityButtonRect, "Hidden", toggledElementButtonStyle))
                {
                    replayer.RecordingByIndexSetActorVisibility(index, true);
                }
            }
        }

        float soloButtonWidth = 40.0f;
        Rect soloButtonRect = new Rect(visibilityButtonRect.x - soloButtonWidth - buttonRightPlacementPadding,
                                       visibilityButtonRect.y, soloButtonWidth, visibilityButtonRect.height);
        if (replayer.RecordingByIndexIsActorSolo(index))
        {
            if(GUI.Button(soloButtonRect, "Solo", toggledElementButtonStyle))
            {
                replayer.RecordingByIndexSetActorSolo(index, false);
            }
        }
        else
        {
            if(GUI.Button(soloButtonRect, "Solo", elementButtonStyle))
            {
                replayer.RecordingByIndexSetActorSolo(index, true);
            }
        }

        float cameraButtonWidth = 57.0f;
        Rect cameraButtonRect = new Rect(soloButtonRect.x - cameraButtonWidth - buttonRightPlacementPadding,
                                         soloButtonRect.y, cameraButtonWidth, soloButtonRect.height);
        if (replayer.RecordingByIndexIsCameraController(index))
        {
            if(GUI.Button(cameraButtonRect, "Camera", toggledElementButtonStyle))
            {
                replayer.RecordingByIndexSetCameraController(index, false);
            }
        }
        else
        {
            if(GUI.Button(cameraButtonRect, "Camera", elementButtonStyle))
            {
                replayer.RecordingByIndexSetCameraController(index, true);
            }
        }


        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(target);
        }
    }
    
}
