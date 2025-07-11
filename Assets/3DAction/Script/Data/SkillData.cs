using UnityEngine;
using System.Collections.Generic;
public enum BuffTarget
{
    User = 0,
    HitTarget = 1,
}
public enum BuffType
{
    MoveSpeedIncrease = 0,
    MoveSpeedDecrease = 1,
}
public enum BuffGraphType
{ 
    Flat = 0,
    Increasing = 1, //우상향
    Decreasing = 2, //우하향
}
public struct SkillAttackInfo
{
    public float damage { get; private set; }
    public SkillAttackInfo(Bio user, float _damage)
    {
        damage = _damage;
    }
    public SkillAttackInfo(Bio user, SkillData skillData)
    {
        damage = skillData.Damages[0];
    }
}
[System.Serializable]
public class SkillData
{
    public const string FileName = "Skill";
    public class SkillSheetData
    {
        public int ID { get; private set; }
        public int SkillLevel { get; private set; }
        public int SkillName { get; private set; }
        public int SkillDesc { get; private set; }
        public string IconPath { get; private set; }
        public float Damage_0 { get; private set; }
        public float SDMulDamage_0 { get; private set; }
        public float Damage_1 { get; private set; }
        public float SDMulDamage_1 { get; private set; }
        public float MPValue { get; private set; }
        public float Cooldown { get; private set; }
    }

    public int ID { get; private set; }
    public int SkillLevel { get; private set; }
    public int SkillName { get; private set; }
    public int SkillDesc { get; private set; }
    public string IconPath { get; private set; }
    public float[] Damages { get; private set; }
    public float[] SDMulDamages { get; private set; }
    public float MPValue { get; private set; }
    public float Cooldown { get; private set; }

    static Dictionary<int, SkillData[]> D_Data;

    SkillData(SkillSheetData sheetData)
    {
        ID = sheetData.ID;
        SkillLevel = sheetData.SkillLevel;
        SkillName = sheetData.SkillName;
        SkillDesc = sheetData.SkillDesc;
        IconPath = sheetData.IconPath;
        Damages = new float[2] { sheetData.Damage_0, sheetData.Damage_1 };
        SDMulDamages = new float[2] { sheetData.SDMulDamage_0, sheetData.SDMulDamage_1 };
        MPValue = sheetData.MPValue;
        Cooldown = sheetData.Cooldown;
    }
    static void SetDicData(SkillSheetData[] datas)
    {
        List<SkillData> l_skills = new List<SkillData>();
        D_Data = new Dictionary<int, SkillData[]>();
        int lastID = 0;
        for (int i = 0; i < datas.Length; i++)
        {
            if(l_skills.Count > 0 && datas[i].ID != lastID)
            {
                D_Data[lastID] = l_skills.ToArray();
                l_skills.Clear();
            }
            l_skills.Add(new SkillData(datas[i]));
            lastID = datas[i].ID;
            if(i == datas.Length - 1)
            {
                D_Data[lastID] = l_skills.ToArray();
                l_skills.Clear();
            }
        }
    }
    public static async void LoadGoogleSheetAndSaveBinary(GoogleSheetReader.GoogleSheetInfo info)
    {
        Debug.Log($"SkillData LoadGoogleSheet");
        SkillSheetData[] sheetDatas = await GoogleSheetReader.LoadGoogleSheet2Instances<SkillSheetData>(info);
        SetDicData(sheetDatas);
        GoogleSheetReader.SaveBinary(D_Data, SkillData.FileName);
    }
    public static void LoadStreamingData()
    {
        GoogleSheetReader.ReadBinary(out D_Data, FileName);
        Debug.Log($"SkillData Load Success {D_Data.Count}");
    }

}
