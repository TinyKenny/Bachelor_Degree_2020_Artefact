using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecordingSystem : MonoBehaviour
{
    public int testcounter = 0;
    public float dTime = 0.0f;
    public Vector3 pos = Vector3.zero;
    public Quaternion rot = Quaternion.identity;

    [SerializeField] private RecordingStartTrigger startTrigger = null;
    [SerializeField] private RecordingEndTrigger endTrigger = null;
    

    private bool isRecording = false;
    private Transform playerCameraTransform;



    private void Awake()
    {
        startTrigger.RegisterRecordingSystem(this);
        endTrigger.RegisterRecordingSystem(this);
        playerCameraTransform = Camera.main.transform;
        
    }

    public void StartRecording()
    {
        Debug.Log("Recodring should start now.");
        isRecording = true;
    }

    public void StopRecording()
    {
        Debug.Log("recodring should end now.");
        isRecording = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isRecording)
        {
            testcounter += 1;
            dTime = Time.deltaTime;
            pos = playerCameraTransform.position;
            rot = playerCameraTransform.rotation;
        }
    }


}
