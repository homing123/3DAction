using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CurveM))]
public class Ed_CurveM : Editor
{
    CurveM Ins;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Ins = (CurveM)target;
        if(GUILayout.Button("CreatePath"))
        {
            Ins.CreateCurve();
        }
    }
    void OnSceneGUI()
    {
        Debug.Log("¾À");
    }
}
