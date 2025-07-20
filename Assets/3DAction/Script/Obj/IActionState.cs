using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public enum ActionState
{
    Idle,
    MoveToDesti,
    Stop,
    Hold,
    AttackDesti,
    ChaseTarget,
    Attack,
    Skill
}

public interface IActionState
{
    public void StateStart();
    public void StateUpdate();
    public void StateLast();
}


public class ActionStateIdle : IActionState
{
    Character m_Bio;
    public ActionStateIdle(Character bio)
    {
        m_Bio = bio;
    }
    public void StateStart()
    {
        
    }
    public void StateUpdate()
    {
        if(m_Bio.GetNearestTargetAttackRange(out Bio target))
        {
            m_Bio.ChangeActionState(ActionState.ChaseTarget, default, target);
        }
    }
    public void StateLast()
    {

    }
}
public class ActionStateMoveToDesti : IActionState
{
    Character m_Bio;
    Vector3 m_MoveDesti;
    public ActionStateMoveToDesti(Character bio)
    {
        m_Bio = bio;
    }
    public void SetMoveDesti(Vector3 groundPos)
    {
        m_MoveDesti = groundPos;
    }
    public void StateStart()
    {
        m_Bio.m_BioNavMove.MoveToDesti(m_MoveDesti, () => m_Bio.ChangeActionState(ActionState.Idle));
    }
    public void StateUpdate()
    {

    }
    public void StateLast()
    {
        m_Bio.m_BioNavMove.MoveStop();
    }
}
public class ActionStateStop: IActionState
{
    Bio m_Bio;
    public ActionStateStop(Bio bio)
    {
        m_Bio = bio;
    }
    public void StateStart()
    {

    }
    public void StateUpdate()
    {

    }
    public void StateLast()
    {

    }
}
public class ActionStateHold : IActionState
{
    Character m_Bio;
    Bio m_Target;
    public ActionStateHold(Character bio)
    {
        m_Bio = bio;
    }
    public void StateStart()
    {
        m_Target = null;
    }
    public void StateUpdate()
    {
        if(m_Target == null)
        {
            if(m_Bio.GetNearestTargetAttackRange(out Bio target))
            {
                m_Target = target;
            }
        }
        else
        {
            Vector2 targetPosVT2XZ = m_Target.transform.position.VT2XZ();
            Vector2 curPosVT2XZ = m_Bio.transform.position.VT2XZ();
            float dis = Vector2.Distance(targetPosVT2XZ, curPosVT2XZ);
            if(dis > m_Bio.GetAttackRange())
            {
                m_Target = null;
                if (m_Bio.GetNearestTargetAttackRange(out Bio target))
                {
                    m_Target = target;
                }
            }
        }

        if (m_Target != null)
        {
            m_Bio.ChangeActionState(ActionState.Attack, default, m_Target);
        }
    }
    public void StateLast()
    {

    }
}
public class ActionStateAttackDesti : IActionState
{
    [Tooltip("어택땅 찍었을 때 찍은곳 주변의 적을 찾는 범위 반지름")] const float AttackDestiSearchRange = 5;

    Character m_Bio;
    Vector3 m_MoveDesti;

    public ActionStateAttackDesti(Character bio)
    {
        m_Bio = bio;
    }
    public void SetMoveDesti(Vector3 groundPos)
    {
        m_MoveDesti = groundPos;
    }
    public void StateStart()
    {
        if(m_Bio.GetNearestTarget(m_MoveDesti, AttackDestiSearchRange, out Bio target))
        {
            m_Bio.ChangeActionState(ActionState.ChaseTarget, default, target);
        }
        else
        {
            m_Bio.m_BioNavMove.MoveToDesti(m_MoveDesti, () => m_Bio.ChangeActionState(ActionState.Idle));
        }
    }
    public void StateUpdate()
    {
        if (m_Bio.GetNearestTargetAttackRange(out Bio target))
        {
            m_Bio.ChangeActionState(ActionState.ChaseTarget, default, target);
        }
    }
    public void StateLast()
    {
        m_Bio.m_BioNavMove.MoveStop();
    }
}
public class ActionStateChaseTarget : IActionState
{
    Character m_Bio;
    Bio m_Target;
    public ActionStateChaseTarget(Character bio)
    {
        m_Bio = bio;
    }
    public void SetTarget(Bio target)
    {
        m_Target = target;
    }
    public void StateStart()
    {

    }
    public void StateUpdate()
    {
        if(m_Target == null)
        {
            m_Bio.ChangeActionState(ActionState.Idle);
            return;
        }

        float disToTarget = Vector2.Distance(m_Target.transform.position.VT2XZ(), m_Bio.transform.position.VT2XZ());
        if (disToTarget <= m_Bio.GetAttackRange())
        {
            m_Bio.m_BioNavMove.MoveStop();
            m_Bio.ChangeActionState(ActionState.Attack, default, m_Target);
        }
        else
        {
            m_Bio.m_BioNavMove.MoveToDesti(m_Target.transform.position);
        }

    }
    public void StateLast()
    {
        m_Bio.m_BioNavMove.MoveStop();
    }
}
public class ActionStateAttack : IActionState
{
    Bio m_Bio;
    public Bio m_Target { get; private set; }
    public ActionState m_LastActionState { get; private set; }
    public ActionStateAttack(Bio bio)
    {
        m_Bio = bio;
    }
    public void Set(Bio target, ActionState lastActionState)
    {
        m_Target = target;
        m_LastActionState = lastActionState;
    }
    public void StateStart()
    {
        m_Bio.Attack();
    }
    public void StateUpdate()
    {

    }
    public void StateLast()
    {
        m_Bio.CancelAttack();
    }
}

public class ActionStateSkill : IActionState
{
    Character m_Bio;
    SkillPos m_SkillPos;
    public ActionStateSkill(Character bio)
    {
        m_Bio = bio;
    }
    public void Set(SkillPos skillPos)
    {
        m_SkillPos = skillPos;
    }
    public void StateStart()
    {
        m_Bio.UseSkill(m_SkillPos);
    }
    public void StateUpdate()
    {

    }
    public void StateLast()
    {

    }
}