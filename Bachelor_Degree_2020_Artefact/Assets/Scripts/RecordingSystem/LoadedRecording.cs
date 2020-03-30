using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class LoadedRecording
{
    private ReplaySystem owner;
    /*[SerializeField] */public string name = "Test-text";
    private string filePath = "";
    private RecordedDataList dataList = null;
    private readonly GameObject replayActorPrefab = null;
    private GameObject replayActor = null;
    private Camera replayCamera = null;
    private Transform controlledTransform = null;
    private int currentNode = 0;
    private bool visible = true;
    private bool soloMode = false;
    //private bool currentVisibility = true;

    public LoadedRecording(ReplaySystem ownerSystem, string filename, string path, RecordedDataList recordedData, GameObject actorPrefab, Camera camera)
    {
        owner = ownerSystem;
        name = filename;
        filePath = path;
        dataList = recordedData;
        replayActorPrefab = actorPrefab;
        replayActor = GameObject.Instantiate(actorPrefab);
        replayActor.name = "Actor_" + name;
        replayCamera = camera;
        controlledTransform = replayActor.transform;
    }

    public void DoCleanup()
    {
        GameObject.DestroyImmediate(replayActor);
    }

    public string GetFilePath()
    {
        return filePath;
    }

    public void GoToNextNode()
    {
        Debug.Log("Loaded recording: Go to next node");
    }

    public void GoToPreviousNode()
    {
        Debug.Log("Loaded recording: Go to previous node");
    }

    public void SetReplayActorVisibility(bool shouldBeVisible)
    {
        visible = shouldBeVisible;
    }

    public bool IsVisible()
    {
        return visible;
    }

    public void MakeReplayActorSoloVisible(bool shouldBeSoloVisible)
    {
        soloMode = shouldBeSoloVisible;
    }

    public bool IsSoloVisible()
    {
        return soloMode;
    }

    public bool GetActualVisibility()
    {
        return replayActor.activeSelf;
    }

    public void UpdateActorActualVisibility(bool anySolo)
    {
        if(IsSoloVisible())
        {
            SetActorActualVisibility(true);
        }
        else
        {
            SetActorActualVisibility(!anySolo && visible);
        }
    }

    private void SetActorActualVisibility(bool shouldBeVisible)
    {
        if (shouldBeVisible && !replayActor.activeSelf)
        {
            replayActor.SetActive(true);
        }
        else if(!shouldBeVisible && replayActor.activeSelf)
        {
            replayActor.SetActive(false);
        }
    }
}
