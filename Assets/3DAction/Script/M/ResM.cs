using UnityEngine;

public class ResM : SingleM<ResM>
{
    public GameObject UI_Parent;

    //UI
    [Header("UI")]

    //Not UI
    [Header("Obj")]
    public HitRange HitRange_Box;
    public HitRange HitRange_Sphere;

    [Header("VFX")]
    public VFX VFX_Slash;

    [Header("Projectile")]
    public Projectile Proj_Rock;

   
}
