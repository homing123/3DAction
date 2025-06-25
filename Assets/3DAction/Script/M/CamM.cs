using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class CamM : MonoBehaviour
{
    [SerializeField] float m_CamDis;
    const string MouseY = "Mouse Y";
    const string MouseX = "Mouse X";
    public static CamM Ins;

    [SerializeField] Transform m_Target;

    private void Awake()
    {
        if(Ins != null && Ins != this)
        {
            Destroy(Ins.gameObject);
        }
        Ins = this;
    }

    void Start()
    {

    }

    // Update is called once per frame

    private void LateUpdate()
    {
        SetHeight();
    }

  
    void SetHeight()
    {
        if(m_Target)
        {
            transform.position = m_Target.transform.position - transform.forward * m_CamDis;
        }
        else if(Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, m_CamDis + 5, Define.D_LayerMask[Layer.Ground]))
        {
            transform.position = hit.point - transform.forward * m_CamDis;
        }
    }

    public void RegisterTarget(Transform target)
    {
        m_Target = target;
    }
    public void ReleaseTarget()
    {
        m_Target = null;
    }
  
#if UNITY_EDITOR
    [ContextMenu("LookPlayer")]
    public void SetLookPlayer()
    {
        m_Target = FindFirstObjectByType<Character>().transform;
        if(m_Target == null)
        {
            Debug.LogWarning("플레이어 없음.");
        }
        else 
        {
            RegisterTarget(m_Target);
            SetHeight();
        }
    }
#endif

  

}
