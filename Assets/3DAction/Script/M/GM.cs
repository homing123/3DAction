using UnityEngine;

public class GM : MonoBehaviour
{
    static Canvas canvas;
    public static Canvas Canvas
    {
        get
        {
            if (canvas == null || canvas.gameObject == null)
            {
                canvas = FindFirstObjectByType<Canvas>();
            }
            return canvas;
        }
    }

    public static GM Ins;
    private void Awake()
    {
        Ins = this;
        DontDestroyOnLoad(this.gameObject);
    }




}
