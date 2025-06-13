using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(HumanAnim))]
[RequireComponent(typeof(HumanMove))]
public class PlayerMove : MonoBehaviour
{
    public const float MoveDirRotSpeed = 720;
    public const float JumpDelay = 0.1f;

    public static PlayerMove Ins;
    HumanAnim m_HumanAnim;
    HumanMove m_Move;


    Vector3 m_MoveDir = Vector3.forward;
    private void Awake()
    {
        Ins = this;
        m_HumanAnim = GetComponent<HumanAnim>();
        m_Move = GetComponent<HumanMove>();
    }

    private void Start()
    {
    }
    private void Update()
    {
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
            m_Move.TryJump();
        }

        if (inputDir != Vector2.zero)
        {
            Vector2 norm = inputDir.normalized;
            Vector2 camRotNorm = Util.Vt2XZRotAxisY(norm, PlayerCam.Ins.CamRotY);
            m_MoveDir = new Vector3(camRotNorm.x, 0, camRotNorm.y);
            m_Move.InputMoveDir(m_MoveDir);
        }
    }
   

  
 

}
