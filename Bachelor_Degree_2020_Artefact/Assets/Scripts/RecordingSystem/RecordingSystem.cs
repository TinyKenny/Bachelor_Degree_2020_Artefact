using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecordingSystem : MonoBehaviour
{
    public float mazeTime = 0.0f;
    public Vector3 pos = Vector3.zero;
    public Quaternion rot = Quaternion.identity;

    [SerializeField] private RecordingStartTrigger startTrigger = null;
    [SerializeField] private RecordingEndTrigger endTrigger = null;
    
    private bool isRecording = false;
    private Transform playerCameraTransform;
    private RecordedDataList dataList;

    private void Awake()
    {
        startTrigger.RegisterRecordingSystem(this);
        endTrigger.RegisterRecordingSystem(this);
        dataList = new RecordedDataList();
        playerCameraTransform = Camera.main.transform;
    }

    public void StartRecording()
    {
        Debug.Log("Recodring should start now.");
        isRecording = true;
        mazeTime = 0.0f;
    }

    public void StopRecording()
    {
        Debug.Log("recodring should end now.");
        isRecording = false;
    }

    void Update()
    {
        if (isRecording)
        {
            pos = playerCameraTransform.position;
            rot = playerCameraTransform.rotation;
            dataList.RecordData(mazeTime, pos.x, pos.y, pos.z, rot.x, rot.y, rot.z, rot.w);
            mazeTime += Time.deltaTime;
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            Debug.Log(Time.fixedDeltaTime);
        }
    }

    private void TestFuncc()
    {
        float timeSinceLastRecord = 0.0f;
        float timeBetweenRecords = 0.1f;


        if (isRecording)
        {
            if(timeSinceLastRecord >= timeBetweenRecords)
            {

            }
        }
    }
}
