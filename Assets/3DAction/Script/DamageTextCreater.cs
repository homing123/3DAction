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
    }
    const float CreateDelay = 0.1f;
    public static DamageTextCreater Ins;

    Dictionary<Bio, List<DamageTextInfo>> m_DamageTextInfo = new Dictionary<Bio, List<DamageTextInfo>>();
    Dictionary<Bio, List<HealTextInfo>> m_HealTextInfo = new Dictionary<Bio, List<HealTextInfo>>();

    private void Awake()
    {
        Ins = this;
    }


    private void Update()
    {
        foreach(Bio key in m_DamageTextInfo.Keys)
        {
            List<DamageTextInfo> damageTextInfose = m_DamageTextInfo[key];
            if(m_DamageTextInfo.Count == 0)
            {
                continue;
            }
            Bio attacker = m_DamageTextInfo[key]
            float attackDamage = 0;
            bool isCritical = false;
            float skillDamage = 0;
            float trueDamage = 0;
            for(int i=0;i<damageTextInfose.Count;i++)
            {
                attackDamage += damageTextInfose[i].AttackDamage;
                skillDamage += damageTextInfose[i].SkillDamage;
                trueDamage += damageTextInfose[i].TrueDamage;
                isCritical = (isCritical | damageTextInfose[i].IsCritical);
            }
            int createCount = 0;
            if (skillDamage > 0)
            {
                UI_Damage.CreateSkillDamage(damage)
                createCount++;
            }
            if (attackDamage > 0)
            {
                createCount++;
            }
            if(trueDamage > 0)
            {
                createCount++;
            }
        }

        foreach (Bio key in m_HealTextInfo.Keys)
        {
            List<HealTextInfo> healTextInfo = m_HealTextInfo[key];
            if (m_HealTextInfo.Count == 0)
            {
                continue;
            }
            float hpHeal = 0;
            float mpHeal = 0;
            for (int i = 0; i < healTextInfo.Count; i++)
            {
                hpHeal += healTextInfo[i].HPHealValue;
                mpHeal += healTextInfo[i].MPHealValue;
            }
            int createCount = 0;
            if (hpHeal > 0)
            {
                createCount++;
            }
            if (mpHeal > 0)
            {
                createCount++;
            }
        }
    }
    public void CreateDamage(Bio attacker, Bio hitter, bool isCritical, float attackDamage, float skillDamage, float trueDamage)
    {
        if(m_DamageTextInfo.ContainsKey(hitter)== false)
        {
            m_DamageTextInfo.Add(hitter, new List<DamageTextInfo>());
        }
        m_DamageTextInfo[hitter].Add(new DamageTextInfo(attacker, hitter, isCritical, attackDamage, skillDamage, trueDamage));
    }
    public void CreateHeal(Bio target, float hpHeal, float mpHeal)
    {
        if (m_HealTextInfo.ContainsKey(target) == false)
        {
            m_HealTextInfo.Add(target, new List<HealTextInfo>());
        }
        m_HealTextInfo[target].Add(new HealTextInfo(target, hpHeal, mpHeal));
    }
}
