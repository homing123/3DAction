using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
}

[RequireComponent (typeof(Status))]
public class Bio : MonoBehaviour
{

    //공통 애니메이션 이름

    public Status m_Status { get; private set; }
    public Bio_Type m_BioType { get; protected set; }


    protected virtual void Awake()
    {
        m_Status = GetComponent<Status>();
    }
    protected virtual void Start()
    {
        m_Status.OnDeath += Death;
        m_Status.OnStateChanged += StateChanged;
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
    public void GetAttacked(Bio user, in DamageInfo dmgInfo, Vector3 knockbackValue)
    {
        Vector3 knockbackDir = default;
        switch (dmgInfo.knockbackType)
        {
            case Knockback_Type.User2Target:
                knockbackDir = transform.position - user.transform.position;
                break;
            case Knockback_Type.Direction:
                knockbackDir = knockbackValue;
                break;
            case Knockback_Type.Position:
                knockbackDir = (knockbackValue - transform.position).normalized;
                break;
        }

        knockbackDir.y = 0;
        SkillInfo skillHitInfo = new SkillInfo(this, in dmgInfo, knockbackDir.normalized);
        m_Status.TakeDamage(skillHitInfo);
    }

    protected void SkillEnd()
    {
        m_Status.EndSkillCasting();
    }

   
    

    public static bool CheckisBio(Collider col, out Bio bio)
    {
        return col.TryGetComponent(out bio);
    }

   


   

}
