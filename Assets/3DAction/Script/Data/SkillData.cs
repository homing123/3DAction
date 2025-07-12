using UnityEngine;
using System.Collections.Generic;
public enum SkillType
{
    WeaponSkill,
    SpellSkill,
    CharacterSkill
}

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
//캐릭터의 각 스킬의 상태는 어처피 매프레임 쿨타임감소를 하니 매프레임 체크하는걸로 하자
public enum SkillState
{
    Useable = 0,
    CharacterStateUnUseableSkill = 1, //캐릭터가 스킬을 쓸 수 없는상태
    NotEnoughMP = 2, //마나부족
    Level0 = 3, //스킬레벨0 배우지않음
    Cooldown = 4, //쿨타임
    Other = 5, //해당구조 고려 필요, ex 이바의 바이탈포스, 셀린의 폭탄갯수 부족 과 같은 캐릭터 고유의 수치부족
}
public enum SkillPos
{
    Q,
    W,
    E,
    R,
    P,
    Weapon,
    Spell
}

public struct SkillAttackInfo
{
    public float damage { get; private set; }
    public SkillAttackInfo(Bio user, float _damage)
    {
        damage = _damage;
    }
    public SkillAttackInfo(Bio user, SkillData skillData, int idx = 0)
    {
        damage = user.m_Status.CalcSkillDamage(skillData, idx);
    }
}

public class CharacterSkill
{
    public SkillData[] skillDatas { get; private set; }
    public float skillCooldown { get; private set; }
    public int skillLevel { get; private set; }
    public SkillState skillState { get; private set; }
    public CharacterSkill(int id)
    {
        skillDatas = SkillData.GetData(id);
        skillLevel = 0;
        skillCooldown = 0;
    }
    public void LevelUp()
    {
        skillLevel++;
    }
    public void CooldownSet(float cooldownSetValue)
    {
        skillCooldown = cooldownSetValue;
    }
    public void CooldownSet(SkillData skillData)
    {
        skillCooldown = skillData.Cooldown;
    }
    public void CooldownReduction(float reductionTime)
    {
        skillCooldown -= reductionTime;
        skillCooldown = skillCooldown < 0 ? 0 : skillCooldown;
    }
    public void CheckState(Character character)
    {
        if (skillLevel == 0)
        {
            skillState = SkillState.Level0;
        }
        else if (character.m_Status.Skillable() == false || character.m_IsSkill)
        {
            skillState = SkillState.CharacterStateUnUseableSkill;
        }
        else if (skillCooldown > 0)
        {
            skillState = SkillState.Cooldown;
        }
        else if (skillDatas[skillLevel - 1].MPValue > character.m_Status.m_CurMP)
        {
            skillState = SkillState.NotEnoughMP;
        }
        else
        {
            skillState = SkillState.Useable;
        }
    }
    public SkillData GetSkillData()
    {
        if (skillLevel == 0)
        {
            return skillDatas[0];
        }
        else
        {
            return skillDatas[skillLevel];
        }
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
        public SkillType SkillType { get; private set; }
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
    public SkillType SkillType { get; private set; }
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
        SkillType = sheetData.SkillType;
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

    public static SkillData[] GetData(int id)
    {
        if(D_Data.ContainsKey(id))
        {
            return D_Data[id];
        }
        return null;
    }
}
