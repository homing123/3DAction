using UnityEngine;

public class TestCar : MonoBehaviour
{
    [SerializeField] Tire[] m_Tire;
    private void Start()
    {
        
    }
    private void Update()
    {
        
    }
    private void FixedUpdate()
    {
        //제일 높은바퀴 찾고
        //해당바퀴 대각바퀴랑 
        for(int i=0;i<m_Tire.Length;i++)
        {

        }
    }
}
