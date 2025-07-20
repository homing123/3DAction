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
    public SkillPosSkill(Character user, SkillPos skillPos)
    {
        m_User = user;
        m_SkillPos = skillPos;
    }
    public void SetSkillFunctionAndSkillDatas(SkillData[] skillDatas)
    {
        m_SkillDatas = skillDatas;
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
    [SerializeField] int m_CharacterID;

    ActionStateIdle m_ActionStateIdle;
    ActionStateMoveToDesti m_ActionStateMoveToDesti;
    ActionStateStop m_ActionStateStop;
    ActionStateHold m_ActionStateHold;
    ActionStateAttackDesti m_ActionStateAttackDesti;
    ActionStateChaseTarget m_ActionStateChaseTarget;
    protected ActionStateSkill m_ActionStateSkill { get; private set; }
    protected Dictionary<SkillPos, SkillPosSkill> D_SkillPosSkill;

    public event Action OnSkillPosSkillChanged;

    public BioNaviMove m_BioNavMove { get; private set; }
    public CharacterData m_CharacterData { get; private set; }
    public int m_TeamID { get; private set; }



    protected override void Awake()
    {
        base.Awake();
        m_BioNavMove = GetComponent<BioNaviMove>();
        PlayerM.Ins.RegisterCharacter(this);
        m_ActionStateIdle = new ActionStateIdle(this);
        m_ActionStateMoveToDesti = new ActionStateMoveToDesti(this);
        m_ActionStateStop = new ActionStateStop(this);
        m_ActionStateHold = new ActionStateHold(this);
        m_ActionStateAttackDesti = new ActionStateAttackDesti(this);
        m_ActionStateChaseTarget = new ActionStateChaseTarget(this);
        m_ActionStateAttack = new ActionStateAttack(this);
        m_ActionStateSkill = new ActionStateSkill(this);
        ChangeActionState(ActionState.Idle);
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

    void OnInput(InputInfo info)
    {
        switch (info.command)
        {
            case InputCommandType.Move2GroundPos:
                ChangeActionState(ActionState.MoveToDesti, info.groundPos);
                break;
            case InputCommandType.Move2ObjectTarget:
                //SetCharacterAction(CharacterAction.Move2Object, default, null, info.objectTarget);
                break;
            case InputCommandType.Stop:
                ChangeActionState(ActionState.Stop);
                break;
            case InputCommandType.Hold:
                ChangeActionState(ActionState.Hold);
                break;
            case InputCommandType.AttackDesti:
                ChangeActionState(ActionState.AttackDesti, info.groundPos);
                break;
            case InputCommandType.ChaseTarget2Monster:
                ChangeActionState(ActionState.ChaseTarget, default, info.monsterTarget);
                break;
            case InputCommandType.ChaseTarget2Character:
                ChangeActionState(ActionState.ChaseTarget, default, info.characterTarget);
                break;
            case InputCommandType.ChaseTarget2Sandbag:
                ChangeActionState(ActionState.ChaseTarget, default, info.sandbagTarget);
                break;
            case InputCommandType.UseSkill:
                ChangeActionState(ActionState.Skill, default, null, info.skillKey);
                break;
        }
    }

    public override void ChangeActionState(ActionState actionState, Vector3 groundPos = default, Bio target = null, SkillPos skillPos = 0)
    {
        switch(actionState)
        {
            case ActionState.Attack:
                if (CheckAttackable() == false)
                {
                    return;
                }
                break;
            case ActionState.Skill:
                if (D_SkillPosSkill[skillPos].m_SkillState != SkillState.Useable)
                {
                    UI_SkillFailText.SetSkillState(D_SkillPosSkill[skillPos].m_SkillState);
                    return;
                }
                break;
        }

        if (m_CurActionState != null)
        {
            m_CurActionState.StateLast();
        }

        switch (actionState)
        {
            case ActionState.Idle:
                m_CurActionState = m_ActionStateIdle;
                break;
            case ActionState.MoveToDesti:
                m_CurActionState = m_ActionStateMoveToDesti;
                m_ActionStateMoveToDesti.SetMoveDesti(groundPos);
                break;
            case ActionState.Stop:
                m_CurActionState = m_ActionStateStop;
                break;
            case ActionState.Hold:
                m_CurActionState = m_ActionStateHold;
                break;
            case ActionState.AttackDesti:
                m_CurActionState = m_ActionStateAttackDesti;
                m_ActionStateAttackDesti.SetMoveDesti(groundPos);
                break;
            case ActionState.ChaseTarget:
                m_CurActionState = m_ActionStateChaseTarget;
                m_ActionStateChaseTarget.SetTarget(target);
                break;
            case ActionState.Attack:
                m_CurActionState = m_ActionStateAttack;
                m_ActionStateAttack.Set(target, m_CurActionStateType);
                break;
            case ActionState.Skill:
                m_CurActionState = m_ActionStateSkill;
                m_ActionStateSkill.Set(skillPos);
                break;
        }
        Debug.Log($"ActionState Change {m_CurActionStateType} => {actionState}");
        m_CurActionStateType = actionState;
        m_CurActionState.StateStart();
    }
    #region SearchTarget
    public bool GetNearestTargetAttackRange(out Bio target)
    {
        return GetNearestTarget(transform.position, GetAttackRange(), out target);
    }
    public bool GetNearestTarget(Vector3 pos, float radius, out Bio target)
    {
        target = null;
        float closestSqrDist = float.MaxValue;

        foreach (var hit in Physics.OverlapSphere(pos, radius, (1 << (int)Layer.Character) | (1 << (int)Layer.Sandbag)))
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
    #endregion
    #region Skill
    public bool CheckStateSkillUseable()
    {
        if(m_CurActionStateType == ActionState.Skill)
        {
            return false;
        }
        CCState[] UnableCC = { CCState.Knockback, CCState.Stun };
        for (int i = 0; i < UnableCC.Length; i++)
        {
            if ((m_CCState & (uint)UnableCC[i]) > 0)
            {
                return false;
            }
        }
        return true;
    }
    public void UseSkill(SkillPos skillPos)
    {
        CancelAttack();
        if (m_SkillCTS != null)
        {
            m_SkillCTS.Dispose();
        }
        m_SkillCTS = new CancellationTokenSource();
        switch (skillPos)
        {
            case SkillPos.Q:
                QSkill(m_SkillCTS, D_SkillPosSkill[skillPos]).Forget();
                break;
            case SkillPos.W:
                WSkill(m_SkillCTS, D_SkillPosSkill[skillPos]).Forget();
                break;
            case SkillPos.E:
                ESkill(m_SkillCTS, D_SkillPosSkill[skillPos]).Forget();
                break;
            case SkillPos.R:
                RSkill(m_SkillCTS, D_SkillPosSkill[skillPos]).Forget();
                break;
            case SkillPos.Weapon:
                //Skill(m_SkillCTS, D_SkillPosSkill[skillPos]).Forget();
                break;
            case SkillPos.Spell:
                //QSkill(m_SkillCTS, D_SkillPosSkill[skillPos]).Forget();
                break;
        }
    }
    protected void SkillEnd()
    {
        ChangeActionState(ActionState.Idle);
    }
    protected void SkillCancel()
    {
        if(m_CurActionStateType == ActionState.Skill)
        {
            m_SkillCTS.Cancel();
            SkillEnd();
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

    protected abstract UniTaskVoid QSkill(CancellationTokenSource cts, SkillPosSkill skillPosSkill);
    protected abstract UniTaskVoid WSkill(CancellationTokenSource cts, SkillPosSkill skillPosSkill);
    protected abstract UniTaskVoid ESkill(CancellationTokenSource cts, SkillPosSkill skillPosSkill);
    protected abstract UniTaskVoid RSkill(CancellationTokenSource cts, SkillPosSkill skillPosSkill);
    #endregion
}