using System.Collections.Generic;
using System.Threading;
using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

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
        int skillLevel = m_User.m_Status.SkillLevel[(int)m_SkillPos];
        if (skillLevel == 0)
        {
            m_SkillState = SkillState.Level0;
        }
        else if (m_User.CheckStateSkillUseable() == false)
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
        int skillLevel = m_User.m_Status.SkillLevel[(int)m_SkillPos];
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

    ActionStateIdle m_ActionStateIdle;
    ActionStateMoveToDesti m_ActionStateMoveToDesti;
    ActionStateStop m_ActionStateStop;
    ActionStateHold m_ActionStateHold;
    ActionStateAttackDesti m_ActionStateAttackDesti;
    ActionStateChaseTarget m_ActionStateChaseTarget;
    ActionStateSkill m_ActionStateSkill;
    protected Dictionary<SkillPos, SkillPosSkill> D_SkillPosSkill;

    public CharacterData m_CharacterData { get; private set; }
    public int m_TeamID { get; private set; }



    protected override void Awake()
    {
        base.Awake();
        PlayerM.Ins.RegisterCharacter(this);
        m_ActionStateIdle = new ActionStateIdle(this);
        m_ActionStateMoveToDesti = new ActionStateMoveToDesti(this);
        m_ActionStateStop = new ActionStateStop(this);
        m_ActionStateHold = new ActionStateHold(this);
        m_ActionStateAttackDesti = new ActionStateAttackDesti(this);
        m_ActionStateChaseTarget = new ActionStateChaseTarget(this);
        m_ActionStateSkill = new ActionStateSkill(this);
        m_CurActionState = m_ActionStateIdle;
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
        PlayerInput.Ins.OnInput += OnInput;
        m_CharacterData = CharacterData.GetData(m_CharacterID);
        m_Status.CharacterStatusInit(m_CharacterData);
    }
    protected override void Update()
    {
        base.Update();
        SkillCooldownUpdate();
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
    }
    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = new Color(0, 0.5f, 0, 0.5f);
            Gizmos.DrawSphere(transform.position, GetAttackRange());
        }
    }

    bool CharacterActionable()
    {
        if (m_ActionState != ActionState.Idle && m_ActionState != ActionState.Move)
        {
            return false;
        }
        return true;
    }
    void OnInput(InputInfo info)
    {
        if (CharacterActionable() == false)
        {
            return;
        }
        switch (info.command)
        {
            case InputCommandType.Move2GroundPos:
                m_CurActionState = m_ActionStateMoveToDesti;
                m_ActionStateMoveToDesti.SetMoveDesti(info.groundPos);
                break;
            case InputCommandType.Move2ObjectTarget:
                //SetCharacterAction(CharacterAction.Move2Object, default, null, info.objectTarget);
                break;
            case InputCommandType.Stop:
                m_CurActionState = m_ActionStateStop;
                break;
            case InputCommandType.Hold:
                m_CurActionState = m_ActionStateHold;
                break;
            case InputCommandType.AttackDesti:
                m_CurActionState = m_ActionStateAttackDesti;
                m_ActionStateAttackDesti.SetMoveDesti(info.groundPos);
                break;
            case InputCommandType.ChaseTarget2Monster:
                m_CurActionState = m_ActionStateChaseTarget;
                m_ActionStateChaseTarget.SetTarget(info.monsterTarget);
                break;
            case InputCommandType.ChaseTarget2Character:
                m_CurActionState = m_ActionStateChaseTarget;
                m_ActionStateChaseTarget.SetTarget(info.characterTarget);
                break;
            case InputCommandType.UseSkill:
                m_CurActionState = m_ActionStateSkill;
                //TryUseSkill(info.skillKey);
                break;
        }
        m_CurActionState.StateChange();
    }
    bool GetNearestTargetAttackRange(out Bio target)
    {
        return GetNearestTarget(transform.position, GetAttackRange(), out target);
    }
    bool GetNearestTarget(Vector3 pos, float radius, out Bio target)
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
    //void CharacterActionUpdate()
    //{
    //    if (CharacterActionable() == false)
    //    {
    //        SetCharacterAction(CharacterAction.Idle);
    //        return;
    //    }
    //    Bio target = null;
    //    switch(m_CharacterAction)
    //    {
    //        case CharacterAction.Idle:
    //            if (TryGetNearestTargetAttackRange(out target))
    //            {
    //                SetCharacterAction(CharacterAction.ChaseTarget, default, target);
    //            }        
    //            break;
    //        case CharacterAction.Move2Desti:
    //            if(m_ActionState == ActionState.Idle)
    //            {
    //                SetCharacterAction(CharacterAction.Idle);
    //            }
    //            break;
    //        case CharacterAction.Move2Object:
    //            break;
    //        case CharacterAction.Stop:
    //            break;
    //        case CharacterAction.Hold:
    //            bool targetInRange = false;
    //            if(m_AttackTarget == null)
    //            {
    //                //사거리 안의 가장 가까운 적 탐색
    //                if (TryGetNearestTargetAttackRange(out target))
    //                {
    //                    m_AttackTarget = target;
    //                    targetInRange = true;
    //                }
    //            }
    //            else
    //            {
    //                float dis2Target = (m_AttackTarget.transform.position - transform.position).VT2XZ().magnitude;
    //                if (dis2Target <= GetAttackRange())
    //                {
    //                    targetInRange = true;
    //                }
    //                else
    //                {
    //                    m_AttackTarget = null;
    //                    if (TryGetNearestTargetAttackRange(out target))
    //                    {
    //                        m_AttackTarget = target;
    //                        targetInRange = true;
    //                    }
    //                }
    //            }
    //            if (targetInRange && CheckAttackable())
    //            {
    //                Attack();
    //            }
    //            break;
    //        case CharacterAction.AttackDesti:
    //            if (TryGetNearestTargetAttackRange(out target))
    //            {
    //                SetCharacterAction(CharacterAction.ChaseTarget, default, m_AttackTarget);
    //            }
    //            break;
    //        case CharacterAction.ChaseTarget:
    //            if(m_AttackTarget == null)
    //            {
    //                SetCharacterAction(CharacterAction.Idle);
    //            }
    //            else
    //            {
    //                float dis2Target = (m_AttackTarget.transform.position - transform.position).VT2XZ().magnitude;
    //                if (dis2Target > GetAttackRange())
    //                {
    //                    ChangeActionState(ActionState.Move, m_AttackTarget.transform.position);
    //                }
    //                else
    //                {
    //                    ChangeActionState(ActionState.Idle);
    //                    if (CheckAttackable())
    //                    {
    //                        Attack();
    //                    }
    //                }
    //            }
    //            break;
    //    }
    //}

    //void SetCharacterAction(CharacterAction CharacterAction, Vector3 groundPos = default, Bio target = null, Transform obj = null)
    //{
    //    m_CharacterAction = CharacterAction;
    //    m_AttackTarget = null;

    //    switch (m_CharacterAction)
    //    {
    //        case CharacterAction.Move2Desti:
    //            ChangeActionState(ActionState.Move, groundPos);
    //            break;
    //        case CharacterAction.Move2Object:
    //            break;
    //        case CharacterAction.Stop:
    //            ChangeActionState(ActionState.Idle);
    //            break;
    //        case CharacterAction.Hold:
    //            ChangeActionState(ActionState.Idle);
    //            break;
    //        case CharacterAction.AttackDesti:
    //            Bio nearestTarget = null;
    //            if(TryGetNearestTarget(groundPos, AttackDestiSearchRange, out nearestTarget))
    //            {
    //                SetCharacterAction(CharacterAction.ChaseTarget, default, nearestTarget);
    //            }
    //            else
    //            {
    //                if (TryGetNearestTargetAttackRange(out nearestTarget))
    //                {
    //                    SetCharacterAction(CharacterAction.ChaseTarget, default, nearestTarget);
    //                }
    //                else
    //                {
    //                    ChangeActionState(ActionState.Move, groundPos);
    //                }
    //            }
    //            break;
    //        case CharacterAction.ChaseTarget:
    //            if(m_AttackTarget != null && m_AttackTarget != target)
    //            {
    //                CancelAttack();
    //            }
    //            m_AttackTarget = target;
    //            break;
    //    }
    //}
    bool TryUseSkill(KeyCode key)
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
            CancelAttack();
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
    void SkillEnd()
    {
        m_SkillCTS.Dispose();
        m_SkillCTS = null;
    }
    void SkillCancel()
    {
        if(m_SkillCTS != null)
        {
            m_SkillCTS.Cancel();
            m_SkillCTS.Dispose();
            m_SkillCTS = null;
        }
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
   
    
    public abstract float GetAttackRange();
    protected abstract UniTaskVoid QSkill(CancellationTokenSource cts, SkillPosSkill skillPosSkill);
    protected abstract UniTaskVoid WSkill(CancellationTokenSource cts, SkillPosSkill skillPosSkill);
    protected abstract UniTaskVoid ESkill(CancellationTokenSource cts, SkillPosSkill skillPosSkill);
    protected abstract UniTaskVoid RSkill(CancellationTokenSource cts, SkillPosSkill skillPosSkill);
}