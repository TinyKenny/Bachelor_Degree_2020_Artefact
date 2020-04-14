using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class RecordingEndTrigger : MonoBehaviour
{
    private RecordingSystem recorder;

    private void Awake()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    public void RegisterRecordingSystem(RecordingSystem rS)
    {
        if (recorder == null && rS != null)
        {
            recorder = rS;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //other.gameObject.SetActive(false);
            recorder?.StopRecording();
        }
    }
}
