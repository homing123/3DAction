using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class CamM : MonoBehaviour
{
    [SerializeField] float m_CamDis;
    const string MouseY = "Mouse Y";
    const string MouseX = "Mouse X";
    public static CamM Ins;

    [SerializeField] float m_CamMoveSpeed = 0.15f;
    [SerializeField] Transform m_Target;
    [SerializeField] float m_CamHeightMoveSpeed = 2;
    public Vector2 m_CamScreenRight { get; private set; }
    public Vector2 m_CamScreenForward { get; private set; }
    [SerializeField] bool m_MoveCam = true;

    float m_CurCamHeightDis;
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
        Vector3 xMove = Vector3.Cross(Vector3.up, transform.forward).normalized;
        Vector3 zMove = Vector3.Cross(xMove, Vector3.up).normalized;
        m_CamScreenRight = xMove.VT2XZ();
        m_CamScreenForward = zMove.VT2XZ();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            m_MoveCam = !m_MoveCam;
        }
    }

    // Update is called once per frame

    private void LateUpdate()
    {
        SetHeight();
    }

  
    void SetHeight()
    {
        if(Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, m_CamDis + 5, Define.D_LayerMask[Layer.Ground]))
        {
            float curCam2GroundDis = Vector3.Distance(hit.point, transform.position);
            float curMoveSpeed = m_CamHeightMoveSpeed * Time.deltaTime;
            float moveSign = Mathf.Sign(m_CamDis - curCam2GroundDis);
            float abs = Mathf.Abs(m_CamDis - curCam2GroundDis);
            float curCamDis = 0;

            if (abs < curMoveSpeed)
            {
                curCamDis = m_CamDis;
            }
            else
            {
                curCamDis = curCam2GroundDis + moveSign * curMoveSpeed;
            }
            transform.position = hit.point - transform.forward * curCamDis;
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
  
    public void CamMove(Vector2 dir)
    {
        if (m_MoveCam == false)
        {
            return;
        }
        Vector2 moveDis = m_CamScreenRight * dir.x + m_CamScreenForward * dir.y;
        transform.position += moveDis.VT2XZToVT3() * m_CamMoveSpeed;
    }
    public void CamLookTarget(Transform target)
    {
        if (m_MoveCam == false)
        {
            return;
        }
        transform.position = target.transform.position - transform.forward * m_CamDis;
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
