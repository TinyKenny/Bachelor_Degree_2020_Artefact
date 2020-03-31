using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplaySystem : MonoBehaviour
{
    private abstract class PlayMode
    {
        public abstract void HandleUpdate(List<LoadedRecording> loadedRecordings);
    }

    private class PlaybackMode : PlayMode
    {
        private float playDirection;

        public PlaybackMode(bool forward)
        {
            playDirection = forward ? 1.0f : -1.0f;
        }

        public override void HandleUpdate(List<LoadedRecording> loadedRecordings)
        {
            float TimeChange = Time.deltaTime * playDirection;
            foreach (LoadedRecording recording in loadedRecordings)
            {
                recording.ChangeTimeBy(TimeChange);
            }
        }
    }

    private class PausedMode : PlayMode
    {
        public override void HandleUpdate(List<LoadedRecording> loadedRecordings)
        {

        }
    }

    [SerializeField] private GameObject replayActorPrefab = null;
    [SerializeField] private Camera replayCamera = null;

    [HideInInspector]
    [SerializeField] private List<LoadedRecording> loadedRecordings = new List<LoadedRecording>();
    [HideInInspector]
    [SerializeField] private List<LoadedRecording> removedRecordings = new List<LoadedRecording>();
    [HideInInspector]
    [SerializeField] private int numberOfSolo = 0;

    private LoadedRecording currentCameraController = null;

    private PlayMode currentMode;

    private PlayMode pausedMode;
    private PlayMode playingMode;
    private PlayMode reverseMode;

    private void Awake()
    {
        pausedMode = new PausedMode();
        playingMode = new PlaybackMode(true);
        reverseMode = new PlaybackMode(false);

        currentMode = pausedMode;
    }

    void Start()
    {
        
    }

    void Update()
    {
        currentMode.HandleUpdate(loadedRecordings);
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

        loadedRecordings.Add(new LoadedRecording(recordingName, filepath, recordedDataList, replayActorPrefab, replayCamera));
    }

    public void RecordingByIndexRemove(int recordingIndex)
    {
        removedRecordings.Add(loadedRecordings[recordingIndex]);
    }

    public void UnloadRemovedRecordings()
    {
        for(int i = 0; i < removedRecordings.Count; i++)
        {
            if (removedRecordings[i].IsSoloVisible())
            {
                numberOfSolo--;
            }
            if (removedRecordings[i] == currentCameraController)
            {
                currentCameraController = null;
            }
            loadedRecordings.Remove(removedRecordings[i]);
            removedRecordings[i].DoCleanup();
        }
        removedRecordings.Clear();
    }

    public void GoToPreviousNode()
    {
        foreach (LoadedRecording recording in loadedRecordings)
        {
            recording.GoToPreviousNode();
        }
    }

    public void GoToNextNode()
    {
        foreach(LoadedRecording recording in loadedRecordings)
        {
            recording.GoToNextNode();
        }
    }

    public void StartPlayback()
    {
        currentMode = playingMode;
    }

    public void StartReversePlayback()
    {
        currentMode = reverseMode;
    }

    public void PausePlayback()
    {
        currentMode = pausedMode;
    }

    public void GoToFirstNode()
    {
        foreach (LoadedRecording recording in loadedRecordings)
        {
            recording.GoToFirstNode();
        }
    }

    public void GoToFinalNode()
    {
        foreach (LoadedRecording recording in loadedRecordings)
        {
            recording.GoToFinalNode();
        }
    }

    public void GoToNode(int nodeIndex)
    {
        foreach (LoadedRecording recording in loadedRecordings)
        {
            recording.GoToNodeIndex(nodeIndex);
        }
    }

    public void GoToTime(float targetTime)
    {
        foreach (LoadedRecording recording in loadedRecordings)
        {
            recording.GoToTime(targetTime);
        }
    }

    public void GoToTimePercent(float targetTimePercent)
    {
        foreach (LoadedRecording recording in loadedRecordings)
        {
            recording.GoToTimePercent(targetTimePercent);
        }
    }

    public string RecordingByIndexGetName(int recordingIndex)
    {
        return loadedRecordings[recordingIndex].GetRecordingName();
    }

    public float RecordingByIndexGetTime(int recordingIndex)
    {
        return loadedRecordings[recordingIndex].GetCurrentTime();
    }

    public int RecordingByIndexGetNodeIndex(int recordingIndex)
    {
        return loadedRecordings[recordingIndex].GetCurrentNodeIndex();
    }

    public bool RecordingByIndexIsActorVisible(int recordingIndex)
    {
        return loadedRecordings[recordingIndex].IsVisible();
    }

    public void RecordingByIndexSetActorVisibility(int recordingIndex, bool visibility)
    {
        loadedRecordings[recordingIndex].SetReplayActorVisibility(visibility);
    }

    public bool RecordingByIndexIsActorSolo(int recordingIndex)
    {
        return loadedRecordings[recordingIndex].IsSoloVisible();
    }

    public void RecordingByIndexSetActorSolo(int recordingIndex, bool shouldBeSoloVisible)
    {
        if(RecordingByIndexIsActorSolo(recordingIndex) && !shouldBeSoloVisible)
        {
            numberOfSolo--;
            loadedRecordings[recordingIndex].MakeReplayActorSoloVisible(shouldBeSoloVisible);
        }
        else if(!RecordingByIndexIsActorSolo(recordingIndex) && shouldBeSoloVisible)
        {
            numberOfSolo++;
            loadedRecordings[recordingIndex].MakeReplayActorSoloVisible(shouldBeSoloVisible);
        }
    }

    public bool IsAnyActorSoloVisible()
    {
        return numberOfSolo > 0;
    }

    public bool RecordingByIndexIsCameraController(int recordingIndex)
    {
        return loadedRecordings[recordingIndex] == currentCameraController;
    }

    public void RecordingByIndexSetCameraController(int recordingIndex, bool shouldControlCamera)
    {
        currentCameraController?.MakeCameraController(false);
        if(RecordingByIndexIsCameraController(recordingIndex) && !shouldControlCamera)
        {
            currentCameraController = null;
        }
        else if(!RecordingByIndexIsCameraController(recordingIndex) && shouldControlCamera)
        {
            currentCameraController = loadedRecordings[recordingIndex];
            currentCameraController.MakeCameraController(true);
        }
    }

    public bool RecordingByIndexGetActualVisibility(int recordingIndex)
    {
        return loadedRecordings[recordingIndex].GetActualVisibility();
    }

    public void UpdateActorsActualVisibility()
    {
        bool anyActorSolo = IsAnyActorSoloVisible();
        for(int i = 0; i < loadedRecordings.Count; i++)
        {
            loadedRecordings[i].UpdateActorActualVisibility(anyActorSolo);
        }
    }

    /// <summary>
    /// this function exists purely for thesting, and should be removed.
    /// </summary>
    public void ResetValues()
    {
        foreach(LoadedRecording loaded in loadedRecordings)
        {
            removedRecordings.Add(loaded);
        }
        UnloadRemovedRecordings();
        loadedRecordings.Clear();
        removedRecordings.Clear();
        numberOfSolo = 0;
        currentCameraController = null;
    }
}
