using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class CamM : MonoBehaviour
{
    public enum CamTargetType
    {
        None = 0,
        Player = 1,
        Car = 2,
    }
    const string MouseY = "Mouse Y";
    const string MouseX = "Mouse X";
    const float RotYClamp = 25;
    public static CamM Ins;
    float m_CurRotY;
    float m_CurRotX;

    Transform m_Target;
    CamTargetType m_TargetType = CamTargetType.None;

    [SerializeField] bool m_CamRotActive;
    [SerializeField] float m_MouseRotSpeed = 200;

    [Header("Player")]
    [SerializeField] float m_Dis = 5;
    [SerializeField] float m_Height = 1;
    [SerializeField] float m_PlayerFocusHeight = 1.7f;
    [Header("Car")]
    [SerializeField] float m_CarFocusHeight;
    [SerializeField] float m_CarDis;
    [SerializeField] float m_CarHeight;
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
        LookTarget();
    }

 
    void LookTarget()
    {
        if(m_Target == null)
        {
            TargetRelease();
        }
        switch(m_TargetType)
        {
            case CamTargetType.Player:
                {
                    SetThirdCam(m_PlayerFocusHeight, m_Dis, m_Height);
                    transform.rotation = Quaternion.Euler(m_CurRotX, m_CurRotY, 0);
                }
                break;
            case CamTargetType.Car:
                {
                    SetThirdCam(m_CarFocusHeight, m_CarDis, m_CarDis);
                    transform.rotation = Quaternion.Euler(m_CurRotX, m_CurRotY, 0);
                }
                break;
        }
    }
    void SetThirdCam(float focusHeight, float dis, float height)
    {
        Vector3 lookPos = m_Target.position + new Vector3(0, m_PlayerFocusHeight, 0);
        Vector2 dirXZ = Util.Deg2Vt2(m_CurRotY);
        float disXZ = Mathf.Sqrt(m_Dis * m_Dis - m_Height * m_Height);
        Vector2 xzOffset = dirXZ * disXZ;
        transform.position = lookPos + new Vector3(-xzOffset.x, m_Height, -xzOffset.y);
    }

    public void TargetRegister(Transform target, CamTargetType type)
    {
        m_Target = target;
        m_TargetType = type;
    }
    public void TargetRelease()
    {
        m_Target = null;
        m_TargetType = CamTargetType.None;
    }

}
