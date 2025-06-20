using UnityEngine;

public enum MOVE_TYPE
{
    Walk,
    JUMP,
    FALLING,
}
public class HumanMove : MonoBehaviour
{
    public const float MoveDirRotSpeed = 720;
    public const float JumpDelay = 0.1f;
    const float Gravity = 12.0f;
    const float DecelerationValue_Walk = 0.5f;
    const float DecelerationValue_Falling = 0.95f;

    const float MaxSlope = 45.0f;
    [SerializeField] float m_WalkSpeed;
    [SerializeField] float m_JumpSpeed;
    MOVE_TYPE m_MoveType;

    Rigidbody m_Rigid;
    HumanAnim m_HumanAnim;

    Vector2 m_CurVelocityXZ;
    float m_CurVelocityY;
    Vector3 m_MoveDir = Vector3.forward;

    bool m_IsGround;
    Vector3 m_GroundNormal = Vector3.up;
    Transform m_Ground;

    Vector3 m_CurInputDir;

    public bool IsGround { get { return m_IsGround; } }
    float m_CurJumpDelay;
    private void Awake()
    {
        m_Rigid = GetComponent<Rigidbody>();
        m_HumanAnim = GetComponent<HumanAnim>();
    }
    private void Start()
    {
        CheckMoveType();
    }
    private void Update()
    {
        RotationToMoveDir();
        SetAnim();
        m_CurInputDir = Vector3.zero;
    }
    private void FixedUpdate()
    {
        m_CurJumpDelay -= Time.fixedDeltaTime;
        m_Rigid.linearVelocity = Vector3.zero;
        MoveVelocity();
        CheckMoveType();
        VelocityFixedUpdate();

    }
    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.DrawLine(transform.position + new Vector3(0, 0.2f, 0), transform.position + new Vector3(0, 0.2f, 0) + gizmoforward * 3);
        }
    }
    Vector3 gizmoforward;
    void MoveVelocity()
    {
        Vector3 move = Vector3.zero;
        if(m_CurVelocityXZ != Vector2.zero)
        {
            switch(m_MoveType)
            {
                case MOVE_TYPE.Walk:
                    m_GroundNormal.Normalize();
                    float curMoveDis = m_CurVelocityXZ.magnitude;
                    Vector3 right = Vector3.Cross(m_GroundNormal, new Vector3(m_CurVelocityXZ.x, 0, m_CurVelocityXZ.y));
                    right.Normalize();
                    Vector3 forward = Vector3.Cross(right, m_GroundNormal);
                    move += forward * curMoveDis * Time.fixedDeltaTime;
                    break;
                default:
                    move += new Vector3(m_CurVelocityXZ.x, 0, m_CurVelocityXZ.y) * Time.fixedDeltaTime;
                    break;
            }
            m_MoveDir.x = move.x;
            m_MoveDir.z = move.z;
            gizmoforward = move;
        }
        if (m_CurVelocityY != 0)
        {
            switch(m_MoveType)
            {
                case MOVE_TYPE.JUMP:
                case MOVE_TYPE.FALLING:
                    move.y += m_CurVelocityY * Time.fixedDeltaTime;
                    break;
            }
            m_MoveDir.y = move.y;
        }
        if (move != Vector3.zero)
        {
            m_Rigid.MovePosition(m_Rigid.position + move);
        }
    }
    void SetAnim()
    {
        switch(m_MoveType)
        {
            case MOVE_TYPE.Walk:
                if(m_CurInputDir == Vector3.zero)
                {
                    m_HumanAnim.SetAnimTrigger(ANIM_TRIGGER.IDLE);
                }
                else
                {
                    m_HumanAnim.SetAnimTrigger(ANIM_TRIGGER.RUN);
                }
                break;
        }
    }
    void VelocityFixedUpdate()
    {
        if (m_CurVelocityXZ != Vector2.zero)
        {
            float decelerationValue = 1;
            switch(m_MoveType)
            {
                case MOVE_TYPE.Walk:
                    decelerationValue = DecelerationValue_Walk;
                    break;
                case MOVE_TYPE.JUMP:
                case MOVE_TYPE.FALLING:
                    decelerationValue = DecelerationValue_Falling;
                    break;
            }
            m_CurVelocityXZ *= decelerationValue;
            m_CurVelocityXZ.x = Mathf.Abs(m_CurVelocityXZ.x) < 0.04f ? 0 : m_CurVelocityXZ.x;
            m_CurVelocityXZ.y = Mathf.Abs(m_CurVelocityXZ.y) < 0.04f ? 0 : m_CurVelocityXZ.y;
        }

        switch(m_MoveType)
        {
            case MOVE_TYPE.JUMP:
            case MOVE_TYPE.FALLING:
                m_CurVelocityY -= Gravity * Time.fixedDeltaTime;
                break;
            case MOVE_TYPE.Walk:
                m_CurVelocityY = 0;
                break;
        }
    }
    void CheckMoveType()
    {
        Vector3 rayStartPos = m_Rigid.position + new Vector3(0, 0.5f, 0);
        Vector3 halfExSize = new Vector3(0.2f, 0.05f, 0.2f);
        bool isGround = Physics.BoxCast(rayStartPos, halfExSize, Vector3.down, out RaycastHit hit, Quaternion.Euler(0, transform.rotation.y, 0), 0.5f, (1 << (int)Layer.Object | 1 << (int)Layer.Ground));
        
        if(isGround)
        {
            m_GroundNormal = hit.normal;
            float normalY = Mathf.Clamp01(m_GroundNormal.y);
            float slopeAngle = 90 - Mathf.Asin(normalY) * Mathf.Rad2Deg;
            if(slopeAngle > MaxSlope)
            {
                m_MoveType = MOVE_TYPE.FALLING;
            }
            else
            {
                m_MoveType = MOVE_TYPE.Walk;
            }
           
        }
        else
        {
            switch (m_MoveType)
            {
                case MOVE_TYPE.Walk:
                    //Debug.Log("walk -> falling");

                    m_MoveType = MOVE_TYPE.FALLING;
                    break;
                case MOVE_TYPE.JUMP:
                    break;
                case MOVE_TYPE.FALLING:
                    break;
            }
        }
    }

    void RotationToMoveDir()
    {
        switch(m_MoveType)
        {
            case MOVE_TYPE.Walk:
            case MOVE_TYPE.JUMP:
            case MOVE_TYPE.FALLING:
                Vector3 moveDirHor = new Vector3(m_MoveDir.x,0, m_MoveDir.z);
                moveDirHor.Normalize();
                Quaternion curRot = transform.rotation;
                Quaternion lookRot = Quaternion.LookRotation(moveDirHor, Vector3.up);
                float maxDegree = MoveDirRotSpeed * Time.deltaTime;
                transform.rotation = Quaternion.RotateTowards(curRot, lookRot, maxDegree);
                break;
        }
        
    }

    public void InputMoveDir(Vector3 dir)
    {
        switch(m_MoveType)
        {
            case MOVE_TYPE.Walk:
            case MOVE_TYPE.JUMP:
            case MOVE_TYPE.FALLING:
                m_CurVelocityXZ = dir.VT2XZ() * m_WalkSpeed;
                m_CurInputDir = m_CurVelocityXZ;
                break;
        }
    }
    public bool TryJump()
    {
        if (m_MoveType == MOVE_TYPE.Walk)
        {
            m_CurVelocityY = m_JumpSpeed;
            m_CurJumpDelay = JumpDelay;
            m_MoveType = MOVE_TYPE.JUMP;
            m_HumanAnim.SetAnimTrigger(ANIM_TRIGGER.JUMP);
            return true;
        }
        return false;
    }
}
