using UnityEngine;

//public enum Knockback_Type
//{
//    User2Target = 0, //시전자->타겟 방향 //위칙 같을경우 +z방향
//    Direction = 1, //일정방향으로
//    Position = 2, //타겟 -> 특정 위치로
//}


//[System.Serializable]
//public struct DamageInfo
//{
//    public float damage;
//    public Knockback_Type knockbackType;
//    public float knockbackDis;
//    public float stunTime;
//}
//[System.Serializable]
//public struct Skill
//{
//    public float coolTime;
//    public float useMP;
//    public DamageInfo dmg;
//}

//public struct SkillInfo
//{
//    public float knockbackDis { get; private set; }
//    public Vector3 knockBackDir { get; private set; }
//    public float damage { get; private set; }
//    public float stunTime { get; private set; }
//    public SkillInfo(Bio user, in DamageInfo dmgInfo, Vector3 knockbackdir)
//    {
//        damage = user.m_Status.m_AttackDamage * dmgInfo.damage;
//        knockbackDis = dmgInfo.knockbackDis;
//        knockBackDir = knockbackdir;
//        stunTime = dmgInfo.stunTime;
//    }
//}
