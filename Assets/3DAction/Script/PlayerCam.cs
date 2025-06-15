using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    const string MouseY = "Mouse Y";
    const string MouseX = "Mouse X";
    const float RotYClamp = 25;
    public static PlayerCam Ins;
    PlayerMove m_Player;
    float m_CurRotY;
    float m_CurRotX;

    [SerializeField] float m_MouseRotSpeed = 200;

    [SerializeField] float m_Dis = 5;
    [SerializeField] float m_Height = 1;
    [SerializeField] float m_PlayerFocusHeight = 1.7f;
    [SerializeField] bool m_CamRotActive;

    public float CamRotY { get { return m_CurRotY; } }

    private void Awake()
    {
        if(Ins != null && Ins != this)
        {
            Destroy(Ins.gameObject);
        }
        Ins = this;
    }

    void Start()
    {
        m_CamRotActive = true;
        m_Player = PlayerMove.Ins;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            m_CamRotActive = !m_CamRotActive;
        }
        if(m_CamRotActive== false)
        {
            return;
        }
        m_CurRotY += Input.GetAxis(MouseX) * Time.deltaTime * m_MouseRotSpeed;
        m_CurRotX -= Input.GetAxis(MouseY) * Time.deltaTime * m_MouseRotSpeed;
        m_CurRotX = Mathf.Clamp(m_CurRotX, -RotYClamp, RotYClamp);
        LookPlayer();
    }

    void LookPlayer()
    {
        if(m_Player == null)
        {
            return;
        }

        Vector3 lookPos = m_Player.transform.position + new Vector3(0, m_PlayerFocusHeight, 0);
        Vector2 dirXZ = Util.Deg2Vt2(m_CurRotY);
        float disXZ = Mathf.Sqrt(m_Dis * m_Dis - m_Height * m_Height);
        Vector2 xzOffset = dirXZ * disXZ;
        transform.position = lookPos + new Vector3(-xzOffset.x, m_Height, -xzOffset.y);
        transform.rotation = Quaternion.Euler(m_CurRotX, m_CurRotY, 0);
    }
   
}
