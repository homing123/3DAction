using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.AI;

public class CharacterMove : MonoBehaviour
{
    const float MoveDirAngularSpeed = 1440;
    NavMeshAgent m_NavAgent;
    HumanAnim m_HumanAnim;
    bool m_CallOnArrived = true;
    Status m_Status;
    public bool m_IsMove { get { return !m_NavAgent.isStopped; } }

    [SerializeField] Transform m_MoveDestiObj;
    private void Awake()
    {
        m_NavAgent = GetComponent<NavMeshAgent>();
        m_HumanAnim = GetComponent<HumanAnim>();
        m_Status = GetComponent<Status>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_NavAgent.updateRotation = false;
        m_NavAgent.speed = m_Status.m_MoveSpeed;
        m_Status.OnStatusChanged += OnStatusChanged;
    }
    private void OnDestroy()
    {
        m_Status.OnStatusChanged -= OnStatusChanged;

    }

    void OnStatusChanged()
    {
        m_NavAgent.speed = m_Status.m_MoveSpeed;
    }
    // Update is called once per frame
    void Update()
    {
        UpdateAngular();
        CheckMoveArrived();
    }
    
    void UpdateAngular()
    {
        Vector3 moveDirHor = new Vector3(m_NavAgent.velocity.x, 0, m_NavAgent.velocity.z);
        if(moveDirHor == Vector3.zero)
        {
            return;
        }
        moveDirHor.Normalize();
        Quaternion curRot = transform.rotation;
        Quaternion lookRot = Quaternion.LookRotation(moveDirHor, Vector3.up);
        float maxDegree = MoveDirAngularSpeed * Time.deltaTime;
        transform.rotation = Quaternion.RotateTowards(curRot, lookRot, maxDegree);
    }
    void CheckMoveArrived()
    {
        if(m_CallOnArrived == false && m_NavAgent.pathStatus == NavMeshPathStatus.PathComplete && m_NavAgent.remainingDistance < 0.1f)
        {
            m_CallOnArrived = true;
            OnMoveArrived();
        }
    }
    void OnMoveArrived()
    {
        m_HumanAnim.SetAnimTrigger(Anim_Trigger.Idle);
    }
    public void Move(Vector3 groundPos)
    {
        if (m_MoveDestiObj != null)
        {
            m_MoveDestiObj.transform.position = groundPos;
        }
        m_HumanAnim.SetAnimTrigger(Anim_Trigger.Run);
        m_NavAgent.isStopped = false;
        m_CallOnArrived = false;
        m_NavAgent.SetDestination(groundPos);
    }
    public void MoveStop()
    {
        m_NavAgent.isStopped = true;
    }
    
    
}
