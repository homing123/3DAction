using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using static TextHandler;

public class TextHandler : MonoBehaviour
{
    public enum TextType
    { 
        Text,
        TextID,
    }
   
    [SerializeField] TextMeshProUGUI m_Text;
    [SerializeField] TextType m_TextType;
    [SerializeField] int m_ID;
    [SerializeField] string m_String;
    
    private void Awake()
    {
        if(m_Text == null)
        {
            m_Text = GetComponent<TextMeshProUGUI>();
        }
    }
    private void Start()
    {
        switch(m_TextType)
        {
            case TextType.Text:
                m_Text.text = m_String;
                break;
            case TextType.TextID:
                m_Text.text = TextData.GetData(m_ID);
                break;
        }
    }
    public void SetTextID(int id)
    {
        m_TextType = TextType.TextID;
        m_ID = id;
        m_Text.text = TextData.GetData(id);
    }
    public void SetText(string str)
    {
        m_TextType = TextType.Text;
        m_String = str;
        m_Text.text = str;

    }
}