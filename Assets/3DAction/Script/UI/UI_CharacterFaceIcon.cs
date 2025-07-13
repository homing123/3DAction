using UnityEngine;
using UnityEngine.UI;

public class UI_CharacterFaceIcon : MonoBehaviour
{
    [SerializeField] Image I_CharacterFaceIcon;
    Character m_PlayerCharacter;
    private void Start()
    {
        m_PlayerCharacter = PlayerM.Ins.GetPlayerCharacter();
        m_PlayerCharacter.RegisterInitialized(Setting);
    }
    void Setting()
    {
        I_CharacterFaceIcon.sprite = ResM.Ins.GetSprite(PlayerM.Ins.GetPlayerCharacter().m_CharacterData.FaceIconPath);
    }
}
