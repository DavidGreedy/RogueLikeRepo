using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshData
{
    public List<Vector3> vertices = new List<Vector3>();
    public List<int> indices = new List<int>();
    public List<Vector3> normals = new List<Vector3>();
    public List<Vector2> uvs = new List<Vector2>();


    public void AddVertex(Vector3 vertex)
    {
        vertices.Add(vertex);
    }

    public void AddVertex(float x, float y, float z)
    {
        vertices.Add(new Vector3(x, y, z));
    }

    public void AddIndex(int index)
    {
        indices.Add(index);
    }

    public void CreateQuadFromLast()
    {
        AddIndex(vertices.Count - 4);
        AddIndex(vertices.Count - 3);
        AddIndex(vertices.Count - 2);
        AddIndex(vertices.Count - 4);
        AddIndex(vertices.Count - 2);
        AddIndex(vertices.Count - 1);
    }
}