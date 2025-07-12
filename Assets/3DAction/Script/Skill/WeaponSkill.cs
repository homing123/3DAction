using System.Collections.Generic;
using UnityEngine;

public enum WeaponType
{
    Arcana,
    Bat,
}

public class WeaponSkill
{
    const int ArcanaSkillID = 1;
    static Dictionary<WeaponType, int> D_WeaponSkillID = new Dictionary<WeaponType, int>()
    {
        { WeaponType.Arcana, ArcanaSkillID}
    };

    public static int GetWeaponSkillID(WeaponType weaponType)
    {
        return D_WeaponSkillID[WeaponType];
    }

    
}
