using Unity.VisualScripting;
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
public abstract class Character : Bio
{
    const float AttackDestiSearchRange = 8;
    protected CharacterMove m_Move;

    Vector3 m_LastPos;
    CharacterAction m_CharacterAction;
    Transform m_ObjectTarget;


    protected Bio m_AttackTarget;

    public int m_TeamID { get; private set; }
    public Vector3 m_LastMoveDis { get; private set; }
    float m_CurAttackDelay;

    protected override void Awake()
    {
        base.Awake();
        m_BioType = Bio_Type.Character;
        m_Move = GetComponent<CharacterMove>();
        m_CharacterAction = CharacterAction.Stop;
    }
    protected override void Start()
    {
        base.Start();
        m_LastPos = transform.position;
        PlayerInput.Ins.OnInput += OnInput;
    }
    protected override void Update()
    {
        base.Update();
        m_CurAttackDelay += Time.deltaTime;
        CharacterActionUpdate();
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
        Gizmos.color = new Color(0, 0.5f, 0, 0.5f);
        Gizmos.DrawSphere(transform.position, GetAttackRange());
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
                        TryAttack();
                    }
                }
                else
                {
                    float dis2Target = (m_AttackTarget.transform.position - transform.position).VT2XZ().magnitude;
                    if (dis2Target <= GetAttackRange())
                    {
                        TryAttack();
                    }
                    else
                    {
                        m_AttackTarget = null;
                        if (TryGetNearestTargetAttackRange(out target))
                        {
                            m_AttackTarget = target;
                            TryAttack();
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
                        TryAttack();
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
                m_Move.Move(groundPos);
                break;
            case CharacterAction.Move2Object:
                break;
            case CharacterAction.Stop:
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
                m_AttackTarget = target;
                break;
        }
    }

    void TryAttack()
    {
        float attackDelay = 1 / m_Status.m_AttackSpeed;
        if(m_CurAttackDelay >= attackDelay)
        {
            Attack();
            m_CurAttackDelay = 0;
        }
    }
    public void SetTeamID(int teamid)
    {
        m_TeamID = teamid;
    }

    protected abstract void TryUseSkill(KeyCode key);
    protected abstract void Attack();
    public abstract float GetAttackRange();
}