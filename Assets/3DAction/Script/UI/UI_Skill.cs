using UnityEngine;

public class UI_Skill : MonoBehaviour
{
    [SerializeField] UI_SkillIcon[] m_SkillIcons; //7°³ q,w,e,r,p,weapon,spell
    Character m_PlayerCharacter;
    private void Start()
    {
        m_PlayerCharacter = PlayerM.Ins.GetPlayerCharacter();
        m_PlayerCharacter.RegisterInitialized(Setting);
    }
    private void Setting()
    {
        m_PlayerCharacter = PlayerM.Ins.GetPlayerCharacter();
        SkillPos[] enumArray = EnumArray<SkillPos>.GetArray();
        for (int i = 0; i < 7; i++)
        {
            m_SkillIcons[i].Setting(m_PlayerCharacter.GetCharacterSkill(enumArray[i]), enumArray[i]);
        }
    }
}
