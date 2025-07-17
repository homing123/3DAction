using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor.Experimental.GraphView;
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
    float m_DamageCurveMoveDis;
    [SerializeField] string m_HealCurveKey;
    [SerializeField] float m_HealCurveSpeed;
    [SerializeField] AnimationCurve m_HealCurveSpeedCurve;
    [SerializeField] AnimationCurve m_HealSizeCurve;
    [SerializeField] AnimationCurve m_HealFadeAlphaCurve;
    [SerializeField] float m_HealLifeTime;




    void SetPosDamage(Bio attacker, Bio hitter)
    {
        Vector2 user2Target = (hitter.transform.position - attacker.transform.position).VT2XZ();
        m_isLeft = Vector2.Dot(CamM.Ins.m_CamScreenRight, user2Target) >= 0;
        transform.position = Util.World2Screen(hitter.transform.position);
    }
    void SetPosHeal(Bio target)
    {
        transform.position = Util.World2Screen(target.transform.position);
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



}
