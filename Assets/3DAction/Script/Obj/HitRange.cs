using UnityEngine;

public enum HitRangeType
{
    Quad,
    Circle
}
public enum HitRangePosType
{
    Pos,
    FollowTarget
}

public struct HitRangeInfo
{
    public HitRangeType type;
    public float time;
    public HitRangePosType posType;
    public Transform target;
    public Vector3 pos;
    public Vector2 quadSize;
    public float quadRotY;
    public float circleRadius;

    public void SetQuadPos(Vector3 _pos, Vector2 size, float roty, float _time)
    {
        type = HitRangeType.Quad;
        posType = HitRangePosType.Pos;
        pos = _pos;
        quadSize = size;
        quadRotY = roty;
        time = _time;
    }
    public void SetQuadFollowTarget(Transform _target, Vector2 size, float roty, float _time)
    {
        type = HitRangeType.Quad;
        posType = HitRangePosType.FollowTarget;
        target = _target;
        quadSize = size;
        quadRotY = roty;
        time = _time;
    }

    public void SetCirclePos(Vector3 _pos, float radius, float _time)
    {
        type = HitRangeType.Circle;
        posType = HitRangePosType.Pos;
        pos = _pos;
        circleRadius = radius;
        time = _time;
    }
    public void SetCircleFollowTarget(Transform _target, float radius, float _time)
    {
        type = HitRangeType.Circle;
        posType = HitRangePosType.FollowTarget;
        target = _target;
        circleRadius = radius;
        time = _time;
    }
   
}
public class HitRange : MonoBehaviour
{
    public const bool CreateHitRange = true;

    public static HitRange Create(in HitRangeInfo info)
    {
        switch (info.type)
        {
            case HitRangeType.Quad:
                {
                    HitRange hitRange = Instantiate(ResM.Ins.HitRange_Quad).GetComponent<HitRange>();
                    hitRange.transform.position = info.pos;
                    hitRange.transform.localScale = new Vector3(info.quadSize.x, hitRange.transform.localScale.y, info.quadSize.y);
                    hitRange.transform.rotation = Quaternion.Euler(0, info.quadRotY, 0);
                    hitRange.m_Info = info;
                    return hitRange;
                }
            case HitRangeType.Circle:
                {
                    HitRange hitRange = Instantiate(ResM.Ins.HitRange_Circle).GetComponent<HitRange>();
                    hitRange.transform.position = info.pos;
                    hitRange.transform.localScale = new Vector3(info.circleRadius * 2, info.circleRadius * 2, info.circleRadius * 2);
                    hitRange.m_Info = info;
                    return hitRange;
                }
        }
        return null;
    }
    HitRangeInfo m_Info;
    float m_CurTime;
    MaterialPropertyBlock m_MPB;
    [SerializeField] MeshRenderer m_MeshRenderer;

    const string PropertyColor = "_Color";
    const string PropertyProgress = "_Progress";
    private void Start()
    {
        SetPos();
        m_MPB = new MaterialPropertyBlock();
        m_MPB.SetColor(PropertyColor, Color.red);
        m_MPB.SetFloat(PropertyProgress, 0);
        m_MeshRenderer.SetPropertyBlock(m_MPB);
    }
    private void Update()
    {
        SetPos();
        m_CurTime += Time.deltaTime;
        m_MPB.SetFloat(PropertyProgress, m_CurTime / m_Info.time);
        if (m_CurTime >= m_Info.time)
        {
            m_MPB.SetFloat(PropertyProgress, 1);
            Destroy(this.gameObject);
        }
        m_MeshRenderer.SetPropertyBlock(m_MPB);

    }
    void SetPos()
    {
        if(m_Info.posType == HitRangePosType.FollowTarget)
        {
            transform.position = m_Info.target.position;
        }
    }
    public void Destroy()
    {
        Destroy(this.gameObject);
    }

    public static Collider[] Overlap(in HitRangeInfo info, out int count)
    {
        Vector3 QuadHalfSize = new Vector3(info.quadSize.x, 1, info.quadSize.y) * 0.5f;
        Collider[] col = new Collider[10];
        count = 0;
        Vector3 pos = info.posType == HitRangePosType.Pos ? info.pos : info.target.transform.position;
        int layerMask = Define.D_LayerMask[Layer.Character] | Define.D_LayerMask[Layer.Monster];
        switch (info.type)
        {
            case HitRangeType.Quad:
                count = Physics.OverlapBoxNonAlloc(pos, QuadHalfSize, col, Quaternion.Euler(0, info.quadRotY, 0), layerMask);
                break;
            case HitRangeType.Circle:
                count = Physics.OverlapSphereNonAlloc(pos, info.circleRadius, col, layerMask);
                break;
        }

        return col;
    }
}
