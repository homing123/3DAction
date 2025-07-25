using UnityEngine;

public class UI_SkillFailText : MonoBehaviour
{
    static UI_SkillFailText Ins;
    public static void SetID(int id)
    {
        Ins.m_Text.SetTextID(id);
        Ins.Active();
    }
    public static void SetText(string str)
    {
        Ins.m_Text.SetText(str);
        Ins.Active();
    }
    public static void SetSkillState(SkillState skillState)
    {
        switch(skillState)
        {
            case SkillState.CharacterStateUnUseableSkill:
                SetID(TextData.SkillUnUseable);
                break;
            case SkillState.NotEnoughMP:
                SetID(TextData.NotEnoughMP);
                break;
            case SkillState.Level0:
                SetID(TextData.SkillLevel0);
                break;
            case SkillState.Cooldown:
                SetID(TextData.SkillCooldown);
                break;
            case SkillState.Other:
                SetText("SkillState Other");
                break;
        }
    }


    [SerializeField] TextHandler m_Text;
    [SerializeField] float m_LifeTime;
    float m_CurTime;

    private void Awake()
    {
        Ins = this;
    }
    private void Update()
    {
        m_CurTime -= Time.deltaTime;
        if(m_CurTime <= 0)
        {
            gameObject.SetActive(false);
        }
    }
    void Active()
    {
        gameObject.SetActive(true);
        m_CurTime = m_LifeTime;
    }
}
