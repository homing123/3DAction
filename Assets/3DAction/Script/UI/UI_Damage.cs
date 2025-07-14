using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UI_Damage : MonoBehaviour
{
    public static void CreateDamage(bool isCritical, float attackDamage, float skillDamage, float trueDamage)
    {
        UI_Damage uiDamage = Instantiate(ResM.Ins.DamageText);

        if(attackDamage > 0)
        {
            uiDamage.SetADDamage(isCritical, attackDamage);
        }
        if(skillDamage > 0)
        {
            uiDamage.SetSkillDamage(skillDamage);
        }
        if(trueDamage > 0)
        {
            uiDamage.SetTrueDamage(trueDamage);
        }
    }

    void SetADDamage(bool isCritical, float damage)
    {
        
    }
    void SetSkillDamage(float damage)
    {

    }
    void SetTrueDamage(float trueDamage)
    {

    }
    
    public static void CreateHealDamage(float heal)
    {

    }
    [SerializeField] TextMeshProUGUI T_Damage;
    [SerializeField] Image I_Icon;
 

}
