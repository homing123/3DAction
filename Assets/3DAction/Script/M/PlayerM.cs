using UnityEngine;

public class PlayerM : MonoBehaviour
{
    public static PlayerM Ins;
    private void Awake()
    {
        Ins = this;
    }
    Character m_PlayerCharacter;
    public void RegisterCharacter(Character character)
    {
        m_PlayerCharacter = character;
    }
    public Character GetPlayerCharacter()
    {
        return m_PlayerCharacter;
    }
}
