using UnityEngine;
using TMPro;

public class UI_Status : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI T_AD;
    [SerializeField] TextMeshProUGUI T_SD;
    [SerializeField] TextMeshProUGUI T_AttackDmgMul;
    [SerializeField] TextMeshProUGUI T_Defense;
    [SerializeField] TextMeshProUGUI T_AttackSpeed;
    [SerializeField] TextMeshProUGUI T_SkillCooldown;
    [SerializeField] TextMeshProUGUI T_CriticalPer;
    [SerializeField] TextMeshProUGUI T_MoveSpeed;

    Character m_PlayerCharacter;
    private void Start()
    {
        m_PlayerCharacter = PlayerM.Ins.GetPlayerCharacter();
        m_PlayerCharacter.RegisterInitialized(Setting);
        m_PlayerCharacter.m_Status.OnStatusChanged += Setting;
    }

    void Setting()
    {
        T_AD.text = m_PlayerCharacter.m_Status.m_TotalDmg.ToString("F0");
        T_SD.text = m_PlayerCharacter.m_Status.m_TotalSkillDmg.ToString("F0");
        T_AttackDmgMul.text = "0";
        T_Defense.text = m_PlayerCharacter.m_Status.m_TotalArmor.ToString("F0");
        T_AttackSpeed.text = m_PlayerCharacter.m_Status.m_TotalAttackSpeed.ToString("F2");
        T_SkillCooldown.text = m_PlayerCharacter.m_Status.m_TotalSkillCooldown.ToString("F0");
        T_CriticalPer.text = m_PlayerCharacter.m_Status.m_TotalCriticalPer.ToString("F0");
        T_MoveSpeed.text = (m_PlayerCharacter.m_Status.m_TotalMoveSpeed * 100).ToString("F0");
    }



}
