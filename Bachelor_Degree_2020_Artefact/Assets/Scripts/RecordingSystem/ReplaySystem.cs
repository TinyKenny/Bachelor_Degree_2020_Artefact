using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using System;

public class ReplaySystem : MonoBehaviour
{
    [SerializeField] private GameObject replayActorPrefab = null;
    [SerializeField] private Camera replayCamera = null;

    [HideInInspector]
    [SerializeField] private List<LoadedRecording> loadedRecordings = new List<LoadedRecording>();
    //[HideInInspector]
    //[SerializeField] private LoadedRecording soloRecording = null;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void LoadSingleFile()
    {
        Debug.Log("Single-file loading not implemented yet.");
        int counter = 0;
        while(loadedRecordings.Exists(element => element.name.Equals(counter.ToString())))
        {
            counter++;
        }
        loadedRecordings.Add(new LoadedRecording(this, counter.ToString(), null, null, null));
    }

    public void LoadMultipleFiles()
    {
        Debug.Log("Multi-file loading not implemented yet.");
    }

    public void UnloadRecording(int index)
    {
        loadedRecordings.RemoveAt(index);

        if(!IsAnyActorSoloVisible())
        {
            Debug.Log("Replay system unload recoding: TODO - restore from solo mode!");
        }
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
        Debug.Log("Replay system: setting visibility should affect solo tag");
        loadedRecordings[index].SetReplayActorVisibility(visibility);
    }

    public bool IsRecordingActorSolo(int index)
    {
        return loadedRecordings[index].IsSoloVisible();
    }

    public void SetRecordingActorSolo(int index)
    {

    }

    public bool IsAnyActorSoloVisible(LoadedRecording excludedRecording = null)
    {
        for(int i = 0; i < loadedRecordings.Count; i++)
        {
            if(loadedRecordings[i] != excludedRecording && loadedRecordings[i].IsSoloVisible())
            {
                return true;
            }
        }
        return false;
    }

    
}
