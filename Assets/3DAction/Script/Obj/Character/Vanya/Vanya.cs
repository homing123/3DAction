using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
//ĳ���� ��ų
//�⺻����
//Q,W,E,R
//�ֹ�
//���⽺ų

//�⺻������ ���� ���� ����� ��Ÿ� �ȿ� �ִ��� Ȯ�� �� ��Ÿ� �ȿ� �ִٸ� �ڵ� ����
//���� ��Ÿ��(���ݼӵ�)�� ������, ���ݸ�� �����ĵ��� ����

//�ٸ� ��ų���� ����� �Է¿� �����õ��� ��

//�⺻������ �̵����� ��ҵ����� ��ų�� �̵����� ��� 
public class Vanya : Character
{
    [SerializeField] Vanya_Attack m_VanyaAttackPrefab;
    [SerializeField] Vanya_Q m_VanyaQPrefab;
    [SerializeField] Transform m_VanyaQStartPos;

    [SerializeField] float m_AttackRange;
    [SerializeField] Skill m_AttackSkill;
    [SerializeField] Skill m_QSkill;
    [SerializeField] Skill m_WSkill;
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
    [SerializeField] float BaseAttack_PreDelay;
    async UniTaskVoid BaseAttack(CancellationTokenSource ct)
    {
        m_IsAttack = true;
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

        m_IsAttack = false;
    }

    protected override bool TryUseSkill(KeyCode key)
    {
        int idx = -1;
        if (key == KeyCode.Q)
        {
            idx = 0;
        }
        if (idx == -1)
        {
            return false;
        }
        if (m_SkillCoolTime[idx] > 0)
        {
            //��Ÿ��
            return false;
        }
        if (m_Status.m_CurMP < m_Skill[idx].useMP)
        {
            //���Ǻ���
            return false;
        }
        if (m_IsSkill)
        {
            //�ٸ� �ൿ��
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
        QSkill(m_SkillCTS).Forget();
        return true;
    }
   
    [SerializeField] float QSkill_PreDelay;
    void QSkillReturn2Vanya()
    {

    }
    async UniTaskVoid QSkill(CancellationTokenSource ct)
    {
        m_IsSkill = true;
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
        m_IsSkill = false;

    }
    public override void CancelAttack()
    {
        if (m_IsAttack)
        {
            m_AttackCTS.Cancel();
            m_IsAttack = false;
        }
    }
    public override void CancelSkill()
    {
        if (m_IsSkill)
        {
            m_SkillCTS.Cancel();
            m_IsSkill = false;
        }
    }
    public override float GetAttackRange()
    {
        return m_AttackRange;
    }


}
