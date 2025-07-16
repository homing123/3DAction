using UnityEngine;
using System.Collections.Generic;
using HMCurve;
using System;

public class CurveTest : MonoBehaviour
{
    int m_Count;
    public BezierCurvePoint[] m_Points;

    [SerializeField] GameObject m_LookPointPrefab;
    List<GameObject> L_Obj = new List<GameObject>();

    [SerializeField] GameObject m_MoveObj;
    [SerializeField]
    [Range(0, 1)] float m_T;
    // Update is called once per frame
    void Update()
    {
        SetCountObj();
        SetPos();
    }
    void SetCountObj()
    {
        int segmentCount = m_Points.Length - 1;
        int dataCount = BezierCurveInfo.SplitCount + (BezierCurveInfo.SplitCount - 1) * (segmentCount - 1);
        m_Count = dataCount;
        if (L_Obj.Count > m_Count)
        {
            for(int i= m_Count; i< L_Obj.Count; i++)
            {
                DestroyImmediate(L_Obj[i]);
            }
            L_Obj.RemoveRange(m_Count, L_Obj.Count - m_Count);
        }
        else if(L_Obj.Count < m_Count)
        {
            for(int i=L_Obj.Count;i<m_Count;i++)
            {
                L_Obj.Add(Instantiate(m_LookPointPrefab));
            }
        }
    }
    void SetPos()
    {
        BezierCurveInfo info = new BezierCurveInfo(m_Points);
        for(int i=0;i<m_Count;i++)
        {
            L_Obj[i].transform.position = info.GetPos(i);
        }

        m_MoveObj.transform.position = info.GetPosByDistanceWeight(m_T);
        //float t = m_T * (m_Count - 1);
        //int floor = Mathf.FloorToInt(t); //내림
        //int ceil = Mathf.CeilToInt(t); //올림
        //if(floor == ceil)
        //{
        //    m_MoveObj.transform.position = info.GetPos(floor);
        //}
        //else
        //{
        //    Vector3 floorPos = info.GetPos(floor);
        //    Vector3 ceilPos = info.GetPos(ceil);
        //    float frac = t - floor;
        //    Vector3 curPos = Vector3.Lerp(floorPos, ceilPos, frac);
        //    m_MoveObj.transform.position = curPos;
        //}
    }

}
