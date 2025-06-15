using Unity.VisualScripting;
using UnityEngine;

public class CarMove : MonoBehaviour
{
    public enum CarRotType
    {
        Straight,
        Right,
        Left
    }

    Rigidbody m_Rigid;
    CarRotType m_RotType = CarRotType.Straight;
    float m_CurSpeed;
    [SerializeField] float m_MaxSpeedForward;
    [SerializeField] float m_MaxSpeedBack;
    [SerializeField] float m_MaxAcc;
    [SerializeField] float m_RotAngle;
   

    private void Awake()
    {
        m_Rigid = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        CamM.Ins.TargetRegister(transform, CamM.CamTargetType.Car);

    }
    private void Update()
    {
        if (InputM.InputDir.y != 0)
        {
            Acc(InputM.InputDir.y > 0);
        }
        if (InputM.InputDir.x != 0)
        {
            SetRotType(InputM.InputDir.x > 0 ? CarRotType.Left : CarRotType.Right);
        }
        else
        {
            SetRotType(CarRotType.Straight);
        }
        if (Input.GetKey(KeyCode.Space))
        {
            Break();
        }
    }
    private void FixedUpdate()
    {
        VelocityFixedUpdate();
    }

    //감속 + 중력
    void VelocityFixedUpdate()
    {
        float curRotAngle = 0;
        switch (m_RotType)
        {
            case CarRotType.Straight:
                break;
            case CarRotType.Right:
                curRotAngle = m_RotAngle;
                break;
            case CarRotType.Left:
                curRotAngle = -m_RotAngle;
                break;
        }
        Vector3 forward = Quaternion.AngleAxis(-90 + curRotAngle, transform.up) * transform.forward;

        m_Rigid.linearVelocity = forward * m_CurSpeed;
    }

    public void Acc(bool isForward)
    {
        //전진의 경우 후진중일땐 가속도 최대값 0~최대전진속도 까지 Linear로 최대가속~0 
        //후진의 경우는 반대
        float acc = 0;
        if(isForward)
        {
            acc = Mathf.Clamp01((m_MaxSpeedForward - m_CurSpeed) / m_MaxSpeedForward);
            acc *= m_MaxAcc;
            //Debug.Log("전진 : "+((m_MaxSpeedForward - m_CurSpeed) / m_MaxSpeedForward )+ " " + acc);
        }
        else
        {
            acc = Mathf.Clamp01((m_CurSpeed + m_MaxSpeedBack) / m_MaxSpeedBack);
            acc *= -m_MaxAcc;
            //Debug.Log("후진 : " + ((m_CurSpeed - m_MaxSpeedBack) / m_MaxSpeedBack) + " " + acc);
        }
        m_CurSpeed += acc * Time.deltaTime;
        m_CurSpeed = Mathf.Clamp(m_CurSpeed, -m_MaxSpeedBack, m_MaxSpeedForward);
    }
    public void Break()
    {

    }
    public void SetRotType(CarRotType rotType)
    {
        m_RotType = rotType;
    }
}
