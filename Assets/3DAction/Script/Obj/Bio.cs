using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;
using Unity.VisualScripting.Antlr3.Runtime;
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

[RequireComponent (typeof(Status))]
public class Bio : MonoBehaviour
{
    const float MoveDirAngularSpeed = 1440;

    //공통 애니메이션 이름

    public Status m_Status { get; private set; }
    public Bio_Type m_BioType { get; protected set; }

    protected bool m_isInit;
    protected Action OnInitialized;
    protected virtual void Awake()
    {
        m_Status = GetComponent<Status>();
    }
    protected virtual void Start()
    {
        m_Status.OnDeath += Death;
        m_Status.OnStateChanged += StateChanged;
        UI_HPBar.Create(this);
    }
    protected virtual void Update()
    {

    }
    protected virtual void FixedUpdate()
    {

    }
   
    void Death()
    {
        Debug.Log("주금");
        Destroy(gameObject, 2f);
    }
    void StateChanged(Status.State xorState, Status.State curState)
    {
        if (((xorState & Status.State.Knockback) > 0) && ((curState & Status.State.Knockback) > 0))
        {
            SkillCancel();
            //SetAnim(Anim_Knockback);
        }
    }
    void SkillCancel()
    {

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

    protected async UniTask RotToDir(Vector2 dir, CancellationTokenSource cts)
    {
        while (true)
        {
            if (cts.Token.IsCancellationRequested)
            {
                return;
            }
            await UniTask.Yield();
            UpdateAngular(dir, out bool arrived);
            if(arrived)
            {
                break;
            }
        }
    }

   
    

    public static bool CheckisBio(Collider col, out Bio bio)
    {
        return col.TryGetComponent(out bio);
    }

    public virtual Vector3 GetHitPos()
    {
        return transform.position + Vector3.up * 1.2f;
    }
    public virtual Vector3 GetUIPivotPos()
    {
        return transform.position + Vector3.up * 1.8f;
    }
    public void UpdateAngular(Vector2 dir, out bool arrived)
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
        if(Quaternion.Angle(curRot, lookRot) < maxDegree)
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

}
