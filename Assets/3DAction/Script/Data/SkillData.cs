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
    Increasing = 1, //�����
    Decreasing = 2, //������
}
//ĳ������ �� ��ų�� ���´� ��ó�� �������� ��Ÿ�Ӱ��Ҹ� �ϴ� �������� üũ�ϴ°ɷ� ����
public enum SkillState
{
    Useable = 0,
    CharacterStateUnUseableSkill = 1, //ĳ���Ͱ� ��ų�� �� �� ���»���
    NotEnoughMP = 2, //��������
    Level0 = 3, //��ų����0 ���������
    Cooldown = 4, //��Ÿ��
    Other = 5, //�ش籸�� ��� �ʿ�, ex �̹��� ����Ż����, ������ ��ź���� ���� �� ���� ĳ���� ������ ��ġ����
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
