using UnityEngine;
using System;

public class LateFixedUpdate : MonoBehaviour
{
    public static event Action OnLateFixedUpdate;

    bool m_hasFixed = false;
    private void FixedUpdate()
    {
        m_hasFixed = true;
    }
    private void Update()
    {
        if(m_hasFixed == true)
        {
            m_hasFixed = false;
            OnLateFixedUpdate?.Invoke();
        }
    }
}
