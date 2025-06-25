using System.Drawing;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

public class VFX : MonoBehaviour
{
    public static void Create(VFX vfx, in HitRangeInfo info)
    {
        VFX _vfx = Instantiate(vfx.gameObject).GetComponent<VFX>();
        _vfx.Init(in info);
    }

    [SerializeField] Transform m_OffsetQuad;

    Vector3 m_RotYZeroPosOffset;
    private void Awake()
    {
        m_OffsetQuad.gameObject.SetActive(false);
        m_RotYZeroPosOffset = -Vt3RotY(m_OffsetQuad.transform.localPosition, -m_OffsetQuad.eulerAngles.y);
    }
    void Init(in HitRangeInfo info)
    {
        switch(info.type)
        {
            case HITRANGE_TYPE.BOX:
                {
                    Vector3 curPosOffset = Vt3RotY(m_RotYZeroPosOffset, info.boxRotY);
                    transform.position = info.pos + curPosOffset;
                    transform.rotation = Quaternion.Euler(0, info.boxRotY - m_OffsetQuad.eulerAngles.y, 0);
                    transform.localScale = new Vector3(info.boxSize.x / m_OffsetQuad.localScale.x, 1 / m_OffsetQuad.localScale.y, info.boxSize.y / m_OffsetQuad.localScale.z);
                }
                break;
            case HITRANGE_TYPE.SPHERE:
                break;
        }
     
    }

    Vector3 Vt3RotY(Vector3 vt3, float roty)
    {
        float cos = Mathf.Cos(roty * Mathf.Deg2Rad);
        float sin = Mathf.Sin(roty * Mathf.Deg2Rad);

        Vector3 result = new Vector3(cos * vt3.x + sin * vt3.z, vt3.y, -sin * vt3.x + cos * vt3.z);
        return result;
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}
