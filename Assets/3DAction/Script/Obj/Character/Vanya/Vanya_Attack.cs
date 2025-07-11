using UnityEngine;
using UnityEngine.InputSystem;

public class Vanya_Attack : MonoBehaviour
{
    [SerializeField] float m_Speed;
    [SerializeField] AnimationCurve m_SpeedCurveAtLifeTime;

    //생성 후 손위치에 맞게 이동하는 선딜연출 도중에 타겟이 없어지는 경우 제자리에서 멈추고 n초정도 지난 후 사라진다.
    

    Vector3 m_Desti;
    Bio m_Target;
    Bio m_User;
    SkillAttackInfo m_SkillAttackInfo;
    float m_LifeTime;

    public void Setting(Bio user, Bio target, in SkillAttackInfo skillAttackInfo)
    {
        m_Target = target;
        m_SkillAttackInfo = skillAttackInfo;
        m_Desti = m_Target.GetHitPos();
        m_User = user;
    }
    private void FixedUpdate()
    {
        m_LifeTime += Time.fixedDeltaTime;
        MoveToTarget();
    }
    void MoveToTarget()
    {
        if(m_Target!= null)
        {
            m_Desti = m_Target.GetHitPos();
        }

        transform.position = Util.MoveDesti(transform.position, m_Desti, m_SpeedCurveAtLifeTime.Evaluate(Mathf.Clamp01(m_LifeTime)) * m_Speed * Time.fixedDeltaTime, out bool arrived);
        if (arrived)
        {
            if (m_Target != null)
            {
                m_Target.GetAttacked(m_User, m_SkillAttackInfo, default);
                Destroy(this.gameObject);
            }
        }
    }
}
