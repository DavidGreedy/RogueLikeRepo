using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethods
{
    public static bool IsTouching(this Rect rect, Rect other)
    {
        return (double)other.xMax >= (double)rect.xMin && (double)other.xMin <= (double)rect.xMax && (double)other.yMax >= (double)rect.yMin && (double)other.yMin <= (double)rect.yMax;
    }

    public static Line Border(this Rect rect, Rect other)
    {
        return new Line(new Vector2(Mathf.Min(rect.xMax, other.xMax), Mathf.Min(rect.yMax, other.yMax)), new Vector2(Mathf.Max(rect.xMin, other.xMin), Mathf.Max(rect.yMin, other.yMin)));
    }
}