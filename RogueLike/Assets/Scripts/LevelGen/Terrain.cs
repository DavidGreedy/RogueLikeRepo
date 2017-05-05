using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Terrain
{
    public static WorldPosition GetBlockPos(Vector3 pos)
    {
        return new WorldPosition(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z));
    }

    public static WorldPosition GetBlockPos(RaycastHit hit, bool adjacent = false)
    {
        Vector3 pos = new Vector3(BlockFromNormal(hit.point.x, hit.normal.x, adjacent),
                                  BlockFromNormal(hit.point.y, hit.normal.y, adjacent),
                                  BlockFromNormal(hit.point.z, hit.normal.z, adjacent));

        return GetBlockPos(pos);
    }

    static float BlockFromNormal(float position, float normal, bool adjacent = false)
    {
        if (position - Mathf.RoundToInt(position) < 0.51f)
        {
            if (adjacent)
            {
                position += (normal / 2);
            }
            else
            {
                position -= (normal / 2);
            }
        }

        return (float)position;
    }

    public static bool SetBlock(RaycastHit hit, Block block, bool adjacent = false)
    {
        Chunk chunk = hit.collider.GetComponent<Chunk>();
        if (chunk == null)
        {
            return false;
        }

        WorldPosition pos = GetBlockPos(hit, adjacent);

        chunk.world.SetBlock(pos.x, pos.y, pos.z, block);

        return true;
    }

    public static Block GetBlock(RaycastHit hit, bool adjacent = false)
    {
        Chunk chunk = hit.collider.GetComponent<Chunk>();
        if (chunk == null)
            return null;

        WorldPosition pos = GetBlockPos(hit, adjacent);

        return chunk.world.GetBlock(pos.x, pos.y, pos.z);
    }
}