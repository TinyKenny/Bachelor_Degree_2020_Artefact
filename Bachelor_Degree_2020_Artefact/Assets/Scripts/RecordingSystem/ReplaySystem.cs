using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using System;

public class ReplaySystem : MonoBehaviour
{
    [SerializeField] private GameObject replayActorPrefab = null;
    [SerializeField] private Camera replayCamera = null;

    //[HideInInspector]
    [SerializeField] private List<LoadedRecording> loadedRecordings = new List<LoadedRecording>();
    [HideInInspector]
    [SerializeField] private List<LoadedRecording> removedRecordings = new List<LoadedRecording>();
    [HideInInspector]
    [SerializeField] private int numberOfSolo = 0;
    


    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void LoadSingleFile(string filepath)
    {
        if(loadedRecordings.Exists(element => element.GetFilePath() == filepath))
        {
            Debug.LogWarning("The selected file is already loaded.");
            return;
        }
        RecordedDataList recordedDataList = RecordedDataList.LoadDataFromFile(filepath);

        string[] splittedFilePath = filepath.Split('/');
        string recordingName = splittedFilePath[splittedFilePath.Length - 1].Split('.')[0];

        loadedRecordings.Add(new LoadedRecording(this, recordingName, filepath, recordedDataList, replayActorPrefab, replayCamera));
    }

    public void LoadMultipleFiles()
    {
        Debug.Log("Multi-file loading not implemented yet.");
    }

    public void RemoveRecording(int index)
    {
        removedRecordings.Add(loadedRecordings[index]);
    }

    public void UnloadRemovedRecordings()
    {
        for(int i = 0; i < removedRecordings.Count; i++)
        {
            if (removedRecordings[i].IsSoloVisible())
            {
                numberOfSolo--;
            }
            loadedRecordings.Remove(removedRecordings[i]);
            removedRecordings[i].DoCleanup();
        }
        removedRecordings.Clear();
    }

    public void GoToPreviousNode()
    {
        //Debug.Log("Replay system: Go to previous node");
        foreach (LoadedRecording recording in loadedRecordings)
        {
            recording.GoToPreviousNode();
        }
    }

    public void GoToNextNode()
    {
        //Debug.Log("Replay system: Go to next node");
        foreach(LoadedRecording recording in loadedRecordings)
        {
            recording.GoToNextNode();
        }
    }

    public void StartPlayback()
    {
        Debug.Log("Replay system: Start playback");
    }

    public void StartReversePlayback()
    {
        Debug.Log("Replay system: Start reverse playback");
    }

    public void PausePlayback()
    {
        Debug.Log("Replay system: Pause playback");
    }

    public void GoToFirstNode()
    {
        Debug.Log("Replay system: Go to first node");
    }

    public void GoToFinalNode()
    {
        Debug.Log("Replay system: Go to final node");
    }

    public bool IsRecordingActorVisible(int index)
    {
        return loadedRecordings[index].IsVisible();
    }

    public void SetRecordingActorVisibility(int index, bool visibility)
    {
        loadedRecordings[index].SetReplayActorVisibility(visibility);
    }

    public bool IsRecordingActorSolo(int index)
    {
        return loadedRecordings[index].IsSoloVisible();
    }

    public void SetRecordingActorSolo(int index, bool shouldBeSoloVisible)
    {
        if(IsRecordingActorSolo(index) && !shouldBeSoloVisible)
        {
            numberOfSolo--;
            loadedRecordings[index].MakeReplayActorSoloVisible(shouldBeSoloVisible);
        }
        else if(!IsRecordingActorSolo(index) && shouldBeSoloVisible)
        {
            numberOfSolo++;
            loadedRecordings[index].MakeReplayActorSoloVisible(shouldBeSoloVisible);
        }
    }

    public bool IsAnyActorSoloVisible()
    {
        return numberOfSolo > 0;
    }

    public bool GetActualVisibility(int index)
    {
        return loadedRecordings[index].GetActualVisibility();
    }

    public void UpdateActorsActualVisibility()
    {
        bool anyActorSolo = IsAnyActorSoloVisible();
        for(int i = 0; i < loadedRecordings.Count; i++)
        {
            loadedRecordings[i].UpdateActorActualVisibility(anyActorSolo);
        }
        //Debug.Log("actor visibility updated");
    }

    /// <summary>
    /// this function exists purely for thesting, and should be removed.
    /// </summary>
    public void SoloNumberReset()
    {
        numberOfSolo = 0;
    }
}
