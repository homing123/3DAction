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

    void Start()
    {
        SetHPMP();
        SetCredit();
        Character.PlayerCharacter.m_Status.OnStatusChanged += SetHPMP;
    }
    void OnDestroy()
    {
        Character.PlayerCharacter.m_Status.OnStatusChanged -= SetHPMP;
    }
    void SetHPMP()
    {
        if(Character.PlayerCharacter ==null)
        {
            Debug.Log("플레이어없");
        }
        if(Character.PlayerCharacter.m_Status == null)
        {
            Debug.Log("스테이터스 없");
        }
        Status playerStatus = Character.PlayerCharacter.m_Status;
        float maxHP = playerStatus.m_MaxHP;
        float curHP = playerStatus.m_CurHP;
        float maxMP = playerStatus.m_MaxMP;
        float curMP = playerStatus.m_CurMP;

        int ceilMaxHP = Mathf.CeilToInt(maxHP);
        int ceilCurHP = Mathf.CeilToInt(curHP);
        int ceilMaxMP = Mathf.CeilToInt(maxMP);
        int ceilCurMP = Mathf.CeilToInt(curMP);

        T_HP.text = ceilCurHP + " / " + ceilMaxHP;
        T_MP.text = ceilCurMP + " / " + ceilMaxMP;

        m_HPGauge.fillAmount = Mathf.Clamp01(curHP / maxHP);
        m_MPGauge.fillAmount = Mathf.Clamp01(curMP / maxMP);
    }
    void SetCredit()
    {
        T_Credit.text = "9999";
    }


}
