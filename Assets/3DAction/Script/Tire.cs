using UnityEngine;
using System.Collections.Generic;


//현재 진행방향과 노말의 방향이 90도 보다 작을경우 오른쪽 위로 진행중에 수평땅이 오는경우 
//한쪽바퀴만 공중에 떠있는 경우
//양쪽바퀴다 공중에 떠있는 경우
//롤러코스터 회전하듯이 360도 빠르게달려서 회전
//자체회전
//드드드드드드득 왓따갔다 버그걸릴때
public class Tire : MonoBehaviour
{
    const float SphereRayCastRadius = 0.32f;
    const float SphereRaycastDis = 0.08f;
    const float TireFrictionCurveMaxValueRCP = 1 / 200.0f;

    Collider m_Col;
    Collider m_GroundCol;
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
    Vector3 m_LastMoveDir;
    private void Awake()
    {
        m_Rigid = GetComponent<Rigidbody>();
        m_Col = GetComponent<Collider>();
    }

    private void FixedUpdate()
    {
        //m_Rigid.linearVelocity = Vector3.zero;
    }
    private void OnDrawGizmos()
    {
        if (m_hasGizmo == false)
        {
            return;
        }
        if (GetGroundHit(out RaycastHit hit))
        {
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(hit.point, SphereRayCastRadius);
            BoxCollider box = hit.collider as BoxCollider;
            Vector3 normal = Vector3.zero;
            if (box != null)
            {
                normal = Util.NotInterpolationNormalBox(hit.point, box);
            }
            else
            {
                normal = hit.normal;
            }
            Gizmos.color = Color.orange;
            Gizmos.DrawLine(hit.point, hit.point + normal);
        }

        //Gizmos.color = Color.black;
        //Gizmos.DrawLine(transform.position, transform.position - transform.up); 
        //Gizmos.color = Color.yellow;
        //Gizmos.DrawLine(transform.position, transform.position + m_GroundNormal);
        //Gizmos.color = Color.blue;
        //Gizmos.DrawLine(transform.position, transform.position - m_LastMoveDir);
    }

    bool GetGroundHit(out RaycastHit hit)
    {
        Vector3 rayStartPos = transform.position + transform.up * 0.5f;
        float rayDis = SphereRaycastDis + 0.5f;
        RaycastHit[] hits = Physics.SphereCastAll(rayStartPos, SphereRayCastRadius, -transform.up, rayDis, 1 << (int)Layer.Ground);
        hit = default;
        if (hits.Length == 0)
        {
            return false;
        }
        else
        {
            RaycastHit topHit;
            Vector3 tire2Hitpoint = hits[0].point - transform.position;
            float nearSqauredDis = tire2Hitpoint.sqrMagnitude;
            int nearIdx = 0;

            for (int i = 1; i < hits.Length; i++)
            {
                tire2Hitpoint = hits[i].point - transform.position;
                float disSquare = tire2Hitpoint.sqrMagnitude;
                if (nearSqauredDis > disSquare)
                {
                    nearIdx = i;
                    nearSqauredDis = disSquare;
                }
            }
            topHit = hits[nearIdx];
            hit = topHit;
        }
        return true;
    }
    public bool CheckGround()
    {
        if(GetGroundHit(out RaycastHit hit))
        {
            BoxCollider box = hit.collider as BoxCollider;
            Vector3 normal = Vector3.zero;
            if (box != null)
            {
                normal = Util.NotInterpolationNormalBox(hit.point, box);
            }
            else
            {
                normal = hit.normal;
            }
            if (m_LastMoveDir != Vector3.zero)
            {
                float cos = Vector3.Dot(normal, m_LastMoveDir);
                float angle = Mathf.Acos(cos) * Mathf.Rad2Deg;
                if (angle > 85)
                {
                    m_GroundNormal = normal;
                    m_isGround = true;
                    m_GroundCol = hit.collider;
                    return true;
                }
                else
                {
                    if (m_hasLog)
                    {
                        Debug.Log(hit.transform.name + " " + normal + " " + angle);
                    }
                }
            }
            else
            {
                m_GroundNormal = normal;
                m_isGround = true;
                m_GroundCol = hit.collider;
                return true;
            }
        }
        m_isGround = false;
        return false;
    }

    public Vector3 GroundMove(Vector3 bodyRight, float speed)
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
            //transform.position += new Vector3(0, -0.01f, 0);
            //땅과 충돌시 충돌한 만큼 바깥으로 이동
            transform.position += moveVector;
            if (Physics.ComputePenetration(m_Col, transform.position, transform.rotation, m_GroundCol, m_GroundCol.transform.position, m_GroundCol.transform.rotation, out Vector3 direction, out float distance))
            {
                transform.position += direction * (distance - 0.01f);
                moveVector += direction * (distance - 0.01f);
            }
        }
        m_LastMoveDir = moveVector.normalized;

        return moveVector;
    }
    public void AirMove(Vector3 velocity)
    {
        Vector3 moveVector = Vector3.zero;
        moveVector += velocity * Time.fixedDeltaTime;
        transform.position += moveVector;
        if (GetGroundHit(out RaycastHit hit))
        {
            if (Physics.ComputePenetration(m_Col, transform.position, transform.rotation, hit.collider, hit.transform.position, hit.transform.rotation, out Vector3 direction, out float distance))
            {
                transform.position += direction * (distance - 0.01f);
                moveVector += direction * (distance - 0.01f);
            }
        }
        m_LastMoveDir = moveVector.normalized;
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
