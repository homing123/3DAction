using UnityEngine;
using System;

public class BioPhysics : MonoBehaviour
{
    const float Gravity = 12.0f;
    const float DecelerationValue = 0.5f;
    public const float GroundCheckMargin = 0.02f;
    Rigidbody m_Rigid;
    Vector3 m_CurVelocity;
    bool m_IsGround;

    public Vector3 Velocity { get { return m_CurVelocity; }  set { m_CurVelocity = value; } }
    public bool IsGround { get { return m_IsGround; } }
    bool m_LastIsGround;
    float m_CurJumpDelay;

    public event Action OnLand;
    private void Awake()
    {
        m_Rigid = GetComponent<Rigidbody>();
    }
    private void Start()
    {
        GroundCheck();
        m_LastIsGround = m_IsGround;
    }
    private void Update()
    {
        if(m_LastIsGround == false && m_IsGround == true)
        {
            OnLand?.Invoke();
        }
        m_LastIsGround = m_IsGround;

    }
    private void FixedUpdate()
    {
        m_Rigid.linearVelocity = Vector3.zero;
        Move();
        GroundCheck();
        VelocityFixedUpdate();
    }
    void Move()
    {
        if (m_CurVelocity != Vector3.zero)
        {
            m_Rigid.MovePosition(m_Rigid.position + m_CurVelocity * Time.fixedDeltaTime);
        }
    }
    void VelocityFixedUpdate()
    {
        if (m_CurVelocity.x != 0 || m_CurVelocity.z != 0)
        {
            m_CurVelocity.x = m_CurVelocity.x * DecelerationValue;
            m_CurVelocity.z = m_CurVelocity.z * DecelerationValue;
            m_CurVelocity.x = Mathf.Abs(m_CurVelocity.x) < 0.1f ? 0 : m_CurVelocity.x;
            m_CurVelocity.z = Mathf.Abs(m_CurVelocity.z) < 0.1f ? 0 : m_CurVelocity.z;
        }

        if (m_IsGround == false)
        {
            m_CurVelocity.y -= Gravity * Time.fixedDeltaTime;
        }
        else
        {
            m_CurVelocity.y = 0;
        }
    }
    void GroundCheck()
    {
        if(m_CurJumpDelay > 0)
        {
            m_CurJumpDelay -= Time.fixedDeltaTime;
            return;
        }
        Vector3 rayStartPos = m_Rigid.position + new Vector3(0, 0.5f, 0);
        Vector3 halfExSize = new Vector3(0.2f, 0.05f, 0.2f);
        bool isGround = Physics.BoxCast(rayStartPos, halfExSize, Vector3.down, out RaycastHit hit, Quaternion.Euler(0, transform.rotation.y, 0), 0.5f, (1 << (int)LAYER.OBJECT | 1 << (int)LAYER.GROUND));

        if (isGround) 
        {
            Debug.Log(hit.transform.name + " " + hit.normal);
        }
        else
        {
            Debug.Log("충돌x");
        }
        //m_IsGround = Physics.Raycast(m_Rigid.position + new Vector3(0, 0.2f, 0), Vector3.down, out RaycastHit hit, GroundCheckMargin + 0.2f, (1 << (int)LAYER.OBJECT | 1 << (int)LAYER.GROUND));
        //if(m_IsGround)
        //{
        //    hit = 
        //}
        m_IsGround = isGround;
    }
    
    public void Jump(float jumpVelo)
    {
        m_IsGround = false;
        m_CurVelocity = new Vector3(m_CurVelocity.x, jumpVelo, m_CurVelocity.z);
        m_CurJumpDelay = PlayerMove.JumpDelay;
    }

    private void OnDrawGizmos()
    {
        //if(Physics.BoxCast(transform.position, Vector3.one, Vector3.forward, out RaycastHit hit,Quaternion.identity,5, (1 << (int)LAYER.OBJECT | 1 << (int)LAYER.GROUND)))
        //{
        //    Debug.Log("충돌"+ hit.transform.name+" " +hit.point);
        //    Gizmos.DrawWireCube(hit.point, Vector3.one * 2);
        //}
    }

}
