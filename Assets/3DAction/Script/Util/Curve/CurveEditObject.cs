using UnityEngine;

public class CurveEditObject : MonoBehaviour
{

    public int m_Idx;
    public Transform m_Prev;
    public Transform m_Next;
    private void Start()
    {
        gameObject.SetActive(false);
    }
}
