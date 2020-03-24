using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class LoadedRecording
{
    private RecordedDataList dataList = null;
    private GameObject replayActor = null;
    private Camera replayCamera = null;
    private Transform controlledTransform = null;
    private int currentNode = 0;
    [SerializeField] public string name = "Test-text";

    public LoadedRecording(string filename, RecordedDataList recordedData, GameObject actor, Camera camera)
    {
        name = filename;
        dataList = recordedData;
        replayActor = actor;
        replayCamera = camera;
        //controlledTransform = replayActor.transform;
    }

    public void GoToNextNode()
    {
        Debug.Log("Loaded recording: Go to next node");
    }

    public void HideReplayActor()
    {
        Debug.Log("Loaded recording: hide recording: " + name);
    }
}
