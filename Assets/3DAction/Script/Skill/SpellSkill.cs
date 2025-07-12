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

    public static int GetSpellSkillID(SpellType spellType)
    {
        return D_SpellSkillID[spellType];
    }

}