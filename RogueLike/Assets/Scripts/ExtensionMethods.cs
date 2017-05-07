using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public static class ExtensionMethods
{
    public static bool IsTouching(this Rect rect, Rect other)
    {
        return (double)other.xMax >= (double)rect.xMin && (double)other.xMin <= (double)rect.xMax && (double)other.yMax >= (double)rect.yMin && (double)other.yMin <= (double)rect.yMax;
    }

    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
    {
        Random rnd = new Random();
        return source.OrderBy<T, int>((item) => rnd.Next());
    }

    public static int ToGridPosition(float number, int gridSize)
    {
        return Mathf.FloorToInt(((number + gridSize - 1) / gridSize)) * gridSize;
    }

    public static int ToOddGridPosition(float number, int gridSize)
    {
        int position = Mathf.FloorToInt(((number + gridSize - 1) / gridSize)) * gridSize;
        if ((position & 1) == 0)
        {
            position += 1;
        }
        return position;
    }

    public static int ToEvenGridPosition(float number, int gridSize)
    {
        int position = Mathf.FloorToInt(((number + gridSize - 1) / gridSize)) * gridSize;
        if ((position & 1) == 1)
        {
            position += 1;
        }
        return position;
    }
}