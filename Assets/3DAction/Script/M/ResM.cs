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

    Dictionary<string, Sprite> D_Sprite = new Dictionary<string, Sprite>();
    Dictionary<string, GameObject> D_Prefab = new Dictionary<string, GameObject>();

    public Sprite GetSprite(string path)
    {
        if(D_Sprite.ContainsKey(path) == false)
        {
            D_Sprite[path] = Resources.Load<Sprite>(path);
        }

        if (D_Sprite[path] == null)
        {
            return null;
        }
        return D_Sprite[path];
    }
    public GameObject GetPrefab(string path)
    {
        if (D_Prefab.ContainsKey(path) == false)
        {
            D_Prefab[path] = Resources.Load<GameObject>(path);
        }

        if (D_Prefab[path] == null)
        {
            return null;
        }
        return D_Prefab[path];
    }



}
