using System.Collections.Generic;
using UnityEngine;

public enum SpellType
{
    Artifact,
    Stridjer,
    Blink
}

public class SpellSkill
{
    const int ArtifactSkillID = 100;
    static Dictionary<SpellType, int> D_SpellSkillID = new Dictionary<SpellType, int>()
    {
        { SpellType.Artifact, ArtifactSkillID}
    };

    public static SkillData[] GetSpellSkillDatas(SpellType spellType)
    {
        return SkillData.GetData(D_SpellSkillID[spellType]);
    }

}