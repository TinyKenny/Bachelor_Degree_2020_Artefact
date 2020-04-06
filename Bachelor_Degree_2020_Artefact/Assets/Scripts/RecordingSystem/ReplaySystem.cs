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

    private enum DisplayMode
    {
        REPLAY,
        HEATMAP
    }

    [SerializeField] private GameObject heatmapMarkerPrefab = null;
    [SerializeField] private GameObject replayActorPrefab = null;
    [SerializeField] private Camera replayCamera = null;

    [HideInInspector]
    [SerializeField] private List<LoadedRecording> loadedRecordings = new List<LoadedRecording>();
    [HideInInspector]
    [SerializeField] private List<LoadedRecording> removedRecordings = new List<LoadedRecording>();
    [HideInInspector]
    [SerializeField] private int numberOfSolo = 0;

    private DisplayMode currentDisplayMode = DisplayMode.REPLAY;

    #region replay_mode
    private LoadedRecording currentCameraController = null;

    private PlayMode currentPlayMode;

    private PlayMode pausedMode;
    private PlayMode playingMode;
    private PlayMode reverseMode;
    #endregion //replay_mode

    #region heatmap_mode
    private float heatmapObjectScale = 1.0f;
    #endregion //heatmap_mode

    #region manual camera controls
    [SerializeField] private float manualCameraMovementSpeed = 5.0f;
    [SerializeField] private float manualCameraShiftSpeedMult = 3.0f;
    [SerializeField] private float manualCameraLookSensitivity = 1.0f;
    private float manualCameraMaxXDegrees = 90.0f;
    private float manualCameraRotationX = 0.0f;
    private float manualCameraRotationY = 0.0f;
    #endregion // manual camera controls

    private void Awake()
    {
        if (Application.isEditor)
        {
            CameraController playerCameraController = replayCamera.GetComponent<CameraController>();
            if(playerCameraController != null)
            {
                Destroy(playerCameraController);
            }

            PlayerMovement playerMovement = FindObjectOfType<PlayerMovement>();
            if (playerMovement != null)
            {
                Destroy(playerMovement.gameObject);
            }


            pausedMode = new PausedMode();
            playingMode = new PlaybackMode(true);
            reverseMode = new PlaybackMode(false);

            currentPlayMode = pausedMode;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        
    }

    void Update()
    {
        if (IsReplayDisplayMode())
        {
            currentPlayMode.HandleUpdate(loadedRecordings);
        }

        if(!IsReplayDisplayMode() || currentCameraController == null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (Cursor.lockState == CursorLockMode.None)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    manualCameraRotationX = replayCamera.transform.eulerAngles.x;
                    manualCameraRotationY = replayCamera.transform.eulerAngles.y;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.None;
                }
            }

            if (Cursor.lockState == CursorLockMode.Locked)
            {
                float mouseX = Input.GetAxisRaw("Mouse X") * manualCameraLookSensitivity;
                float mouseY = Input.GetAxisRaw("Mouse Y") * manualCameraLookSensitivity;

                manualCameraRotationY += mouseX;
                manualCameraRotationX -= mouseY;

                manualCameraRotationX = Mathf.Clamp(manualCameraRotationX, -manualCameraMaxXDegrees, manualCameraMaxXDegrees);

                replayCamera.transform.rotation = Quaternion.Euler(manualCameraRotationX, manualCameraRotationY, 0.0f);

                Vector3 movementInputVector = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Jump") - Input.GetAxisRaw("Fire1"), Input.GetAxisRaw("Vertical"));
                if (movementInputVector.sqrMagnitude > 0.1f)
                {
                    movementInputVector *= manualCameraMovementSpeed * (Input.GetKey(KeyCode.LeftShift) ? manualCameraShiftSpeedMult : 1.0f);
                    movementInputVector = replayCamera.transform.rotation * movementInputVector;
                    replayCamera.transform.position += movementInputVector * Time.deltaTime;
                }
            }
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
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

        loadedRecordings.Add(new LoadedRecording(recordingName, filepath, recordedDataList, heatmapMarkerPrefab, replayActorPrefab, replayCamera));
    }

    public void RecordingByIndexRemove(int recordingIndex)
    {
        removedRecordings.Add(loadedRecordings[recordingIndex]);
    }

    public bool UnloadRemovedRecordings()
    {
        int countBeforeRemove = loadedRecordings.Count;

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

        return countBeforeRemove != loadedRecordings.Count;
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
        currentPlayMode = playingMode;
    }

    public void StartReversePlayback()
    {
        currentPlayMode = reverseMode;
    }

    public void PausePlayback()
    {
        currentPlayMode = pausedMode;
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

    public Color RecordingByIndexGetColor(int recordingIndex)
    {
        return loadedRecordings[recordingIndex].GetColor();
    }

    public void RecordingByIndexSetColor(int recordingIndex, Color colorToSet)
    {
        loadedRecordings[recordingIndex].SetColor(colorToSet);
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
            loadedRecordings[i].UpdateActorActualVisibility(anyActorSolo, IsReplayDisplayMode());
        }
    }

    public int GetCurrentDisplayMode()
    {
        return (int)currentDisplayMode;
    }

    public void SetCurrentDisplayMode(int desiredDisplayMode)
    {
        DisplayMode desired = (DisplayMode)desiredDisplayMode;
        if (currentDisplayMode != desired)
        {
            if(currentDisplayMode == DisplayMode.HEATMAP)
            {
                foreach(LoadedRecording recording in loadedRecordings)
                {
                    recording.HideHeatmap();
                }
            }
            Debug.Log("replay system: display mode changed, do stuff");
            currentDisplayMode = desired;
            UpdateActorsActualVisibility();
        }
    }

    public bool IsReplayDisplayMode()
    {
        return currentDisplayMode == DisplayMode.REPLAY;
    }

    public bool IsHeatmapDisplayMode()
    {
        return currentDisplayMode == DisplayMode.HEATMAP;
    }

    public void GenerateNewHeatMap()
    {
        foreach(LoadedRecording recording in loadedRecordings)
        {
            recording.GenerateNewHeatMap(heatmapObjectScale);
        }
    }

    public void ClearHeatmap()
    {
        foreach(LoadedRecording recording in loadedRecordings)
        {
            recording.ClearHeatmap();
        }
    }

    public float GetHeatmapObjectScale()
    {
        return heatmapObjectScale;
    }

    public void SetHeatmapObjectScale(float scale)
    {
        heatmapObjectScale = scale;
        foreach(LoadedRecording recording in loadedRecordings)
        {
            recording.SetHeatmapObjectScale(heatmapObjectScale);
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
