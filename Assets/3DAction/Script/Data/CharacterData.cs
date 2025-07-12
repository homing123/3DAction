using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;

[System.Serializable]
public class CharacterData
{
    public const string FileName = "Character";
    public class CharacterSheetData
    {
        public int ID { get; private set; }
        public int Name { get; private set; }
        public string FaceIconPath { get; private set; }
        public string PrefabPath { get; private set; }
        public float HP_1 { get; private set; }
        public float HP_20 { get; private set; }
        public float MP_1 { get; private set; }
        public float MP_20 { get; private set; }
        public float Armor_1 { get; private set; }
        public float Armor_20 { get; private set; }
        public float Dmg_1 { get; private set; }
        public float Dmg_20 { get; private set; }
        public float AttackSpeed_1 { get; private set; }
        public float AttackSpeed_20 { get; private set; }
        public float SkillDmg { get; private set; }
        public float SkillCooldown { get; private set; }
        public float CriticalPer { get; private set; }
        public float MoveSpeed { get; private set; }
        public float Sight { get; private set; }
        public float AttackRange { get; private set; }
        public float AllLifeSteal { get; private set; }
        public float AttackLifeSteal { get; private set; }
    }

    public int ID { get; private set; }
    public int Name { get; private set; }
    public string FaceIconPath { get; private set; }
    public string PrefabPath { get; private set; }
    public float[] HP { get; private set; }
    public float[] MP { get; private set; }
    public float[] Armor { get; private set; }
    public float[] Dmg { get; private set; }
    public float[] AttackSpeed { get; private set; }
    public float SkillDmg { get; private set; }
    public float SkillCooldown { get; private set; }
    public float CriticalPer { get; private set; }
    public float MoveSpeed { get; private set; }
    public float Sight { get; private set; }
    public float AttackRange { get; private set; }
    public float AllLifeSteal { get; private set; }
    public float AttackLifeSteal { get; private set; }

    static Dictionary<int, CharacterData> D_Data;

    float[] GetLevelArray(float level1, float level20)
    {
        float increaseByLevel = (level20 - level1) / 19;
        float[] levelArray = new float[20];
        for (int i = 0; i < 20; i++)
        {
            levelArray[i] = level1 + increaseByLevel * i;
        }
        return levelArray;
    }
    CharacterData(CharacterSheetData sheetData)
    {
        ID = sheetData.ID;
        ID = sheetData.ID;
        Name = sheetData.Name;
        FaceIconPath = sheetData.FaceIconPath;
        PrefabPath = sheetData.PrefabPath;
        HP = GetLevelArray(sheetData.HP_1, sheetData.HP_20);
        MP = GetLevelArray(sheetData.MP_1, sheetData.MP_20);
        Armor = GetLevelArray(sheetData.Armor_1, sheetData.Armor_20);
        Dmg = GetLevelArray(sheetData.Dmg_1, sheetData.Dmg_20);
        AttackSpeed = GetLevelArray(sheetData.AttackSpeed_1, sheetData.AttackSpeed_20);
        SkillDmg = sheetData.SkillDmg;
        SkillCooldown = sheetData.SkillCooldown;
        CriticalPer = sheetData.CriticalPer;
        MoveSpeed = sheetData.MoveSpeed;
        Sight = sheetData.Sight;
        AttackRange = sheetData.AttackRange;
        AllLifeSteal = sheetData.AllLifeSteal;
        AttackLifeSteal = sheetData.AttackLifeSteal;
    }
    static void SetDicData(CharacterSheetData[] datas)
    {
        D_Data = new Dictionary<int, CharacterData>();
        for (int i = 0; i < datas.Length; i++)
        {
            D_Data[datas[i].ID] = new CharacterData(datas[i]);
        }
    }
    public static async void LoadGoogleSheetAndSaveBinary(GoogleSheetReader.GoogleSheetInfo info)
    {
        Debug.Log($"CharacterData LoadGoogleSheet");
        CharacterSheetData[] sheetDatas = await GoogleSheetReader.LoadGoogleSheet2Instances<CharacterSheetData>(info);
        SetDicData(sheetDatas);
        GoogleSheetReader.SaveBinary(D_Data, CharacterData.FileName);
    }
    public static void LoadStreamingData()
    {
        GoogleSheetReader.ReadBinary(out D_Data, FileName);
        Debug.Log($"CharacterData Load Success {D_Data.Count}");
    }
    public static CharacterData GetData(int id)
    {
        if(D_Data.ContainsKey(id))
        {
            return D_Data[id];
        }
        return null;
    }

}
