using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecordingSystem : MonoBehaviour
{
    public int testcounter = 0;

    [SerializeField] private RecordingStartTrigger startTrigger = null;
    [SerializeField] private RecordingEndTrigger endTrigger = null;

    private bool isRecording = false;


    private void Awake()
    {
        startTrigger.RegisterRecordingSystem(this);
        endTrigger.RegisterRecordingSystem(this);

        
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



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isRecording)
        {
            testcounter += 1;
        }
    }


}
