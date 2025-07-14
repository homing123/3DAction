using System.Collections.Generic;
using System.Threading;
using System;
using UnityEngine;
using static UnityEditor.PlayerSettings;

//타겟서치는 무조건 적 캐릭터 우선임
//야생동물은 나와 전투상태일때만 자동 타겟에 포함됨
//전투상태 : 공격을 받거나 주거나 한번이라도 하면 돌아가서 초기화 되기 전까진 공격상태임
public enum CharacterAction
{
    Idle = 0, //가만히 서서 타겟을 찾는 상태, 찾으면 ChaseTarget으로 변경
    Move2Desti = 1, //움직임 타겟서치x
    Move2Object = 2, //오브젝트 타겟으로 움직이고 상호작용 거리안에 들어오면 바로 상효작용
    Stop = 3, //가만히 서서 타겟서치x
    Hold = 4, //가만히 서서 타겟을 찾되 쫓아가지는 않음
    AttackDesti = 5, //움직이며 타겟서치, 찾으면 ChaseTarget으로 변경
    ChaseTarget = 6, //타겟을 쫓아가며 공격 타겟이 죽거나 없어질경우 멈춤
}

//skillPos 마다 장착된 캐릭터스킬(스킬레벨, 스킬쿨타임, 현재스킬 상태, 레벨별 스킬데이터, 사용자)
//현재 skillPos에 장착된 캐릭터 스킬이 변할때 불리는 이벤트
//현재 skillPos에 해당하는 스킬 시전 액션
public class SkillPosSkill
{
    public Character m_User { get; private set; }
    public SkillData[] m_SkillDatas { get; private set; }
    public float m_SkillCooldown { get; private set; }
    public SkillState m_SkillState { get; private set; }
    public SkillPos m_SkillPos { get; private set; }

    public event Action OnSkillPosSkillChanged;
    public Action<CancellationTokenSource, SkillPosSkill> ac_SkillFunction { get; private set; }
    public SkillPosSkill(Character user, SkillPos skillPos)
    {
        m_User = user;
        m_SkillPos = skillPos;
    }
    public void SetSkillFunctionAndSkillDatas(SkillData[] skillDatas, Action<CancellationTokenSource, SkillPosSkill> ac_skillFunction)
    {
        m_SkillDatas = skillDatas;
        ac_SkillFunction = ac_skillFunction;
    }
    public void CooldownSet(float cooldownSetValue)
    {
        if (m_SkillCooldown != cooldownSetValue)
        {
            m_SkillCooldown = cooldownSetValue;
        }
    }
    public void CooldownSet(SkillData skillData)
    {
        float skillCool = m_User.m_Status.CalcSkillCooldown(skillData);
        m_SkillCooldown = skillCool;
    }
    public void CooldownReduction(float reductionTime)
    {
        float lastskillCooldown = m_SkillCooldown;
        m_SkillCooldown -= reductionTime;
        m_SkillCooldown = m_SkillCooldown < 0 ? 0 : m_SkillCooldown;
    }

    //일단 임시로 매프레임 lateupdate 에서 부르자
    public void CheckState()
    {
        int skillLevel = m_User.m_Status.m_SkillLevel[(int)m_SkillPos];
        if (skillLevel == 0)
        {
            m_SkillState = SkillState.Level0;
        }
        else if (m_User.m_Status.Skillable() == false || m_User.m_IsSkill)
        {
            m_SkillState = SkillState.CharacterStateUnUseableSkill;
        }
        else if (m_SkillCooldown > 0)
        {
            m_SkillState = SkillState.Cooldown;
        }
        else if (m_SkillDatas[skillLevel - 1].MPValue > m_User.m_Status.m_CurMP)
        {
            m_SkillState = SkillState.NotEnoughMP;
        }
        else
        {
            m_SkillState = SkillState.Useable;
        }
        OnSkillPosSkillChanged?.Invoke();
    }
    public SkillData GetSkillData()
    {
        int skillLevel = m_User.m_Status.m_SkillLevel[(int)m_SkillPos];
        if (skillLevel == 0)
        {
            return m_SkillDatas[0];
        }
        else
        {
            return m_SkillDatas[skillLevel - 1];
        }
    }
    public int GetMaxLevel()
    {
        return m_SkillDatas.Length;
    }
}
public abstract class Character : Bio
{
    [Tooltip("어택땅 찍었을 때 찍은곳 주변의 적을 찾는 범위 반지름")] const float AttackDestiSearchRange = 8;
    [SerializeField] int m_CharacterID;

    Vector3 m_LastPos;
    CharacterAction m_CharacterAction;
    Transform m_ObjectTarget;
    float m_CurAttackDelay;

    protected Bio m_AttackTarget;
    protected Dictionary<SkillPos, SkillPosSkill> D_SkillPosSkill;
    protected CancellationTokenSource m_SkillCTS;
    protected CancellationTokenSource m_AttackCTS;
    protected CharacterMove m_Move;

    public CharacterData m_CharacterData { get; private set; }
    public bool m_IsSkill { get; protected set; }
    public bool m_IsAttack { get; protected set; }
    public int m_TeamID { get; private set; }
    public Vector3 m_LastMoveDis { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        PlayerM.Ins.RegisterCharacter(this);
        m_BioType = Bio_Type.Character;
        m_Move = GetComponent<CharacterMove>();
        m_CharacterAction = CharacterAction.Stop;
        SkillPos[] enumArray = EnumArray<SkillPos>.GetArray();
        D_SkillPosSkill = new Dictionary<SkillPos, SkillPosSkill>(enumArray.Length);
        for(int i=0;i<enumArray.Length;i++)
        {
            D_SkillPosSkill.Add(enumArray[i], new SkillPosSkill(this, enumArray[i]));
        }
    }
    protected override void Start()
    {
        base.Start();
        m_LastPos = transform.position;
        PlayerInput.Ins.OnInput += OnInput;
        m_CharacterData = CharacterData.GetData(m_CharacterID);
        m_Status.CharacterStatusInit(m_CharacterData);
    }
    protected override void Update()
    {
        base.Update();
        m_CurAttackDelay += Time.deltaTime;
        SkillCooldownUpdate();
        CharacterActionUpdate();
    }
    protected virtual void LateUpdate()
    {
        SkillCheckState();
    }
    private void OnDestroy()
    {
        PlayerInput.Ins.OnInput -= OnInput;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        m_LastMoveDis = transform.position - m_LastPos;
        m_LastPos = transform.position;
    }
    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = new Color(0, 0.5f, 0, 0.5f);
            Gizmos.DrawSphere(transform.position, GetAttackRange());
        }
    }

    void OnInput(InputInfo info)
    {
        switch(info.command)
        {
            case InputCommandType.Move2GroundPos:
                SetCharacterAction(CharacterAction.Move2Desti, info.groundPos);
                break;
            case InputCommandType.Move2ObjectTarget:
                SetCharacterAction(CharacterAction.Move2Object, default, null, info.objectTarget);
                break;
            case InputCommandType.Stop:
                SetCharacterAction(CharacterAction.Stop);
                break;
            case InputCommandType.Hold:
                SetCharacterAction(CharacterAction.Hold);
                break;
            case InputCommandType.AttackDesti:
                SetCharacterAction(CharacterAction.AttackDesti, info.groundPos);
                break;
            case InputCommandType.ChaseTarget2Monster:
                SetCharacterAction(CharacterAction.ChaseTarget, default, info.monsterTarget);
                break;
            case InputCommandType.ChaseTarget2Character:
                SetCharacterAction(CharacterAction.ChaseTarget, default, info.characterTarget);
                break;
            case InputCommandType.UseSkill:
                TryUseSkill(info.skillKey);
                break;
        }
    }
    bool TryGetNearestTargetAttackRange(out Bio target)
    {
        return TryGetNearestTarget(transform.position, GetAttackRange(), out target);
    }
    bool TryGetNearestTarget(Vector3 pos, float radius, out Bio target)
    {
        target = null;
        float closestSqrDist = float.MaxValue;

        foreach (var hit in Physics.OverlapSphere(pos, radius, 1 << (int)Layer.Character))
        {
            Bio candidate = null;

            if (hit.TryGetComponent<Character>(out var cha) && cha.m_TeamID != m_TeamID)
            {
                candidate = cha;
            }
            else if (hit.TryGetComponent<Sandbag>(out var sandbag))
            {
                candidate = sandbag;
            }

            if (candidate != null)
            {
                float sqrDist = (candidate.transform.position - pos).VT2XZ().sqrMagnitude;
                if (sqrDist < closestSqrDist)
                {
                    closestSqrDist = sqrDist;
                    target = candidate;
                }
            }
        }

        return target != null;
    }
    void CharacterActionUpdate()
    {
        Bio target = null;
        switch(m_CharacterAction)
        {
            case CharacterAction.Idle:
                if (TryGetNearestTargetAttackRange(out target))
                {
                    SetCharacterAction(CharacterAction.ChaseTarget, default, target);
                }        
                break;
            case CharacterAction.Move2Desti:
                if (m_Move.m_IsMove == false)
                {
                    if (TryGetNearestTargetAttackRange(out target))
                    {
                        SetCharacterAction(CharacterAction.ChaseTarget, default, target);
                    }
                    else
                    {
                        SetCharacterAction(CharacterAction.Idle);
                    }
                }             
                break;
            case CharacterAction.Move2Object:
                break;
            case CharacterAction.Stop:
                break;
            case CharacterAction.Hold:
                if(m_AttackTarget == null)
                {
                    //사거리 안의 가장 가까운 적 탐색
                    if (TryGetNearestTargetAttackRange(out target))
                    {
                        m_AttackTarget = target;
                        CheckAttackDelay();
                    }
                }
                else
                {
                    float dis2Target = (m_AttackTarget.transform.position - transform.position).VT2XZ().magnitude;
                    if (dis2Target <= GetAttackRange())
                    {
                        CheckAttackDelay();
                    }
                    else
                    {
                        m_AttackTarget = null;
                        if (TryGetNearestTargetAttackRange(out target))
                        {
                            m_AttackTarget = target;
                            CheckAttackDelay();
                        }
                    }
                }
                break;
            case CharacterAction.AttackDesti:
                if (TryGetNearestTargetAttackRange(out target))
                {
                    SetCharacterAction(CharacterAction.ChaseTarget, default, m_AttackTarget);
                }
                break;
            case CharacterAction.ChaseTarget:
                if(m_AttackTarget == null)
                {
                    SetCharacterAction(CharacterAction.Idle);
                }
                else
                {
                    float dis2Target = (m_AttackTarget.transform.position - transform.position).VT2XZ().magnitude;
                    if (dis2Target > GetAttackRange())
                    {
                        m_Move.Move(m_AttackTarget.transform.position);
                    }
                    else
                    {
                        m_Move.MoveStop();
                        CheckAttackDelay();
                    }
                }
                break;
        }
    }

    void SetCharacterAction(CharacterAction CharacterAction, Vector3 groundPos = default, Bio target = null, Transform obj = null)
    {
        m_CharacterAction = CharacterAction;
        m_AttackTarget = null;

        switch (m_CharacterAction)
        {
            case CharacterAction.Move2Desti:
                CancelAttack();
                m_Move.Move(groundPos);
                break;
            case CharacterAction.Move2Object:
                break;
            case CharacterAction.Stop:
                m_Move.MoveStop();
                CancelAttack();
                break;
            case CharacterAction.Hold:
                m_Move.MoveStop();
                break;
            case CharacterAction.AttackDesti:
                Bio nearestTarget = null;
                if(TryGetNearestTarget(groundPos, AttackDestiSearchRange, out nearestTarget))
                {
                    SetCharacterAction(CharacterAction.ChaseTarget, default, nearestTarget);
                }
                else
                {
                    if (TryGetNearestTargetAttackRange(out nearestTarget))
                    {
                        SetCharacterAction(CharacterAction.ChaseTarget, default, nearestTarget);
                    }
                    else
                    {
                        m_Move.Move(groundPos);
                    }
                }
                break;
            case CharacterAction.ChaseTarget:
                if(m_AttackTarget != null && m_AttackTarget != target)
                {
                    CancelAttack();
                }
                m_AttackTarget = target;
                break;
        }
    }

    void CheckAttackDelay()
    {
        float attackDelay = 1 / m_Status.m_TotalAttackSpeed;
        if (m_CurAttackDelay >= attackDelay)
        {
            if (TryAttack())
            {
                m_CurAttackDelay = 0;
            }
        }
    }
    public void SetTeamID(int teamid)
    {
        m_TeamID = teamid;
    }
    protected virtual void SkillCooldownUpdate()
    {
        foreach (SkillPos skillPos in D_SkillPosSkill.Keys)
        {
            D_SkillPosSkill[skillPos].CooldownReduction(Time.deltaTime);
        }
    }
    protected virtual void SkillCheckState()
    {
        foreach (SkillPos skillPos in D_SkillPosSkill.Keys)
        {
            D_SkillPosSkill[skillPos].CheckState();
        }
    }
    public virtual void SkillLevelUp(SkillPos skillPos)
    {
        if (m_Status.m_SkillLevelUpPoint > 0)
        {
            m_Status.UseSkillLevelUpPoint(skillPos);
        }
    }

    public SkillPosSkill GetSkillPosSkill(SkillPos skillPos)
    {
        return D_SkillPosSkill[skillPos];
    }
   
    
    protected abstract bool TryUseSkill(KeyCode key);
    protected abstract bool TryAttack();
    public abstract float GetAttackRange();
    public abstract void CancelAttack();
    public abstract void CancelSkill();
}