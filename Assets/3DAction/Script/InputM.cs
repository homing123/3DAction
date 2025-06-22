using UnityEngine;


public class InputM : MonoBehaviour
{
    public static InputM Ins;
    private void Awake()
    {
        if (Ins != null && Ins != this)
        {
            Destroy(Ins.gameObject);
        }
        Ins = this;
    }
   
   

  
 

}
