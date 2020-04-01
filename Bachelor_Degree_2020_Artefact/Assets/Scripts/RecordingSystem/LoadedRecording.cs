using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class LoadedRecording
{
    [SerializeField] private string name = "Test-text";
    private string filePath = "";
    private RecordedDataList dataList = null;

    private readonly GameObject replayActorPrefab = null;
    private GameObject replayActor = null;
    private Camera replayCamera = null;
    private Transform controlledTransform = null;
    private RecordedDataList.RecordedData currentNode = null;
    private int currentNodeIndex = 0;
    private float currentTime = 0.0f;
    private bool visible = true;
    private bool soloMode = false;

    private readonly GameObject heatmapObjectPrefab = null;
    private GameObject heatmapParent = null;

    public LoadedRecording(string filename, string path, RecordedDataList recordedData, GameObject heatmapMarkerPrefab, GameObject actorPrefab, Camera camera)
    {
        name = filename;
        filePath = path;
        dataList = recordedData;
        replayActorPrefab = actorPrefab;
        replayActor = GameObject.Instantiate(actorPrefab);
        replayActor.name = "Actor_" + name;
        replayCamera = camera;
        controlledTransform = replayActor.transform;
        GoToFirstNode();

        heatmapObjectPrefab = heatmapMarkerPrefab;
        heatmapParent = new GameObject();
    }

    public void DoCleanup()
    {
        GameObject.DestroyImmediate(replayActor);
        GameObject.DestroyImmediate(heatmapParent);
    }

    public string GetRecordingName()
    {
        return name;
    }

    public string GetFilePath()
    {
        return filePath;
    }

    public int GetCurrentNodeIndex()
    {
        return currentNodeIndex;
    }

    public int GetFinalNodeIndex()
    {
        return dataList.GetNodeCount() - 1;
    }

    public void GoToNextNode()
    {
        if(currentNode.nextData != null)
        {
            currentNode = currentNode.nextData;
            currentNodeIndex++;
            UpdateToCurrentNode();
        }
    }

    public void GoToPreviousNode()
    {
        if(currentNode.previousData != null)
        {
            currentNode = currentNode.previousData;
            currentNodeIndex--;
            UpdateToCurrentNode();
        }
    }

    public void GoToFirstNode()
    {
        currentNode = dataList.GetFirstNode();
        currentNodeIndex = 0;
        UpdateToCurrentNode();
    }

    public void GoToFinalNode()
    {
        currentNode = dataList.GetFinalNode();
        currentNodeIndex = GetFinalNodeIndex();
        UpdateToCurrentNode();
    }

    public void GoToNodeIndex(int nodeIndex)
    {
        if(nodeIndex <= 0)
        {
            GoToFirstNode();
            return;
        }
        if(nodeIndex >= GetFinalNodeIndex())
        {
            GoToFinalNode();
            return;
        }

        while (currentNodeIndex < nodeIndex && currentNode.nextData != null)
        {
            currentNode = currentNode.nextData;
            currentNodeIndex++;
        }
        while (currentNodeIndex > nodeIndex && currentNode.previousData != null)
        {
            currentNode = currentNode.previousData;
            currentNodeIndex--;
        }
        
        //currentNode = dataList.GetDataByIndex(nodeIndex);
        //currentNodeIndex = nodeIndex;
        UpdateToCurrentNode();
    }

    public float GetCurrentTime()
    {
        return currentTime;
    }

    public void GoToTime(float targetTime)
    {
        if (targetTime <= 0.001f)
        {
            GoToFirstNode();
        }
        else if (targetTime >= dataList.GetFinalNode().time - 0.001f)
        {
            GoToFinalNode();
        }
        else
        {
            while(currentNode.nextData != null && targetTime > currentNode.nextData.time)
            {
                currentNode = currentNode.nextData;
                currentNodeIndex++;
            }
            while(currentNode.previousData != null && targetTime < currentNode.time)
            {
                currentNode = currentNode.previousData;
                currentNodeIndex--;
            }

            if (currentNode.nextData == null)
            {
                Debug.LogError("LoadedRecording (" + name + "), GoToTime: target time " + targetTime.ToString() + " reached final node through while-loop");
                GoToFinalNode();
            }
            else
            {

                float timeBetweenNodes = currentNode.nextData.time - currentNode.time;
                float targetTimeFromNode = targetTime - currentNode.time;
                float lerpControl = targetTimeFromNode / timeBetweenNodes;

                controlledTransform.position = Vector3.Lerp(GetNodePosition(currentNode),
                                                            GetNodePosition(currentNode.nextData),
                                                            lerpControl);
                controlledTransform.rotation = Quaternion.Slerp(GetNodeRotation(currentNode),
                                                                GetNodeRotation(currentNode.nextData),
                                                                lerpControl);
                currentTime = targetTime;
            }
        }
    }

    public void GoToTimePercent(float targetTimePercent)
    {
        GoToTime(targetTimePercent * dataList.GetFinalNode().time);
    }

    public void ChangeTimeBy(float timeStep)
    {
        GoToTime(currentTime + timeStep);
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

    public void UpdateActorActualVisibility(bool anySolo, bool displayReplayMode)
    {
        if(IsSoloVisible())
        {
            SetActorActualVisibility(true, displayReplayMode);
        }
        else
        {
            SetActorActualVisibility(!anySolo && visible, displayReplayMode);
        }
    }

    public bool IsCameraController()
    {
        return controlledTransform == replayCamera.transform;
    }

    public void MakeCameraController(bool shouldControlCamera)
    {
        Transform oldControlledTransform = controlledTransform;
        if (shouldControlCamera)
        {
            controlledTransform = replayCamera.transform;
        }
        else
        {
            controlledTransform = replayActor.transform;
        }
        controlledTransform.position = oldControlledTransform.position;
        controlledTransform.rotation = oldControlledTransform.rotation;
    }

    private Vector3 GetNodePosition(RecordedDataList.RecordedData node)
    {
        return new Vector3(node.posX, node.posY, node.posZ);
    }

    private Quaternion GetNodeRotation(RecordedDataList.RecordedData node)
    {
        return new Quaternion(node.rotX, node.rotY, node.rotZ, node.rotW);
    }

    private void UpdateToCurrentNode()
    {
        controlledTransform.position = GetNodePosition(currentNode);
        controlledTransform.rotation = GetNodeRotation(currentNode);
        currentTime = currentNode.time;
    }

    public void HideHeatmap()
    {
        heatmapParent.SetActive(false);
    }

    private void SetActorActualVisibility(bool shouldBeVisible, bool displayReplayMode)
    {
        if (displayReplayMode)
        {
            if (shouldBeVisible && !replayActor.activeSelf && !IsCameraController())
            {
                replayActor.SetActive(true);
            }
            else if ((IsCameraController() || !shouldBeVisible) && replayActor.activeSelf)
            {
                replayActor.SetActive(false);
            }
        }
        else
        {
            replayActor.SetActive(false);
            heatmapParent.SetActive(shouldBeVisible);
        }
    }
}
