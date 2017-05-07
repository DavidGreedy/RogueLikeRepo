using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block
{
    const float tileSize = 1.0f/16.0f;

    public virtual Tile TexturePosition(DIRECTION direction)
    {
        return new Tile(0, 0);
    }

    public struct Tile
    {
        public int x;
        public int y;

        public Tile(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public enum DIRECTION
    {
        D_UP, D_DOWN, D_LEFT, D_RIGHT, D_FORWARD, D_BACKWARD
    }

    public virtual bool IsSolid(DIRECTION direction)
    {
        switch (direction)
        {
            case DIRECTION.D_UP:
            return true;
            case DIRECTION.D_DOWN:
            return true;
            case DIRECTION.D_LEFT:
            return true;
            case DIRECTION.D_RIGHT:
            return true;
            case DIRECTION.D_FORWARD:
            return true;
            case DIRECTION.D_BACKWARD:
            return true;
        }
        return false;
    }

    public virtual MeshData Data(Chunk chunk, int x, int y, int z, MeshData meshData)
    {
        if (!chunk.GetBlock(x, y + 1, z).IsSolid(DIRECTION.D_DOWN))
        {
            meshData = FaceData(chunk, x, y, z, meshData, DIRECTION.D_UP);
        }

        if (!chunk.GetBlock(x, y - 1, z).IsSolid(DIRECTION.D_UP))
        {
            meshData = FaceData(chunk, x, y, z, meshData, DIRECTION.D_DOWN);
        }

        if (!chunk.GetBlock(x + 1, y, z).IsSolid(DIRECTION.D_LEFT))
        {
            meshData = FaceData(chunk, x, y, z, meshData, DIRECTION.D_RIGHT);
        }

        if (!chunk.GetBlock(x - 1, y, z).IsSolid(DIRECTION.D_RIGHT))
        {
            meshData = FaceData(chunk, x, y, z, meshData, DIRECTION.D_LEFT);
        }

        if (!chunk.GetBlock(x, y, z + 1).IsSolid(DIRECTION.D_BACKWARD))
        {
            meshData = FaceData(chunk, x, y, z, meshData, DIRECTION.D_FORWARD);
        }

        if (!chunk.GetBlock(x, y, z - 1).IsSolid(DIRECTION.D_FORWARD))
        {
            meshData = FaceData(chunk, x, y, z, meshData, DIRECTION.D_BACKWARD);
        }

        return meshData;
    }

    protected virtual MeshData FaceData(Chunk chunk, int x, int y, int z, MeshData meshData, DIRECTION direction)
    {
        switch (direction)
        {
            case DIRECTION.D_UP:
            {
                meshData.AddVertex(x - 0.5f, y + 0.5f, z + 0.5f);
                meshData.AddVertex(x + 0.5f, y + 0.5f, z + 0.5f);
                meshData.AddVertex(x + 0.5f, y + 0.5f, z - 0.5f);
                meshData.AddVertex(x - 0.5f, y + 0.5f, z - 0.5f);
            }
            break;
            case DIRECTION.D_DOWN:
            {
                meshData.AddVertex(x - 0.5f, y - 0.5f, z - 0.5f);
                meshData.AddVertex(x + 0.5f, y - 0.5f, z - 0.5f);
                meshData.AddVertex(x + 0.5f, y - 0.5f, z + 0.5f);
                meshData.AddVertex(x - 0.5f, y - 0.5f, z + 0.5f);
            }
            break;
            case DIRECTION.D_LEFT:
            {
                meshData.AddVertex(x - 0.5f, y - 0.5f, z + 0.5f);
                meshData.AddVertex(x - 0.5f, y + 0.5f, z + 0.5f);
                meshData.AddVertex(x - 0.5f, y + 0.5f, z - 0.5f);
                meshData.AddVertex(x - 0.5f, y - 0.5f, z - 0.5f);
            }
            break;
            case DIRECTION.D_RIGHT:
            {
                meshData.AddVertex(x + 0.5f, y - 0.5f, z - 0.5f);
                meshData.AddVertex(x + 0.5f, y + 0.5f, z - 0.5f);
                meshData.AddVertex(x + 0.5f, y + 0.5f, z + 0.5f);
                meshData.AddVertex(x + 0.5f, y - 0.5f, z + 0.5f);
            }
            break;
            case DIRECTION.D_FORWARD:
            {
                meshData.AddVertex(x + 0.5f, y - 0.5f, z + 0.5f);
                meshData.AddVertex(x + 0.5f, y + 0.5f, z + 0.5f);
                meshData.AddVertex(x - 0.5f, y + 0.5f, z + 0.5f);
                meshData.AddVertex(x - 0.5f, y - 0.5f, z + 0.5f);
            }
            break;
            case DIRECTION.D_BACKWARD:
            {
                meshData.AddVertex(x - 0.5f, y - 0.5f, z - 0.5f);
                meshData.AddVertex(x - 0.5f, y + 0.5f, z - 0.5f);
                meshData.AddVertex(x + 0.5f, y + 0.5f, z - 0.5f);
                meshData.AddVertex(x + 0.5f, y - 0.5f, z - 0.5f);
            }
            break;
        }

        meshData.CreateQuadFromLast();
        meshData.uvs.AddRange(FaceUVs(direction));

        return meshData;
    }

    public virtual Vector2[] FaceUVs(DIRECTION direction)
    {
        Vector2[] UVs = new Vector2[4];
        Tile tilePos = TexturePosition(direction);

        UVs[0] = new Vector2(tileSize * (float)tilePos.x + tileSize, tileSize * (float)tilePos.y);
        UVs[1] = new Vector2(tileSize * (float)tilePos.x + tileSize, tileSize * (float)tilePos.y + tileSize);
        UVs[2] = new Vector2(tileSize * (float)tilePos.x, tileSize * (float)tilePos.y + tileSize);
        UVs[3] = new Vector2(tileSize * (float)tilePos.x, tileSize * (float)tilePos.y);

        //UVs[0] = new Vector2(1, 0);
        //UVs[1] = new Vector2(1, 1);
        //UVs[2] = new Vector2(0, 1);
        //UVs[3] = new Vector2(0, 0);

        return UVs;
    }
}