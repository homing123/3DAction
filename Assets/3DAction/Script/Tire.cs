using UnityEngine;
using System.Collections.Generic;

public class Tire : MonoBehaviour
{
    const float SphereRayCastRadius = 0.32f;
    const float SphereRaycastDis = 0.08f;
    const float TireFrictionCurveMaxValueRCP = 1 / 200.0f;

    Rigidbody m_Rigid;
    public Vector3 m_GroundNormal { get; private set; }
    public bool m_isGround { get; private set; }

    public float m_Rot { get; private set; }
    


    [SerializeField] float m_SpringStrength;
    [SerializeField] float m_SpringDamping;
    float m_SpringVelocity;
    [SerializeField] AnimationCurve m_FrictionCurve;
    [SerializeField] bool m_hasGizmo;
    [SerializeField] bool m_hasLog;
    Quaternion m_OriginQuat;
    private void Awake()
    {
        m_Rigid = GetComponent<Rigidbody>();

    }

    private void FixedUpdate()
    {
        m_Rigid.linearVelocity = Vector3.zero;
    }
    private void OnDrawGizmos()
    {
        if(m_hasGizmo==false)
        {
            return;
        }
        Vector3 rayStartPos = transform.position + transform.up * 0.5f;
        float rayDis = SphereRaycastDis + 0.5f;
        RaycastHit[] hits = Physics.SphereCastAll(rayStartPos, SphereRayCastRadius, -transform.up, rayDis, 1 << (int)Layer.Ground);
        if (hits.Length == 0)
        {
            
        }
        else
        {
            RaycastHit topHit;
            Vector3 tire2Hitpoint = hits[0].point - transform.position;
            float nearSqauredDis = tire2Hitpoint.sqrMagnitude;
            int nearIdx = 0;
            for (int i = 1; i < hits.Length; i++)
            {
                tire2Hitpoint = hits[1].point - transform.position;
                float disSquare = tire2Hitpoint.sqrMagnitude;
                if (nearSqauredDis < disSquare)
                {
                    nearIdx = i;
                    nearSqauredDis = disSquare;
                }
            }
            topHit = hits[nearIdx];
            Gizmos.DrawSphere(topHit.point, SphereRayCastRadius);
        }
        Gizmos.DrawLine(transform.position, transform.position - transform.up);
    }
    public void CheckState()
    {
        //Debug.Log(-transform.up);
        Vector3 rayStartPos = transform.position + transform.up * 0.5f;
        float rayDis = SphereRaycastDis + 0.5f;
        RaycastHit[] hits = Physics.SphereCastAll(rayStartPos, SphereRayCastRadius, -transform.up, rayDis, 1 << (int)Layer.Ground);
        if (hits.Length == 0)
        {
            m_isGround = false;
        }
        else
        {
            RaycastHit topHit;
            Vector3 tire2Hitpoint = hits[0].point - transform.position;
            float nearSqauredDis = tire2Hitpoint.sqrMagnitude;
            int nearIdx = 0;
            for (int i=1;i<hits.Length;i++)
            {
                tire2Hitpoint = hits[1].point - transform.position;
                float disSquare = tire2Hitpoint.sqrMagnitude;
                if(nearSqauredDis < disSquare)
                {
                    nearIdx = i;
                    nearSqauredDis = disSquare;
                }
            }
            topHit = hits[nearIdx];
            m_GroundNormal = topHit.normal;
            m_isGround = true;
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
            transform.position += new Vector3(0, -0.01f, 0);
            //땅과 충돌시 충돌한 만큼 바깥으로 이동
            //if(Physics.ComputePenetration(m_Col, transform.position, transform.rotation, m_GroundCol, m_GroundCol.transform.position, m_GroundCol.transform.rotation, out Vector3 direction, out float distance))
            //{
            //    transform.position += direction * distance;
            //}
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
