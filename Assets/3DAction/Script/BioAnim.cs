using System.Collections.Generic;
using UnityEngine;
public enum ANIM_TRIGGER
{
    IDLE = 0,
    WALK = 1,
    RUN = 2,
    JUMP = 3,
}
public class BioAnim : MonoBehaviour
{
    const float AnimDelay = 0.2f;
    public static Dictionary<ANIM_TRIGGER, string> D_AnimTrigger = new Dictionary<ANIM_TRIGGER, string>()
    {
        { ANIM_TRIGGER.IDLE, "Idle" },
        { ANIM_TRIGGER.WALK, "Walk" },
        { ANIM_TRIGGER.RUN, "Run" },
        { ANIM_TRIGGER.JUMP, "Jump" }
    };
    Animator m_Anim;
    float m_CurDelay;
    ANIM_TRIGGER m_CurPlayAnimTrigger = ANIM_TRIGGER.IDLE;
    ANIM_TRIGGER m_CurAnimTrigger = ANIM_TRIGGER.IDLE;
    private void Awake()
    {
        m_Anim = GetComponentInChildren<Animator>();
    }
    private void Update()
    {
        if(m_CurDelay > 0)
        {
            m_CurDelay -= Time.deltaTime;
        }
        if(m_CurPlayAnimTrigger != m_CurAnimTrigger)
        {
            ChangePlayAnimTrigger();
        }
    }

    void ChangePlayAnimTrigger()
    {
        m_CurPlayAnimTrigger = m_CurAnimTrigger;
        SetAnimTrigger(D_AnimTrigger[m_CurPlayAnimTrigger]);
        m_CurDelay = AnimDelay;
    }
    public void SetAnimTrigger(ANIM_TRIGGER animTrigger)
    {
        if(m_CurAnimTrigger == animTrigger)
        {
            return;
        }
        m_CurAnimTrigger = animTrigger;
        switch(m_CurAnimTrigger)
        {
            case ANIM_TRIGGER.JUMP:
                ChangePlayAnimTrigger();
                break;
            default:
                if (m_CurDelay <= 0)
                {
                    ChangePlayAnimTrigger();
                }
                break;
        }
    }
    public void SetAnimTrigger(string trigger)
    {
        m_Anim.SetTrigger(trigger);
    }
}
