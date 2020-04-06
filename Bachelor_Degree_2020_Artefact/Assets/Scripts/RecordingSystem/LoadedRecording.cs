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
    private Color currentColor = Color.white;

    private readonly GameObject replayActorPrefab = null;
    private ReplayActorColorController replayActor = null;
    private Camera replayCamera = null;
    private Transform controlledTransform = null;
    private RecordedDataList.RecordedData currentReplayNode = null;
    private int currentNodeIndex = 0;
    private float currentTime = 0.0f;
    private bool visible = true;
    private bool soloMode = false;

    private readonly GameObject heatmapObjectPrefab = null;
    private HeatmapHolder heatmapParent = null;

    public LoadedRecording(string filename, string path, RecordedDataList recordedData, GameObject heatmapMarkerPrefab, GameObject actorPrefab, Camera camera)
    {
        name = filename;
        filePath = path;
        dataList = recordedData;
        currentColor = Color.black;

        replayActorPrefab = actorPrefab;
        replayActor = GameObject.Instantiate(actorPrefab).GetComponent<ReplayActorColorController>();
        replayActor.name = "Actor_" + name;
        replayCamera = camera;
        controlledTransform = replayActor.transform;
        GoToFirstNode();

        heatmapObjectPrefab = heatmapMarkerPrefab;
        GetHeatmapParent();

        SetColor(Color.white);
    }

    public void DoCleanup()
    {
        GameObject.DestroyImmediate(replayActor.gameObject);
        GameObject.DestroyImmediate(GetHeatmapParent().gameObject);
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
        if(currentReplayNode.nextData != null)
        {
            currentReplayNode = currentReplayNode.nextData;
            currentNodeIndex++;
            UpdateToCurrentNode();
        }
    }

    public void GoToPreviousNode()
    {
        if(currentReplayNode.previousData != null)
        {
            currentReplayNode = currentReplayNode.previousData;
            currentNodeIndex--;
            UpdateToCurrentNode();
        }
    }

    public void GoToFirstNode()
    {
        currentReplayNode = dataList.GetFirstNode();
        currentNodeIndex = 0;
        UpdateToCurrentNode();
    }

    public void GoToFinalNode()
    {
        currentReplayNode = dataList.GetFinalNode();
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

        while (currentNodeIndex < nodeIndex && currentReplayNode.nextData != null)
        {
            currentReplayNode = currentReplayNode.nextData;
            currentNodeIndex++;
        }
        while (currentNodeIndex > nodeIndex && currentReplayNode.previousData != null)
        {
            currentReplayNode = currentReplayNode.previousData;
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
            while(currentReplayNode.nextData != null && targetTime > currentReplayNode.nextData.time)
            {
                currentReplayNode = currentReplayNode.nextData;
                currentNodeIndex++;
            }
            while(currentReplayNode.previousData != null && targetTime < currentReplayNode.time)
            {
                currentReplayNode = currentReplayNode.previousData;
                currentNodeIndex--;
            }

            if (currentReplayNode.nextData == null)
            {
                Debug.LogError("LoadedRecording (" + name + "), GoToTime: target time " + targetTime.ToString() + " reached final node through while-loop");
                GoToFinalNode();
            }
            else
            {

                float timeBetweenNodes = currentReplayNode.nextData.time - currentReplayNode.time;
                float targetTimeFromNode = targetTime - currentReplayNode.time;
                float lerpControl = targetTimeFromNode / timeBetweenNodes;

                controlledTransform.position = Vector3.Lerp(GetNodePosition(currentReplayNode),
                                                            GetNodePosition(currentReplayNode.nextData),
                                                            lerpControl);
                controlledTransform.rotation = Quaternion.Slerp(GetNodeRotation(currentReplayNode),
                                                                GetNodeRotation(currentReplayNode.nextData),
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
        return replayActor.gameObject.activeSelf;
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
        if (shouldControlCamera)
        {
            controlledTransform = replayCamera.transform;
        }
        else
        {
            controlledTransform = replayActor.transform;
        }
        GoToTime(currentTime);
    }

    public Color GetColor()
    {
        return currentColor;
    }

    public void SetColor(Color colorToSet)
    {
        if (!colorToSet.Equals(currentColor))
        {
            currentColor = colorToSet;

            heatmapParent.SetColor(colorToSet);
            replayActor.SetColor(colorToSet);
        }
    }

    public void HideHeatmap()
    {
        GetHeatmapParent().gameObject.SetActive(false);
    }

    public void GenerateNewHeatMap(float scale)
    {
        ClearHeatmap();
        Transform parentTransform = GetHeatmapParent().transform;

        RecordedDataList.RecordedData currentHeatmapNode = dataList.GetFirstNode();
        int heatmapNodeIndex = 0;

        while (currentHeatmapNode != null)
        {
            GameObject heatmapObject = GameObject.Instantiate(heatmapObjectPrefab,
                                                              GetNodePosition(currentHeatmapNode),
                                                              GetNodeRotation(currentHeatmapNode),
                                                              parentTransform);
            heatmapObject.name = "Node_" + heatmapNodeIndex.ToString();

            heatmapNodeIndex++;
            currentHeatmapNode = currentHeatmapNode.nextData;
        }

        GetHeatmapParent().UpdateRendererList();
        GetHeatmapParent().SetColor(currentColor);
    }

    public void ClearHeatmap()
    {
        GetHeatmapParent().ClearHeatmap();
    }

    public void SetHeatmapObjectScale(float scale)
    {
        Vector3 childScale = new Vector3(scale, scale, scale);
        Transform parentTransform = GetHeatmapParent().transform;
        foreach(Transform child in parentTransform)
        {
            child.localScale = childScale;
        }
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
        controlledTransform.position = GetNodePosition(currentReplayNode);
        controlledTransform.rotation = GetNodeRotation(currentReplayNode);
        currentTime = currentReplayNode.time;
    }

    private void SetActorActualVisibility(bool shouldBeVisible, bool displayReplayMode)
    {
        if (displayReplayMode)
        {
            HideHeatmap();
            if (shouldBeVisible && !replayActor.gameObject.activeSelf && !IsCameraController())
            {
                replayActor.gameObject.SetActive(true);
            }
            else if ((IsCameraController() || !shouldBeVisible) && replayActor.gameObject.activeSelf)
            {
                replayActor.gameObject.SetActive(false);
            }
        }
        else
        {
            replayActor.gameObject.SetActive(false);
            GetHeatmapParent().gameObject.SetActive(shouldBeVisible);
        }
    }

    private HeatmapHolder GetHeatmapParent()
    {
        if(heatmapParent == null)
        {
            GameObject heatmapParentObject = new GameObject(name + "_heatmap_holder");
            heatmapParent = heatmapParentObject.AddComponent<HeatmapHolder>();
        }
        return heatmapParent;
    }
}
