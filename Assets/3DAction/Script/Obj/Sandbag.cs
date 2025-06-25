using UnityEngine;

public class Sandbag : Bio
{
    protected override void Awake()
    {
        base.Awake();
        m_BioType = Bio_Type.Character;
    }
    protected override void Update()
    {
        base.Update();
        m_Status.Heal(m_Status.m_MaxHP);
    }
}
