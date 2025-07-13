using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_EXPBarLevel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI T_Level;
    [SerializeField] Image I_EXPBar;

    Character m_Character;
    private void Start()
    {
        m_Character = PlayerM.Ins.GetPlayerCharacter();
        m_Character.RegisterInitialized(Setting);
        m_Character.m_Status.OnEXPLevelChanged += Setting;
    }
    void Setting()
    {
        T_Level.text = (m_Character.m_Status.m_Level + 1).ToString();
        float levelUpEXP = m_Character.m_Status.GetLevelUpEXP();
        float curEXP = m_Character.m_Status.m_EXP;
        if (levelUpEXP == 0)
        {
            I_EXPBar.fillAmount = 0;
        }
        else
        {
            I_EXPBar.fillAmount = curEXP / levelUpEXP;
        }
    }
}
