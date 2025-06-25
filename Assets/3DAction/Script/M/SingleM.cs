using UnityEngine;

public class SingleM<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _ins;
    public static T Ins
    {
        get
        {
            if (_ins == null)
            {
                _ins = GM.Ins.GetComponent<T>();
                if (_ins == null)
                {
                    Debug.LogError($"Singleton of type {typeof(T)} not found in GM.");
                }
            }
            return _ins;
        }
    }

    protected virtual void Awake()
    {
        if (_ins == null)
        {
            _ins = this as T;
        }
        else if (_ins != this)
        {
            Destroy(gameObject);
        }
    }
}