using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public Dictionary<WorldPosition, Chunk> chunks;
    public GameObject chunkPrefab;

    void Start()
    {
        chunks = new Dictionary<WorldPosition, Chunk>();
        //chunks = new List<Chunk>();
        for (int x = -2; x < 2; x++)
        {
            for (int y = -1; y < 1; y++)
            {
                for (int z = -1; z < 1; z++)
                {
                    CreateChunk(x * 16, y * 16, z * 16);
                }
            }
        }
    }

    public void CreateChunk(int x, int y, int z)
    {
        WorldPosition worldPos = new WorldPosition(x, y, z);

        GameObject newChunkObject = Instantiate(chunkPrefab, new Vector3(worldPos.x, worldPos.y, worldPos.z), Quaternion.Euler(Vector3.zero)) as GameObject;

        Chunk newChunk = newChunkObject.GetComponent<Chunk>();

        newChunk.worldPosition = worldPos;
        newChunk.world = this;

        chunks.Add(worldPos, newChunk);

        for (int ix = 0; ix < 16; ix++)
        {
            for (int iy = 0; iy < 16; iy++)
            {
                for (int iz = 0; iz < 16; iz++)
                {
                    if (true)
                    {
                        SetBlock(x + ix, y + iy, z + iz, new BlockGrass());
                    }
                    else
                    {
                        SetBlock(x + ix, y + iy, z + iz, new BlockAir());
                    }
                }
            }
        }
    }

    public Chunk GetChunk(int x, int y, int z)
    {
        float multiple = Chunk.chunkSize;
        WorldPosition pos = new WorldPosition(Mathf.FloorToInt(x / multiple) * Chunk.chunkSize,
                                              Mathf.FloorToInt(y / multiple) * Chunk.chunkSize,
                                              Mathf.FloorToInt(z / multiple) * Chunk.chunkSize);

        Chunk containerChunk = null;
        chunks.TryGetValue(pos, out containerChunk);

        return containerChunk;
    }
    public Block GetBlock(int x, int y, int z)
    {
        Chunk containerChunk = GetChunk(x, y, z);
        if (containerChunk != null)
        {
            return containerChunk.GetBlock(x - containerChunk.worldPosition.x,
                                                  y - containerChunk.worldPosition.y,
                                                  z - containerChunk.worldPosition.z);
        }
        return new BlockAir();
    }

    public void SetBlock(int x, int y, int z, Block block)
    {
        Chunk chunk = GetChunk(x, y, z);

        if (chunk != null)
        {
            chunk.SetBlock(x - chunk.worldPosition.x, y - chunk.worldPosition.y, z - chunk.worldPosition.z, block);
            chunk.update = true;

            UpdateIfEqual(x - chunk.worldPosition.x, 0, new WorldPosition(x - 1, y, z));
            UpdateIfEqual(x - chunk.worldPosition.x, Chunk.chunkSize - 1, new WorldPosition(x + 1, y, z));
            UpdateIfEqual(y - chunk.worldPosition.y, 0, new WorldPosition(x, y - 1, z));
            UpdateIfEqual(y - chunk.worldPosition.y, Chunk.chunkSize - 1, new WorldPosition(x, y + 1, z));
            UpdateIfEqual(z - chunk.worldPosition.z, 0, new WorldPosition(x, y, z - 1));
            UpdateIfEqual(z - chunk.worldPosition.z, Chunk.chunkSize - 1, new WorldPosition(x, y, z + 1));
        }
    }

    public void DestroyChunk(int x, int y, int z)
    {
        Chunk chunk = null;
        if (chunks.TryGetValue(new WorldPosition(x, y, z), out chunk))
        {
            Object.Destroy(chunk.gameObject);
            chunks.Remove(new WorldPosition(x, y, z));
        }
    }

    void UpdateIfEqual(int value1, int value2, WorldPosition pos)
    {
        if (value1 == value2)
        {
            Chunk chunk = GetChunk(pos.x, pos.y, pos.z);
            if (chunk != null)
                chunk.update = true;
        }
    }

}