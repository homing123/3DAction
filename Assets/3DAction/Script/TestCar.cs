using UnityEngine;

public class TestCar : MonoBehaviour
{
    [SerializeField] Transform[] m_Tire;
    float[] m_Offset;
    float[] m_SpringVelocity;

    [SerializeField] float m_SpringStrength;
    [SerializeField] float m_SpringDamping;

    [SerializeField][Range(0,1)] float m_TireFriction;
    
    private void Awake()
    {
        m_Offset = new float[m_Tire.Length];
        m_SpringVelocity = new float[m_Tire.Length];
        for(int i=0;i< m_Tire.Length;i++)
        {
            m_Offset[i] = m_Tire[i].transform.localPosition.y;
        }
    }
    private void Start()
    {
        
    }
    private void Update()
    {
        
    }
    private void FixedUpdate()
    {
        Vector3 up = transform.up;
        for(int i=0;i<m_Tire.Length;i++)
        {
            float curHeight = Vector3.Dot(m_Tire[i].transform.localPosition, up);
            float disFromOffset = curHeight - m_Offset[i];
            m_SpringVelocity[i] += m_SpringStrength * -disFromOffset;
            m_SpringVelocity[i] -= m_SpringVelocity[i] * m_SpringDamping * Time.fixedDeltaTime;
            m_Tire[i].localPosition = m_Tire[i].localPosition + up * m_SpringVelocity[i] * Time.fixedDeltaTime;
        }
    }
}
