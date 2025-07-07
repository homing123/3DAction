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
    public static void SetDicData(TextData[] datas)
    {
        D_Data = new Dictionary<int, TextData>(datas.Length);
        for(int i=0;i<datas.Length;i++)
        {
            D_Data[datas[i].ID] = datas[i];
        }
    }
    public static Dictionary<int, TextData> GetDicData()
    {
        return D_Data;
    }
    
    public static void LoadStreamingData()
    {
        GoogleSheetReader.ReadBinary(D_Data, FileName);
    }
}


