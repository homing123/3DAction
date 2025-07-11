using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine.Animations;


public enum LanguageKind
{
    KOR,
    ENG
}
[System.Serializable]
public class TextData
{
    public const int SkillNotReady = 10000;
    public const int SkillUnUseable = 10001;
    public const int NotEnoughMP = 10002;

    public const string FileName = "Text";
    public static LanguageKind UserLanguageKind = LanguageKind.KOR; //후에 유저데이터로 변경
    
    public int ID { get; private set; }
    public string KOR { get; private set; }
    public string ENG { get; private set; }

    static Dictionary<int, TextData> D_Data;
    public static string GetData(int id)
    {
        if(D_Data!= null && D_Data.ContainsKey(id))
        {
            switch(UserLanguageKind)
            {
                case LanguageKind.KOR:
                    return D_Data[id].KOR;
                case LanguageKind.ENG:
                    return D_Data[id].ENG;
            }
        }
        return "null";
    }
    static void SetDicData(TextData[] datas)
    {
        D_Data = new Dictionary<int, TextData>(datas.Length);
        for(int i=0;i<datas.Length;i++)
        {
            D_Data[datas[i].ID] = datas[i];
        }
    }
 
    public static void Log()
    {
        foreach(int key in D_Data.Keys)
        {
            Debug.Log($"{key}, {D_Data[key].KOR}");
        }
    }
    public static async void LoadGoogleSheetAndSaveBinary(GoogleSheetReader.GoogleSheetInfo info)
    {
        Debug.Log($"TextData LoadGoogleSheet");
        TextData[] textDatas = await GoogleSheetReader.LoadGoogleSheet2Instances<TextData>(info);
        SetDicData(textDatas);
        GoogleSheetReader.SaveBinary(D_Data, TextData.FileName);
    }
    public static void LoadStreamingData()
    {
        GoogleSheetReader.ReadBinary(out D_Data, FileName);
        Debug.Log($"TextData Load Success {D_Data.Count}");
    }
}


