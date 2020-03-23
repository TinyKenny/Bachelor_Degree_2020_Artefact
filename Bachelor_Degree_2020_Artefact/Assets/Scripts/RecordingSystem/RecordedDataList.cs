﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Globalization;

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

        public string AsString(NumberFormatInfo nfi)
        {
            string ToReturn = "";
            ToReturn += time.ToString(nfi) + ";";
            ToReturn += posX.ToString(nfi) + ";";
            ToReturn += posY.ToString(nfi) + ";";
            ToReturn += posZ.ToString(nfi) + ";";
            ToReturn += rotX.ToString(nfi) + ";";
            ToReturn += rotY.ToString(nfi) + ";";
            ToReturn += rotZ.ToString(nfi) + ";";
            ToReturn += rotW.ToString(nfi);
            return ToReturn;
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
        Debug.Log("TODO: Difference between struc and light?");

        RecordedData currentNode = head;

        if(currentNode == null)
        {
            Debug.LogError("Cannot save recodring to file: head node of data recording is null");
            return;
        }

        NumberFormatInfo nfi = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
        nfi.NumberDecimalSeparator = ".";
        nfi.NumberGroupSeparator = "";

        string filepath = Application.persistentDataPath + "/";
        int fileNumber = 0;

        while(File.Exists(filepath + fileNumber.ToString()))
        {
            fileNumber++;
        }
        filepath += fileNumber.ToString();

        using (StreamWriter writer = new StreamWriter(filepath))
        {
            while (currentNode != null)
            {
                writer.WriteLine(currentNode.AsString(nfi));

                currentNode = currentNode.nextData;
            }
        }

        Debug.Log("saved to: " + filepath);
    }
}
