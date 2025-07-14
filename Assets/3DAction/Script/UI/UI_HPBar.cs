using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class UI_HPBar : MonoBehaviour
{
    const float HPBarBGWidth = 248;
    [SerializeField] TextHandler T_Name;
    [SerializeField] TextMeshProUGUI T_Level;
    [SerializeField] Image m_HPBarcodePrefab;
    [SerializeField] Image m_HPBarcodeWhitePrefab;
    [SerializeField] Image m_MPBar;
    [SerializeField] Transform m_HPBarcodeParent;
    [SerializeField] Transform m_HPBarcodeWhiteParent;
    [SerializeField] Vector2 m_ScreenOffset;
    [SerializeField] float m_WhiteMoveSpeed = 1.0f;

    List<Image> L_HPBarcodes = new List<Image>(10);
    List<Image> L_HPWhiteBarcodes = new List<Image>(10);
    Bio m_Target;
    Camera m_MainCam;
    float m_CurTotalFillAmount;
    float m_WhiteCurTotalFillAmount;
    Character m_Character;
    Sandbag m_Sandbag;
    private void Start()
    {
        m_MainCam = Camera.main;
        m_Target.m_Status.OnStatusChanged += SetStatus;
        m_Target.m_Status.OnEXPLevelChanged += SetLevel;
        switch(m_Target.m_BioType)
        {
            case Bio_Type.Character:
                m_Character = m_Target as Character;
                m_Character.RegisterInitialized(Setting);
                break;
            case Bio_Type.Sandbag:
                m_Sandbag = m_Target as Sandbag;
                m_Sandbag.RegisterInitialized(Setting);

                break;
        }
    }


    private void OnDestroy()
    {
        m_Target.m_Status.OnStatusChanged -= SetStatus;
        m_Target.m_Status.OnEXPLevelChanged -= SetLevel;
    }

    private void Update()
    {
        if (m_Target != null)
        {
            Vector2 screenPos = Util.World2Screen(m_Target.GetUIPivotPos());
            transform.position = screenPos + m_ScreenOffset;
            SetHPBarcodeWitheFillAmount();
        }
    }
    void Setting()
    {
        switch (m_Target.m_BioType)
        {
            case Bio_Type.Character:
                T_Name.SetTextID(m_Character.m_CharacterData.Name);
                break;
            case Bio_Type.Sandbag:
                T_Name.SetText("Sandbag");
                break;
        }
        SetStatus();
        SetLevel();
    }
    void SetLevel()
    {
        T_Level.text = (m_Target.m_Status.m_Level + 1).ToString();
    }
    void SetStatus()
    {
        if (m_Target == null)
        {
            return;
        }
        float maxHP = m_Target.m_Status.m_TotalMaxHP;
        float curHP = m_Target.m_Status.m_CurHP;
        float maxMP = m_Target.m_Status.m_TotalMaxMP;
        float curMP = m_Target.m_Status.m_CurMP;

        float hpBarcodeWidth = HPBarBGWidth / (maxHP * 0.001f);
        int hpBarcodCount = Mathf.CeilToInt(maxHP / 1000);
        m_CurTotalFillAmount = curHP * 0.001f;
        SetHPBarcodeCount(hpBarcodCount);
        SetHPBarcodePosAndWidth(hpBarcodeWidth);
        SetHPBarcodeFillAmount();
        m_MPBar.fillAmount = curMP / maxMP;
    }
    void SetHPBarcodeCount(int count)
    {
        if(L_HPBarcodes.Count > count)
        {
            int removeCount = L_HPBarcodes.Count - count;
            for (int i = 0; i < removeCount; i++)
            {
                Destroy(L_HPBarcodes[i].gameObject);
                Destroy(L_HPWhiteBarcodes[i].gameObject);
                L_HPBarcodes.RemoveAt(L_HPBarcodes.Count - 1);
                L_HPWhiteBarcodes.RemoveAt(L_HPWhiteBarcodes.Count - 1);
            }
        }
        else if(L_HPBarcodes.Count < count)
        {
            int addCount = count - L_HPBarcodes.Count;
            for(int i=0;i<addCount;i++)
            {
                Image hpBarcode = Instantiate(m_HPBarcodePrefab, m_HPBarcodeParent);
                hpBarcode.gameObject.SetActive(true);
                L_HPBarcodes.Add(hpBarcode);
                Image hpWhiteBarcode = Instantiate(m_HPBarcodeWhitePrefab, m_HPBarcodeWhiteParent);
                hpWhiteBarcode.gameObject.SetActive(true);
                L_HPWhiteBarcodes.Add(hpWhiteBarcode);
            }
        }
    }
    void SetHPBarcodePosAndWidth(float hpBarcodeWidth)
    {
        for(int i=0;i<L_HPBarcodes.Count;i++)
        {
            Vector2 anchoredPos = L_HPBarcodes[i].rectTransform.anchoredPosition;
            Vector2 sizeDelta = L_HPBarcodes[i].rectTransform.sizeDelta;
            L_HPBarcodes[i].rectTransform.sizeDelta = new Vector2(hpBarcodeWidth, sizeDelta.y);
            L_HPBarcodes[i].rectTransform.anchoredPosition = new Vector2(i * hpBarcodeWidth, anchoredPos.y);
            L_HPWhiteBarcodes[i].rectTransform.sizeDelta = new Vector2(hpBarcodeWidth, sizeDelta.y);
            L_HPWhiteBarcodes[i].rectTransform.anchoredPosition = new Vector2(i * hpBarcodeWidth, anchoredPos.y);
        }
    }
    void SetHPBarcodeFillAmount()
    {
        float totalFillAmount = m_CurTotalFillAmount;
        for (int i=0;i<L_HPBarcodes.Count;i++)
        {
            if(totalFillAmount >= 1)
            {
                L_HPBarcodes[i].fillAmount = 1;
                totalFillAmount--;
            }
            else
            {
                L_HPBarcodes[i].fillAmount = totalFillAmount > 0 ? totalFillAmount : 0;
                totalFillAmount--;
            }
        }
    }
    void SetHPBarcodeWitheFillAmount()
    {
        if(m_WhiteCurTotalFillAmount <= m_CurTotalFillAmount)
        {
            m_WhiteCurTotalFillAmount = m_CurTotalFillAmount;
        }
        else
        {
            m_WhiteCurTotalFillAmount -= m_WhiteMoveSpeed * Time.deltaTime;
        }
        float totalFillAmount = m_WhiteCurTotalFillAmount;
        for (int i = 0; i < L_HPWhiteBarcodes.Count; i++)
        {
            if (totalFillAmount >= 1)
            {
                L_HPWhiteBarcodes[i].fillAmount = 1;
                totalFillAmount--;
            }
            else
            {
                L_HPWhiteBarcodes[i].fillAmount = totalFillAmount > 0 ? totalFillAmount : 0;
                totalFillAmount--;
            }
        }
    }
    
    public static void Create(Bio target)
    {
        UI_HPBar hpBar = Instantiate(ResM.Ins.HPBar, GM.Canvas.transform);
        hpBar.m_Target = target;
    }
}
