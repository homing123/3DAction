using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
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
    [SerializeField] float m_AttackRange;
    [SerializeField] Vanya_Attack m_VanyaAttackPrefab;
    [SerializeField] Vanya_Q m_VanyaQPrefab;
    [SerializeField] Transform m_VanyaQStartPos;
    [SerializeField] Skill m_AttackSkill;
    [SerializeField] Skill m_QSkill;
    [SerializeField] Skill m_WSkill;
    [SerializeField] Skill m_ESkill;
    [SerializeField] Skill m_RSkill;

    Skill[] m_Skill;

    float[] m_SkillCoolTime;

    CancellationTokenSource m_SkillToken;
    protected override void Start()
    {
        base.Start();
        m_Skill = new Skill[7];
        m_Skill[0] = m_QSkill;
        m_Skill[1] = m_WSkill;
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

    void TryUseSkill(int idx)
    {
        if (m_SkillCoolTime[idx] > 0)
        {
            //쿨타임
            return;
        }
        if(m_Status.m_CurMP < m_Skill[idx].useMP)
        {
            //엠피부족
            return;
        }

        UseSkill(idx);
    }
    void UseSkill(int idx)
    {        
        m_SkillCoolTime[idx] = m_Skill[idx].coolTime;
        m_SkillToken = new CancellationTokenSource();
        QSkill(m_SkillToken).Forget();
    }

    [SerializeField] float BaseAttack_PreDelay;
    async UniTaskVoid BaseAttack(CancellationTokenSource ct)
    {
        float preDelay = BaseAttack_PreDelay;
        float curTime = 0;
        Vector2 dir = (m_AttackTarget.transform.position - transform.position).VT2XZ().normalized;
        await RotToDir(dir, ct);
        while (curTime < preDelay)
        {
            if (ct.IsCancellationRequested)
            {
                return;
            }
            await UniTask.Yield();

            curTime += Time.deltaTime;
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
        else
        {
            Debug.Log("타겟없음");
        }
    }

    [SerializeField] float QSkill_PreDelay;
    void QSkillReturn2Vanya()
    {

    }
    async UniTaskVoid QSkill(CancellationTokenSource ct)
    {
        float preDelay = QSkill_PreDelay;
        float curTime = 0;
        Vector2 dir = PlayerInput.Ins.GetSkillDirVT2(transform.position);
        await RotToDir(dir, ct);
        while (curTime < preDelay)
        {
            if(ct.IsCancellationRequested)
            {
                return;
            }
            await UniTask.Yield();

            curTime += Time.deltaTime;
        }
        Vanya_Q vanyaQ = Instantiate(m_VanyaQPrefab, m_VanyaQStartPos.position, Quaternion.identity);
        vanyaQ.Setting(this, dir, m_VanyaQStartPos.localPosition.y, in m_QSkill, QSkillReturn2Vanya);


    }


    public override float GetAttackRange()
    {
        return m_AttackRange;
    }
    protected override void TryUseSkill(KeyCode key)
    {
        if (key == KeyCode.Q)
        {
            TryUseSkill(0);
        }
    }
    protected override void Attack()
    {
        m_SkillToken = new CancellationTokenSource();
        BaseAttack(m_SkillToken).Forget();
    }

}
