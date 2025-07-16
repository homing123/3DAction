using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEditor.Experimental.GraphView;

public class DamageTextCreater : MonoBehaviour
{
    public struct DamageTextInfo
    {
        public Bio Attacker;
        public Bio Hitter;
        public bool IsCritical;
        public float AttackDamage;
        public float SkillDamage;
        public float TrueDamage;
        public DamageTextInfo(Bio attacker, Bio hitter, bool isCritical, float attackDamage, float skillDamage, float trueDamage)
        {
            Attacker = attacker;
            Hitter = hitter;
            IsCritical = isCritical;
            AttackDamage = attackDamage;
            SkillDamage = skillDamage;
            TrueDamage = trueDamage;
        }
        public void Add(DamageTextInfo otherinfo)
        {
            IsCritical |= otherinfo.IsCritical;
            AttackDamage += otherinfo.AttackDamage;
            SkillDamage += otherinfo.SkillDamage;
            TrueDamage += otherinfo.TrueDamage;
        }
    }
    public struct HealTextInfo
    {
        public Bio Target;
        public float HPHealValue;
        public float MPHealValue;
        public HealTextInfo(Bio target, float hpHealValue, float mpHealValue)
        {
            Target = target;
            HPHealValue = hpHealValue;
            MPHealValue = mpHealValue;
        }
        public void Add(HealTextInfo otherInfo)
        {
            HPHealValue+= otherInfo.HPHealValue;
            MPHealValue+= otherInfo.MPHealValue;
        }
    }
    const float CreateDelay = 0.1f;
    public static DamageTextCreater Ins;

    List<DamageTextInfo> L_DamageTextInfo = new List<DamageTextInfo>();
    List<HealTextInfo> L_HealTextInfo = new List<HealTextInfo>();

    private void Awake()
    {
        Ins = this;
    }


    private void Update()
    {
        for(int i=0;i< L_DamageTextInfo.Count;i++)
        {
            Bio attacker = L_DamageTextInfo[i].Attacker;
            Bio hitter = L_DamageTextInfo[i].Hitter;
            float attackDamage = L_DamageTextInfo[i].AttackDamage;
            bool isCritical = L_DamageTextInfo[i].IsCritical;
            float skillDamage = L_DamageTextInfo[i].SkillDamage;
            float trueDamage = L_DamageTextInfo[i].TrueDamage;
            int createIdx = 0;
            if (skillDamage > 0)
            {
                UI_Damage.CreateSkillDamage(attacker, hitter, skillDamage, createIdx);
                createIdx++;
            }
            if (attackDamage > 0)
            {
                UI_Damage.CreateAttackDamage(attacker, hitter, attackDamage, isCritical, createIdx);
                createIdx++;
            }
            if (trueDamage > 0)
            {
                UI_Damage.CreateTrueDamage(attacker, hitter, trueDamage, createIdx);
                createIdx++;
            }
        }

        for (int i = 0; i < L_HealTextInfo.Count;i++)
        {
            Bio target = L_HealTextInfo[i].Target;
            float hpHealValue = L_HealTextInfo[i].HPHealValue;
            float mpHealValue = L_HealTextInfo[i].MPHealValue;
            int createIdx = 0;
            if(hpHealValue > 0)
            {
                UI_Damage.CreateHPHeal(target, hpHealValue, createIdx);
                createIdx++;
            }
            if(mpHealValue>0)
            {
                UI_Damage.CreateMPHeal(target, mpHealValue, createIdx);
                createIdx++;
            }
        }
        L_DamageTextInfo.Clear();
        L_HealTextInfo.Clear();
    }
    public void CreateDamage(Bio attacker, Bio hitter, bool isCritical, float attackDamage, float skillDamage, float trueDamage)
    {
        Character player = PlayerM.Ins.GetPlayerCharacter();
        if (attacker != player && hitter != player)
        {
            return;
        }
        DamageTextInfo dmgTextInfo = new DamageTextInfo(attacker, hitter, isCritical, attackDamage, skillDamage, trueDamage);
        for (int i = 0; i < L_DamageTextInfo.Count; i++)
        {
            if (L_DamageTextInfo[i].Attacker == attacker && L_DamageTextInfo[i].Hitter == hitter)
            {
                L_DamageTextInfo[i].Add(dmgTextInfo);
                return;
            }
        }

        L_DamageTextInfo.Add(dmgTextInfo);
    }
    public void CreateHeal(Bio target, float hpHeal, float mpHeal)
    {
        HealTextInfo healTextInfo = new HealTextInfo(target, hpHeal, mpHeal);
        for (int i = 0; i < L_HealTextInfo.Count; i++)
        {
            if (L_HealTextInfo[i].Target == target)
            {
                L_HealTextInfo[i].Add(healTextInfo);
                return;
            }
        }

        L_HealTextInfo.Add(healTextInfo);
    }
}
