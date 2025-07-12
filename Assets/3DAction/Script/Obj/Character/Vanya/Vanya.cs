using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using System.Collections.Generic;

public class Vanya : Character
{
    const int QSkillID = 1000;
    const int WSkillID = 1001;
    const int W2SkillID = 1002;
    const int ESkillID = 1003;
    const int RSkillID = 1004;
    const int P1SkillID = 1005;
    const int P2SkillID = 1006;

    [SerializeField] Vanya_Attack m_VanyaAttackPrefab;
    [SerializeField] Vanya_Q m_VanyaQPrefab;
    [SerializeField] Vanya_W m_VanyaWPrefab;
    [SerializeField] Vanya_E m_VanyaEPrefab;
    [SerializeField] Vanya_R m_VanyaRPrefab;
    [SerializeField] Transform m_VanyaQStartPos;

    Dictionary<SkillPos, CharacterSkill> D_CharacterSkill;

    [Header("Attack")]
    [SerializeField] float m_AttackPreDelay;

    [Header("QSkill")]
    [SerializeField] float m_QSkillPreDelay;

    [Header("WSkill")]
    const int WBasicLoopCount = 5;
    [SerializeField] float m_WSkillRadius = 3;

    [SerializeField] float m_W1SkillPreDelay = 0.1f;
    [SerializeField] float m_W1LoopDelay; //w스킬 사이의 반복 딜레이

    [SerializeField] float m_W2TotalAnimTime;
    [SerializeField] float m_W2AttackTime;

    //[Header("ESkill")]

    //[Header("RSkill")]

    //[Header("PSkill")]

    Action OnWKeyInput;


    protected override void Start()
    {
        base.Start();
        D_CharacterSkill = new Dictionary<SkillPos, CharacterSkill>(7);
        D_CharacterSkill[SkillPos.Q] = new CharacterSkill(QSkillID);
        D_CharacterSkill[SkillPos.W] = new CharacterSkill(WSkillID);
        D_CharacterSkill[SkillPos.E] = new CharacterSkill(ESkillID);
        D_CharacterSkill[SkillPos.R] = new CharacterSkill(RSkillID);
        D_CharacterSkill[SkillPos.P] = new CharacterSkill(P1SkillID);
        D_CharacterSkill[SkillPos.Weapon] = new CharacterSkill(WeaponSkill.GetWeaponSkillID(WeaponType.Arcana));
        D_CharacterSkill[SkillPos.Spell] = new CharacterSkill(SpellSkill.GetSpellSkillID(SpellType.Artifact));

    }
    protected override void Update()
    {
        base.Update();
        foreach(SkillPos skillPos in D_CharacterSkill.Keys)
        {
            D_CharacterSkill[skillPos].CooldownReduction(Time.deltaTime);
            D_CharacterSkill[skillPos].CheckState(this);
        }
    }

    #region Attack

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
        bool isCritical = false;
        float damage = m_Status.CalcAttackDamage(out isCritical);

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
                SkillAttackInfo attackInfo = new SkillAttackInfo(this, damage);
                vanyaAttack.Setting(this, m_AttackTarget, in attackInfo);
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
        return m_Status.m_TotalAttackRange;
    }
    #endregion
    #region Skill
    protected override bool TryUseSkill(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.Q:
                if(D_CharacterSkill[SkillPos.Q].skillState == SkillState.Useable)
                {
                    if (m_IsAttack)
                    {
                        CancelAttack();
                    }
                    if (m_SkillCTS != null)
                    {
                        m_SkillCTS.Dispose();
                    }
                    m_SkillCTS = new CancellationTokenSource();
                    QSkill(m_SkillCTS).Forget();
                    return true;
                }
                else
                {
                    UI_SkillFailText.SetSkillState(D_CharacterSkill[SkillPos.Q].skillState);
                    return false;
                }
                break;
            case KeyCode.W:
                if (D_CharacterSkill[SkillPos.W].skillState == SkillState.Useable)
                {
                    if (m_IsAttack)
                    {
                        CancelAttack();
                    }
                    if (m_SkillCTS != null)
                    {
                        m_SkillCTS.Dispose();
                    }
                    m_SkillCTS = new CancellationTokenSource();
                    WSkill(m_SkillCTS).Forget();
                    return true;
                }
                else
                {
                    UI_SkillFailText.SetSkillState(D_CharacterSkill[SkillPos.W].skillState);
                    return false;
                }
                //idx = 1;
                ////w2 스킬 쓰는곳
                //if (m_WBasicCasting)
                //{
                //    m_WBasicCasting = false;
                //    CancelSkill();
                //    if (m_SkillCTS != null)
                //    {
                //        m_SkillCTS.Dispose();
                //    }
                //    m_SkillCTS = new CancellationTokenSource();
                //    WSecondSkill(m_SkillCTS).Forget();
                //    m_SkillCoolTime[idx] = m_Status.CalcSkillCooldown(m_WSkillData_1) + m_Status.CalcSkillCooldown(m_WSkillData_2);
                //    return true;
                //}
                //skillUnitask = () =>
                //{
                //    WBasicSkill(m_SkillCTS).Forget();
                //};
                break;
            case KeyCode.E:
                break;
            case KeyCode.R:
                break;
            case KeyCode.D:
                break;
            case KeyCode.F:
                break;
            default:
                return false;
        }
       
        return true;
    }
    #region QSkill
    void QSkillReturn2Vanya()
    {
        Debug.Log("QSkill Return to Vanya");
    }
    async UniTaskVoid QSkill(CancellationTokenSource cts)
    {
        m_IsSkill = true;
        SkillData skillData = D_CharacterSkill[SkillPos.Q].GetSkillData();
        D_CharacterSkill[SkillPos.Q].CooldownSet(skillData);
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
        SkillAttackInfo attackInfo = new SkillAttackInfo(this, D_CharacterSkill[SkillPos.Q].GetSkillData());
        vanyaQ.Setting(this, dir, m_VanyaQStartPos.localPosition.y, in attackInfo, QSkillReturn2Vanya);
        m_IsSkill = false;

    }
    #endregion
    #region WSkill

    void WSkillHit(in HitRangeInfo hitRangeInfo, in SkillAttackInfo dmginfo)
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
    void WSkillCancelAndW2SkillCasting()
    {
        CancelSkill();
        //이속증가효과 제거

    }
    async UniTaskVoid WSkill(CancellationTokenSource cts)
    {
        //w스킬은 5번공격하는 오브젝트를 생성하는 형태로 가자
        //그리고 오브젝트가 생성되어 있는동안 w2번째 스킬을 쓸 수 있는 형태로
        m_IsSkill = true;
        SkillData skillData = D_CharacterSkill[SkillPos.W].GetSkillData();
        D_CharacterSkill[SkillPos.W].CooldownSet(skillData);
        await Util.WaitDelay(m_W1SkillPreDelay, cts);
        if(cts.IsCancellationRequested)
        {
            return;
        }
        Vanya_W vanyaW = Instantiate(m_VanyaWPrefab);
        vanyaW.Setting(this, skillData);
        OnWKeyInput += WSkillCancelAndW2SkillCasting;

        //이속증가
        m_IsSkill = false;
        Debug.Log("WBasicEnd");
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
