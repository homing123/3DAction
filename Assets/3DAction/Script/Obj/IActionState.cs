using UnityEngine;

public interface IActionState
{
    public void StateChange();
    public void StateUpdate();
}


public class ActionStateIdle : IActionState
{
    Bio m_Bio;
    Bio m_Target;
    public ActionStateIdle(Bio bio)
    {
        m_Bio = bio;
    }
    public void StateChange()
    {
        
    }
    public void StateUpdate()
    {

    }
}
public class ActionStateMoveToDesti : IActionState
{
    Bio m_Bio;
    Bio m_Target;
    Vector3 m_MoveDesti;
    public ActionStateMoveToDesti(Bio bio)
    {
        m_Bio = bio;
    }
    public void SetMoveDesti(Vector3 groundPos)
    {
        m_MoveDesti = groundPos;
    }
    public void StateChange()
    {

    }
    public void StateUpdate()
    {

    }
}
public class ActionStateStop: IActionState
{
    Bio m_Bio;
    Bio m_Target;
    public ActionStateStop(Bio bio)
    {
        m_Bio = bio;
    }
    public void StateChange()
    {

    }
    public void StateUpdate()
    {

    }
}
public class ActionStateHold : IActionState
{
    Bio m_Bio;
    Bio m_Target;
    public ActionStateHold(Bio bio)
    {
        m_Bio = bio;
    }
    public void StateChange()
    {

    }
    public void StateUpdate()
    {

    }
}
public class ActionStateAttackDesti : IActionState
{
    Bio m_Bio;
    Bio m_Target;
    Vector3 m_MoveDesti;

    public ActionStateAttackDesti(Bio bio)
    {
        m_Bio = bio;
    }
    public void SetMoveDesti(Vector3 groundPos)
    {
        m_MoveDesti = groundPos;
    }
    public void StateChange()
    {

    }
    public void StateUpdate()
    {

    }
}
public class ActionStateChaseTarget : IActionState
{
    Bio m_Bio;
    Bio m_Target;
    public ActionStateChaseTarget(Bio bio)
    {
        m_Bio = bio;
    }
    public void SetTarget(Bio target)
    {
        m_Target = target;
    }
    public void StateChange()
    {

    }
    public void StateUpdate()
    {

    }
}
public class ActionStateSkill : IActionState
{
    Bio m_Bio;
    Bio m_Target;
    public ActionStateSkill(Bio bio)
    {
        m_Bio = bio;
    }
    public void StateChange()
    {

    }
    public void StateUpdate()
    {

    }
}