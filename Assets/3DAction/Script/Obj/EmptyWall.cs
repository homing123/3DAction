using UnityEngine;

public class EmptyWall : MonoBehaviour
{
    [SerializeField] MeshRenderer m_MeshRednerer;
    void Start()
    {
        m_MeshRednerer.enabled = false;
    }
}
