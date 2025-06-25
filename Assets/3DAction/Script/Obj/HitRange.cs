using System.Drawing;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public enum HITRANGE_TYPE
{
    BOX,
    SPHERE
}

public struct HitRangeInfo
{
    public HITRANGE_TYPE type;
    public Vector3 pos;
    public Vector2 boxSize;
    public float boxRotY;
    public float sphereRadius;

    public static HitRangeInfo Box(Vector3 _pos, Vector2 _size, float roty)
    {
        HitRangeInfo info = new HitRangeInfo();
        info.type = HITRANGE_TYPE.BOX;
        info.pos = _pos;
        info.boxSize = _size;
        info.boxRotY = roty;
        return info;
    }

    /// <summary>
    /// 방향과 거리에 맞춰 위치설정, 각도도 방향에 맞춰 세팅됨
    /// </summary>
    /// <param name="dir"></param>
    /// <param name="dis"></param>
    /// <param name="_size"></param>
    /// <param name="addRotY">방향각도에 더할 값</param>
    /// <returns></returns>
    public static HitRangeInfo Box(Vector3 curPos, Vector3 dir, float dis, Vector2 _size, float addRotY = 0)
    {
        HitRangeInfo info = new HitRangeInfo();
        info.type = HITRANGE_TYPE.BOX;
        info.pos = curPos + dir * dis;
        info.boxSize = _size;
        info.boxRotY = Util.Vt2ToYRotDeg(dir.VT2XZ()) + addRotY;
        return info;
    }


    public static HitRangeInfo Sphere(Vector3 _pos, float radius)
    {
        HitRangeInfo info = new HitRangeInfo();
        info.type = HITRANGE_TYPE.SPHERE;
        info.pos = _pos;
        info.sphereRadius = radius;
        return info;
    }
    public static HitRangeInfo Sphere(Vector3 curPos, Vector3 dir, float dis, float radius)
    {
        HitRangeInfo info = new HitRangeInfo();
        info.type = HITRANGE_TYPE.SPHERE;
        info.pos = curPos + dir * dis;
        info.sphereRadius = radius;
        return info;
    }
}
public class HitRange : MonoBehaviour
{
    public const bool CreateHitRange = true;

    public static void Create(in HitRangeInfo info)
    {
        switch(info.type)
        {
            case HITRANGE_TYPE.BOX:
                {
                    HitRange hitRange = Instantiate(ResM.Ins.HitRange_Box).GetComponent<HitRange>();
                    hitRange.transform.position = info.pos;
                    hitRange.transform.localScale = new Vector3(info.boxSize.x, hitRange.transform.localScale.y, info.boxSize.y);
                    hitRange.transform.rotation = Quaternion.Euler(0, info.boxRotY, 0);
                }
                break;
            case HITRANGE_TYPE.SPHERE:
                {
                    HitRange hitRange = Instantiate(ResM.Ins.HitRange_Sphere).GetComponent<HitRange>();
                    hitRange.transform.position = info.pos;
                    hitRange.transform.localScale = new Vector3(info.sphereRadius * 2, hitRange.transform.localScale.y, info.sphereRadius * 2);
                }
                break;
        }
      
    }

    [SerializeField] float m_LifeTime = 0.5f;
    float m_CurTime;
    private void Start()
    {
        m_CurTime = m_LifeTime;
    }
    private void Update()
    {
        m_CurTime -= Time.deltaTime;
        if(m_CurTime < 0)
        {
            Destroy(this.gameObject);
        }
    }




    public static Collider[] Overlap(in HitRangeInfo info, out int count)
    {
        Vector3 boxHalfSize = new Vector3(info.boxSize.x, 1, info.boxSize.y) * 0.5f;
        Collider[] col = new Collider[10];
        count = 0;
        switch (info.type)
        {
            case HITRANGE_TYPE.BOX:
                count = Physics.OverlapBoxNonAlloc(info.pos, boxHalfSize, col, Quaternion.Euler(0, info.boxRotY, 0));
                if (HitRange.CreateHitRange)
                {
                    HitRange.Create(info);
                }
                break;
            case HITRANGE_TYPE.SPHERE:
                count = Physics.OverlapSphereNonAlloc(info.pos, info.sphereRadius, col);
                if (HitRange.CreateHitRange)
                {
                    HitRange.Create(info);
                }
                break;
        }
       
        return col;
    }
}
