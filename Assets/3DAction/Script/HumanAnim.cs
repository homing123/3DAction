using System.Collections.Generic;
using UnityEngine;
public enum ANIM_TRIGGER
{
    IDLE = 0,
    WALK = 1,
    RUN = 2,
    JUMP = 3,
    FALLING = 4,
}
public class HumanAnim : MonoBehaviour
{
    public static Dictionary<ANIM_TRIGGER, string> D_AnimTrigger = new Dictionary<ANIM_TRIGGER, string>()
    {
        { ANIM_TRIGGER.IDLE, "Idle" },
        { ANIM_TRIGGER.WALK, "Walk" },
        { ANIM_TRIGGER.RUN, "Run" },
        { ANIM_TRIGGER.JUMP, "Jump" },
        { ANIM_TRIGGER.FALLING, "Falling" }
    };
    Animator m_Anim;
    ANIM_TRIGGER m_CurAnimTrigger = ANIM_TRIGGER.IDLE;
    ANIM_TRIGGER m_LastTrigger = ANIM_TRIGGER.WALK;
    private void Awake()
    {
        m_Anim = GetComponentInChildren<Animator>();
    }

    public void SetAnimTrigger(ANIM_TRIGGER animTrigger)
    {
        if (m_CurAnimTrigger == animTrigger)
        {
            return;
        }
        m_CurAnimTrigger = animTrigger;
        ResetTrigger(D_AnimTrigger[m_LastTrigger]);
        m_LastTrigger = m_CurAnimTrigger;
        SetTrigger(D_AnimTrigger[m_CurAnimTrigger]);
    }
    public void ResetTrigger(string trigger)
    {
        m_Anim.ResetTrigger(trigger);
    }
    public void SetTrigger(string trigger)
    {
        m_Anim.SetTrigger(trigger);
    }
}
