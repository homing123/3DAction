using UnityEngine;
using UnityEngine.AI;
using System;
public class BioNaviMove : MonoBehaviour
{
    NavMeshAgent m_NavAgent;
    Status m_Status;
    Bio m_Bio;

    Action OnArrived;
    public bool IsMove { get { return !m_NavAgent.isStopped; } }
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
        if (m_NavAgent.isStopped == false && m_NavAgent.pathStatus == NavMeshPathStatus.PathComplete && remainDis < 0.1f)
        {
            //µµÂø
            m_NavAgent.isStopped = true;
            OnArrived?.Invoke();
        }
    }
    public void MoveToDesti(Vector3 desti, Action acArrived = null)
    {
        m_NavAgent.isStopped = false;
        m_NavAgent.SetDestination(desti);
        OnArrived = acArrived;
    }
    public void MoveStop()
    {
        m_NavAgent.isStopped = true;
    }


}
