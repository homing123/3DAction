using UnityEngine;
using System;

//### 레벨에 따른 증가
//체력
//마나
//방어력
//공격력
//공격속도
//---
//### 캐릭터별 고정값
//스킬증폭
//쿨다운감소
//치명타확률
//이동속도
//시야
//기본공격사거리
//모든피해흡혈
//생명력흡수
[System.Serializable]
public class Status : MonoBehaviour
{
    readonly static float[] LevelEXP = new float[19] { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 };
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
    }

    //통일 상수
    const float CriticalDMG = 75f;
    const float MaxSkillCooldown = 35f;

    [Header("기본 스테이터스")]

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
    public int m_SkillLevelUpPoint { get; private set; }
    public int[] m_SkillLevel = new int[7];

    State m_State;
    public float HPPercent { get { return m_CurHP / m_TotalMaxHP; } }

    public event Action OnDeath; // 죽음 이벤트
    public event Action OnStatusChanged;
    public event Action<State, State> OnStateChanged; //STATE_1 = xor 이전과 달라진 비트 => 1, STATE_2 = 현재STATE => m_State
    public event Action OnEXPLevelChanged;
    public event Action OnSkillLevelUpPointChanged;

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
                m_TotalMaxHP = m_CharacterData.HP[m_Level];
                m_TotalMaxMP = m_CharacterData.MP[m_Level];
                m_TotalArmor = m_CharacterData.Armor[m_Level];
                m_TotalDmg = m_CharacterData.Dmg[m_Level];
                m_TotalAttackSpeed = m_CharacterData.AttackSpeed[m_Level];

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
        m_SkillLevelUpPoint = 1;
        //캐릭터 스테이터스
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
    public void AddEXP(float exp)
    {
        int lastLevel = m_Level;
        m_EXP += exp;
        if(m_Level >= 19)
        {
            return;
        }
        while(m_EXP >= LevelEXP[m_Level])
        {
            m_EXP -= LevelEXP[m_Level];
            m_Level++;
            m_SkillLevelUpPoint++;
            if(m_Level>=19)
            {
                return;
            }
        }
        if (m_Level != lastLevel)
        {
            CalculateTotalStatus();
            OnStatusChanged?.Invoke();
        }
        OnEXPLevelChanged?.Invoke();
        OnSkillLevelUpPointChanged?.Invoke();
    }
    public void UseSkillLevelUpPoint(SkillPos skillPos)
    {
        m_SkillLevelUpPoint--;
        m_SkillLevel[(int)skillPos]++;
        OnSkillLevelUpPointChanged?.Invoke();
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

    // 데미지를 받는 메서드
    public void TakeDamage(in SkillAttackInfo skillAttackInfo)
    {
        if (m_Death) return;

        // 방어력에 의한 데미지 감소 계산
        float adDamage = skillAttackInfo.damage <= 0 ? 0 : CalculateDamageAfterDefense(skillAttackInfo.damage);
        float skillDamage = skillAttackInfo.skillDamage <= 0 ? 0 : CalculateDamageAfterDefense(skillAttackInfo.skillDamage);
        float trueDamage = skillAttackInfo.trueDamage;

        float totalDamage = adDamage + skillDamage + trueDamage;
        if(totalDamage <= 0)
        {
            return;
        }

        DamageTextCreater.Ins.CreateDamage(skillAttackInfo.user, m_Bio, skillAttackInfo.isCritical, adDamage, skillDamage, trueDamage);
        // 체력 감소
        //UI_Damage.Create(m_Bio, reducedDamage);
        m_CurHP -= totalDamage;
        m_CurHP = Mathf.Max(0, m_CurHP);

        // 이벤트 호출
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
    public void Heal(float healValue)
    {
        m_CurHP += healValue;
        if(m_CurHP > m_TotalMaxHP)
        {
            m_CurHP = m_TotalMaxHP;
        }
        OnStatusChanged?.Invoke();

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
    public float GetLevelUpEXP()
    {
        if(m_Level < 19)
        {
            return LevelEXP[m_Level];
        }
        else
        {
            return 0;
        }
    }
}
