using UnityEngine;
using System;

public class BioPhysics : MonoBehaviour
{
    const float Gravity = 12.0f;
    const float DecelerationValue = 0.5f;
    public const float GroundCheckMargin = 0.02f;
    [SerializeField] float m_MaxSlope;
    Rigidbody m_Rigid;
    Vector3 m_CurVelocity;
    bool m_IsGround;
    Vector3 m_GroundNormal = Vector3.up;
    Transform m_Ground;

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
    Vector3 m_GizmoForward;
    void Move()
    {
        Vector2 velocityXZ = new Vector2(m_CurVelocity.x, m_CurVelocity.z);
        Vector3 movePos = Vector3.zero;
        if(velocityXZ != Vector2.zero)
        {
            if(m_IsGround == false)
            {
                Debug.Log("공중이동");
                Vector3 right = Vector3.Cross(m_GroundNormal, new Vector3(velocityXZ.x, 0, velocityXZ.y));
                Vector3 forward = Vector3.Cross(right, m_GroundNormal);
                m_GizmoForward = forward;
                movePos = forward * Time.fixedDeltaTime;
            }
            else
            {
                float normalY = Mathf.Clamp01(m_GroundNormal.y);
                float slopeAngle = 90 - Mathf.Asin(normalY) * Mathf.Rad2Deg;
                if (slopeAngle < m_MaxSlope)
                {
                    Vector3 right = Vector3.Cross(m_GroundNormal, new Vector3(velocityXZ.x, 0, velocityXZ.y));
                    Vector3 forward = Vector3.Cross(right, m_GroundNormal);
                    m_GizmoForward = forward;
                    movePos = forward * Time.fixedDeltaTime;
                    Debug.Log("이동가능 각도");
                }
                else
                {
                    Debug.Log("이동불가능 각도");
                }
            }
          
          
        }
        float velocityY = m_CurVelocity.y;
        if (velocityY != 0)
        {
            movePos.y = velocityY * Time.fixedDeltaTime;
        }
        if(movePos!= Vector3.zero)
        {
            m_Rigid.MovePosition(m_Rigid.position + movePos);
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
            m_Ground = hit.transform;
            m_GroundNormal = hit.normal;
        }
        else
        {
            m_Ground = null;
            m_GroundNormal = Vector3.up;
        }
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
        if (Application.isPlaying)
        {
            Gizmos.DrawLine(transform.position + new Vector3(0, 0.2f, 0), transform.position + new Vector3(0, 0.2f, 0) + m_GizmoForward * 1);
            Vector3 rayStartPos = m_Rigid.position + new Vector3(0, 0.5f, 0);
            Vector3 halfExSize = new Vector3(0.2f, 0.05f, 0.2f);
            bool isGround = Physics.BoxCast(rayStartPos, halfExSize, Vector3.down, out RaycastHit hit, Quaternion.Euler(0, transform.rotation.y, 0), 0.5f, (1 << (int)LAYER.OBJECT | 1 << (int)LAYER.GROUND));
            if (isGround)
            {
                Gizmos.DrawCube(hit.point, new Vector3(0.1f, 0.05f, 0.1f) * 2);
            }
        }
    }

}
