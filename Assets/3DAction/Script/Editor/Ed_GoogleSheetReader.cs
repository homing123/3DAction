using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(GoogleSheetReader))]
public class Ed_GoogleSheetReader : Editor
{
    GoogleSheetReader m_Ins;
    public void LoadTextSheet()
    {
        TextData.LoadGoogleSheetAndSaveBinary(m_Ins.m_TextSheetInfo);
    }

    public void LoadSkillSheet()
    {
        SkillData.LoadGoogleSheetAndSaveBinary(m_Ins.m_SkillSheetInfo);
    }
    public void LoadCharacterSheet()
    {
        CharacterData.LoadGoogleSheetAndSaveBinary(m_Ins.m_CharacterSheetInfo);
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        m_Ins = (GoogleSheetReader)target;
        if(GUILayout.Button("Load TextData"))
        {
            LoadTextSheet();
        }
        if(GUILayout.Button("Load SkillData"))
        {
            LoadSkillSheet();
        }
        if (GUILayout.Button("Load CharacterData"))
        {
            LoadCharacterSheet();
        }
        if (GUILayout.Button("Load All Data"))
        {
            LoadTextSheet();
            LoadSkillSheet();
            LoadCharacterSheet();
        }

    }
}
