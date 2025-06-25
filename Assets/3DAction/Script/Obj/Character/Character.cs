using Unity.VisualScripting;
using UnityEngine;

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
    protected CharacterMove m_Move;

    Vector3 m_LastPos;
    CharacterAction m_CharacterAction;

    public int m_TeamID { get; private set; }
    public Vector3 m_LastMoveDis { get; private set; }

    Character m_CharacterTarget;
    Monster m_MonsterTarget;
    Transform m_ObjectTarget;
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
    private void OnDestroy()
    {
        PlayerInput.Ins.OnInput -= OnInput;
    }
    private void FixedUpdate()
    {
        m_LastMoveDis = transform.position - m_LastPos;
        m_LastPos = transform.position;
    }


    void OnInput(InputInfo info)
    {
        switch(info.command)
        {
            case InputCommandType.Move2GroundPos:
                SetCharacterAction(CharacterAction.Move2Desti, info.groundPos);
                break;
            case InputCommandType.Move2ObjectTarget:
                SetCharacterAction(CharacterAction.Move2Object, default, null, null, info.objectTarget);
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
                SetCharacterAction(CharacterAction.ChaseTarget, default, null, info.monsterTarget);
                break;
            case InputCommandType.ChaseTarget2Character:
                SetCharacterAction(CharacterAction.ChaseTarget, default, info.characterTarget);
                break;
            case InputCommandType.UseSkill:
                TryUseSkill(info.skillKey);
                break;
        }
    }
    public void SetTeamID(int teamid)
    {
        m_TeamID = teamid;
    }
    bool SearchTarget()
    {
        float attackRange = GetAttackRange();
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, 1 << (int)Layer.Character);
        if(hits.Length > 0)
        {
            int enemyIdx = -1;
            float enemySqrDisMin = 0;
            for (int i=0;i<hits.Length;i++)
            {
                Character cha = hits[i].GetComponent<Character>();
                if(cha.m_TeamID != m_TeamID)
                {
                    float sqrDis = (cha.transform.position - transform.position).VT2XZ().sqrMagnitude;
                    if (enemyIdx == -1 || (enemySqrDisMin > sqrDis))
                    {
                        enemyIdx = i;
                        enemySqrDisMin = sqrDis;
                    }
                }
            }

            if(enemyIdx != -1)
            {
                m_CharacterTarget = hits[enemyIdx].GetComponent<Character>();
                return true;
            }
        }
        return false;
    }
    void CharacterActionUpdate()
    {
        switch(m_CharacterAction)
        {
            case CharacterAction.Idle:
                if (SearchTarget())
                {
                    SetCharacterAction(CharacterAction.ChaseTarget, default, m_CharacterTarget);
                }
                break;
            case CharacterAction.Move2Desti:
                if(m_Move.m_IsMove == false)
                {
                    if(SearchTarget())
                    {
                        SetCharacterAction(CharacterAction.ChaseTarget, default, m_CharacterTarget);
                    }
                }
                break;
            case CharacterAction.Move2Object:
                break;
            case CharacterAction.Stop:
                break;
            case CharacterAction.Hold:
                //가만히 서서 범위에 있는 적을 가까운 순서로 때림
                if(SearchTarget())
                {

                }
                break;
            case CharacterAction.AttackDesti:
                if (SearchTarget())
                {
                    SetCharacterAction(CharacterAction.ChaseTarget, default, m_CharacterTarget);
                }
                break;
            case CharacterAction.ChaseTarget:
                if(m_CharacterTarget != null)
                {
                    float dis2Target = (m_CharacterTarget.transform.position - transform.position).VT2XZ().magnitude;
                    if(dis2Target > GetAttackRange())
                    {
                        m_Move.Move(m_CharacterTarget.transform.position);
                    }
                }
                //타겟을 쫓아가며 때림
                break;
        }
    }

    void SetCharacterAction(CharacterAction CharacterAction, Vector3 groundPos = default, Character character = null, Monster monster = null, Transform obj = null)
    {
        m_CharacterAction = CharacterAction;

        switch (m_CharacterAction)
        {
            case CharacterAction.Idle:
                break;
            case CharacterAction.Move2Desti:
                m_Move.Move(groundPos);
                break;
            case CharacterAction.Move2Object:
                break;
            case CharacterAction.Stop:
                m_Move.MoveStop();
                break;
            case CharacterAction.Hold:
                m_Move.MoveStop();
                break;
            case CharacterAction.AttackDesti:
                m_Move.Move(groundPos);
                break;
            case CharacterAction.ChaseTarget:
                m_CharacterTarget = character;
                break;
        }
    }

    protected abstract float GetAttackRange();
    protected abstract void TryUseSkill(KeyCode key);
}