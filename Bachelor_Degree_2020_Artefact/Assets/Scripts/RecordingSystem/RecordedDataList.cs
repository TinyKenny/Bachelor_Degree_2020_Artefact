using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecordedDataList : MonoBehaviour
{
    private class RecordedData
    {
        public RecordedData nextData;
        public float time { get; private set; }
        public float posX { get; private set; }
        public float posY { get; private set; }
        public float posZ { get; private set; }
        public float rotX { get; private set; }
        public float rotY { get; private set; }
        public float rotZ { get; private set; }
        public float rotW { get; private set; }

        public RecordedData(float cTime, float cPosX, float cPosY, float cPosZ, float cRotX, float cRotY, float cRotZ, float cRotW)
        {
            time = cTime;
        }
    }

    private RecordedData head;
    private RecordedData tail;

    public void Save()
    {

    }

}
