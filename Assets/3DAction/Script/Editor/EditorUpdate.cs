using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[InitializeOnLoad]
public static class EditorUpdate
{
    static EditorUpdate()
    {
        EditorApplication.update += OnEditorUpdate;
        Selection.selectionChanged += OnSelectionChanged;
    }

    static void OnEditorUpdate()
    {
        CurveEditUpdate();
    }

    static bool IsContainCurveEditObject = false;
    static void OnSelectionChanged()
    {
        if (CurveM.Ins == null || CurveM.Ins.m_EditCurve == null)
        {
            return;
        }
        if (Selection.objects.Length == 0)
        {
            return;
        }

        for (int i = 0; i < Selection.gameObjects.Length; i++)
        {
            if (Selection.gameObjects[i] != CurveM.Ins.transform && Selection.gameObjects[i].transform.root == CurveM.Ins.transform)
            {
                IsContainCurveEditObject = true;
                return;
            }
        }
        IsContainCurveEditObject = false;
    }

    static void CurveEditUpdate()
    {
        if(CurveM.Ins == null)
        {
            return;
        }

        if(IsContainCurveEditObject == true)
        {
            CurveM.Ins.CheckUpdateEditCurve();
        }
    }
}
