using UnityEngine;
using System.Collections.Generic;
using HMCurve;
using System;

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
    public class BezierCurveByKey
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
    [SerializeField] List<BezierCurveByKey> m_Curves;
    [SerializeField] GameObject m_CurvePointPrefab;
    [SerializeField] string m_FindID;

    [SerializeField] public BezierCurveByKey m_EditCurve;


    Dictionary<string, BezierCurveByKey> D_BezierCurveKey = new Dictionary<string, BezierCurveByKey>();


    private void Awake()
    {
        for(int i=0;i<m_Curves.Count;i++)
        {
            D_BezierCurveKey[m_Curves[i].m_ID] = m_Curves[i];
        }    
    }
    public BezierCurveInfo GetCurve(string id)
    {
        if(D_BezierCurveKey.ContainsKey(id))
        {
            return D_BezierCurveKey[id].m_CurveInfo;
        }
        else
        {
            Debug.Log($"GetCurve {id} is null");
            return null;
        }
    }
    public void CreateNewEditCurve()
    {
        BezierCurveByKey curveByKey = CreateDefaultCurve();
        if (curveByKey == null)
        {
            Debug.LogWarning("CreateCurve Fail");
            return;
        }
        SetEditCurve(curveByKey);
    }
    public void CurveToEdit()
    {
        if (m_FindID == null || string.IsNullOrEmpty(m_FindID) || string.IsNullOrWhiteSpace(m_FindID))
        {
            Debug.LogWarning($"FindID �� �Է� �ʿ�");
            return;
        }

        if (FindBezierCurveByKey(m_FindID, out BezierCurveByKey tempCurveByKey))
        {
            BezierCurveByKey copyCurveByKey = GetDeepCopyCurveByKey(tempCurveByKey);
            SetEditCurve(copyCurveByKey);
        }
        else
        {
            Debug.LogWarning($"{m_FindID} id �� ���� Ŀ�갡 �����ϴ�.");
            return;
        }
    }
    public void SaveEditCurve()
    {
        if (m_EditCurve == null)
        {
            return;
        }
        if (FindBezierCurveByKey(m_EditCurve.m_ID, out BezierCurveByKey curveByKey))
        {
            int idx = m_Curves.IndexOf(curveByKey);
            m_Curves[idx] = GoogleSheetReader.DeepCopyJson<BezierCurveByKey>(m_EditCurve);
        }
        else
        {
            m_Curves.Add(GoogleSheetReader.DeepCopyJson<BezierCurveByKey>(m_EditCurve));
        }
    }
    public void AddEditCurvePoint()
    {
        if (m_EditCurve == null || m_EditCurve.m_Points.Length < 2)
        {
            return;
        }
        int pointCount = m_EditCurve.m_Points.Length;

        BezierCurvePoint pointa = m_EditCurve.m_Points[pointCount - 2];
        BezierCurvePoint pointb = m_EditCurve.m_Points[pointCount - 1];

        Vector3 a2bDir = (pointb.Pos - pointa.Pos).normalized;
        Vector3 addPointPos = pointb.Pos + a2bDir * 2;
        Vector3 addPointToPrev = pointb.ToPrev;
        float addPointPrevDis = 1;
        float addPointNextDis = 1;
        Instantiate(m_CurvePointPrefab, transform);
        BezierCurvePoint[] newPoints = new BezierCurvePoint[pointCount + 1];
        for (int i = 0; i < pointCount; i++)
        {
            Vector3 curPos= m_EditCurve.m_Points[i].Pos;
            Vector3 toPrev = m_EditCurve.m_Points[i].ToPrev;
            float prevDis = m_EditCurve.m_Points[i].PrevDis;
            float nextDis = m_EditCurve.m_Points[i].NextDis;
            newPoints[i] = new BezierCurvePoint(curPos, toPrev, prevDis, nextDis);
        }
        newPoints[pointCount] = new BezierCurvePoint(addPointPos, addPointToPrev, addPointPrevDis, addPointNextDis);
        m_EditCurve = new BezierCurveByKey(m_EditCurve.m_ID, newPoints);
        SceneCurveEditObjectToPoints(m_EditCurve.m_Points);
    }
 
    /// <summary>
    /// m_EditCurve�� ������ ���� CurveEditObjects���� ������ �ٸ���� CurveEditObjects -> m_EditCurve�� ������Ʈ ���ִ� �Լ�
    /// �̶� prev�� next�� 1�ڰ�ΰ� �ƴ� ��� ó�� �ʿ�
    /// prev�� ������ ��� prev ����
    /// next�� ������ ��� next ����
    /// �Ѵ� ������ ��� prev ����
    /// </summary>
    public void CheckUpdateEditCurve()
    {

        if (m_EditCurve == null)
        {
            return;
        }
        CurveEditObject[] sceneCurveEditObjects = GetSceneCurveEditObjects();

        if (sceneCurveEditObjects.Length <= 1)
        {
            return;
        }

        bool isDiff = false;
        if (sceneCurveEditObjects.Length != m_EditCurve.m_Points.Length)
        {
            isDiff = true;
        }

        for (int i = 0; i < sceneCurveEditObjects.Length; i++)
        {
            if(m_EditCurve.m_Points.Length <= i) 
            {
                break;
            }
            Vector3 pointPos = m_EditCurve.m_Points[i].Pos;
            Vector3 prevPos = pointPos + m_EditCurve.m_Points[i].ToPrev * m_EditCurve.m_Points[i].PrevDis;
            Vector3 nextPos = pointPos - m_EditCurve.m_Points[i].ToPrev * m_EditCurve.m_Points[i].NextDis;
            Vector3 scenePointPos = sceneCurveEditObjects[i].transform.position;
            Vector3 scenePrevPos = sceneCurveEditObjects[i].m_Prev.position;
            Vector3 sceneNextPos = sceneCurveEditObjects[i].m_Next.position;
            Vector3 sceneToPrev = (scenePrevPos - scenePointPos).normalized;
            //Debug.Log(pointPos + " " + prevPos + " " + nextPos + " " + scenePointPos + " " + scenePrevPos + " " + sceneNextPos);
            float scenePrevDis = Vector3.Distance(scenePointPos, scenePrevPos);
            float sceneNextDis = Vector3.Distance(scenePointPos, sceneNextPos);
            bool isPointDiff = scenePointPos != pointPos;
            bool isPrevDiff = scenePrevPos != prevPos;
            bool isNextDiff = sceneNextPos != nextPos;
            //prev�� �ٲ�ų�, prev�� next ��� �ٲ��� prev�������� next �缼��
            if (isPrevDiff == true)
            {
                float nextDis = m_EditCurve.m_Points[i].NextDis;
                sceneNextPos = scenePointPos - sceneToPrev * nextDis;
            }
            else if(isNextDiff == true)
            {
                //next�� �ٲ��� next �������� prev �缼��
                sceneToPrev = (scenePointPos - sceneNextPos).normalized;
                float prevDis = m_EditCurve.m_Points[i].PrevDis;
                scenePrevPos = scenePointPos + sceneToPrev * prevDis;
            }

            isDiff |= isPointDiff | isPrevDiff | isNextDiff;

            if (isPointDiff | isPrevDiff | isNextDiff)
            {
                sceneCurveEditObjects[i].transform.position = scenePointPos;
                sceneCurveEditObjects[i].m_Prev.position = scenePrevPos;
                sceneCurveEditObjects[i].m_Next.position = sceneNextPos;
            }
        }

        if(isDiff)
        {
            m_EditCurve = new BezierCurveByKey(m_EditCurve.m_ID, GetPointsBySceneCurveEditObjects());
        }    
    }
    BezierCurvePoint[] GetPointsBySceneCurveEditObjects()
    {
        CurveEditObject[] sceneCurveEditObjects = GetSceneCurveEditObjects();
        int pointCount = sceneCurveEditObjects.Length;
        BezierCurvePoint[] points = new BezierCurvePoint[pointCount];
        for (int i = 0; i < pointCount; i++)
        {
            CurveEditObject curPoint = sceneCurveEditObjects[i];
            Vector3 pointPos = curPoint.transform.position;
            Vector3 prevPos = curPoint.m_Prev.transform.position;
            Vector3 nextPos = curPoint.m_Next.transform.position;
            Vector3 toPrev = (prevPos - pointPos).normalized;
            float prevDis = Vector3.Distance(prevPos, pointPos);
            float nextDis = Vector3.Distance(nextPos, pointPos);
            points[i] = new BezierCurvePoint(pointPos, toPrev,prevDis, nextDis);
        }
        return points;
    }
    void SetSceneCurveEditObjectsCount(int count)
    {
        CurveEditObject[] sceneCurveEditObjects = GetSceneCurveEditObjects();
        int pointCount = count;
        if (pointCount > sceneCurveEditObjects.Length)
        {
            //�߰��ʿ�     
            for (int i = sceneCurveEditObjects.Length; i < pointCount; i++)
            {
                Instantiate(m_CurvePointPrefab, transform);
            }
        }
        else if (pointCount < sceneCurveEditObjects.Length)
        {
            //�����ʿ�
            for (int i = pointCount; i < sceneCurveEditObjects.Length; i++)
            {
                DestroyImmediate(sceneCurveEditObjects[i].gameObject);
            }
        }
    }
    void SceneCurveEditObjectToPoints(BezierCurvePoint[] points)
    {
        SetSceneCurveEditObjectsCount(points.Length);
        CurveEditObject[] sceneCurveEditObjects = GetSceneCurveEditObjects();
        for (int i = 0; i < points.Length; i++)
        {
            Vector3 pointPos = points[i].Pos;
            Vector3 prevPos = pointPos + points[i].ToPrev * points[i].PrevDis;
            Vector3 nextPos = pointPos - points[i].ToPrev * points[i].NextDis;
            sceneCurveEditObjects[i].transform.position = pointPos;
            sceneCurveEditObjects[i].m_Prev.transform.position = prevPos;
            sceneCurveEditObjects[i].m_Next.transform.position = nextPos;
            sceneCurveEditObjects[i].m_Idx = i;
        }
    }
    CurveEditObject[] GetSceneCurveEditObjects()
    {
        int childCount = transform.childCount;
        CurveEditObject[] curveEditObjects = new CurveEditObject[childCount];
        for (int i = 0; i < childCount; i++)
        {
            CurveEditObject curPointEditObject = transform.GetChild(i).GetComponent<CurveEditObject>();

            curveEditObjects[i] = curPointEditObject;
        }
        return curveEditObjects;
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
            Debug.LogWarning("for�� ���� �ø����� ���̵� �ٲ����");
            return null;
        }
        BezierCurvePoint[] points = new BezierCurvePoint[2];
        points[0] = new BezierCurvePoint(new Vector3(0, 0, 0), new Vector3(-1, 0, 0), 1, 1);
        points[1] = new BezierCurvePoint(new Vector3(1, 2, 0), new Vector3(-1, 0, 0), 1, 1);
        BezierCurveByKey curveByKey = new BezierCurveByKey(id, points);
        return curveByKey;
    }
    BezierCurveByKey GetDeepCopyCurveByKey(BezierCurveByKey curveByKey)
    {
        return GoogleSheetReader.DeepCopyJson<BezierCurveByKey>(curveByKey); ;
    }
    void SetEditCurve(BezierCurveByKey curveByKey)
    {
        m_EditCurve = curveByKey;
        SceneCurveEditObjectToPoints(m_EditCurve.m_Points);
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
            Vector3 prevPos = curPos + points[i].ToPrev * points[i].PrevDis;
            Vector3 nextPos = curPos - points[i].ToPrev * points[i].NextDis;

            Gizmos.DrawLine(curPos, prevPos);
            Gizmos.DrawLine(curPos, nextPos);
        }

        int lastIdx = points.Length - 1;
        Vector3 lastPointCurPos = points[lastIdx].Pos;
        Vector3 lastPointPrevPos = lastPointCurPos + points[lastIdx].ToPrev * points[lastIdx].PrevDis;
        Vector3 lastPointNextPos = lastPointCurPos - points[lastIdx].ToPrev * points[lastIdx].NextDis;
        Gizmos.DrawLine(lastPointCurPos, lastPointPrevPos);
        Gizmos.DrawLine(lastPointCurPos, lastPointNextPos);

        Vector3[] markers = info.GetMarkers();
        Gizmos.DrawLineStrip(markers, false);
    }

}

namespace HMCurve
{
    [System.Serializable]
    public class BezierCurveInfo
    {
        public const int SplitCount = 32; //���� ����� ���� ��а���

        [SerializeField] float m_TotalDis;
        [SerializeField] Vector3[] m_PosSplitT; //����ġ T�� SplitCount������ŭ ��е������� ��ġ
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

            float[] prefixDisPerT; //����ġ T�� SplitCount������ŭ ��е����� �����Ÿ�
            int segmentCount = arr_Points.Length - 1;
            int dataCount = SplitCount + (SplitCount - 1) * (segmentCount - 1);
            m_PosSplitT = new Vector3[dataCount];
            m_DisWeight = new float[dataCount];
            prefixDisPerT = new float[dataCount];

            for (int i = 0; i < dataCount - 1; i++)
            {
                int segmentIdx = i / (SplitCount - 1);
                float t = (i % (SplitCount - 1)) / (float)(SplitCount - 1);
                Vector3 v1 = arr_Points[segmentIdx].Pos;
                Vector3 v2 = v1 - arr_Points[segmentIdx].ToPrev * arr_Points[segmentIdx].NextDis;
                Vector3 v4 = arr_Points[segmentIdx + 1].Pos;
                Vector3 v3 = v4 + arr_Points[segmentIdx + 1].ToPrev * arr_Points[segmentIdx + 1].PrevDis;
                m_PosSplitT[i] = GetCubicBezierPos(v1, v2, v3, v4, t);
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
            m_TotalDis = prefixDis;

            for (int i = 0; i < dataCount; i++)
            {
                m_DisWeight[i] = prefixDisPerT[i] / m_TotalDis;
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
        public float GetDistance()
        {
            return m_TotalDis;
        }
        public Vector3 GetPosByDistance(float distance)
        {
            float t = distance / m_TotalDis;
            t = t - Mathf.CeilToInt(t);
            if(t < 0)
            {
                t += 1;
            }

            return GetPosByWeight01(t);
        }
        public Vector3 GetPosByWeight01(float t)
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
            Debug.Log(m_DisWeight.Length);
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
        public Vector3 ToPrev;
        public float PrevDis;
        public float NextDis;

        public BezierCurvePoint(Vector3 pos, Vector3 toPrev, float prevDis, float nextDis)
        {
            Pos = pos;
            ToPrev = toPrev;
            PrevDis = prevDis;
            NextDis = nextDis;
        }
    }
}