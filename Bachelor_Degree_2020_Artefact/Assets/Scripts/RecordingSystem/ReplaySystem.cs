using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ReplaySystem : MonoBehaviour
{
    [SerializeField] private GameObject replayActorPrefab = null;
    [SerializeField] private Camera replayCamera = null;

    [HideInInspector]
    [SerializeField] private List<LoadedRecording> loadedRecordings = new List<LoadedRecording>();

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
        loadedRecordings.Add(new LoadedRecording(counter.ToString(), null, null, null));
    }

    public void LoadMultipleFiles()
    {
        Debug.Log("Multi-file loading not implemented yet.");
    }

    public void UnloadRecording(int index)
    {
        loadedRecordings.RemoveAt(index);
    }

    public void GoToPreviousNode()
    {
        Debug.Log("Replay system: Go to previous node");
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

    public void HideRecordingByIndex(int index)
    {
        loadedRecordings[index].HideReplayActor();
    }
}
