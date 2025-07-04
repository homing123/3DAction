using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using Cysharp.Threading.Tasks.CompilerServices;
using System;
//캐릭터 스킬
//기본공격
//Q,W,E,R
//주문
//무기스킬

//기본공격은 현재 공격 대상이 사거리 안에 있는지 확인 후 사거리 안에 있다면 자동 시전
//내부 쿨타임(공격속도)를 가지며, 공격모션 선딜후딜이 있음

//다른 스킬들은 사용자 입력에 시전시도를 함

//기본공격은 이동으로 취소되지만 스킬은 이동으로 취소 
public class Vanya : Character
{
    [SerializeField] Vanya_Attack m_VanyaAttackPrefab;
    [SerializeField] Vanya_Q m_VanyaQPrefab;
    [SerializeField] Transform m_VanyaQStartPos;

    [SerializeField] Skill m_ESkill;
    [SerializeField] Skill m_RSkill;

    Skill[] m_Skill;
    float[] m_SkillCoolTime;

    bool m_IsSkill;
    CancellationTokenSource m_SkillCTS;
    bool m_IsAttack;
    CancellationTokenSource m_AttackCTS;
    protected override void Start()
    {
        base.Start();
        m_Skill = new Skill[7];
        m_Skill[0] = m_QSkill;
        m_Skill[1] = m_WBasicSkill;
        m_Skill[2] = m_ESkill;
        m_Skill[3] = m_RSkill;
        m_SkillCoolTime = new float[7];

    }
    protected override void Update()
    {
        base.Update();
        for (int i = 0; i < 7; i++)
        {
            m_SkillCoolTime[i] = m_SkillCoolTime[i] > 0 ? m_SkillCoolTime[i] - Time.deltaTime : m_SkillCoolTime[i];
        }
    }

    #region Attack
    [Header("Attack")]
    [SerializeField] float m_AttackRange;
    [SerializeField] Skill m_AttackSkill;
    [SerializeField] float m_AttackPreDelay;

    protected override bool TryAttack()
    {
        if (m_IsSkill || m_IsAttack)
        {
            return false;
        }
        if (m_AttackCTS != null)
        {
            m_AttackCTS.Dispose();
        }
        m_AttackCTS = new CancellationTokenSource();
        BaseAttack(m_AttackCTS).Forget();
        return true;
    }
    async UniTaskVoid BaseAttack(CancellationTokenSource cts)
    {
        m_IsAttack = true;
        Vector2 dir = (m_AttackTarget.transform.position - transform.position).VT2XZ().normalized;
        await RotToDir(dir, cts);
        if (cts.IsCancellationRequested)
        {
            return;
        }
        await Util.WaitDelay(m_AttackPreDelay, cts);
        if (cts.IsCancellationRequested)
        {
            return;
        }

        if (m_AttackTarget != null)
        {
            float disToTarget = (transform.position - m_AttackTarget.transform.position).VT2XZ().magnitude;
            if (disToTarget <= GetAttackRange())
            {
                Vanya_Attack vanyaAttack = Instantiate(m_VanyaAttackPrefab, m_VanyaQStartPos.position, Quaternion.identity);
                vanyaAttack.Setting(this, m_AttackTarget, in m_AttackSkill);
            }
        }

        m_IsAttack = false;
    }
    public override void CancelAttack()
    {
        if (m_IsAttack)
        {
            m_AttackCTS.Cancel();
            m_IsAttack = false;
        }
    }
    public override float GetAttackRange()
    {
        return m_AttackRange;
    }
    #endregion
    #region Skill
    protected override bool TryUseSkill(KeyCode key)
    {
        int idx = -1;
        Action skillUnitask = null;
        switch (key)
        {
            case KeyCode.Q:
                idx = 0;
                skillUnitask = () =>
                {
                    QSkill(m_SkillCTS).Forget();
                };
                break;
            case KeyCode.W:
                idx = 1;
                if (m_WBasicCasting)
                {
                    m_WBasicCasting = false;
                    CancelSkill();
                    if (m_SkillCTS != null)
                    {
                        m_SkillCTS.Dispose();
                    }
                    m_SkillCTS = new CancellationTokenSource();
                    WSecondSkill(m_SkillCTS).Forget();
                    m_SkillCoolTime[idx] = m_WSecondSkill.coolTime;
                    return true;
                }
                skillUnitask = () =>
                {
                    WBasicSkill(m_SkillCTS).Forget();
                };
                break;
            case KeyCode.E:
                idx = 2;
                break;
            case KeyCode.R:
                idx = 3;
                break;
            case KeyCode.D:
                idx = 4;
                break;
            case KeyCode.F:
                idx = 5;
                break;
            default:
                return false;
        }
        if (idx == -1)
        {
            return false;
        }
        if (m_SkillCoolTime[idx] > 0)
        {
            //쿨타임
            return false;
        }
        if (m_Status.m_CurMP < m_Skill[idx].useMP)
        {
            //엠피부족
            return false;
        }
        if (m_IsSkill)
        {
            //다른 행동중
            return false;
        }
        if(m_IsAttack)
        {
            CancelAttack();
        }

        m_SkillCoolTime[idx] = m_Skill[idx].coolTime;
        if (m_SkillCTS != null)
        {
            m_SkillCTS.Dispose();
        }
        m_SkillCTS = new CancellationTokenSource();
        skillUnitask();
        return true;
    }
    #region QSkill
    [Header("QSkill")]
    [SerializeField] Skill m_QSkill;
    [SerializeField] float m_QSkillPreDelay;
    void QSkillReturn2Vanya()
    {

    }
    async UniTaskVoid QSkill(CancellationTokenSource cts)
    {
        m_IsSkill = true;
        Vector2 dir = PlayerInput.Ins.GetSkillDirVT2(transform.position);
        await RotToDir(dir, cts);
        if (cts.IsCancellationRequested)
        {
            return;
        }
        await Util.WaitDelay(m_QSkillPreDelay, cts);
        if (cts.IsCancellationRequested)
        {
            return;
        }
        Vanya_Q vanyaQ = Instantiate(m_VanyaQPrefab, m_VanyaQStartPos.position, Quaternion.identity);
        vanyaQ.Setting(this, dir, m_VanyaQStartPos.localPosition.y, in m_QSkill, QSkillReturn2Vanya);
        m_IsSkill = false;

    }
    #endregion
    #region WSkill
    [Header("WSkill")]
    [SerializeField] float m_WSkillPreDelay = 0.1f;
    [SerializeField] float m_WSkillRadius = 3;
    [SerializeField] Skill m_WBasicSkill;
    [SerializeField] float m_WBasicLoopDelay; //w스킬 사이의 반복 딜레이
    bool m_WBasicCasting;
    const int WBasicLoopCount = 5;

    [SerializeField] Skill m_WSecondSkill;
    [SerializeField] float m_WSecondAnimTime;
    void WSkillHit(in HitRangeInfo hitRangeInfo, in DamageInfo dmginfo)
    {
        Collider[] hits = HitRange.Overlap(in hitRangeInfo, out int count);
        for(int i=0;i<count;i++)
        {
            Bio hitBio = hits[i].GetComponent<Bio>();
            if (Bio.IsHit(this, hitBio, Skill_Target_Type.Enemy | Skill_Target_Type.Monster))
            {
                hitBio.GetAttacked(this, in dmginfo, default);
            }
        }
    }
    async UniTaskVoid WBasicSkill(CancellationTokenSource cts)
    {
        Debug.Log("WBasicStart");

        m_IsSkill = true;
        await Util.WaitDelay(m_WSkillPreDelay, cts);
        if(cts.IsCancellationRequested)
        {
            return;
        }
        m_WBasicCasting = true;
        //이속증가
        for (int i = 0; i < WBasicLoopCount; i++)
        {
            HitRangeInfo hitRangeInfo = new HitRangeInfo();
            hitRangeInfo.SetCircleFollowTarget(transform, m_WSkillRadius, m_WBasicLoopDelay);
            HitRange.Create(in hitRangeInfo);

            WSkillHit(in hitRangeInfo, in m_WBasicSkill.dmg);
            if(i== WBasicLoopCount - 1)
            {
                break;
            }
            await Util.WaitDelay(m_WBasicLoopDelay, cts);
            if (cts.IsCancellationRequested)
            {
                return;
            }
        }
        m_WBasicCasting = false;
        m_IsSkill = false;
        Debug.Log("WBasicEnd");
    }

    async UniTaskVoid WSecondSkill(CancellationTokenSource cts)
    {
        Debug.Log("WSecondStart");

        m_IsSkill = true;
        //이동불가
        //목표지정불가
        HitRangeInfo hitRangeInfo = new HitRangeInfo();
        hitRangeInfo.SetCircleFollowTarget(transform, m_WSkillRadius, m_WSecondAnimTime);
        HitRange.Create(in hitRangeInfo);
        await Util.WaitDelay(m_WSecondAnimTime, cts);
        if (cts.IsCancellationRequested)
        {
            return;
        }
        WSkillHit(in hitRangeInfo, in m_WSecondSkill.dmg);
        Debug.Log("WSecondEnd");
    }
    #endregion
    public override void CancelSkill()
    {
        if (m_IsSkill)
        {
            m_SkillCTS.Cancel();
            m_IsSkill = false;
        }
    }
    #endregion
}
