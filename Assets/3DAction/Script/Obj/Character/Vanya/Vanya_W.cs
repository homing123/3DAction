using System;
using UnityEngine;

public class Vanya_W : MonoBehaviour
{
    const int LoopCount = 5;
    const float LoopDelay = 0.6f;
    [SerializeField] float m_Height;
    Vanya m_User;
    SkillData m_SkillData;
    Action ac_WSkillEnd;
    float m_Radius;

    int m_AttackCount;
    float m_CurTime;
    SkillAttackInfo m_SkillAttackInfo;
    HitRangeInfo m_HitRangeInfo;
    public void Setting(Vanya user, SkillData skillData, float radius, Action wSkillEnd)
    {
        m_User = user;
        m_SkillData = skillData;
        m_Radius = radius;
        ac_WSkillEnd = wSkillEnd;
        m_SkillAttackInfo = new SkillAttackInfo(m_User, m_SkillData);
        m_HitRangeInfo = new HitRangeInfo();
        m_HitRangeInfo.SetCircleFollowTarget(m_User.transform, m_Radius, LoopDelay);
    }
    private void Start()
    {
        HitRange.Create(in m_HitRangeInfo);
    }
    private void Update()
    {
        transform.position = m_User.transform.position;

        m_CurTime += Time.deltaTime;
        if(m_CurTime >= LoopDelay)
        {
            m_CurTime -= LoopDelay;
            m_AttackCount++;
            Attack();
            if(m_AttackCount == LoopCount)
            {
                ac_WSkillEnd?.Invoke();
                Destroy();
            }
            else
            {
                HitRange.Create(in m_HitRangeInfo);
            }
        }
    }
    void Attack()
    {
        m_User.AttackOverlap(in m_HitRangeInfo, in m_SkillAttackInfo);
    }
    public void Cancel()
    {

    }
    public void Stop()
    {

    }
    public void Resume()
    {

    }
    public void Destroy()
    {
        Destroy(this.gameObject);
    }

}
