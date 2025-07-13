using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text;

public class UI_HPMPCredit : MonoBehaviour
{
    [SerializeField] Image m_HPGauge;
    [SerializeField] TextMeshProUGUI T_HP;
    [SerializeField] Image m_MPGauge;
    [SerializeField] TextMeshProUGUI T_MP;
    [SerializeField] TextMeshProUGUI T_Credit;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    Character m_PlayerCharacter;

    void Start()
    {
        m_PlayerCharacter = PlayerM.Ins.GetPlayerCharacter();
        m_PlayerCharacter.m_Status.OnStatusChanged += SetHPMP;

        m_PlayerCharacter.RegisterInitialized(SetHPMP);
    }
    void OnDestroy()
    {
        m_PlayerCharacter.m_Status.OnStatusChanged -= SetHPMP;
    }
    void SetHPMP()
    {
        if(m_PlayerCharacter == null)
        {
            Debug.Log("플레이어없");
        }
        if(m_PlayerCharacter.m_Status == null)
        {
            Debug.Log("스테이터스 없");
        }
        Status playerStatus = m_PlayerCharacter.m_Status;
        float maxHP = playerStatus.m_TotalMaxHP;
        float curHP = playerStatus.m_CurHP;
        float maxMP = playerStatus.m_TotalMaxMP;
        float curMP = playerStatus.m_CurMP;

        int ceilMaxHP = Mathf.CeilToInt(maxHP);
        int ceilCurHP = Mathf.CeilToInt(curHP);
        int ceilMaxMP = Mathf.CeilToInt(maxMP);
        int ceilCurMP = Mathf.CeilToInt(curMP);

        T_HP.text = ceilCurHP + " / " + ceilMaxHP;
        T_MP.text = ceilCurMP + " / " + ceilMaxMP;

        float hpFill = maxHP == 0 ? 0 : Mathf.Clamp01(curHP / maxHP);
        m_HPGauge.fillAmount = hpFill;

        float mpFill = maxMP == 0 ? 0 : Mathf.Clamp01(curMP / maxMP);
        m_MPGauge.fillAmount = mpFill;
    }
    void SetCredit()
    {
        T_Credit.text = "9999";
    }


}
