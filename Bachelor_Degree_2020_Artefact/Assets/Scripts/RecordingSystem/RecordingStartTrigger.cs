﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class RecordingStartTrigger : MonoBehaviour
{
    [SerializeField] private Transform playerStart;

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
            other.transform.position = playerStart.position;
            Camera.main.GetComponent<CameraController>().SetRotation(transform.eulerAngles.x, transform.eulerAngles.y);
            recorder?.StartRecording();
        }
    }
}
