using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(GoogleSheetReader))]
public class Ed_GoogleSheetReader : Editor
{
    GoogleSheetReader m_Ins;
    public async void LoadTextSheet()
    {
        TextData[] textDatas = await GoogleSheetReader.LoadGoogleSheetAndSaveBinary<TextData>(m_Ins.m_TextSheetInfo);
        TextData.SetDicData(textDatas);
        GoogleSheetReader.SaveBinary(TextData.GetDicData(), TextData.FileName);
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        m_Ins = (GoogleSheetReader)target;
        if(GUILayout.Button("Load TextData"))
        {
            LoadTextSheet();
        }

    }
}
