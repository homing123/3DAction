using UnityEngine;

public class UI_Cheat : MonoBehaviour
{
    public void CharacterLevelUp()
    {
        Character character = PlayerM.Ins.GetPlayerCharacter();
        int curLevel = character.m_Status.m_Level;
        if(curLevel >= 19)
        {
            return;
        }
        float curEXP = character.m_Status.m_EXP;
        float remainingEXP = Status.LevelEXP[curLevel] - curEXP;
        character.m_Status.AddEXP(remainingEXP);
    }
}

