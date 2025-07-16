using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CurveObject))]
public class Ed_CurveObject : Editor
{
    CurveObject Ins;

    Vector3 m_LastPos;
    Quaternion m_LastRot;
    private void OnEnable()
    {
        Ins = (CurveObject)target;
        m_LastPos = Ins.transform.position;
        m_LastRot = Ins.transform.rotation;
    }
    private void OnSceneGUI()
    {
        if(Ins.transform.position != m_LastPos || m_LastRot != Ins.transform.rotation)
        {
            m_LastPos = Ins.transform.position;
            m_LastRot = Ins.transform.rotation;
            CurveM.Ins.UpdateCurEditCurve();
        }
    }
}

[InitializeOnLoad]
public static class EditTemp
{ 

    public static void Update()
    {
        Debug.Log("이거불림?");
    }
}
