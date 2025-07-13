using UnityEngine;

public class UI_SkillLevelUP : MonoBehaviour
{
    [SerializeField] GameObject m_QSkillLevelUp;
    [SerializeField] GameObject m_WSkillLevelUp;
    [SerializeField] GameObject m_ESkillLevelUp;
    [SerializeField] GameObject m_RSkillLevelUp;
    [SerializeField] GameObject m_PSkillLevelUp;
    [SerializeField] GameObject m_SpellSkillLevelUp;

    Character m_PlayerCharacter;
    void Start()
    {
        m_PlayerCharacter = PlayerM.Ins.GetPlayerCharacter();
        m_PlayerCharacter.m_Status.OnSkillLevelUpPointChanged += Setting;
    }
    private void OnDestroy()
    {
        m_PlayerCharacter.m_Status.OnSkillLevelUpPointChanged -= Setting;
    }
    void Setting()
    {
        if(m_PlayerCharacter.m_Status.m_SkillLevelUpPoint > 0)
        {
            m_QSkillLevelUp.SetActive(true);
            m_WSkillLevelUp.SetActive(true);
            m_ESkillLevelUp.SetActive(true);
            m_RSkillLevelUp.SetActive(true);
            m_PSkillLevelUp.SetActive(true);
            m_SpellSkillLevelUp.SetActive(true);
        }
        else
        {
            m_QSkillLevelUp.SetActive(false);
            m_WSkillLevelUp.SetActive(false);
            m_ESkillLevelUp.SetActive(false);
            m_RSkillLevelUp.SetActive(false);
            m_PSkillLevelUp.SetActive(false);
            m_SpellSkillLevelUp.SetActive(false);
        }
    }
    public void Btn_SkillLevelUp(int skillPos)
    {
        LevelUp((SkillPos)skillPos);
    }
    void LevelUp(SkillPos skillPos)
    {
        m_PlayerCharacter.SkillLevelUp(skillPos);
    }
}
