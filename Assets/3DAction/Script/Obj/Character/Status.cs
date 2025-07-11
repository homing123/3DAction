using UnityEngine;
using System;

[System.Serializable]
public class Status : MonoBehaviour
{
    //여러가지 상태가 정의될거임 하지만 행동은 움직임, 스킬시전 2개뿐임
    //현재 상태정의에 따른 움직임, 스킬시전 가능여부를 항상체크해야함

    //넉백 : 움직임 불가능, 스킬시전 불가능, 시전중인 스킬 유지
    //스턴 : 움직임 불가능, 스킬시전 불가능, 시전중인 스킬 취소
    //기본 스킬 : 움직임 불가능, 스킬시전 불가능
    //이동형 스킬 : 움직임 불가능, 스킬시전 불가능 (스킬로직에서 움직임 처리)

    public enum State
    {
        Knockback = 1 << 0,
        Stun = 1<<1,
        Silence = 1<<2,
        TargetingHide = 1<< 3,
        Skill = 1<<4,
        Sleep = 1 << 5,

    }

    //통일 상수
    const float KnockbackAcc = -2;
    const float KnockbackTime = 0.35f;


    [Header("기본 스테이터스")]
    [SerializeField] float MaxHP = 100f;
    [SerializeField] float MaxMP = 50f;
    [SerializeField] float AttackDamage = 10f;
    [SerializeField] float SkillDamage = 10f;
    [SerializeField] float Defense = 5f;
    [SerializeField] float MoveSpeed = 5f;
    [SerializeField] float AttackSpeed = 1f;
    // 프로퍼티들
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

    public event Action OnDeath; // 죽음 이벤트
    public event Action OnStatusChanged;
    public event Action<float> OnGetDamage; // 데미지 받은 이벤트
    public event Action<State, State> OnStateChanged; //STATE_1 = xor 이전과 달라진 비트 => 1, STATE_2 = 현재STATE => m_State

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
    // 방어력에 따른 데미지 계산
    float CalculateDamageAfterDefense(float originalDamage)
    {
        // 방어력이 높을수록 데미지 감소 (최대 감소율 제한)
        float damageReduction = m_Defense / (m_Defense + 100f);

        float reducedDamage = originalDamage * (1f - damageReduction);
        return Mathf.Max(1f, reducedDamage); // 최소 1의 데미지는 받도록
    }

    void Death()
    {
        m_Death = true;
        OnDeath?.Invoke();
    }

    // 데미지를 받는 메서드
    public void TakeDamage(in SkillAttackInfo skillAttackInfo)
    {
        if (m_Death) return;

        // 방어력에 의한 데미지 감소 계산
        float reducedDamage = CalculateDamageAfterDefense(skillAttackInfo.damage);

        // 체력 감소
        //UI_Damage.Create(m_Bio, reducedDamage);
        m_CurHP -= reducedDamage;
        m_CurHP = Mathf.Max(0, m_CurHP);

        // 이벤트 호출
        OnGetDamage?.Invoke(reducedDamage);
        OnStatusChanged?.Invoke();

        // 체력이 0 이하면 죽음 처리
        if (m_CurHP <= 0)
        {
            Death();
        }
        else
        {
            //안죽은 경우 상태 처리
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

    // 체력 회복 메서드
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
