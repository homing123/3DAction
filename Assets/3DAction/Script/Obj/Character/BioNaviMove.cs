using UnityEngine;
using UnityEngine.AI;
using System;
public class BioNaviMove : MonoBehaviour
{
    NavMeshAgent m_NavAgent;
    Status m_Status;
    Bio m_Bio;

    [SerializeField] Transform m_MoveDestiObj;
    private void Awake()
    {
        m_NavAgent = GetComponent<NavMeshAgent>();
        m_Status = GetComponent<Status>();
        m_Bio = GetComponent<Bio>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_NavAgent.updateRotation = false;
        m_NavAgent.speed = m_Status.m_TotalMoveSpeed;
        m_Status.OnStatusChanged += OnStatusChanged;
        m_Bio.OnActionStateChanged += OnActionStateChanged;
        m_Bio.OnMoveDestiChanged += OnMoveDestiChanged;
    }
    void OnMoveDestiChanged(Vector3 desti)
    {
        if (m_Bio.m_ActionState == ActionState.Move)
        {
            MoveToDesti(m_Bio.m_MoveDesti);
        }
    }
    void OnActionStateChanged(ActionState state)
    {
        if(m_Bio.m_ActionState == ActionState.Move)
        {
            MoveToDesti(m_Bio.m_MoveDesti);
        }
        else
        {
            MoveStop();
        }
    }
    void OnStatusChanged()
    {
        m_NavAgent.speed = m_Status.m_TotalMoveSpeed;
    }
    // Update is called once per frame
    void Update()
    {
        if (m_NavAgent.isStopped == false)
        {
            CheckMoveArrived();
        }
    }
    void CheckMoveArrived()
    {
        float remainDis = Vector3.Distance(m_NavAgent.destination, transform.position);
        if (m_NavAgent.pathStatus == NavMeshPathStatus.PathComplete && remainDis < 0.1f)
        {
            //µµÂø
            m_NavAgent.isStopped = true;
            m_Bio.ChangeActionState(ActionState.Idle);
        }
    }
    void MoveToDesti(Vector3 desti)
    {
        if (m_MoveDestiObj != null)
        {
            m_MoveDestiObj.transform.position = desti;
        }
        m_NavAgent.isStopped = false;
        m_NavAgent.SetDestination(desti);
    }
    void MoveStop()
    {
        m_NavAgent.isStopped = true;
    }


}
