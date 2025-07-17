using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HMCurve;
using UnityEngine.Rendering;

public class UI_Damage : MonoBehaviour
{
    public enum DamageTextType
    {
        SkillDamage,
        AttackDamage,
        CriticalAttackDamage,
        TrueDamage,
        HPHeal,
        MPHeal
    }
    public static void CreateSkillDamage(Bio attacker, Bio hitter, float skillDamage, int createIdx)
    {
        UI_Damage uiDamage = Instantiate(ResM.Ins.DamageText, GM.Canvas.transform);
        uiDamage.m_Type = DamageTextType.SkillDamage;
        uiDamage.SetPosDamage(attacker, hitter);
        uiDamage.MoveOffset(createIdx);
        uiDamage.T_Damage.text = skillDamage.ToString("F0");
        uiDamage.T_Damage.color = uiDamage.m_SkillDamageColor;
    }
    public static void CreateAttackDamage(Bio attacker, Bio hitter, float attackDamage, bool isCritical, int createIdx)
    {
        UI_Damage uiDamage = Instantiate(ResM.Ins.DamageText, GM.Canvas.transform);
        uiDamage.m_Type = isCritical == true ? DamageTextType.CriticalAttackDamage : DamageTextType.AttackDamage;
        uiDamage.SetPosDamage(attacker, hitter);
        uiDamage.MoveOffset(createIdx);
        uiDamage.T_Damage.text = attackDamage.ToString("F0");
        uiDamage.T_Damage.color = uiDamage.m_AttackDamageColor;
        uiDamage.I_Icon.gameObject.SetActive(isCritical);
    }
    public static void CreateTrueDamage(Bio attacker, Bio hitter, float trueDamage, int createIdx)
    {
        UI_Damage uiDamage = Instantiate(ResM.Ins.DamageText, GM.Canvas.transform);
        uiDamage.m_Type = DamageTextType.TrueDamage;
        uiDamage.SetPosDamage(attacker, hitter);
        uiDamage.MoveOffset(createIdx);
        uiDamage.T_Damage.text = trueDamage.ToString("F0");
        uiDamage.T_Damage.color = uiDamage.m_TrueDamageColor;
    }
    public static void CreateHPHeal(Bio target, float hpHeal, int createIdx)
    {
        UI_Damage uiDamage = Instantiate(ResM.Ins.DamageText, GM.Canvas.transform);
        uiDamage.m_Type = DamageTextType.HPHeal;
        uiDamage.SetPosHeal(target);
        uiDamage.MoveOffset(createIdx);
        uiDamage.T_Damage.text = hpHeal.ToString("F0");
        uiDamage.T_Damage.color = uiDamage.m_HPHealColor;
    }
    public static void CreateMPHeal(Bio target, float mpHeal, int createIdx)
    {
        UI_Damage uiDamage = Instantiate(ResM.Ins.DamageText, GM.Canvas.transform);
        uiDamage.m_Type = DamageTextType.MPHeal;
        uiDamage.SetPosHeal(target);
        uiDamage.MoveOffset(createIdx);
        uiDamage.T_Damage.text = mpHeal.ToString("F0");
        uiDamage.T_Damage.color = uiDamage.m_MPHealColor;
    }
    [SerializeField] Color32 m_SkillDamageColor;
    [SerializeField] Color32 m_AttackDamageColor;
    [SerializeField] Color32 m_TrueDamageColor;
    [SerializeField] Color32 m_HPHealColor;
    [SerializeField] Color32 m_MPHealColor;

    [SerializeField] Vector2 m_ScreenOffsetDamages;
    [SerializeField] Vector2 m_ScreenOffsetHeals;
    [SerializeField] float m_CreateIdxOffset;

    [SerializeField] TextMeshProUGUI T_Damage;
    [SerializeField] Image I_Icon;
    DamageTextType m_Type;
    bool m_isLeft;
    [SerializeField] string m_DamageCurveKey;
    [SerializeField] float m_DamageCurveSpeed;
    [SerializeField] AnimationCurve m_DamageCurveSpeedCurve;
    [SerializeField] AnimationCurve m_DamageSizeCurve;
    [SerializeField] AnimationCurve m_DamageFadeAlphaCurve;
    [SerializeField] float m_DamageLifeTime;
    [SerializeField] float m_DamageFontSize;

    [SerializeField] string m_HealCurveKey;
    [SerializeField] float m_HealCurveSpeed;
    [SerializeField] AnimationCurve m_HealCurveSpeedCurve;
    [SerializeField] AnimationCurve m_HealSizeCurve;
    [SerializeField] AnimationCurve m_HealFadeAlphaCurve;
    [SerializeField] float m_HealLifeTime;
    [SerializeField] float m_HealFontSize;
    float m_CurveMoveDis;
    float m_CurTime;

    Vector3 m_StartWorldPos;
    BezierCurveInfo m_CurveInfo;
    private void Start()
    {
        
    }
    void SetPosDamage(Bio attacker, Bio hitter)
    {
        Vector2 user2Target = (hitter.transform.position - attacker.transform.position).VT2XZ();
        m_isLeft = Vector2.Dot(CamM.Ins.m_CamScreenRight, user2Target) >= 0;
        m_StartWorldPos = hitter.transform.position;
        transform.position = Util.World2Screen(hitter.transform.position);
        m_CurveInfo = CurveM.Ins.GetCurve(m_DamageCurveKey);
    }
    void SetPosHeal(Bio target)
    {
        m_StartWorldPos = target.transform.position;
        transform.position = Util.World2Screen(target.transform.position);
        m_CurveInfo = CurveM.Ins.GetCurve(m_HealCurveKey);
    }

    void MoveOffset(int createIdx)
    {
        switch(m_Type)
        {
            case DamageTextType.SkillDamage:
            case DamageTextType.AttackDamage:
            case DamageTextType.CriticalAttackDamage:
            case DamageTextType.TrueDamage:
                {
                    Vector3 screenOffset = m_ScreenOffsetDamages;
                    if (m_isLeft)
                    {
                        screenOffset.x = -screenOffset.x;
                    }
                    screenOffset.y += -createIdx * m_CreateIdxOffset;
                    transform.position += screenOffset;
                }
                break;

            case DamageTextType.HPHeal:
            case DamageTextType.MPHeal:
                {
                    Vector3 screenOffset = m_ScreenOffsetHeals;
                    screenOffset.y += -createIdx * m_CreateIdxOffset;
                    transform.position += screenOffset;
                }
                break;
        }
    }

    private void Update()
    {
        m_CurTime += Time.deltaTime;

        float curSpeed = 0;
        float curSize = 0;
        float alpha = 0;
        float curLifeTimePer = 0;
        switch (m_Type)
        {
            case DamageTextType.SkillDamage:
            case DamageTextType.AttackDamage:
            case DamageTextType.CriticalAttackDamage:
            case DamageTextType.TrueDamage:
                {
                    curLifeTimePer = m_CurTime / m_DamageLifeTime;
                    curSpeed = m_DamageCurveSpeedCurve.Evaluate(curLifeTimePer) * m_DamageCurveSpeed;
                    curSize = m_DamageSizeCurve.Evaluate(curLifeTimePer) * m_DamageFontSize;
                    alpha = m_DamageFadeAlphaCurve.Evaluate(curLifeTimePer);
                }
                break;

            case DamageTextType.HPHeal:
            case DamageTextType.MPHeal:
                {
                    curLifeTimePer = m_CurTime / m_HealLifeTime;
                    curSpeed = m_HealCurveSpeedCurve.Evaluate(curLifeTimePer) * m_HealCurveSpeed;
                    curSize = m_HealSizeCurve.Evaluate(curLifeTimePer) * m_HealFontSize;
                    alpha = m_HealFadeAlphaCurve.Evaluate(curLifeTimePer);
                }
                break;
        }
        T_Damage.alpha = alpha;
        T_Damage.fontSize = curSize;
        float maxCurveDis = m_CurveInfo.GetDistance();
        m_CurveMoveDis += curSpeed * Time.deltaTime;
        m_CurveMoveDis = Mathf.Clamp(m_CurveMoveDis, 0, maxCurveDis);
        Vector3 curvePos = m_CurveInfo.GetPosByDistance(m_CurveMoveDis);
        transform.position = Util.World2Screen(m_StartWorldPos + curvePos);

        switch (m_Type)
        {
            case DamageTextType.SkillDamage:
            case DamageTextType.AttackDamage:
            case DamageTextType.CriticalAttackDamage:
            case DamageTextType.TrueDamage:
                {
                    if (m_CurTime >= m_DamageLifeTime)
                    {
                        Destroy(gameObject);
                    }
                }
                break;

            case DamageTextType.HPHeal:
            case DamageTextType.MPHeal:
                {
                    if (m_CurTime >= m_HealLifeTime)
                    {
                        Destroy(gameObject);
                    }
                }
                break;
        }
    }

}
