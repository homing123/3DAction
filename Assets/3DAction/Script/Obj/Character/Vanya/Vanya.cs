using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public class Vanya : Character
{
    [SerializeField] float m_AttackRange;
    [SerializeField] Vanya_Q m_VanyaQPrefab;
    [SerializeField] Transform m_VanyaQStartPos;
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
    private void Update()
    {
        for (int i = 0; i < 7; i++)
        {
            m_SkillCoolTime[i] = m_SkillCoolTime[i] > 0 ? m_SkillCoolTime[i] - Time.deltaTime : m_SkillCoolTime[i];
        }
    }

    void TryUseSkill(int idx)
    {
        if (m_SkillCoolTime[idx] > 0)
        {
            //ÄðÅ¸ÀÓ
            return;
        }
        if(m_Status.m_CurMP < m_Skill[idx].useMP)
        {
            //¿¥ÇÇºÎÁ·
            return;
        }

        UseSkill(idx);
    }
    void UseSkill(int idx)
    {        
        m_SkillCoolTime[idx] = m_Skill[idx].coolTime;
        m_SkillToken = new CancellationTokenSource();
        QSkill(m_SkillToken.Token).Forget();
    }

    [SerializeField] float QSkill_PreDelay;
    void QSkillReturn2Vanya()
    {

    }
    async UniTask QSkill(CancellationToken ct)
    {
        float preDelay = QSkill_PreDelay;
        float curTime = 0;
        Vector2 dir = PlayerInput.Ins.GetSkillDirVT2(transform.position);
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


    protected override void TryUseSkill(KeyCode key)
    {
        if (key == KeyCode.Q)
        {
            TryUseSkill(0);
        }
    }
    protected override float GetAttackRange()
    {
        return m_AttackRange;
    }

}
