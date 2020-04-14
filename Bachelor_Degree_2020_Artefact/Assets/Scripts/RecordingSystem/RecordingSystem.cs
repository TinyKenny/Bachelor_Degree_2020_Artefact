using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class RecordingSystem : MonoBehaviour
{
    public float mazeTime = 0.0f;
    //public Vector3 pos = Vector3.zero;
    //public Quaternion rot = Quaternion.identity;

    [SerializeField] private PlayerBaseState playerCharacter = null;
    [SerializeField] private RecordingStartTrigger startTrigger = null;
    [SerializeField] private RecordingEndTrigger endTrigger = null;
    [SerializeField] private Canvas endCanvas = null;
    [SerializeField] private Text exitPromptText = null;
    [SerializeField] private Text goalReachedText = null;
    [SerializeField] private Text noTimeText = null;

    private bool isRecording = false;
    private Transform playerCameraTransform;
    private RecordedDataList dataList;
    private float nextTimeToRecord = 0.0f;
    private float timeBetweenRecords = 0.1f;
    private float timeLimit = 1200.0f; // 20 minutes

    private bool showPromptText = false;

    private void Awake()
    {
        startTrigger.RegisterRecordingSystem(this);
        endTrigger.RegisterRecordingSystem(this);
        dataList = new RecordedDataList();
        playerCameraTransform = Camera.main.transform;
        endCanvas.gameObject.SetActive(false);
        exitPromptText.gameObject.SetActive(false);
        noTimeText.gameObject.SetActive(false);
    }

    public void StartRecording()
    {
        isRecording = true;
        mazeTime = 0.0f;
        nextTimeToRecord = 0.0f;
    }

    public void StopRecording()
    {
        playerCharacter.gameObject.SetActive(false);
        isRecording = false;
        endCanvas.gameObject.SetActive(true);
        
        Thread thread = new Thread(SaveRecording);
        thread.Start();
    }

    private void SaveRecording()
    {
        dataList.SaveRecordingToFile();
        showPromptText = true;
    }

    void Update()
    {
        if (isRecording)
        {
            if (mazeTime - nextTimeToRecord >= -Mathf.Epsilon * 2.0f)
            {
                dataList.RecordData(mazeTime,
                                    playerCameraTransform.position.x,
                                    playerCameraTransform.position.y,
                                    playerCameraTransform.position.z,
                                    playerCameraTransform.rotation.x,
                                    playerCameraTransform.rotation.y,
                                    playerCameraTransform.rotation.z,
                                    playerCameraTransform.rotation.w);

                nextTimeToRecord += timeBetweenRecords;
            }

            if (mazeTime > timeLimit)
            {
                goalReachedText.gameObject.SetActive(true);
                noTimeText.gameObject.SetActive(true);
                //stop recording, remove player object

            }

            mazeTime += Time.deltaTime;
        }
        else if (!exitPromptText.gameObject.activeSelf && showPromptText)
        {
            exitPromptText.gameObject.SetActive(true);
        }
        else if(exitPromptText.gameObject.activeSelf && Input.anyKeyDown)
        {
            Debug.Log("quit game");
            Application.Quit(0);
        }
    }
}
