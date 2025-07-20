using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class Sandbag : Bio
{
    [SerializeField] bool m_IsHeal;

    ActionStateStop m_ActionStateStop;
    protected override void Awake()
    {
        base.Awake();
    }
    protected override void Start()
    {
        base.Start();
        m_Status.SandbagStatusInit();
        m_isInit = true;
        OnInitialized?.Invoke();
        m_ActionStateStop = new ActionStateStop(this);
        m_CurActionState = m_ActionStateStop;
        m_CurActionStateType = ActionState.Stop;
    }
    protected override void Update()
    {
        base.Update();
        if (m_IsHeal)
        {
            m_Status.Heal(100);
        }
    }
    protected override UniTaskVoid BaseAttack(CancellationTokenSource cts)
    {
        throw new System.NotImplementedException();
    }
    public override float GetAttackRange()
    {
        throw new System.NotImplementedException();
    }
}
