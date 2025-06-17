using UnityEngine;

public class Autobike : MonoBehaviour
{
    CarAccType m_AccType = CarAccType.None;
    CarRotType m_RotType = CarRotType.Straight;

    float m_Speed;
    Vector3 m_Velocity;
    float m_CurRot;

    [SerializeField] Tire m_FrontTire;
    [SerializeField] Tire m_BackTire;
    [SerializeField] Transform m_Body;

    [SerializeField] float m_Break;
    [SerializeField][Range(1,89)] float m_MaxRot;
    [SerializeField] float m_HandleSensitivity;

    [SerializeField] float m_MaxSpeedForward;
    [SerializeField] float m_MaxSpeedBack;
    [SerializeField] float m_MaxAcc;
    [SerializeField] [Range(0,1)]float m_SelfDeceleration;
    [SerializeField] float m_RotZAnimMaxAngle;

    Vector3 m_FrontOriginOffset;
    Vector3 m_BackOriginOffset;
    Vector3 m_BodyOffset;


    private void Start()
    {
        m_FrontOriginOffset = m_FrontTire.transform.localPosition;
        m_BackOriginOffset = m_BackTire.transform.localPosition;
        m_BodyOffset = m_Body.transform.localPosition;
        CamM.Ins.TargetRegister(transform, CamM.CamTargetType.Car);

    }
    private void Update()
    {
        if(InputM.InputDir.y!= 0)
        {
            Acc(InputM.InputDir.y > 0 ? CarAccType.Forward : CarAccType.Back);
        }
        else
        {
            Acc(CarAccType.None);
        }
        
        if(InputM.InputDir.x != 0)
        {
            Rot(InputM.InputDir.x > 0 ? CarRotType.Right : CarRotType.Left);
        }
        else
        {
            Rot(CarRotType.Straight);
        }

        if(Input.GetKey(KeyCode.Space))
        {
            Acc(CarAccType.Break);
        }

        SetTransform();
    }
    private void FixedUpdate()
    {
        m_FrontTire.CheckState();
        m_BackTire.CheckState();
        VelocityFixedUpdate();
        MoveTire();

    }

    //타이어이동
    //물리이동
    //위치에 맞게 바디세팅

    void MoveTire()
    {
        Vector3 frontTirePos = m_FrontTire.Move(transform.right, m_Speed);
        Vector3 backTirePos = m_BackTire.Move(transform.right, m_Speed);
        m_FrontTire.transform.position = frontTirePos;
        m_BackTire.transform.position = backTirePos;
        //Debug.Log(frontTirePos + " " + backTirePos);
    }
    void VelocityFixedUpdate()
    {
        switch(m_RotType)
        {
            case CarRotType.Left:
                m_CurRot -= m_HandleSensitivity * Time.fixedDeltaTime;
                if (m_CurRot < -m_MaxRot)
                {
                    m_CurRot = -m_MaxRot;
                }

                break;
            case CarRotType.Right:
                m_CurRot += m_HandleSensitivity * Time.fixedDeltaTime;
                if (m_CurRot > m_MaxRot)
                {
                    m_CurRot = m_MaxRot;
                }
                break;
            case CarRotType.Straight:
                float handleRotAngle = m_HandleSensitivity * Time.fixedDeltaTime;
                if (Mathf.Abs(m_CurRot) < handleRotAngle)
                {
                    m_CurRot = 0;
                }
                else
                {
                    m_CurRot -= handleRotAngle * Mathf.Sign(m_CurRot);
                }
                break;
        }
        //회전감속
        m_Speed *= 1 - Mathf.Pow(Mathf.Clamp01(Mathf.Sin(Mathf.Abs(m_CurRot * Mathf.Deg2Rad))), 3);


        m_FrontTire.SetRot(m_CurRot);
        switch(m_AccType)
        {
            case CarAccType.None:
                {
                    float breakForce = m_Break * 0.3f * Time.fixedDeltaTime;
                    if (Mathf.Abs(m_Speed) < breakForce)
                    {
                        m_Speed = 0;
                    }
                    else
                    {
                        m_Speed -= breakForce * Mathf.Sign(m_Speed);
                    }
                }
                break;
            case CarAccType.Forward:
                {
                    float acc = Mathf.Clamp01((m_MaxSpeedForward - m_Speed) / m_MaxSpeedForward);
                    acc *= m_MaxAcc;
                    m_Speed += acc * Time.fixedDeltaTime;
                    m_Speed = Mathf.Clamp(m_Speed, -m_MaxSpeedBack, m_MaxSpeedForward);
                }
                break;
            case CarAccType.Back:
                {
                    float acc = Mathf.Clamp01((m_Speed + m_MaxSpeedBack) / m_MaxSpeedBack);
                    acc *= -m_MaxAcc;
                    m_Speed += acc * Time.fixedDeltaTime;
                    m_Speed = Mathf.Clamp(m_Speed, -m_MaxSpeedBack, m_MaxSpeedForward);
                }
                break;
            case CarAccType.Break:
                {
                    float breakForce = m_Break * Time.fixedDeltaTime;
                    if (Mathf.Abs(m_Speed) < breakForce)
                    {
                        m_Speed = 0;
                    }
                    else
                    {
                        m_Speed -= breakForce * Mathf.Sign(m_Speed);
                    }
                }
                break;
        }
    }

    void SetTransform()
    {
        Vector3 frontTirePos = m_FrontTire.transform.position;
        Vector3 backTirePos = m_BackTire.transform.position;
        Vector3 forward = (frontTirePos - backTirePos).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
        Vector3 up = Vector3.Cross(forward, right);
        //Debug.Log($"forward {forward}, right {right}, up {up}");
        Matrix4x4 rotMat = new Matrix4x4();
        rotMat.SetColumn(0, new Vector4(right.x, right.y, right.z, 0));
        rotMat.SetColumn(1, new Vector4(up.x, up.y, up.z, 0));
        rotMat.SetColumn(2, new Vector4(forward.x, forward.y, forward.z, 0));
        rotMat.SetColumn(3, new Vector4(0, 0, 0, 1));

        transform.rotation = rotMat.rotation;
        Vector3 mainOfFront = frontTirePos - (Vector3)(rotMat * m_FrontOriginOffset);
        Vector3 mainOfBack = backTirePos - (Vector3)(rotMat * m_BackOriginOffset);
        Vector3 mainPos = (mainOfFront + mainOfBack) * 0.5f;
        transform.position = mainPos;
        Vector3 bodyOffset = rotMat * m_BodyOffset;
        Vector3 frontOffset = rotMat * m_FrontOriginOffset;
        Vector3 backOffset = rotMat * m_BackOriginOffset;
        m_Body.transform.position = mainPos + bodyOffset;
        m_FrontTire.transform.position = mainPos + frontOffset;
        m_BackTire.transform.position = mainPos + backOffset;
        float curRotZAnimAngle = -(m_CurRot / m_MaxRot) * Mathf.Clamp01((m_Speed - m_MaxSpeedBack) / (m_MaxSpeedForward - m_MaxSpeedBack)) * m_RotZAnimMaxAngle;
        transform.rotation = Quaternion.AngleAxis(curRotZAnimAngle, forward) * transform.rotation;



        // Debug.Log($"front {frontTirePos} back {backTirePos} main{mainPos}");
    }
    void Acc(CarAccType accType)
    {
        m_AccType = accType;
    }
   
    void Rot(CarRotType rotType)
    {
        m_RotType = rotType;
    }
}
