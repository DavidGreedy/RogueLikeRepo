using System;
using System.Collections.Generic;
using System.Linq;
using TriangleNet.Geometry;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelGenerator : MonoBehaviour
{
    private List<Room> m_rooms;
    private List<Room> m_selectedRooms;

    private TriangleNet.Mesh meshRepresentation;

    private List<Edge2D> spanningTree;

    private List<Edge> edges;

    [SerializeField]
    private int numRooms;

    [SerializeField]
    private int targetWidth;

    [SerializeField]
    private int targetHeight;

    [SerializeField]
    private bool isDone;

    void Start()
    {
        Time.timeScale = 100;
        CreateRoomsInCircle();
        OnPhysicsComplete += SelectRooms;
    }

    void CreateRoomsInCircle()
    {
        m_rooms = new List<Room>();

        for (int i = 0; i < numRooms; i++)
        {
            Vector2 randomPosition = Random.insideUnitCircle * 30.0f;

            m_rooms.Add(new Room());
            m_rooms[i].Init(transform, randomPosition, new Vector2(Mathf.Max(NormalDistribution(2) * targetWidth * 2.0f, 3), Mathf.Max(NormalDistribution(2) * targetHeight * 2.0f, 3)));
        }
    }

    float NormalDistribution(int samples)
    {
        float num = 0f;
        for (int i = 0; i < samples; i++)
        {
            num += Random.value;
        }
        return num / samples;
    }

    void SelectRooms()
    {
        Time.timeScale = 1;
        float meanWidth = m_rooms.Average(x => x.Rect.width);
        float meanHeight = m_rooms.Average(x => x.Rect.height);

        m_selectedRooms = m_rooms.Where(x => x.Rect.width * x.Rect.height > (meanWidth * meanHeight)).ToList();
        //m_rooms.Sort((x, y) => Mathf.Abs(x.Position.x * x.Position.y).CompareTo(Mathf.Abs(y.Position.x * y.Position.y)));
        m_selectedRooms = m_selectedRooms.Take(20).ToList();

        Delaunay(m_selectedRooms);
    }

    public event Action OnPhysicsComplete;

    private void Update()
    {
        if (isDone)
        {
            //for (int i = 0; i < m_rooms.Count; i++)
            //{
            //    for (int j = i; j < m_rooms.Count; j++)
            //    {
            //        Debug.DrawLine(m_rooms[i].Position, m_rooms[j].Position);
            //    }
            //}
        }

        if (!isDone)
        {
            foreach (Room Room in m_rooms)
            {
                Room.UpdatePosition();
            }
            foreach (Room Room in m_rooms)
            {
                if (!Room.rigidbody.IsSleeping())
                {
                    return;
                }
            }
            foreach (Room Room in m_rooms)
            {
                Destroy(Room.rigidbody);
            }
            isDone = true;

            if (OnPhysicsComplete != null)
            {
                OnPhysicsComplete.Invoke();
            }
        }
    }

    private Rect GetBounds()
    {
        float xMin = float.MaxValue;
        float xMax = float.MinValue;
        float yMin = float.MaxValue;
        float yMax = float.MinValue;

        foreach (Room Room in m_rooms)
        {
            xMin = Mathf.Min(Room.Rect.xMin, xMin);
            xMax = Mathf.Max(Room.Rect.xMax, xMax);
            yMin = Mathf.Min(Room.Rect.yMin, yMin);
            yMax = Mathf.Max(Room.Rect.yMax, yMax);
        }

        Rect bounds = new Rect();
        bounds.xMin = xMin;
        bounds.xMax = xMax;
        bounds.yMin = yMin;
        bounds.yMax = yMax;

        return bounds;
    }

    private void OnDrawGizmos()
    {
        if (m_rooms != null)
        {
            foreach (Room Room in m_rooms)
            {
                DrawRect(Room.Rect, Color.white);
            }
            DrawRect(GetBounds(), isDone ? Color.green : Color.red);
        }
        if (m_selectedRooms != null)
        {
            foreach (Room Room in m_selectedRooms)
            {
                DrawRect(Room.Rect, Color.red);
            }
        }
        //if (meshRepresentation != null)
        //{
        //    Gizmos.color = Color.blue;
        //    foreach (KeyValuePair<int, TriangleNet.Data.Triangle> pair in meshRepresentation.triangles)
        //    {
        //        TriangleNet.Data.Triangle triangle = pair.Value;

        //        TriangleNet.Data.Vertex vertex0 = triangle.GetVertex(0);
        //        TriangleNet.Data.Vertex vertex1 = triangle.GetVertex(1);
        //        TriangleNet.Data.Vertex vertex2 = triangle.GetVertex(2);

        //        Vector2 p0 = new Vector2((float)vertex0.x, (float)vertex0.y);
        //        Vector2 p1 = new Vector2((float)vertex1.x, (float)vertex1.y);
        //        Vector2 p2 = new Vector2((float)vertex2.x, (float)vertex2.y);

        //        Gizmos.DrawLine(p0, p1);
        //        Gizmos.DrawLine(p1, p2);
        //        Gizmos.DrawLine(p2, p0);
        //    }
        //}

        if (spanningTree != null)
        {
            for (int i = 0; i < spanningTree.Count; i++)
            {
                Gizmos.DrawLine(spanningTree[i][0], spanningTree[i][1]);
            }
        }
    }

    public static int Round(float n, int m)
    {
        return Mathf.FloorToInt(((n + m - 1) / m)) * m;
    }

    public void DrawRect(Rect rect, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawLine(new Vector2(rect.xMin, rect.yMax), new Vector2(rect.xMax, rect.yMax));
        Gizmos.DrawLine(new Vector2(rect.xMax, rect.yMax), new Vector2(rect.xMax, rect.yMin));
        Gizmos.DrawLine(new Vector2(rect.xMax, rect.yMin), new Vector2(rect.xMin, rect.yMin));
        Gizmos.DrawLine(new Vector2(rect.xMin, rect.yMin), new Vector2(rect.xMin, rect.yMax));
    }

    //public Vector2 COG(List<Room> rooms)
    //{
    //    Vector2 sum = new Vector2();
    //    for (int i = 0; i < rooms.Count; i++)
    //    {
    //        sum += rooms[i].Position;
    //    }

    //    return sum / rooms.Count;
    //}

    //public struct Triangle2D
    //{
    //    private Vector2[] points;

    //    public Triangle2D(Vector2 r0, Vector2 p1, Vector2 p2)
    //    {
    //        points = new Vector2[3];
    //        points[0] = r0;
    //        points[1] = p1;
    //        points[2] = p2;
    //    }

    //    public Vector2[] Positions()
    //    {
    //        return new Vector2[] { points[0], points[1], points[2] };
    //    }

    //    public float Area()
    //    {
    //        Vector2 v0 = points[1] - points[0];
    //        Vector2 v1 = points[2] - points[0];
    //        return (v0.x * v1.y - v0.y * v1.x);
    //    }

    //    public Vector2 this[int index]
    //    {
    //        get { return points[index]; }
    //        set { points[index] = value; }
    //    }
    //}

    public struct Edge2D
    {
        private Vector2[] points;

        public Edge2D(Vector2 p0, Vector2 p1)
        {
            points = new Vector2[2];
            points[0] = p0;
            points[1] = p1;
        }

        public Vector2 this[int i] { get { return points[i]; } }

        public float Length
        {
            get { return Vector2.Distance(points[0], points[1]); }
        }
    }

    public class Tree
    {
        private List<Edge2D> edges;

    }

    //void MakeCounterClockwise(ref Triangle2D tri, float EPS)
    //{
    //    float area = tri.Area();
    //    if (area < -EPS)
    //    {
    //        Vector2 ip1 = tri[1];
    //        Vector2 ip2 = tri[2];
    //        tri[1] = ip2;
    //        tri[2] = ip1;
    //    }
    //}

    public void Delaunay(List<Room> rooms)
    {
        meshRepresentation = new TriangleNet.Mesh();

        InputGeometry geometry = new InputGeometry();
        foreach (Room room in rooms)
        {
            geometry.AddPoint(room.Position.x, room.Position.y);
        }

        meshRepresentation.Triangulate(geometry);

        List<Edge2D> edges = new List<Edge2D>();

        foreach (KeyValuePair<int, TriangleNet.Data.Triangle> pair in meshRepresentation.triangles)
        {
            TriangleNet.Data.Triangle triangle = pair.Value;

            TriangleNet.Data.Vertex vertex0 = triangle.GetVertex(0);
            TriangleNet.Data.Vertex vertex1 = triangle.GetVertex(1);
            TriangleNet.Data.Vertex vertex2 = triangle.GetVertex(2);

            edges.Add(new Edge2D(new Vector2(vertex0.x, vertex0.y), new Vector2(vertex1.x, vertex1.y)));
            edges.Add(new Edge2D(new Vector2(vertex1.x, vertex1.y), new Vector2(vertex2.x, vertex2.y)));
            edges.Add(new Edge2D(new Vector2(vertex2.x, vertex2.y), new Vector2(vertex0.x, vertex0.y)));
        }


        List<Pair<Edge2D, float>> edgeWeights = new List<Pair<Edge2D, float>>();

        foreach (Edge2D edge in edges)
        {
            edgeWeights.Add(new Pair<Edge2D, float>(edge, edge.Length));
        }

        spanningTree = MinimumSpanningTree(edgeWeights);
    }

    private List<Edge2D> MinimumSpanningTree(List<Pair<Edge2D, float>> edges)
    {

        List<Edge2D> tree = new List<Edge2D>();

        List<Vector2> vertices = new List<Vector2>();

        edges.Sort((x, y) => x.Second.CompareTo(y.Second));

        for (int i = 0; i < edges.Count; i++)
        {
            vertices.Add(edges[i].First[0]);
            vertices.Add(edges[i].First[1]);
        }

        vertices = vertices.Distinct().ToList();

        //tree.Add(new Node<Vector2>(vertices[0], null));

        print(vertices.Count);

        for (int i = 0; i < vertices.Count; i++)
        {
            if (vertices.Contains(edges[i].First[0]) || vertices.Contains(edges[i].First[1]))
            {
                tree.Add(edges[i].First);
                vertices.Remove(edges[i].First[0]);
                vertices.Remove(edges[i].First[1]);
            }
        }

        print(tree.Count);
        return tree;
    }
}

public class Room : MonoBehaviour
{
    private BoxCollider2D collider;
    public Rigidbody2D rigidbody;
    private Rect rect;

    public Rect Rect { get { return rect; } }

    public Vector2 Position { get { return Rect.center; } }
    public Vector3 Size { get { return Rect.size; } }

    public float Ratio
    {
        get { return rect.width / rect.height; }
    }

    public void Init(Transform parent, Vector3 position, Vector2 size)
    {
        GameObject g = new GameObject();
        g.transform.position = position;
        g.transform.parent = parent;

        g.name = size.ToString();
        collider = g.AddComponent<BoxCollider2D>();
        collider.size = size;

        rigidbody = g.AddComponent<Rigidbody2D>();
        rigidbody.Sleep();
        rigidbody.gravityScale = 0;
        rigidbody.freezeRotation = true;
    }

    public void UpdatePosition()
    {
        rect.xMin = LevelGenerator.Round(collider.transform.position.x - (collider.size.x / 2f), 1);
        rect.xMax = LevelGenerator.Round(collider.transform.position.x + (collider.size.x / 2f), 1);
        rect.yMin = LevelGenerator.Round(collider.transform.position.y - (collider.size.y / 2f), 1);
        rect.yMax = LevelGenerator.Round(collider.transform.position.y + (collider.size.y / 2f), 1);
    }
}