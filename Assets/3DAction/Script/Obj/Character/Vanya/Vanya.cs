using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

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

    Vanya_W m_VaynaW;

    SkillData[] m_QSkillDatas;
    SkillData[] m_WSkillDatas;
    SkillData[] m_W2SkillDatas;
    SkillData[] m_ESkillDatas;
    SkillData[] m_RSkillDatas;
    SkillData[] m_PSkillDatas;
    SkillData[] m_P2SkillDatas;
    SkillData[] m_WeaponSkillDatas;
    SkillData[] m_SpellSkillDatas;

    [Header("Attack")]
    [SerializeField] float m_AttackPreDelay;

    [Header("QSkill")]
    [SerializeField] float m_QSkillPreDelay;

    [Header("WSkill")]
    const int WBasicLoopCount = 5;
    [SerializeField] float m_WSkillRadius = 3;

    [SerializeField] float m_W1SkillPreDelay = 0.1f;

    [SerializeField] float m_W2TotalAnimTime;
    [SerializeField] float m_W2AttackTime;


    //[Header("ESkill")]

    //[Header("RSkill")]

    //[Header("PSkill")]

    Action OnWKeyInput;


    protected override void Start()
    {
        base.Start();
        m_QSkillDatas = SkillData.GetData(QSkillID);
        m_WSkillDatas = SkillData.GetData(WSkillID);
        m_W2SkillDatas = SkillData.GetData(W2SkillID);
        m_ESkillDatas = SkillData.GetData(ESkillID);
        m_RSkillDatas = SkillData.GetData(RSkillID);
        m_PSkillDatas = SkillData.GetData(P1SkillID);
        m_P2SkillDatas = SkillData.GetData(P2SkillID);
        m_WeaponSkillDatas = WeaponSkill.GetWeaponSkillDatas(WeaponType.Arcana);
        m_SpellSkillDatas = SpellSkill.GetSpellSkillDatas(SpellType.Artifact);

        D_SkillPosSkill[SkillPos.Q].SetSkillFunctionAndSkillDatas(m_QSkillDatas, (cts, skillPosSkill) => { QSkill(cts, skillPosSkill).Forget(); });
        D_SkillPosSkill[SkillPos.W].SetSkillFunctionAndSkillDatas(m_WSkillDatas, (cts, skillPosSkill) => { WSkill(cts, skillPosSkill).Forget(); });
        D_SkillPosSkill[SkillPos.E].SetSkillFunctionAndSkillDatas(m_ESkillDatas, (cts, skillPosSkill) => { WSkill(cts, skillPosSkill).Forget(); });
        D_SkillPosSkill[SkillPos.R].SetSkillFunctionAndSkillDatas(m_RSkillDatas, (cts, skillPosSkill) => { WSkill(cts, skillPosSkill).Forget(); });
        D_SkillPosSkill[SkillPos.P].SetSkillFunctionAndSkillDatas(m_P2SkillDatas, (cts, skillPosSkill) => { WSkill(cts, skillPosSkill).Forget(); });
        D_SkillPosSkill[SkillPos.Weapon].SetSkillFunctionAndSkillDatas(m_WeaponSkillDatas, (cts, skillPosSkill) => { WSkill(cts, skillPosSkill).Forget(); });
        D_SkillPosSkill[SkillPos.Spell].SetSkillFunctionAndSkillDatas(m_SpellSkillDatas, (cts, skillPosSkill) => { WSkill(cts, skillPosSkill).Forget(); });
        m_isInit = true;
        OnInitialized?.Invoke();
    }
    protected override void Update()
    {
        base.Update();
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();
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
                SkillAttackInfo attackInfo = new SkillAttackInfo(this, damage, isCritical);
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
        SkillPos skillPos = SkillPos.Q;
        switch (key)
        {
            case KeyCode.Q:
                skillPos = SkillPos.Q;
                break;
            case KeyCode.W:
                skillPos = SkillPos.W;
                break;
            case KeyCode.E:
                skillPos = SkillPos.E;
                break;
            case KeyCode.R:
                skillPos = SkillPos.R;
                break;
            case KeyCode.D:
                skillPos = SkillPos.Weapon;
                break;
            case KeyCode.F:
                skillPos = SkillPos.Spell;
                break;
            default:
                return false;
        }
        if (D_SkillPosSkill[skillPos].m_SkillState == SkillState.Useable)
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
            D_SkillPosSkill[skillPos].ac_SkillFunction?.Invoke(m_SkillCTS, D_SkillPosSkill[skillPos]);
            return true;
        }
        else
        {
            UI_SkillFailText.SetSkillState(D_SkillPosSkill[skillPos].m_SkillState);
            return false;
        }
    }
    #region QSkill
    void QSkillReturn2Vanya()
    {
        D_SkillPosSkill[SkillPos.Q].CooldownReduction(4);
    }
    async UniTaskVoid QSkill(CancellationTokenSource cts, SkillPosSkill skillPosSkill)
    {
        m_IsSkill = true;
        SkillData skillData = skillPosSkill.GetSkillData();
        skillPosSkill.CooldownSet(skillData);
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
        SkillAttackInfo attackInfo = new SkillAttackInfo(this, skillData);
        vanyaQ.Setting(this, dir, m_VanyaQStartPos.localPosition.y, in attackInfo, QSkillReturn2Vanya);
        m_IsSkill = false;

    }
    #endregion
    #region WSkill

    void WSkillCancelAndW2SkillCasting()
    {
        CancelSkill();
        //이속증가효과 제거

    }
    void WSkillEnd()
    {
        D_SkillPosSkill[SkillPos.W].SetSkillFunctionAndSkillDatas(m_WSkillDatas, (cts, skillPosSkill) => { WSkill(cts, skillPosSkill).Forget(); });
        D_SkillPosSkill[SkillPos.W].CooldownSet(D_SkillPosSkill[SkillPos.W].GetSkillData());
    }
    async UniTaskVoid WSkill(CancellationTokenSource cts, SkillPosSkill skillPosSkill)
    {
        //w스킬은 5번공격하는 오브젝트를 생성하는 형태로 가자
        //그리고 오브젝트가 생성되어 있는동안 w2번째 스킬을 쓸 수 있는 형태로
        m_IsSkill = true;
        SkillData skillData = skillPosSkill.GetSkillData();
        skillPosSkill.CooldownSet(skillData);
        await Util.WaitDelay(m_W1SkillPreDelay, cts);
        if(cts.IsCancellationRequested)
        {
            return;
        }
        m_VaynaW = Instantiate(m_VanyaWPrefab);
        //이속증가
        m_VaynaW.Setting(this, skillData, m_WSkillRadius, WSkillEnd);    
        OnWKeyInput += WSkillCancelAndW2SkillCasting;
        m_IsSkill = false;
        skillPosSkill.SetSkillFunctionAndSkillDatas(m_W2SkillDatas, (cts, skillPosSkill) => { WSecondSkill(cts, skillPosSkill).Forget(); });
        skillPosSkill.CooldownSet(0.5f);
    }

    async UniTaskVoid WSecondSkill(CancellationTokenSource cts, SkillPosSkill skillPosSkill)
    {
        m_VaynaW.Destroy();
        SkillData w2SkillData = skillPosSkill.GetSkillData();
        int skillLevel = m_Status.m_SkillLevel[(int)skillPosSkill.m_SkillPos];
        SkillData w1SkillData = m_WSkillDatas[skillLevel - 1];
        m_IsSkill = true;
        //이동불가
        //목표지정불가
        HitRangeInfo hitRangeInfo = new HitRangeInfo();
        hitRangeInfo.SetCircleFollowTarget(transform, m_WSkillRadius, m_W2AttackTime);
        HitRange.Create(in hitRangeInfo);
        await Util.WaitDelay(m_W2AttackTime, cts);
        if (cts.IsCancellationRequested)
        {
            return;
        }
        SkillAttackInfo attackInfo = new SkillAttackInfo(this, w2SkillData);
        AttackOverlap(in hitRangeInfo, in attackInfo);

        float remainingTime = m_W2TotalAnimTime - m_W2AttackTime;
        await Util.WaitDelay(remainingTime, cts);
        if (cts.IsCancellationRequested)
        {
            return;
        }
        D_SkillPosSkill[SkillPos.W].SetSkillFunctionAndSkillDatas(m_WSkillDatas, (cts,skillPosSkill) => { WSkill(cts, skillPosSkill).Forget(); });
        D_SkillPosSkill[SkillPos.W].CooldownSet(m_Status.CalcSkillCooldown(w2SkillData) + m_Status.CalcSkillCooldown(w1SkillData));
        m_IsSkill = false;
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
