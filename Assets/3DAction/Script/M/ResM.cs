using UnityEngine;

public class ResM : SingleM<ResM>
{
    public GameObject UI_Parent;

    //UI
    [Header("UI")]
    public UI_HPBar HPBar;

    //Not UI
    [Header("Obj")]
    public HitRange HitRange_Quad;
    public HitRange HitRange_Circle;

    [Header("VFX")]
    public VFX VFX_Slash;

    [Header("Projectile")]
    public Projectile Proj_Rock;

   
}
