using UnityEngine;

public class SpringTest : MonoBehaviour
{
    [SerializeField] Transform m_child;
    [SerializeField] float springStrength;
    [SerializeField] float springDamping;
    float springVelocity;
    float springOffset;
    private void Start()
    {
        springOffset = m_child.localPosition.y;
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        float moveFromOffset = m_child.localPosition.y - springOffset;
        springVelocity += springStrength * -moveFromOffset;
        springVelocity = springVelocity - springVelocity * springDamping * Time.fixedDeltaTime;
        m_child.localPosition = m_child.localPosition + new Vector3(0, springVelocity * Time.fixedDeltaTime, 0);

    }
}
