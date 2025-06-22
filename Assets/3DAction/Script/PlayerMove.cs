using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.AI;

public class PlayerMove : MonoBehaviour
{
    const float MoveDirAngularSpeed = 1440;
    public static PlayerMove Ins;
    NavMeshAgent m_NavAgent;
    HumanAnim m_HumanAnim;
    bool m_CallOnArrived = true;
    
    [SerializeField] Transform m_MoveDestiObj;
    private void Awake()
    {
        Ins = this;
        m_NavAgent = GetComponent<NavMeshAgent>();
        m_HumanAnim = GetComponent<HumanAnim>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_NavAgent.updateRotation = false;
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

        m_CallOnArrived = false;
        m_NavAgent.SetDestination(groundPos);
    }
    
}
