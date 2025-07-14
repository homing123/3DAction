using System;
using UnityEngine;

public class Vanya_W : MonoBehaviour
{
    const int LoopCount = 5;
    const float LoopDelay = 0.6f;
    [SerializeField] float m_Height;
    Vanya m_User;
    SkillData m_SkillData;
    float m_Radius;

    int m_AttackCount;
    float m_CurTime;
    SkillAttackInfo m_SkillAttackInfo;
    HitRangeInfo m_HitRangeInfo;
    HitRange m_HitRange;
    Action ac_WEnd;
    bool m_IsDestroy;
    public void Setting(Vanya user, SkillData skillData, float radius, Action ac_wend)
    {
        m_User = user;
        m_SkillData = skillData;
        m_Radius = radius;
        m_SkillAttackInfo = new SkillAttackInfo(m_User, m_SkillData);
        m_HitRangeInfo = new HitRangeInfo();
        m_HitRangeInfo.SetCircleFollowTarget(m_User.transform, m_Radius, LoopDelay);
        ac_WEnd = ac_wend;
    }
    private void Start()
    {
        HitRange.Create(in m_HitRangeInfo);
    }
    private void Update()
    {
        if(m_IsDestroy)
        {
            return;
        }
        transform.position = m_User.transform.position;

        m_CurTime += Time.deltaTime;
        if(m_CurTime >= LoopDelay)
        {
            m_CurTime -= LoopDelay;
            m_AttackCount++;
            Attack();
            if(m_AttackCount == LoopCount)
            {
                ac_WEnd?.Invoke();
                Destroy();
            }
            else
            {
                m_HitRange = HitRange.Create(in m_HitRangeInfo);
            }
        }
    }
    void Attack()
    {
        m_User.AttackOverlap(in m_HitRangeInfo, in m_SkillAttackInfo);
    }
    public void W2Casting()
    {
        Destroy();
        if (m_HitRange.gameObject != null)
        {
            m_HitRange.Destroy();
        }
    }
    public void Stop()
    {

    }
    public void Resume()
    {

    }
    public void Destroy()
    {
        m_IsDestroy = true;
        Destroy(this.gameObject);
        if(m_HitRange.gameObject!= null)
        {
            m_HitRange.Destroy();
        }
    }

}
