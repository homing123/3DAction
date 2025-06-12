using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    const string MouseY = "Mouse Y";
    const string MouseX = "Mouse X";
    public static PlayerCam Ins;
    Player m_Player;
    float m_CurRotY;

    [SerializeField] float m_MouseRotSpeed;

    [SerializeField] float m_Dis;
    [SerializeField] float m_Height;
    [SerializeField] float m_PlayerFocusHeight;

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
        m_Player = Player.Ins;
    }

    // Update is called once per frame
    void Update()
    {
        m_CurRotY += Input.GetAxis(MouseX) * Time.deltaTime * m_MouseRotSpeed;
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
        transform.LookAt(lookPos);
    }
   
}
