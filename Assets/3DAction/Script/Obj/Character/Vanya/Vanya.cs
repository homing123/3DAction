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

        D_SkillPosSkill[SkillPos.Q].SetSkillFunctionAndSkillDatas(m_QSkillDatas);
        D_SkillPosSkill[SkillPos.W].SetSkillFunctionAndSkillDatas(m_WSkillDatas);
        D_SkillPosSkill[SkillPos.E].SetSkillFunctionAndSkillDatas(m_ESkillDatas);
        D_SkillPosSkill[SkillPos.R].SetSkillFunctionAndSkillDatas(m_RSkillDatas);
        D_SkillPosSkill[SkillPos.P].SetSkillFunctionAndSkillDatas(m_P2SkillDatas);
        D_SkillPosSkill[SkillPos.Weapon].SetSkillFunctionAndSkillDatas(m_WeaponSkillDatas);
        D_SkillPosSkill[SkillPos.Spell].SetSkillFunctionAndSkillDatas(m_SpellSkillDatas);
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
    protected override async UniTaskVoid BaseAttack(CancellationTokenSource cts)
    {
        Bio target = m_ActionStateAttack.m_Target;
        Vector2 dir = (target.transform.position - transform.position).VT2XZ().normalized;
        float rotTime = await RotToDir(dir, cts);
        if (cts.IsCancellationRequested)
        {
            return;
        }
        bool isCritical = false;
        float damage = m_Status.CalcAttackDamage(out isCritical);

        float attackPreDelay = m_AttackPreDelay - rotTime;
        if (attackPreDelay > 0)
        {
            await Util.WaitDelay(m_AttackPreDelay, cts);
        }

        if (cts.IsCancellationRequested)
        {
            return;
        }
        m_CurAttackDelay = 0;
        if (target != null)
        {
            Vanya_Attack vanyaAttack = Instantiate(m_VanyaAttackPrefab, m_VanyaQStartPos.position, Quaternion.identity);
            SkillAttackInfo attackInfo = new SkillAttackInfo(this, damage, isCritical);
            vanyaAttack.Setting(this, target, in attackInfo);
        }

        AttackEnd();
    }
   
    public override float GetAttackRange()
    {
        return m_Status.m_TotalAttackRange;
    }
   
    #region QSkill
    void QSkillReturn2Vanya()
    {
        D_SkillPosSkill[SkillPos.Q].CooldownReduction(4);
    }
    protected override async UniTaskVoid QSkill(CancellationTokenSource cts, SkillPosSkill skillPosSkill)
    {
        SkillData skillData = skillPosSkill.GetSkillData();
        skillPosSkill.CooldownSet(skillData);
        Vector2 user2Mouse = PlayerInput.Ins.GetUser2MouseVT2(transform.position);
        Vector2 dir = user2Mouse.normalized;
        RotToDirImmediate(dir);
        await Util.WaitDelay(m_QSkillPreDelay, cts);
        if (cts.IsCancellationRequested)
        {
            return;
        }
        Vanya_Q vanyaQ = Instantiate(m_VanyaQPrefab, m_VanyaQStartPos.position, Quaternion.identity);
        SkillAttackInfo attackInfo = new SkillAttackInfo(this, skillData);
        vanyaQ.Setting(this, dir, m_VanyaQStartPos.localPosition.y, in attackInfo, QSkillReturn2Vanya);
        SkillEnd();
    }
    #endregion
    #region WSkill

    void WSkillEnd()
    {
        D_SkillPosSkill[SkillPos.W].SetSkillFunctionAndSkillDatas(m_WSkillDatas);
        D_SkillPosSkill[SkillPos.W].CooldownSet(D_SkillPosSkill[SkillPos.W].GetSkillData());
    }
    protected override async UniTaskVoid WSkill(CancellationTokenSource cts, SkillPosSkill skillPosSkill)
    {
        SkillData skillData = skillPosSkill.GetSkillData();
        if(skillData.ID == WSkillID)
        {
            WFirstSkill(cts, skillPosSkill).Forget();
        }
        else
        {
            WSecondSkill(cts, skillPosSkill).Forget();
        }
    }

    async UniTaskVoid WFirstSkill(CancellationTokenSource cts, SkillPosSkill skillPosSkill)
    {
        SkillData skillData = skillPosSkill.GetSkillData();
        skillPosSkill.CooldownSet(skillData);
        await Util.WaitDelay(m_W1SkillPreDelay, cts);
        if (cts.IsCancellationRequested)
        {
            return;
        }
        m_VaynaW = Instantiate(m_VanyaWPrefab);
        //�̼�����
        m_VaynaW.Setting(this, skillData, m_WSkillRadius, WSkillEnd);
        D_SkillPosSkill[SkillPos.W].SetSkillFunctionAndSkillDatas(m_W2SkillDatas);
        D_SkillPosSkill[SkillPos.W].CooldownSet(0.5f);
        SkillEnd();
    }
    async UniTaskVoid WSecondSkill(CancellationTokenSource cts, SkillPosSkill skillPosSkill)
    {
        //w1 �̼ӹ���ȿ�� ����
        m_VaynaW.Destroy();
        SkillData w2SkillData = skillPosSkill.GetSkillData();
        int skillLevel = m_Status.SkillLevel[(int)skillPosSkill.m_SkillPos];
        SkillData w1SkillData = m_WSkillDatas[skillLevel - 1];
        //�̵��Ұ�
        //��ǥ�����Ұ�
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
        D_SkillPosSkill[SkillPos.W].SetSkillFunctionAndSkillDatas(m_WSkillDatas);
        D_SkillPosSkill[SkillPos.W].CooldownSet(m_Status.CalcSkillCooldown(w2SkillData) + m_Status.CalcSkillCooldown(w1SkillData));
        SkillEnd();
    }
    #endregion
    #region ESkill
    [SerializeField] float m_ESkillPreDelay;
    [SerializeField] float m_ESkillDistance;
    [SerializeField] float m_ESkillCorrectionDis;
    [SerializeField] float m_ESkillMoveSpeed;
    protected override async UniTaskVoid ESkill(CancellationTokenSource cts, SkillPosSkill skillPosSkill)
    {
        //�̵� �Ÿ��� ª������ preDelay�� ª��

        //e��ų ������Ʈ ����
        //�����Ұ�
        SkillData skillData = skillPosSkill.GetSkillData();
        skillPosSkill.CooldownSet(skillData);
        await Util.WaitDelay(m_ESkillPreDelay, cts);
        Vector2 user2Mouse = PlayerInput.Ins.GetUser2MouseVT2(transform.position);
        Vector2 toMouseDir = user2Mouse.normalized;
        float toMouseDis = user2Mouse.magnitude;
        float moveDis = toMouseDis > m_ESkillDistance ? m_ESkillDistance: toMouseDis;

        Vector3 desti = transform.position + (moveDis * toMouseDir).VT2XZToVT3();
        Debug.Log(desti);

        while (true)
        {
            transform.position += Util.MoveDesti(transform.position, desti, m_ESkillMoveSpeed * Time.deltaTime, out bool isArrived);
            if(isArrived)
            {
                break;
            }
            await UniTask.Yield();
            if (cts.IsCancellationRequested)
            {
                return;
            }
        }

        //�����Ұ� ����
        //e��ų ������Ʈ ����

        SkillEnd();
    }
    #endregion

    protected override async UniTaskVoid RSkill(CancellationTokenSource cts, SkillPosSkill skillPosSkill)
    {

    }

}
