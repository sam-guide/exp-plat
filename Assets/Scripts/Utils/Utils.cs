using System;
using UnityEngine;

namespace Utils 
{
    public static class Misc
    {
        public static Color GetColour(string hex)
        {
            float[] l = { 0, 0, 0 };
            for (var i = 0; i < 6; i += 2)
            {
                float decValue = int.Parse(hex.Substring(i, 2), System.Globalization.NumberStyles.HexNumber);
                l[i / 2] = decValue / 255;
            }
            return new Color(l[0], l[1], l[2]);

        }
    }
    
    public static class Extensions
    {
        public static T[] Append<T>(this T[] array, T item)
        {
            if (array == null) return new T[] { item };
            T[] result = new T[array.Length + 1];
            array.CopyTo(result, 0);
            result[array.Length] = item;
            return result;
        }
    }
}