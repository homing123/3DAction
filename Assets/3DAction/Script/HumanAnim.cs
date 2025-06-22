using System.Collections.Generic;
using UnityEngine;
public enum Anim_Trigger
{
    Idle = 0,
    Walk = 1,
    Run = 2,
    Jump = 3,
    Falling = 4,
}
public class HumanAnim : MonoBehaviour
{
    public static Dictionary<Anim_Trigger, string> D_AnimTrigger = new Dictionary<Anim_Trigger, string>()
    {
        { Anim_Trigger.Idle, "Idle" },
        { Anim_Trigger.Walk, "Walk" },
        { Anim_Trigger.Run, "Run" },
        { Anim_Trigger.Jump, "Jump" },
        { Anim_Trigger.Falling, "Falling" }
    };
    Animator m_Anim;
    Anim_Trigger m_CurAnimTrigger = Anim_Trigger.Idle;
    Anim_Trigger m_LastTrigger = Anim_Trigger.Walk;
    private void Awake()
    {
        m_Anim = GetComponentInChildren<Animator>();
    }

    public void SetAnimTrigger(Anim_Trigger animTrigger)
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
