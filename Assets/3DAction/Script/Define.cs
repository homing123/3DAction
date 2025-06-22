using System.Collections.Generic;
using UnityEngine;

public enum Layer
{
    Ground = 6,
    Player = 7,
    Monster = 8,
    Object = 9
}

public class Define
{
    public static Dictionary<Layer, int> D_LayerMask = new Dictionary<Layer, int>()
    {
        {Layer.Ground, 1<< (int)Layer.Ground },
        {Layer.Player, 1<< (int)Layer.Player },
        {Layer.Monster, 1<< (int)Layer.Monster },
        {Layer.Object, 1<< (int)Layer.Object },
    };

}
