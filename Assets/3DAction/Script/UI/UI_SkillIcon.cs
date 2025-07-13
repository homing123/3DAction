using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
public class UI_SkillIcon : MonoBehaviour
{
    const float SkillLevelObjectIntervalX = 21;
    static Dictionary<int, float[]> D_SkillLevelObjectPosX = new Dictionary<int, float[]>(5)
    {
        {1, new float[1] {0} },
        {2, new float[2] {-SkillLevelObjectIntervalX * 0.5f, SkillLevelObjectIntervalX * 0.5f } },
        {3, new float[3] {-SkillLevelObjectIntervalX, 0, SkillLevelObjectIntervalX}},
        {4, new float[4] {-SkillLevelObjectIntervalX * 1.5f, -SkillLevelObjectIntervalX * 0.5f, SkillLevelObjectIntervalX * 0.5f, SkillLevelObjectIntervalX * 1.5f } },
        {5, new float[5] {-SkillLevelObjectIntervalX * 2f, -SkillLevelObjectIntervalX, 0, SkillLevelObjectIntervalX, SkillLevelObjectIntervalX * 2}},
    };

    [SerializeField] GameObject m_SkillUnuseable;
    [SerializeField] GameObject m_SkillLevel0;
    [SerializeField] GameObject m_SkillNotEnoughMP;
    [SerializeField] GameObject m_SkillCooldown;
    [SerializeField] TextMeshProUGUI T_SkillCooldown;
    [SerializeField] Image m_SkillIcon;
    [SerializeField] RectTransform[] m_SkillLevelEmpties;
    [SerializeField] GameObject[] m_SkillLevelEnables;
    [SerializeField] TextMeshProUGUI T_SkillLevel; //weapon, spell 전용
    [SerializeField] TextMeshProUGUI T_MP; //마나사용량 0이면 제외, weapon, spell은 없음
    [SerializeField] GameObject m_KeyObj;
    [SerializeField] TextMeshProUGUI T_Key; //패시브 제외

    CharacterSkill m_CharacterSkill;
    public void Setting(CharacterSkill characterSkill, SkillPos pos)
    {
        m_CharacterSkill = characterSkill;
        SkillData skillData = m_CharacterSkill.GetSkillData();
        m_SkillIcon.sprite = Resources.Load<Sprite>(skillData.IconPath);
        switch(pos)
        {
            case SkillPos.Q:
                T_Key.text = "Q";
                break;
            case SkillPos.W:
                T_Key.text = "W";
                break;
            case SkillPos.E:
                T_Key.text = "E";
                break;
            case SkillPos.R:
                T_Key.text = "R";
                break;
            case SkillPos.P:
                m_KeyObj.gameObject.SetActive(false);
                break;
            case SkillPos.Weapon:
                T_Key.text = "D";
                break;
            case SkillPos.Spell:
                T_Key.text = "F";
                break;
        }

    }
    
    void SetSkillLevel()
    {
        if (m_SkillLevelEnables.Length > 0)
        {
            int curLevel = m_CharacterSkill.skillLevel;
            int maxLevel = m_CharacterSkill.GetMaxLevel();
            for (int i = 0; i < maxLevel; i++)
            {
                m_SkillLevelEmpties[i].gameObject.SetActive(true);
                m_SkillLevelEmpties[i].anchoredPosition = new Vector2(D_SkillLevelObjectPosX[maxLevel][i], 0);
            }
            for(int i=maxLevel;i<m_SkillLevelEmpties.Length;i++)
            {
                m_SkillLevelEmpties[i].gameObject.SetActive(false);
            }

            for (int i = 0; i < curLevel; i++)
            {
                m_SkillLevelEnables[i].SetActive(true);
            }
            for (int i = curLevel; i < m_SkillLevelEnables.Length; i++)
            {
                m_SkillLevelEnables[i].SetActive(false);
            }
        }
        if(T_SkillLevel != null)
        {
            T_SkillLevel.text = m_CharacterSkill.skillLevel.ToString();
        }
    }

    private void Update()
    {
        SkillData skillData = m_CharacterSkill.GetSkillData();

        SetSkillLevel();
        if(T_MP!=null)
        {
            if(skillData.MPValue > 0)
            {
                T_MP.gameObject.SetActive(true);
                T_MP.text = skillData.MPValue.ToString();
            }
            else
            {
                T_MP.gameObject.SetActive(false);
            }
        }
        m_SkillUnuseable.SetActive(m_CharacterSkill.skillState == SkillState.CharacterStateUnUseableSkill);
        m_SkillLevel0.SetActive(m_CharacterSkill.skillState == SkillState.Level0);
        m_SkillNotEnoughMP.SetActive(m_CharacterSkill.skillState == SkillState.NotEnoughMP);
        m_SkillCooldown.SetActive(m_CharacterSkill.skillState == SkillState.Cooldown);
        switch (m_CharacterSkill.skillState)
        {
            case SkillState.Useable:
                break;
            case SkillState.CharacterStateUnUseableSkill:
                break;
            case SkillState.NotEnoughMP:
                break;
            case SkillState.Level0:
                break;
            case SkillState.Cooldown:
                T_SkillCooldown.text = m_CharacterSkill.skillCooldown.ToString("F0");
                break;
        }
    }
}
