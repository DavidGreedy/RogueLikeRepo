using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
    Block[,,] blocks = new Block[chunkSize, chunkSize, chunkSize];

    public static int chunkSize = 16;
    public bool update = true;

    public World world;
    public WorldPosition worldPosition;

    [SerializeField]
    MeshFilter filter;
    [SerializeField]
    MeshCollider collider;

    public Block GetBlock(int x, int y, int z)
    {
        if (InRange(x) && InRange(y) && InRange(z))
        {
            return blocks[x, y, z];
        }
        return world.GetBlock(worldPosition.x + x, worldPosition.y + y, worldPosition.z + z);
    }

    public static bool InRange(int index)
    {
        return (index >= 0 && index < chunkSize);
    }

    void UpdateChunk()
    {
        MeshData meshData = new MeshData();
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    meshData = blocks[x, y, z].Data(this, x, y, z, meshData);
                }
            }
        }
        RenderMesh(meshData);
    }

    void RenderMesh(MeshData meshData)
    {
        Mesh mesh = new Mesh()
        {
            vertices = meshData.vertices.ToArray(),
            triangles = meshData.indices.ToArray(),
            uv = meshData.uvs.ToArray()
        };

        filter.mesh.Clear();
        mesh.RecalculateNormals();

        filter.mesh = mesh;
        collider.sharedMesh = mesh;
    }

    public void SetBlock(int x, int y, int z, Block block)
    {
        if (InRange(x) && InRange(y) && InRange(z))
        {
            blocks[x, y, z] = block;
        }
        else
        {
            world.SetBlock(worldPosition.x + x, worldPosition.y + y, worldPosition.z + z, block);
        }
    }

    void Update()
    {
        if (update)
        {
            UpdateChunk();
            update = false;
        }
    }


}