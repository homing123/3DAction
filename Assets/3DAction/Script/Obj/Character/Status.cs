using UnityEngine;
using System;

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
        Silence = 1<<2,
        TargetingHide = 1<< 3,
        Skill = 1<<4,
        Sleep = 1 << 5,

    }

    //���� ���
    const float KnockbackAcc = -2;
    const float KnockbackTime = 0.35f;


    [Header("�⺻ �������ͽ�")]
    [SerializeField] float MaxHP = 100f;
    [SerializeField] float MaxMP = 50f;
    [SerializeField] float AttackDamage = 10f;
    [SerializeField] float SkillDamage = 10f;
    [SerializeField] float Defense = 5f;
    [SerializeField] float MoveSpeed = 5f;
    [SerializeField] float AttackSpeed = 1f;
    // ������Ƽ��
    bool m_Death;
    public float m_MaxHP { get; private set; }
    public float m_MaxMP { get; private set; }
    public float m_CurHP { get; private set; }
    public float m_CurMP { get; private set; }
    public float m_AttackDamage { get; private set; }
    public float m_SkillDamage { get; private set; }
    public float m_Defense { get; private set; }
    public float m_MoveSpeed { get; private set; }
    public float m_AttackSpeed { get; private set; }
    State m_State;
    public float HPPercent { get { return m_CurHP / m_MaxHP; } }

    public event Action OnDeath; // ���� �̺�Ʈ
    public event Action OnStatusChanged;
    public event Action<float> OnGetDamage; // ������ ���� �̺�Ʈ
    public event Action<State, State> OnStateChanged; //STATE_1 = xor ������ �޶��� ��Ʈ => 1, STATE_2 = ����STATE => m_State

    Bio m_Bio;
    float m_CurKnockbackTime;
    float m_CurStunTime;


    private void Awake()
    {
        m_Bio = GetComponent<Bio>();
        m_Death = false;
        StatusInit();
    }

    private void Update()
    {
        if (m_Death)
        {
            return;
        }
        if((m_State & State.Knockback) > 0)
        {
            m_CurKnockbackTime -= Time.deltaTime;
            if(m_CurKnockbackTime < 0)
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

    [ContextMenu("Status Updaate")]
    void StatusInit()
    {
        m_MaxHP = MaxHP;
        m_MaxMP = MaxMP;
        m_CurHP = m_MaxHP;
        m_CurMP = m_MaxMP;
        m_AttackDamage = AttackDamage;
        m_SkillDamage = SkillDamage;
        m_Defense = Defense;
        m_MoveSpeed = MoveSpeed;
        m_AttackSpeed = AttackSpeed;
    }
    void SetState(State state, bool on)
    {
        State lastState = m_State;
        m_State = on ? m_State | state : m_State & (~state);
        OnStateChanged?.Invoke(lastState ^ m_State, m_State);
    }
    //void GetKnockback(in SkillInfo skillInfo)
    //{
    //    SetState(State.Knockback, true);
    //    m_CurKnockbackTime = KnockbackTime;
    //    //m_Bio.SetMoveAnimDir(skillInfo.knockBackDir, skillInfo.knockbackDis, KnockbackTime, KnockbackAcc);
    //}
    //void GetStun(in SkillInfo skillInfo)
    //{
    //    SetState(State.Stun, true);
    //    m_CurStunTime = skillInfo.stunTime;
    //}
    // ���¿� ���� ������ ���
    float CalculateDamageAfterDefense(float originalDamage)
    {
        // ������ �������� ������ ���� (�ִ� ������ ����)
        float damageReduction = m_Defense / (m_Defense + 100f);

        float reducedDamage = originalDamage * (1f - damageReduction);
        return Mathf.Max(1f, reducedDamage); // �ּ� 1�� �������� �޵���
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

    // ü�� ȸ�� �޼���
    public void Heal(float healAmount)
    {
        if (m_Death) return;

        m_CurHP += healAmount;
        m_CurHP = Mathf.Min(m_MaxHP, m_CurHP);

        OnStatusChanged?.Invoke();
    }

   

   
    public void StartSkillCasting()
    {
        SetState(State.Skill, true);
    }
    public void EndSkillCasting()
    {
        SetState(State.Skill, false);
    }

    public bool Movable()
    {
        return m_Death == false && m_State == 0;
    }
    public bool Skillable()
    {
        return m_Death == false && m_State == 0;
    }


}
