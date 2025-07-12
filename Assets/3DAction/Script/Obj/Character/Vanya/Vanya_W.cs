using System;
using UnityEngine;

public class Vanya_W : MonoBehaviour
{
    Vanya m_User;
    SkillData m_SkillData;
    Action ac_WSkillEnd;
    public void Setting(Vanya user, SkillData skillData, Action wSkillEnd)
    {
        m_User = user;
        m_SkillData = skillData;
        ac_WSkillEnd = wSkillEnd;
    }
}
