using Unity.VisualScripting;
using UnityEngine;

public enum CarAccType
{
    None,
    Forward,
    Back,
    Break
}
public enum CarRotType
{
    Straight,
    Right,
    Left
}
public class CarMove : MonoBehaviour
{
    const float SelfDeceleration = 0.9f;

    Rigidbody m_Rigid;
    CarRotType m_RotType = CarRotType.Straight;
    CarAccType m_AccType = CarAccType.None;
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
            Acc(InputM.InputDir.y > 0 ? CarAccType.Forward : CarAccType.Back);
        }
        else
        {
            Acc(CarAccType.None);
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
        AccType();
        VelocityFixedUpdate();
    }

    void AccType()
    {
        float acc = 0;
        switch(m_AccType)
        {
            case CarAccType.None:
                acc = -m_CurSpeed * SelfDeceleration;
                break;
            case CarAccType.Forward:
                acc = Mathf.Clamp01((m_MaxSpeedForward - m_CurSpeed) / m_MaxSpeedForward);
                acc *= m_MaxAcc;
                //Debug.Log("���� : "+((m_MaxSpeedForward - m_CurSpeed) / m_MaxSpeedForward )+ " " + acc);
                break;
            case CarAccType.Back:
                acc = Mathf.Clamp01((m_CurSpeed + m_MaxSpeedBack) / m_MaxSpeedBack);
                acc *= -m_MaxAcc;
                //Debug.Log("���� : " + ((m_CurSpeed - m_MaxSpeedBack) / m_MaxSpeedBack) + " " + acc);
                break;
        }
      
        m_CurSpeed += acc * Time.deltaTime;
        m_CurSpeed = Mathf.Clamp(m_CurSpeed, -m_MaxSpeedBack, m_MaxSpeedForward);

        if(m_AccType == CarAccType.None)
        {
            if (Mathf.Abs(m_CurSpeed) < 1f)
            {
                m_CurSpeed = 0;
            }
        }
    }
    //���� + �߷�
    void VelocityFixedUpdate()
    {
        if(m_CurSpeed == 0)
        {
            m_Rigid.linearVelocity = Vector3.zero;
            return;
        }
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
        float curMoveDis = m_CurSpeed * Time.fixedDeltaTime; //���� ���������ӿ� �����̴� �Ÿ�
        float rotAngle = curMoveDis * -curRotAngle;
        Vector3 forward = Quaternion.AngleAxis(-90 + rotAngle, transform.up) * transform.forward;
        m_Rigid.MoveRotation( Quaternion.AngleAxis(rotAngle, transform.up) * m_Rigid.rotation);
        m_Rigid.linearVelocity = forward * m_CurSpeed;
        //ȸ������
        m_CurSpeed *= (90 - Mathf.Clamp(Mathf.Abs(rotAngle),0,90)) / 90;
    }

    public void Acc(CarAccType accType)
    {
        //������ ��� �������϶� ���ӵ� �ִ밪 0~�ִ������ӵ� ���� Linear�� �ִ밡��~0 
        //������ ���� �ݴ�
        m_AccType = accType;
    }
    public void Break()
    {

    }
    public void SetRotType(CarRotType rotType)
    {
        m_RotType = rotType;
    }
}
