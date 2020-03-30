using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Globalization;

public class RecordedDataList
{
    private class RecordedData
    {
        public RecordedData nextData;
        public readonly RecordedData previousData;
        public readonly float time;
        public readonly float posX;
        public readonly float posY;
        public readonly float posZ;
        public readonly float rotX;
        public readonly float rotY;
        public readonly float rotZ;
        public readonly float rotW;

        public RecordedData(RecordedData prevData, float cTime, float cPosX, float cPosY, float cPosZ, float cRotX, float cRotY, float cRotZ, float cRotW)
        {
            previousData = prevData;
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
            ToReturn += time.ToString(nfi) + GetValueSeparator();
            ToReturn += posX.ToString(nfi) + GetValueSeparator();
            ToReturn += posY.ToString(nfi) + GetValueSeparator();
            ToReturn += posZ.ToString(nfi) + GetValueSeparator();
            ToReturn += rotX.ToString(nfi) + GetValueSeparator();
            ToReturn += rotY.ToString(nfi) + GetValueSeparator();
            ToReturn += rotZ.ToString(nfi) + GetValueSeparator();
            ToReturn += rotW.ToString(nfi);
            return ToReturn;
        }
    }

    private RecordedData head;
    private RecordedData tail;

    public static NumberFormatInfo GetNumberFormat()
    {
        NumberFormatInfo nfi = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
        nfi.NumberDecimalSeparator = ".";
        nfi.NumberGroupSeparator = "";
        return nfi;
    }

    public static string GetFileExtension()
    {
        return "artefrecord";
    }

    public static char GetValueSeparator()
    {
        return ';';
    }

    public static RecordedDataList LoadDataFromFile(string filepath)
    {
        RecordedDataList loadedDataList = new RecordedDataList();

        NumberFormatInfo nfi = GetNumberFormat();
        StreamReader reader = new StreamReader(filepath);

        float cTime;
        float cPosX;
        float cPosY;
        float cPosZ;
        float cRotX;
        float cRotY;
        float cRotZ;
        float cRotW;

        while (!reader.EndOfStream)
        {
            string line = reader.ReadLine();
            string[] splitLine = line.Split(GetValueSeparator());

            cTime = float.Parse(splitLine[0].Trim(), nfi);
            cPosX = float.Parse(splitLine[1].Trim(), nfi);
            cPosY = float.Parse(splitLine[2].Trim(), nfi);
            cPosZ = float.Parse(splitLine[3].Trim(), nfi);
            cRotX = float.Parse(splitLine[4].Trim(), nfi);
            cRotY = float.Parse(splitLine[5].Trim(), nfi);
            cRotZ = float.Parse(splitLine[6].Trim(), nfi);
            cRotW = float.Parse(splitLine[7].Trim(), nfi);

            loadedDataList.RecordData(cTime, cPosX, cPosY, cPosZ, cRotX, cRotY, cRotZ, cRotW);
        }

        return loadedDataList;
    }

    public void RecordData(float cTime, float cPosX, float cPosY, float cPosZ, float cRotX, float cRotY, float cRotZ, float cRotW)
    {
        RecordedData data = new RecordedData(tail, cTime, cPosX, cPosY, cPosZ, cRotX, cRotY, cRotZ, cRotW);

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

        NumberFormatInfo nfi = GetNumberFormat();

        string filepath = Application.persistentDataPath + "/recording_";
        int fileNumber = 0;

        while(File.Exists(filepath + fileNumber.ToString() + "." + GetFileExtension()))
        {
            fileNumber++;
        }
        filepath += fileNumber.ToString();
        filepath += "." + GetFileExtension();

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
