using UnityEngine;
using System.Collections.Generic;
using HMCurve;
using System;

public class CurveTest : MonoBehaviour
{
    [SerializeField] string m_CurveKey;
    [SerializeField] Transform m_MoveObj;
    [SerializeField] float m_Speed;

    BezierCurveInfo m_CurveInfo;
    float m_MoveDis;
    public void Start()
    {
        m_CurveInfo = CurveM.Ins.GetCurve(m_CurveKey);
    }
    private void Update()
    {
        float curSpeed = m_Speed * Time.deltaTime;
        m_MoveDis += curSpeed;
        m_MoveObj.transform.position = m_CurveInfo.GetPosByDistance(m_MoveDis);
    }

}
