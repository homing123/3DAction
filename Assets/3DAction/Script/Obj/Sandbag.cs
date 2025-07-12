using UnityEngine;

public class Sandbag : Bio
{
    [SerializeField] bool m_IsHeal;

    protected override void Awake()
    {
        base.Awake();
        m_BioType = Bio_Type.Sandbag;
    }
    protected override void Update()
    {
        base.Update();
        //if (m_IsHeal)
        //{
        //    m_Status.Heal(m_Status.m_MaxHP);
        //}
    }
}
