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

    [SerializeField] private RecordingStartTrigger startTrigger = null;
    [SerializeField] private RecordingEndTrigger endTrigger = null;
    [SerializeField] private Canvas endCanvas = null;
    [SerializeField] private Text exitPromptText = null;

    private bool isRecording = false;
    private Transform playerCameraTransform;
    private RecordedDataList dataList;
    private float nextTimeToRecord = 0.0f;
    private float timeBetweenRecords = 0.1f;

    private void Awake()
    {
        startTrigger.RegisterRecordingSystem(this);
        endTrigger.RegisterRecordingSystem(this);
        dataList = new RecordedDataList();
        playerCameraTransform = Camera.main.transform;
        endCanvas.gameObject.SetActive(false);
        exitPromptText.gameObject.SetActive(false);
    }

    public void StartRecording()
    {
        //Debug.Log("Recodring should start now.");
        isRecording = true;
        mazeTime = 0.0f;
        nextTimeToRecord = 0.0f;
    }

    public void StopRecording()
    {
        //Debug.Log("recodring should end now.");
        isRecording = false;
        endCanvas.gameObject.SetActive(true);
        
        //dataList.SaveRecordingToFile();

        Thread thread = new Thread(SaveRecording);
        thread.Start();
    }

    private void SaveRecording()
    {
        dataList.SaveRecordingToFile();
        exitPromptText.gameObject.SetActive(true);
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
            mazeTime += Time.deltaTime;
        }
        else if(exitPromptText.gameObject.activeSelf && Input.anyKeyDown)
        {
            Application.Quit(0);
        }
    }
}
