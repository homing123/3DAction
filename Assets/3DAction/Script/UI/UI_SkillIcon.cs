using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UI_SkillIcon : MonoBehaviour
{
    [SerializeField] GameObject m_SkillUnuseable;
    [SerializeField] GameObject m_SkillLevel0;
    [SerializeField] GameObject m_SkillNotEnoughMP;
    [SerializeField] Image m_SkillIcon;
    [SerializeField] GameObject[] m_SkillLevels;
    [SerializeField] TextMeshProUGUI T_SkillLevel;
    [SerializeField] TextMeshProUGUI m_MP;
    [SerializeField] TextMeshProUGUI m_Key;

    public void Setting()
    {

    }
}
