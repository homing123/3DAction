using System;
using System.Collections.Generic;

public class EnumArray<T> where T : Enum
{
    static T[] Array;
    public static T[] GetArray()
    {
        if(Array == null)
        {
            List<T> list = new List<T>();
            foreach(T en in Enum.GetValues(typeof(T)))
            {
                list.Add(en);
            }
            Array = list.ToArray();
        }
        return Array;
    }
}
