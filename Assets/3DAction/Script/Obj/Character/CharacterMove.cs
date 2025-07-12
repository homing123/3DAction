using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.AI;

public class CharacterMove : MonoBehaviour
{
    NavMeshAgent m_NavAgent;
    HumanAnim m_HumanAnim;
    bool m_CallOnArrived = true;
    Status m_Status;
    Character m_Character;
    public bool m_IsMove { get { return !m_NavAgent.isStopped; } }

    [SerializeField] Transform m_MoveDestiObj;
    private void Awake()
    {
        m_NavAgent = GetComponent<NavMeshAgent>();
        m_HumanAnim = GetComponent<HumanAnim>();
        m_Status = GetComponent<Status>();
        m_Character = GetComponent<Character>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_NavAgent.updateRotation = false;
        m_NavAgent.speed = m_Status.m_TotalMoveSpeed;
        m_Status.OnStatusChanged += OnStatusChanged;
    }
    private void OnDestroy()
    {
        m_Status.OnStatusChanged -= OnStatusChanged;

    }

    void OnStatusChanged()
    {
        m_NavAgent.speed = m_Status.m_TotalMoveSpeed;
    }
    // Update is called once per frame
    void Update()
    {
        if (m_IsMove == true)
        {
            m_Character.UpdateAngular(new Vector2(m_NavAgent.velocity.x, m_NavAgent.velocity.z), out bool arrived);
            CheckMoveArrived();
        }
    }
    void CheckMoveArrived()
    {
        float remainDis = Vector3.Distance(m_NavAgent.destination, transform.position);
        if (m_CallOnArrived == false && m_NavAgent.pathStatus == NavMeshPathStatus.PathComplete && remainDis < 0.1f)
        {
            m_CallOnArrived = true;
            m_NavAgent.isStopped = true;
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
