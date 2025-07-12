using UnityEngine;
using System;

//### ������ ���� ����
//ü��
//����
//����
//���ݷ�
//���ݼӵ�
//---
//### ĳ���ͺ� ������
//��ų����
//��ٿ��
//ġ��ŸȮ��
//�̵��ӵ�
//�þ�
//�⺻���ݻ�Ÿ�
//�����������
//��������
[System.Serializable]
public class Status : MonoBehaviour
{
    //�������� ���°� ���ǵɰ��� ������ �ൿ�� ������, ��ų���� 2������
    //���� �������ǿ� ���� ������, ��ų���� ���ɿ��θ� �׻�üũ�ؾ���

    //�˹� : ������ �Ұ���, ��ų���� �Ұ���, �������� ��ų ����
    //���� : ������ �Ұ���, ��ų���� �Ұ���, �������� ��ų ���
    //�⺻ ��ų : ������ �Ұ���, ��ų���� �Ұ���
    //�̵��� ��ų : ������ �Ұ���, ��ų���� �Ұ��� (��ų�������� ������ ó��)

    public enum State
    {
        Knockback = 1 << 0,
        Stun = 1<<1,
    }

    //���� ���
    const float CriticalDMG = 75f;
    const float MaxSkillCooldown = 35f;

    [Header("�⺻ �������ͽ�")]

    //Status
    bool m_Death;
    public float m_TotalMaxHP { get; private set; }
    public float m_TotalMaxMP { get; private set; }
    public float m_TotalArmor { get; private set; }
    public float m_TotalDmg { get; private set; }
    public float m_TotalAttackSpeed { get; private set; }
    public float m_TotalSkillDmg { get; private set; }
    public float m_TotalSkillCooldown { get; private set; }
    public float m_TotalCriticalPer { get; private set; }
    public float m_TotalMoveSpeed { get; private set; }
    public float m_TotalSight { get; private set; }
    public float m_TotalAttackRange { get; private set; }
    public float m_TotalAllLifeSteal { get; private set; }
    public float m_TotalAttackLifeSteal { get; private set; }

    public float m_CurHP { get; private set; }
    public float m_CurMP { get; private set; }

    public float m_EXP { get; private set; }
    public int m_Level { get; private set; }

    State m_State;
    public float HPPercent { get { return m_CurHP / m_TotalMaxHP; } }

    public event Action OnDeath; // ���� �̺�Ʈ
    public event Action OnStatusChanged;
    public event Action<float> OnGetDamage; // ������ ���� �̺�Ʈ
    public event Action<State, State> OnStateChanged; //STATE_1 = xor ������ �޶��� ��Ʈ => 1, STATE_2 = ����STATE => m_State

    CharacterData m_CharacterData;

    Bio m_Bio;
    float m_CurKnockbackTime;
    float m_CurStunTime;


    private void Awake()
    {
        m_Bio = GetComponent<Bio>();
        m_Death = false;
    }

    private void Update()
    {
        if (m_Death)
        {
            return;
        }
        if ((m_State & State.Knockback) > 0)
        {
            m_CurKnockbackTime -= Time.deltaTime;
            if (m_CurKnockbackTime < 0)
            {
                SetState(State.Knockback, false);
            }
        }
        if ((m_State & State.Stun) > 0)
        {
            m_CurStunTime -= Time.deltaTime;
            if (m_CurStunTime < 0)
            {
                SetState(State.Stun, false);
            }
        }
    }

    void CalculateTotalStatus()
    {
        switch (m_Bio.m_BioType)
        {
            case Bio_Type.Character:
                m_TotalMaxHP = m_CharacterData.HP[0];
                m_TotalMaxMP = m_CharacterData.MP[0];
                m_TotalArmor = m_CharacterData.Armor[0];
                m_TotalDmg = m_CharacterData.Dmg[0];
                m_TotalAttackSpeed = m_CharacterData.AttackSpeed[0];

                m_TotalSkillDmg = m_CharacterData.SkillDmg;
                m_TotalSkillCooldown = m_CharacterData.SkillCooldown;
                m_TotalCriticalPer = m_CharacterData.CriticalPer;
                m_TotalMoveSpeed = m_CharacterData.MoveSpeed;
                m_TotalSight = m_CharacterData.Sight;
                m_TotalAttackRange = m_CharacterData.AttackRange;
                m_TotalAllLifeSteal = m_CharacterData.AllLifeSteal;
                m_TotalAttackLifeSteal = m_CharacterData.AttackLifeSteal;
                break;
        }
    }
    public void CharacterStatusInit(CharacterData characterData)
    {
        m_Level = 0;
        //ĳ���� �������ͽ�
        m_CharacterData = characterData;
        CalculateTotalStatus();
        m_CurHP = m_TotalMaxHP;
        m_CurMP = m_TotalMaxMP;
        OnStatusChanged?.Invoke();
    }
    public void SandbagStatusInit()
    {
        m_TotalMaxHP = 1000;
        m_CurHP = m_TotalMaxHP;
        OnStatusChanged?.Invoke();
    }
    public void LevelUp()
    {

    }

   

    
    
    void SetState(State state, bool on)
    {
        State lastState = m_State;
        m_State = on ? m_State | state : m_State & (~state);
        OnStateChanged?.Invoke(lastState ^ m_State, m_State);
    }

    float CalculateDamageAfterDefense(float originalDamage)
    {
        return originalDamage * (100 / (m_TotalArmor + 100));
    }

    void Death()
    {
        m_Death = true;
        OnDeath?.Invoke();
    }

    // �������� �޴� �޼���
    public void TakeDamage(in SkillAttackInfo skillAttackInfo)
    {
        if (m_Death) return;

        // ���¿� ���� ������ ���� ���
        float reducedDamage = CalculateDamageAfterDefense(skillAttackInfo.damage);

        // ü�� ����
        //UI_Damage.Create(m_Bio, reducedDamage);
        m_CurHP -= reducedDamage;
        m_CurHP = Mathf.Max(0, m_CurHP);

        // �̺�Ʈ ȣ��
        OnGetDamage?.Invoke(reducedDamage);
        OnStatusChanged?.Invoke();

        // ü���� 0 ���ϸ� ���� ó��
        if (m_CurHP <= 0)
        {
            Death();
        }
        else
        {
            //������ ��� ���� ó��
            //if (skillinfo.knockbackDis > 0)
            //{
            //    GetKnockback(in skillinfo);
            //}
            //if (skillinfo.stunTime > 0)
            //{
            //    GetStun(in skillinfo);
            //}
        }
    }
    public bool Movable()
    {
        return m_Death == false && m_State == 0;
    }
    public bool Skillable()
    {
        return m_Death == false && m_State == 0;
    }


    public float CalcSkillCooldown(SkillData skillData)
    {
        float skillCooldown = MathF.Min(m_TotalSkillCooldown, MaxSkillCooldown);
        return skillData.Cooldown - (skillData.Cooldown * skillCooldown / 100);
    }
    public float CalcAttackDamage(out bool isCritical)
    {
        float random = 100;
        if (m_TotalCriticalPer > 0)
        {
            random = UnityEngine.Random.Range(0f, 100f);
        }
        isCritical = random < m_TotalCriticalPer;
        return m_TotalDmg + (isCritical == true ? m_TotalDmg * CriticalDMG / 100 : 0);
    }
    public float CalcSkillDamage(SkillData skillData, int damageIdx = 0)
    {
        return m_TotalSkillDmg * skillData.SDMulDamages[damageIdx] + skillData.Damages[damageIdx];
    }
}
