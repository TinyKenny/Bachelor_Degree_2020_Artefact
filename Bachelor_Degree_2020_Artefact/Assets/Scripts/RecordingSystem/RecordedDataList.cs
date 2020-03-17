using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecordedDataList
{
    private class RecordedData
    {
        public RecordedData nextData;
        public readonly float time;
        public readonly float posX;
        public readonly float posY;
        public readonly float posZ;
        public readonly float rotX;
        public readonly float rotY;
        public readonly float rotZ;
        public readonly float rotW;

        public RecordedData(float cTime, float cPosX, float cPosY, float cPosZ, float cRotX, float cRotY, float cRotZ, float cRotW)
        {
            time = cTime;
            posX = cPosX;
            posY = cPosY;
            posZ = cPosZ;
            rotX = cRotX;
            rotY = cRotY;
            rotZ = cRotZ;
            rotW = cRotW;
        }
    }



    private RecordedData head;
    private RecordedData tail;

    public void RecordData(float cTime, float cPosX, float cPosY, float cPosZ, float cRotX, float cRotY, float cRotZ, float cRotW)
    {
        RecordedData data = new RecordedData(cTime, cPosX, cPosY, cPosZ, cRotX, cRotY, cRotZ, cRotW);

        if(head == null)
        {
            head = data;
            tail = data;
        }
        else
        {
            tail.nextData = data;
            tail = data;
        }
    }

    public void SaveRecordingToFile()
    {
        Debug.Log("Recording saved to file!");
    }
}
