using UnityEngine;

public class GM : MonoBehaviour
{
    public static GM Ins;
    private void Awake()
    {
        Ins = this;
        DontDestroyOnLoad(this.gameObject);
    }
    




}
