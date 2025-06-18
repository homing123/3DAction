using UnityEngine;
using System.Collections.Generic;

public class Tire : MonoBehaviour
{
    const float TireFrictionCurveMaxValueRCP = 1 / 200.0f;
    Collider m_Col;
    Transform m_Parent;
    Vector3 m_OriginLocalPos;
    Collider m_GroundCol;
    public Vector3 m_GroundNormal { get; private set; }
    public bool m_isGround { get; private set; }

    public float m_Rot { get; private set; }
    


    [SerializeField] float m_SpringStrength;
    [SerializeField] float m_SpringDamping;
    float m_SpringVelocity;
    [SerializeField] AnimationCurve m_FrictionCurve;

    Quaternion m_OriginQuat;
    private void Awake()
    {
        m_OriginLocalPos = transform.localPosition;
        m_Parent = transform.parent;
        m_OriginQuat = transform.localRotation;
        m_Col = GetComponentInChildren<Collider>();
    }

    private void FixedUpdate()
    {

    }
    public void CheckState()
    {
        if(Physics.Raycast(transform.position, -Vector3.up, out RaycastHit hit, 0.37f, 1 << (int)Layer.Ground))
        {
            m_GroundNormal = hit.normal;
            m_isGround = true;
            m_GroundCol = hit.collider;
        }
        else
        {
            m_isGround = false;
        }
    }
    public void Move(Vector3 bodyRight, float speed)
    {
        float dis = speed * Time.fixedDeltaTime;
        Vector3 forward = Vector3.Cross(bodyRight, m_GroundNormal).normalized;
        Vector3 moveVector = Vector3.zero;
        if (m_isGround)
        {
            Vector3 rotatedForward = Quaternion.AngleAxis(m_Rot, m_GroundNormal) * forward;

            Vector3 rotatedTireRightOrLeft = Vector3.zero;
            if(m_Rot > 0)
            {
                //left
                rotatedTireRightOrLeft = Vector3.Cross(rotatedForward, m_GroundNormal);
            }
            else
            {
                //right
                rotatedTireRightOrLeft = Vector3.Cross(m_GroundNormal, rotatedForward);
            }

            float dotRotatedForward = Vector3.Dot(forward * dis, rotatedForward);
            float dotRotatedROL = Vector3.Dot(forward * dis, rotatedTireRightOrLeft);
            moveVector = rotatedForward * dotRotatedForward + rotatedTireRightOrLeft * dotRotatedROL * (1 - m_FrictionCurve.Evaluate(TireFrictionCurveMaxValueRCP * speed));
            transform.position += moveVector;

            //땅과 충돌시 충돌한 만큼 바깥으로 이동
            if(Physics.ComputePenetration(m_Col, transform.position, transform.rotation, m_GroundCol, m_GroundCol.transform.position, m_GroundCol.transform.rotation, out Vector3 direction, out float distance))
            {
                transform.position += direction * distance;
            }
        }

        
    }
    public void SetRot(float rot)
    {
        m_Rot = rot;
        transform.localRotation = Quaternion.AngleAxis(m_Rot, Vector3.up) * m_OriginQuat;
    }
    public float GetFriction(float speed)
    {
        return m_FrictionCurve.Evaluate(TireFrictionCurveMaxValueRCP * speed);
    }
    //Vector3 GetOffset()
    //{

    //    //Vector3 up = transform.up;
    //    //float curHeight = Vector3.Dot(transform.localPosition, up);
    //    //float disFromOffset = curHeight - m_OriginLocalPos.y;
    //    //m_SpringVelocity += m_SpringStrength * -disFromOffset;
    //    //m_SpringVelocity -= m_SpringVelocity * m_SpringDamping * Time.fixedDeltaTime;
    //    //return -m_SpringVelocity * Time.fixedDeltaTime;
    //}
}
