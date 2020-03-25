using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class LoadedRecording
{
    private enum SoloVisibilityMode
    {
        SOLO_VISIBLE,
        NO_SOLO,
        HIDDEN_BY_SOLO
    }

    private ReplaySystem owner;
    private RecordedDataList dataList = null;
    private GameObject replayActor = null;
    private Camera replayCamera = null;
    private Transform controlledTransform = null;
    private int currentNode = 0;
    [SerializeField] public string name = "Test-text";
    private bool visible = true;
    private SoloVisibilityMode soloMode = SoloVisibilityMode.NO_SOLO;

    public LoadedRecording(ReplaySystem ownerSystem, string filename, RecordedDataList recordedData, GameObject actor, Camera camera)
    {
        owner = ownerSystem;
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

    public void GoToPreviousNode()
    {
        Debug.Log("Loaded recording: Go to previous node");
    }

    public void SetReplayActorVisibility(bool shouldBeVisible)
    {
        visible = shouldBeVisible;

        UpdateActorActualVisibility();
    }

    public bool IsVisible()
    {
        return visible;
    }

    public void MakeReplayActorSoloVisible(bool shouldBeSoloVisible)
    {
        if (shouldBeSoloVisible)
        {
            soloMode = SoloVisibilityMode.SOLO_VISIBLE;
        }
        else if(owner.IsAnyActorSoloVisible(this))
        {
            soloMode = SoloVisibilityMode.HIDDEN_BY_SOLO;
        }
        else
        {
            soloMode = SoloVisibilityMode.NO_SOLO;
        }

        UpdateActorActualVisibility();
    }

    public void MakeReplayActorHiddenBySolo(bool shouldBeHidden)
    {
        if (shouldBeHidden)
        {
            if(soloMode == SoloVisibilityMode.SOLO_VISIBLE)
            {
                return;
            }
            else
            {
                soloMode = SoloVisibilityMode.HIDDEN_BY_SOLO;
            }
        }
        else
        {
            soloMode = SoloVisibilityMode.NO_SOLO;
        }
    }

    public bool IsSoloVisible()
    {
        return soloMode == SoloVisibilityMode.SOLO_VISIBLE;
    }

    private void UpdateActorActualVisibility()
    {
        if (soloMode == SoloVisibilityMode.SOLO_VISIBLE
        || (soloMode == SoloVisibilityMode.NO_SOLO && visible))
        {
            Debug.Log("Loaded recording: make actor visible: " + name);
        }
        else
        {
            Debug.Log("Loaded recording: make actor hidden: " + name);
        }
    }
}
