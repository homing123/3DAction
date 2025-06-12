using UnityEngine;
using System.Collections.Generic;

public enum MOVE_TYPE
{
    WALK,
    JUMP,
}
[RequireComponent(typeof(BioAnim))]
[RequireComponent(typeof(BioPhysics))]
public class Player : MonoBehaviour
{
    public const float MoveDirRotSpeed = 720;
    const float JumpDelay = 0.1f;

    public static Player Ins;
    BioPhysics m_BioPhysics;
    BioAnim m_BioAnim;

    [SerializeField] float m_WalkSpeed;
    [SerializeField] float m_JumpPower;
    [SerializeField] float m_RunSpeed;

    Vector3 m_MoveDir = Vector3.forward;
    MOVE_TYPE m_MoveType = MOVE_TYPE.WALK;
    float m_CurJumpDelay;
    private void Awake()
    {
        Ins = this;
        m_BioPhysics = GetComponent<BioPhysics>();
        m_BioAnim = GetComponent<BioAnim>();
    }

    private void Start()
    {
        m_BioPhysics.OnLand += OnLand;
    }
    private void Update()
    {
        if (m_CurJumpDelay > 0)
        {
            m_CurJumpDelay -= Time.deltaTime;
        }

        Vector2 inputDir = Vector2.zero;
        if (Input.GetKey(KeyCode.W))
        {
            inputDir.y = 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            inputDir.x = -1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            inputDir.x = 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            inputDir.y = -1;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryJump();
        }

        if (inputDir != Vector2.zero)
        {
            Vector2 norm = inputDir.normalized;
            Vector2 camRotNorm = Util.Vt2XZRotAxisY(norm, PlayerCam.Ins.CamRotY);
            m_MoveDir = new Vector3(camRotNorm.x, 0, camRotNorm.y);
        }
        switch (m_MoveType)
        {
            case MOVE_TYPE.WALK:
                if (inputDir == Vector2.zero)
                {
                    m_BioAnim.SetAnimTrigger(ANIM_TRIGGER.IDLE);
                }
                else
                {
                    Move();
                    m_BioAnim.SetAnimTrigger(ANIM_TRIGGER.RUN);
                }
                break;
            case MOVE_TYPE.JUMP:
                if (inputDir != Vector2.zero)
                {
                    Move();
                }
               
                break;
        }
        RotationToMoveDir();
    }
    void MoveTypeChange(MOVE_TYPE type)
    {
        if (m_MoveType != type)
        {
            m_MoveType = type;
        }
    }

    void Move()
    {
        m_BioPhysics.Velocity = new Vector3(m_MoveDir.x * m_RunSpeed, m_BioPhysics.Velocity.y, m_MoveDir.z * m_RunSpeed);
    }
    void TryJump()
    {
        if (m_BioPhysics.IsGround == false || m_CurJumpDelay > 0)
        {
            return;
        }
        MoveTypeChange(MOVE_TYPE.JUMP);
        m_BioAnim.SetAnimTrigger(ANIM_TRIGGER.JUMP);
        m_CurJumpDelay = JumpDelay;
        m_BioPhysics.Velocity = new Vector3(m_BioPhysics.Velocity.x, m_JumpPower, m_BioPhysics.Velocity.z);
    }
    void OnLand()
    {
        m_MoveType = MOVE_TYPE.WALK;
    }
    void RotationToMoveDir()
    {
        Quaternion curRot = transform.rotation;
        Quaternion lookRot = Quaternion.LookRotation(m_MoveDir, Vector3.up);
        float maxDegree = MoveDirRotSpeed * Time.deltaTime;
        transform.rotation = Quaternion.RotateTowards(curRot, lookRot, maxDegree);
    }

}
