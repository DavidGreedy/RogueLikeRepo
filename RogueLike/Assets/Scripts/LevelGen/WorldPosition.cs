using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldPosition
{
    public int x, y, z;

    public WorldPosition(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is WorldPosition))
            return false;

        WorldPosition pos = (WorldPosition)obj;

        return (pos.x == x && pos.y == y && pos.z == z);
    }

    public override int GetHashCode()
    {
        return (x ^ y ^ z).GetHashCode();
    }
}