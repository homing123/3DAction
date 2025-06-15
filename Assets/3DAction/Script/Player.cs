using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

public class Player : MonoBehaviour
{
    HumanMove m_Move;
    private void Awake()
    {
        m_Move = GetComponent<HumanMove>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CamM.Ins.TargetRegister(transform, CamM.CamTargetType.Player);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            m_Move.TryJump();
        }

        if (InputM.InputDir != Vector2.zero)
        {
            Vector2 camRotNorm = Util.Vt2XZRotAxisY(InputM.InputDir, CamM.Ins.CamRotY);
            Vector3 moveDir = new Vector3(camRotNorm.x, 0, camRotNorm.y);
            m_Move.InputMoveDir(moveDir);
        }
       
    }
    
}
