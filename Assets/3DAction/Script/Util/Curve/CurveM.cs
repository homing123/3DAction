using UnityEngine;
using System.Collections.Generic;
using HMCurve;
using Unity.Collections.LowLevel.Unsafe;

public class CurveM : MonoBehaviour
{
    static CurveM ins;
    public static CurveM Ins
    {
        get
        { 
            if(ins == null)
            {
                ins = FindFirstObjectByType<CurveM>();
            }
            return ins;
        }
    }

    [System.Serializable]
    class BezierCurveByKey
    {
        public string m_ID;
        public BezierCurveInfo m_CurveInfo;
        public BezierCurvePoint[] m_Points;
        public BezierCurveByKey(string id, BezierCurvePoint[] points)
        {
            m_ID = id;
            m_Points = points;
            m_CurveInfo = new BezierCurveInfo(m_Points);
        }
    }
    [SerializeField] Transform m_CurveObjParent;
    [SerializeField] List<BezierCurveByKey> m_Curves;
    [SerializeField] GameObject m_CurvePointPrefab;
    [SerializeField] string m_FindID;

    [SerializeField] GameObject[] m_EditPoints;
    [SerializeField] BezierCurveByKey m_EditCurve;



    public void CreateCurve()
    {
        if (m_FindID == null || string.IsNullOrEmpty(m_FindID) || string.IsNullOrWhiteSpace(m_FindID))
        {
            Debug.LogWarning($"Create Curve ID를 입력 필요");
            return;
        }

        if (FindBezierCurveByKey(m_FindID, out BezierCurveByKey tempCurveByKey))
        {
            Debug.LogWarning($"Create Curve 이미 존재하는 아이디 입니다. {m_FindID}");
            return;
        }

        BezierCurveByKey curveByKey = CreateDefaultCurve();
        if (curveByKey == null)
        {
            Debug.LogWarning("CreateCurve Fail");
            return;
        }
        m_Curves.Add(curveByKey);
        CreateEditObject(curveByKey);
    }
    public void AddCurvePoint()
    {

    }
    public void SaveEditCurve()
    {

    }
    public void UpdateCurEditCurve()
    {
        if(m_EditCurve == null)
        {
            return;
        }

        int pointCount = m_CurveObjParent.childCount / 3;
        BezierCurvePoint[] points = new BezierCurvePoint[pointCount];
        for (int i = 0; i < pointCount; i++)
        {
            Vector3 curPos = m_CurveObjParent.GetChild(i * 3).position;
            Vector3 prevPos = m_CurveObjParent.GetChild(i * 3 + 1).position;
            Vector3 nextPos = m_CurveObjParent.GetChild(i * 3 + 2).position;
            points[i] = new BezierCurvePoint(curPos, prevPos, nextPos);
        }

        Debug.Log("저장");
        m_EditCurve = new BezierCurveByKey(m_EditCurve.m_ID, points);
    }
    bool FindBezierCurveByKey(string id, out BezierCurveByKey curveByKey)
    {
        curveByKey = null;
        for (int i = 0; i < m_Curves.Count; i++)
        {
            if (m_Curves[i].m_ID == id)
            {
                curveByKey = m_Curves[i];
                return true;
            }
        }
        return false;
    }
    BezierCurveByKey CreateDefaultCurve()
    {
        string id = null;
        for(int i=0;i<1000;i++)
        {
            string curID = $"Curve_{i}";
            if (FindBezierCurveByKey(curID, out BezierCurveByKey tempCurveByKey) == false)
            {
                id = curID;
                break;
            }
        }

        if(id == null)
        {
            Debug.LogWarning("for문 숫자 늘리던가 아이디 바꿔야함");
            return null;
        }
        BezierCurvePoint[] points = new BezierCurvePoint[2];
        points[0] = new BezierCurvePoint(new Vector3(0, 0, 0), new Vector3(-1, 0, 0), new Vector3(1, 0, 0));
        points[1] = new BezierCurvePoint(new Vector3(1, 2, 0), new Vector3(0, 2, 0), new Vector3(2, 2, 0));
        BezierCurveByKey curveByKey = new BezierCurveByKey(id, points);
        return curveByKey;
    }
    void CreateEditObject(BezierCurveByKey curveByKey)
    {
        BezierCurvePoint[] points = curveByKey.m_Points;
        BezierCurveInfo info = curveByKey.m_CurveInfo;

        m_EditCurve = GoogleSheetReader.DeepCopyJson<BezierCurveByKey>(curveByKey);
        //이전 커브 오브젝트 제거
        int curChildCount = m_CurveObjParent.childCount;
        for (int i = curChildCount - 1; i >= 0; i--)
        {
            DestroyImmediate(m_CurveObjParent.GetChild(i).gameObject);
        }

        m_EditPoints = new GameObject[points.Length * 3];
        for (int i = 0; i < points.Length; i++)
        {
            GameObject posObj = Instantiate(m_CurvePointPrefab, m_CurveObjParent);
            GameObject prevObj = Instantiate(m_CurvePointPrefab, m_CurveObjParent);
            GameObject nextObj = Instantiate(m_CurvePointPrefab, m_CurveObjParent);
            posObj.name = $"{i} Pos";
            prevObj.name = $"{i} PrevPos";
            nextObj.name = $"{i} NextPos";
            m_EditPoints[i * 3] = posObj;
            m_EditPoints[i * 3 + 1] = prevObj;
            m_EditPoints[i * 3 + 2] = nextObj;

            posObj.transform.position = points[i].Pos;
            prevObj.transform.position = points[i].PrevPos;
            nextObj.transform.position = points[i].NextPos;
        }
    }

    private void OnDrawGizmos()
    {
        if (m_EditCurve == null)
        {
            return;
        }
        BezierCurvePoint[] points = m_EditCurve.m_Points;
        BezierCurveInfo info = m_EditCurve.m_CurveInfo;
        if(points.Length <= 0)
        {
            return;
        }
        for (int i = 0; i < points.Length - 1; i++)
        {
            Vector3 curPos = points[i].Pos;
            Vector3 prevPos = points[i].PrevPos;
            Vector3 nextPos = points[i].NextPos;

            Gizmos.DrawLine(curPos, prevPos);
            Gizmos.DrawLine(curPos, nextPos);
        }

        int lastIdx = points.Length - 1;
        Vector3 lastPointCurPos = points[lastIdx].Pos;
        Vector3 lastPointPrevPos = points[lastIdx].PrevPos;
        Vector3 lastPointNextPos = points[lastIdx].NextPos;
        Gizmos.DrawLine(lastPointCurPos, lastPointPrevPos);
        Gizmos.DrawLine(lastPointCurPos, lastPointNextPos);

        Vector3[] markers = info.GetMarkers();
        Gizmos.DrawLineStrip(markers, false);
        //0->1, 0->2, 0->3, 3->4, 3->5, 3->6
    }

}

namespace HMCurve
{
    [System.Serializable]
    public class BezierCurveInfo
    {
        public const int SplitCount = 32; //길이 계산을 위한 등분갯수

        [SerializeField] float m_Length;
        [SerializeField] Vector3[] m_PosSplitT; //가중치 T로 SplitCount갯수만큼 등분됐을때의 위치
        [SerializeField] float[] m_DisWeight;
        public BezierCurveInfo(BezierCurvePoint[] arr_Points)
        {
#if UNITY_EDITOR
            if (arr_Points == null)
            {
                throw new System.Exception($"BezierCurveInfo Create Error {arr_Points} is null");
            }
            if (arr_Points.Length <= 1)
            {
                throw new System.Exception($"BezierCurveInfo Create Error points count is {arr_Points.Length}");
            }
#endif

            float[] prefixDisPerT; //가중치 T로 SplitCount갯수만큼 등분됐을때 누적거리
            int segmentCount = arr_Points.Length - 1;
            int dataCount = SplitCount + (SplitCount - 1) * (segmentCount - 1);
            m_PosSplitT = new Vector3[dataCount];
            m_DisWeight = new float[dataCount];
            prefixDisPerT = new float[dataCount];

            for (int i = 0; i < dataCount - 1; i++)
            {
                int segmentIdx = i / (SplitCount - 1);
                float t = (i % (SplitCount - 1)) / (float)(SplitCount - 1);
                m_PosSplitT[i] = GetCubicBezierPos(arr_Points[segmentIdx].Pos, arr_Points[segmentIdx].NextPos, arr_Points[segmentIdx + 1].PrevPos, arr_Points[segmentIdx + 1].Pos, t);
            }
            m_PosSplitT[dataCount - 1] = arr_Points[arr_Points.Length - 1].Pos;

            float prefixDis = 0;
            for (int i = 0; i < dataCount - 1; i++)
            {
                prefixDisPerT[i] = prefixDis;
                float curDis = Vector3.Distance(m_PosSplitT[i], m_PosSplitT[i + 1]);
                prefixDis += curDis;
            }
            prefixDisPerT[dataCount - 1] = prefixDis;
            m_Length = prefixDis;

            for (int i = 0; i < dataCount; i++)
            {
                m_DisWeight[i] = prefixDisPerT[i] / m_Length;
            }
        }


        public static Vector3 GetCubicBezierPos(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, float t)
        {
            float a = 1.0f - t;
            float pos0Coeff = Mathf.Pow(a, 3);
            float pos1Coeff = 3 * Mathf.Pow(a, 2) * t;
            float pos2Coeff = 3 * Mathf.Pow(t, 2) * a;
            float pos3Coeff = Mathf.Pow(t, 3);
            return pos0Coeff * v0 + pos1Coeff * v1 + pos2Coeff * v2 + pos3Coeff * v3;
        }
        public Vector3 GetPos(int idx)
        {
            return m_PosSplitT[idx];
        }
        public Vector3 GetPosByDistanceWeight(float t)
        {
            t = Mathf.Clamp01(t);
            if (t == 0)
            {
                return m_PosSplitT[0];
            }
            if (t == 1)
            {
                return m_PosSplitT[m_PosSplitT.Length - 1];
            }
            float lerpValue = 0;
            int idx = 0;
            for (int i = 0; i < m_DisWeight.Length; i++)
            {
                if (t < m_DisWeight[i])
                {
                    lerpValue = (t - m_DisWeight[i - 1]) / (m_DisWeight[i] - m_DisWeight[i - 1]);
                    idx = i;
                    break;
                }
            }
            return Vector3.Lerp(m_PosSplitT[idx - 1], m_PosSplitT[idx], lerpValue);
        }
        public Vector3[] GetMarkers()
        {
            return m_PosSplitT;
        }

    }
    [System.Serializable]
    public class BezierCurvePoint
    {
        public Vector3 Pos;
        public Vector3 PrevPos;
        public Vector3 NextPos;
        public BezierCurvePoint(Vector3 pos, Vector3 prevPos, Vector3 nextPos)
        {
            Pos = pos;
            PrevPos = prevPos;
            NextPos = nextPos;
        }
    }
}