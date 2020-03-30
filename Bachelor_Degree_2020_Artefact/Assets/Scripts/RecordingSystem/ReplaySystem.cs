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

    public float GetRecordingTimeByIndex(int index)
    {
        return loadedRecordings[index].GetCurrentTime();
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

    public bool IsCameraController(int index)
    {
        return loadedRecordings[index] == currentCameraController;
    }

    public void SetCameraController(int index, bool shouldControlCamera)
    {
        currentCameraController?.MakeCameraController(false);
        if(IsCameraController(index) && !shouldControlCamera)
        {
            currentCameraController = null;
        }
        else if(!IsCameraController(index) && shouldControlCamera)
        {
            currentCameraController = loadedRecordings[index];
            currentCameraController.MakeCameraController(true);
        }
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
