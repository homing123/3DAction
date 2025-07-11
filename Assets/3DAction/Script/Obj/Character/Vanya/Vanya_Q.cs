using UnityEngine;
using System.Collections.Generic;
using System;
public class Vanya_Q : MonoBehaviour
{
    Character m_User;
    Vector2 m_DirVT2;
    Vector2 m_DestiVT2;
    float m_OriginHeight;
    bool m_ArrivedFirstDesti;
    
    [SerializeField] float m_FirstMoveDis;
    [SerializeField] float m_SecondMoveDis;
    [SerializeField] float m_Speed;
    [SerializeField] float m_HeightMoveSpeed;
    [SerializeField] float m_SecondDirAddMoveDis;
    [SerializeField] AnimationCurve m_SpeedCurve;
    
    List<Bio> m_HitTarget = new List<Bio>();

    SkillAttackInfo m_SkillAttackInfo;
    Action OnReturn2Vanya;

    public void Setting(Character user, Vector2 useDirVT2, float originHeight, in SkillAttackInfo skillAttackInfo, Action ac_return2Vanya)
    {
        m_User = user;
        m_DirVT2 = useDirVT2;
        m_DestiVT2 = transform.position.VT2XZ() + m_DirVT2 * m_FirstMoveDis;
        m_OriginHeight = originHeight;
        m_SkillAttackInfo = skillAttackInfo;
        m_ArrivedFirstDesti = false;
        transform.LookAt((transform.position.VT2XZ() + m_DirVT2).VT2XZToVT3(transform.position.y));
        OnReturn2Vanya = ac_return2Vanya;
    }
    private void FixedUpdate()
    {
        Move();
        SetHeight();
    }
    void SetSecondDesti()
    {
        Vector2 dir = default;
        if(m_User.transform.position.VT2XZ() == transform.position.VT2XZ())
        {
            Return2Vanya();
            return;
        }

        if(m_User.m_LastMoveDis == Vector3.zero)
        {
            dir = m_User.transform.position.VT2XZ() - transform.position.VT2XZ();
        }
        else
        {
            Vector2 lastmoveVt2 = m_User.m_LastMoveDis.VT2XZ().normalized;
            Vector2 userMovePos = m_User.transform.position.VT2XZ() + lastmoveVt2 * m_SecondDirAddMoveDis;
            dir = userMovePos - transform.position.VT2XZ();
        }

        m_DirVT2 = dir.normalized;
        m_DestiVT2 = transform.position.VT2XZ() + m_DirVT2 * m_SecondMoveDis;

        transform.LookAt(m_DestiVT2.VT2XZToVT3(transform.position.y));
    }
    void Move()
    {
        Vector2 curPosVT2 = transform.position.VT2XZ();
        Vector2 Cur2Desti = (m_DestiVT2 - curPosVT2);
        float remainDis = Cur2Desti.magnitude;
        float remainDisPer = m_ArrivedFirstDesti == false ? remainDis / m_FirstMoveDis : remainDis / m_SecondMoveDis;
        float moveDis = m_Speed * Time.fixedDeltaTime * m_SpeedCurve.Evaluate(Mathf.Clamp01(1 - Mathf.Clamp01(remainDisPer)));
        if(moveDis < remainDis)
        {
            Vector2 moveVt2 = moveDis * m_DirVT2;
            transform.position += moveVt2.VT2XZToVT3();
        }
        else
        {
            transform.position  += Cur2Desti.VT2XZToVT3();
            if(m_ArrivedFirstDesti == false)
            {
                //첫번째 도착
                m_ArrivedFirstDesti = true;
                //두번째 도착지 세팅해야함
                SetSecondDesti();
            }
            else
            {
                Destroy(this.gameObject);
            }
        }
    }
    void Return2Vanya()
    {
        OnReturn2Vanya();
        Destroy(this.gameObject);
    }
    void SetHeight()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 5, 1<<(int)Layer.Ground))
        {
            float hitHeight = transform.position.y - hit.point.y;
            float heightMoveDis = m_HeightMoveSpeed * Time.fixedDeltaTime;
            if (Mathf.Abs(m_OriginHeight - hitHeight) < heightMoveDis)
            {
                transform.position = new Vector3(transform.position.x, hit.point.y + m_OriginHeight, transform.position.z);
            }
            else
            {
                transform.position += new Vector3(0, Mathf.Sign(m_OriginHeight - hitHeight) * heightMoveDis, 0);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 5, 1 << (int)Layer.Ground))
        {
            Gizmos.DrawSphere(hit.point, 0.5f);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        Character character;
        if(other.TryGetComponent(out character))
        {
            if(character == m_User)
            {
                if(m_ArrivedFirstDesti)
                {
                    Return2Vanya();
                }
                return;
            }

            if(m_User.m_TeamID != character.m_TeamID)
            {
                if(m_HitTarget.Contains(character) == false)
                {
                    m_HitTarget.Add(character);
                    character.GetAttacked(m_User,in m_SkillAttackInfo, default);
                }
            }
            return;
        }

        Monster monster;
        if (other.TryGetComponent(out monster))
        {
            if (m_HitTarget.Contains(monster) == false)
            {
                m_HitTarget.Add(monster);
                monster.GetAttacked(m_User, in m_SkillAttackInfo, default);
            }
            return;
        }

        Sandbag sandbag;
        if(other.TryGetComponent(out sandbag))
        {
            if (m_HitTarget.Contains(sandbag) == false)
            {
                m_HitTarget.Add(sandbag);
                sandbag.GetAttacked(m_User, in m_SkillAttackInfo, default);
            }
            return;
        }

    }
}
