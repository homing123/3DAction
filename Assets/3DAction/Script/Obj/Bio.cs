using System;
using UnityEngine;
using System.Collections.ObjectModel;
using Cysharp.Threading.Tasks;
using System.Threading;

public enum Skill_Target_Type
{
    Monster = 1 << 0,
    Character = 1 << 1,
    Enemy = 1 << 2,
    Team = 1 << 3,
}
public enum Bio_Type
{
    Animal,
    Character,
    Sandbag,
}
public enum CCState
{
    Knockback = 0,
    Stun = 1,
}

[RequireComponent (typeof(Status))]
public abstract class Bio : MonoBehaviour
{
    const float MoveDirAngularSpeed = 1440;

    //공통 애니메이션 이름

    [SerializeField] Bio_Type m_BioType;
    protected float m_CurAttackDelay;
    Vector2 m_LastPos;
    float[] m_CCTime;

    protected bool m_isInit;
    protected CancellationTokenSource m_SkillCTS;
    protected CancellationTokenSource m_AttackCTS;
    protected IActionState m_CurActionState;
    protected ActionState m_CurActionStateType;
    protected ActionStateAttack m_ActionStateAttack;


    public Vector3 m_MoveDesti { get; private set; }
    public Vector2 m_CurMoveDir { get; private set; }
    public Status m_Status { get; private set; }
    public uint m_CCState { get; private set; }
    public Bio_Type BioType { get { return m_BioType; } }
    public ReadOnlyCollection<float> CCTime => Array.AsReadOnly(m_CCTime);


    protected Action OnInitialized;
    public event Action OnActionStateChanged;
    public event Action<Vector3> OnMoveDestiChanged;
    [Tooltip("추가된 CC, 제거된 CC")] public event Action<uint, uint> OnCCStateChanged;

    protected virtual void Awake()
    {
        m_Status = GetComponent<Status>();
        m_CCTime = new float[EnumArray<CCState>.GetArray().Length];

    }
    protected virtual void Start()
    {
        m_LastPos = transform.position.VT2XZ();
        m_Status.OnDeath += Death;
        UI_HPBar.Create(this);
    }
    protected virtual void Update()
    {
        CCTimeUpdate();
        m_CurAttackDelay += Time.deltaTime;
        CalcCurMoveDirAndRotateToDir();
        m_CurActionState.StateUpdate();
    }
    protected virtual void FixedUpdate()
    {

    }

    void Death()
    {
        Destroy(gameObject, 2f);
    }

    void CalcCurMoveDirAndRotateToDir()
    {
        m_CurMoveDir = (transform.position.VT2XZ() - m_LastPos);
        if (m_CurMoveDir != Vector2.zero)
        {
            m_CurMoveDir.Normalize();
            UpdateAngular(m_CurMoveDir, out bool arrived);

        }
        m_LastPos = transform.position.VT2XZ();
    }
    #region CC
    void CCTimeUpdate()
    {
        for (int i = 0; i < m_CCTime.Length; i++)
        {
            if (m_CCTime[i] > 0)
            {
                m_CCTime[i] -= Time.deltaTime;
                if (m_CCTime[i] <= 0)
                {
                    m_CCTime[i] = 0;
                    //cc해제
                    ReleaseCCState((CCState)i);
                }
            }

        }
    }

    public void AddCCState(CCState cc, float time)
    {
        uint ccIdx = (uint)cc;
        if ((m_CCState & ccIdx) > 0)
        {
            m_CCTime[ccIdx] = m_CCTime[ccIdx] < time ? time : m_CCTime[ccIdx];
        }
        else
        {
            m_CCState |= ccIdx;
            m_CCTime[ccIdx] = time;
            OnCCStateChanged?.Invoke(ccIdx, 0);
        }
    }
    public void ReleaseCCState(CCState cc)
    {
        uint ccIdx = (uint)cc;
        if ((m_CCState & ccIdx) > 0)
        {
            m_CCState &= ~ccIdx;
            m_CCTime[ccIdx] = 0;
            OnCCStateChanged?.Invoke(0, ccIdx);
        }
    }
    #endregion
    protected bool CheckMoveable()
    {
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
    protected bool CheckAttackable()
    {
        CCState[] UnableCC = { CCState.Knockback, CCState.Stun };
        for (int i = 0; i < UnableCC.Length; i++)
        {
            if ((m_CCState & (uint)UnableCC[i]) > 0)
            {
                return false;
            }
        }

        float attackDelay = 1 / m_Status.m_TotalAttackSpeed;
        if (m_CurAttackDelay < attackDelay)
        {
            return false;
        }
        return true;

    }
    public virtual void ChangeActionState(ActionState actionState, Vector3 groundPos = default, Bio target = null, SkillPos skillPos = 0)
    {

    }
   

    public void AttackOverlap(in HitRangeInfo hitRangeInfo, in SkillAttackInfo dmginfo)
    {
        Collider[] hits = HitRange.Overlap(in hitRangeInfo, out int count);
        for (int i = 0; i < count; i++)
        {
            Bio hitBio = hits[i].GetComponent<Bio>();
            if (Bio.IsHit(this, hitBio, Skill_Target_Type.Enemy | Skill_Target_Type.Monster))
            {
                hitBio.GetAttacked(this, in dmginfo, default);
            }
        }
    }
    public void GetAttacked(Bio user, in SkillAttackInfo skillAttackInfo, Vector3 knockbackValue)
    {
        Vector3 knockbackDir = default;
        //switch (dmgInfo.knockbackType)
        //{
        //    case Knockback_Type.User2Target:
        //        knockbackDir = transform.position - user.transform.position;
        //        break;
        //    case Knockback_Type.Direction:
        //        knockbackDir = knockbackValue;
        //        break;
        //    case Knockback_Type.Position:
        //        knockbackDir = (knockbackValue - transform.position).normalized;
        //        break;
        //}

        knockbackDir.y = 0;
        //SkillAttackInfo skillAttackInfo = new SkillAttackInfo(user, skillData);
        m_Status.TakeDamage(in skillAttackInfo);
    }

    #region Rotate
    void UpdateAngular(Vector2 dir, out bool arrived)
    {
        Vector3 moveDirHor = new Vector3(dir.x, 0, dir.y);
        if (moveDirHor == Vector3.zero)
        {
            arrived = false;
            return;
        }
        moveDirHor.Normalize();
        Quaternion curRot = transform.rotation;
        Quaternion lookRot = Quaternion.LookRotation(moveDirHor, Vector3.up);
        float maxDegree = MoveDirAngularSpeed * Time.deltaTime;
        if (Quaternion.Angle(curRot, lookRot) < maxDegree)
        {
            transform.rotation = lookRot;
            arrived = true;
        }
        else
        {
            transform.rotation = Quaternion.RotateTowards(curRot, lookRot, maxDegree);
            arrived = false;
        }
    }
    protected void RotToDirImmediate(Vector2 dir)
    {
        Vector3 moveDirHor = new Vector3(dir.x, 0, dir.y);
        if (moveDirHor == Vector3.zero)
        {
            return;
        }
        moveDirHor.Normalize();
        Quaternion curRot = transform.rotation;
        Quaternion lookRot = Quaternion.LookRotation(moveDirHor, Vector3.up);
        transform.rotation = lookRot;
    }
    protected async UniTask<float> RotToDir(Vector2 dir, CancellationTokenSource cts)
    {
        float curRotTime = 0;
        while (true)
        {
            if (cts.Token.IsCancellationRequested)
            {
                return curRotTime;
            }
            await UniTask.Yield();
            curRotTime += Time.deltaTime;
            UpdateAngular(dir, out bool arrived);
            if(arrived)
            {
                break;
            }
        }
        return curRotTime;
    }

    #endregion
    #region Attack
    public void Attack()
    {
        if(m_AttackCTS!= null)
        {
            m_AttackCTS.Dispose();
        }
        m_AttackCTS = new CancellationTokenSource();
        BaseAttack(m_AttackCTS).Forget();
    }
    protected void AttackEnd()
    {
        if(m_ActionStateAttack.m_LastActionState == ActionState.Hold)
        {
            ChangeActionState(ActionState.Hold);
        }
        else if(m_ActionStateAttack.m_LastActionState == ActionState.ChaseTarget)
        {
            ChangeActionState(ActionState.ChaseTarget, default, m_ActionStateAttack.m_Target);
        }
    }
    public void CancelAttack()
    {
        if (m_CurActionStateType == ActionState.Attack)
        {
            m_AttackCTS.Cancel();
        }
    }
    #endregion
   
    public virtual Vector3 GetHitPos()
    {
        return transform.position + Vector3.up * 1.2f;
    }
    public virtual Vector3 GetUIPivotPos()
    {
        return transform.position + Vector3.up * 1.8f;
    }
   
    public void RegisterInitialized(Action action)
    {
        if (m_isInit)
        {
            action();
        }
        else
        {
            OnInitialized += action;
        }
    }
    public static bool IsHit(Bio user, Bio target, Skill_Target_Type type)
    {
        if(target.m_BioType == Bio_Type.Sandbag)
        {
            return true;
        }
        if((type & Skill_Target_Type.Monster) > 0 && target.m_BioType == Bio_Type.Animal)
        {
            return true;
        }
        if((type & Skill_Target_Type.Character) > 0 && target.m_BioType == Bio_Type.Character)
        {
            return true;
        }
        if (user.m_BioType == Bio_Type.Character && target.m_BioType == Bio_Type.Character)
        {
            Character userCha = user as Character;
            Character targetCha = target as Character;
            if ((type & Skill_Target_Type.Enemy) > 0 && userCha.m_TeamID != targetCha.m_TeamID)
            {
                return true;
            }
            if ((type & Skill_Target_Type.Team) > 0 && userCha.m_TeamID == targetCha.m_TeamID)
            {
                return true;
            }
        }
        return false;
    }
    public static bool CheckisBio(Collider col, out Bio bio)
    {
        return col.TryGetComponent(out bio);
    }


    protected abstract UniTaskVoid BaseAttack(CancellationTokenSource cts);
    public abstract float GetAttackRange();

}
