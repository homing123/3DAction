using UnityEngine;
using System.Collections.Generic;

public class ResM : SingleM<ResM>
{
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

    [System.Serializable]
    class SpriteAndKey
    {
        public int key;
        public Sprite sprite;
    }


}
