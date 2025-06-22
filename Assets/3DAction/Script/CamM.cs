using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class CamM : MonoBehaviour
{
    [SerializeField] float m_CamDis;
    const string MouseY = "Mouse Y";
    const string MouseX = "Mouse X";
    public static CamM Ins;

    Transform m_Target;

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
        m_Target = PlayerMove.Ins.transform;
    }

    // Update is called once per frame

    private void LateUpdate()
    {
        LookPlayer();
    }

    void LookPlayer()
    {
        Vector3 targetPos = m_Target.transform.position;
        Vector3 camForward = transform.forward;
        transform.position = targetPos - camForward * m_CamDis;
    }

#if UNITY_EDITOR
    [ContextMenu("LookPlayer")]
    public void SetLookPlayer()
    {
        m_Target = FindFirstObjectByType<PlayerMove>().transform;
        if(m_Target == null)
        {
            Debug.LogWarning("플레이어 없음.");
        }
        else
        {
            LookPlayer();
        }
    }
#endif
    void SetThirdCam(float focusHeight, float dis, float height)
    {
      
    }

  

}
