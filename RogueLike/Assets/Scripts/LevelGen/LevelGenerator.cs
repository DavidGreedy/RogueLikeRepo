using System;
using System.Collections.Generic;
using System.Linq;
using TriangleNet.Data;
using TriangleNet.Geometry;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelGenerator : MonoBehaviour
{
    private List<Room> m_rooms;
    private List<Room> m_selectedRooms;

    private TriangleNet.Mesh meshRepresentation;

    private Graph.Edge[] spanningTree;
    private List<Pair<Room, Room>> connectedRooms;

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

        if (connectedRooms != null)
        {
            for (int i = 0; i < connectedRooms.Count; i++)
            {
                Gizmos.DrawLine(connectedRooms[i].First.Position, connectedRooms[i].Second.Position);

            }
        }
        //if (spanningTree != null)
        //{
        //    List<Vertex> vs = meshRepresentation.Vertices.ToList();

        //    for (int i = 0; i < spanningTree.Length; i++)
        //    {
        //        Vertex v0 = vs[spanningTree[i].src];
        //        Vertex v1 = vs[spanningTree[i].dest];

        //        Gizmos.DrawLine(new Vector2(v0.x, v0.y), new Vector2(v1.x, v1.y));
        //    }
        //}
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

    class Graph
    {
        // A class to represent a graph edge
        public class Edge : IComparable<Edge>
        {
            public int src, dest;
            public float weight;

            // Comparator function used for sorting edges based on
            // their weight
            public Edge()
            {
            }

            public Edge(int src, int dest, float weight)
            {
                this.src = src;
                this.dest = dest;
                this.weight = weight;
            }

            public int CompareTo(Edge compareEdge)
            {
                if (this.weight > compareEdge.weight)
                {
                    return 1;
                }
                else if (this.weight < compareEdge.weight)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }
        };

        // A class to represent a subset for union-find
        class Subset
        {
            public int parent, rank;
        };

        int V, E; // V-> no. of vertices & E->no.of edges
        Edge[] edge; // collection of all edges

        // Creates a graph with V vertices and E edges
        public Graph(List<int> vs, List<Edge> es)
        {
            V = vs.Count;
            E = es.Count;
            edge = es.ToArray();
        }

        // A utility function to find set of an element i
        // (uses path compression technique)
        int find(Subset[] subsets, int i)
        {
            // find root and make root as parent of i (path compression)
            if (subsets[i].parent != i)
                subsets[i].parent = find(subsets, subsets[i].parent);

            return subsets[i].parent;
        }

        // A function that does union of two sets of x and y
        // (uses union by rank)
        void Union(Subset[] subsets, int x, int y)
        {
            int xroot = find(subsets, x);
            int yroot = find(subsets, y);

            // Attach smaller rank tree under root of high rank tree
            // (Union by Rank)
            if (subsets[xroot].rank < subsets[yroot].rank)
                subsets[xroot].parent = yroot;
            else if (subsets[xroot].rank > subsets[yroot].rank)
                subsets[yroot].parent = xroot;

            // If ranks are same, then make one as root and increment
            // its rank by one
            else
            {
                subsets[yroot].parent = xroot;
                subsets[xroot].rank++;
            }
        }

        public Edge[] KruskalMST()
        {
            Edge[] result = new Edge[V]; // Tnis will store the resultant MST
            int e = 0; // An index variable, used for result[]
            int i = 0; // An index variable, used for sorted edges

            for (i = 0; i < V; ++i)
            {
                result[i] = new Edge();
            }

            // Step 1:  Sort all the edges in non-decreasing order of their
            // weight.  If we are not allowed to change the given graph, we
            // can create a copy of array of edges
            Array.Sort(edge);

            // Allocate memory for creating V ssubsets
            Subset[] subsets = new Subset[V];
            for (i = 0; i < V; ++i)
            {
                subsets[i] = new Subset();
            }

            // Create V subsets with single elements
            for (int v = 0; v < V; ++v)
            {
                subsets[v].parent = v;
                subsets[v].rank = 0;
            }

            i = 0; // Index used to pick next edge

            // Number of edges to be taken is equal to V-1
            while (e < V - 1)
            {
                // Step 2: Pick the smallest edge. And increment the index
                // for next iteration
                Edge next_edge = new Edge();
                next_edge = edge[i++];

                int x = find(subsets, next_edge.src);
                int y = find(subsets, next_edge.dest);

                // If including this edge does't cause cycle, include it
                // in result and increment the index of result for next edge
                if (x != y)
                {
                    result[e++] = next_edge;
                    Union(subsets, x, y);
                }
                // Else discard the next_edge
            }

            return result;

            // print the contents of result[] to display the built MST
            print("Following are the edges in the constructed MST");
            for (i = 0; i < e; ++i)
            {
                print(result[i].src + " -- " + result[i].dest + " == " + result[i].weight);
            }
        }
    }

    public void Delaunay(List<Room> rooms)
    {
        meshRepresentation = new TriangleNet.Mesh();

        InputGeometry geometry = new InputGeometry();

        Dictionary<Pair<int, int>, Room> roomDict = new Dictionary<Pair<int, int>, Room>();

        foreach (Room room in rooms)
        {
            geometry.AddPoint(room.Position.x, room.Position.y);
            roomDict.Add(new Pair<int, int>((int)room.Position.x, (int)room.Position.y), room);
        }

        meshRepresentation.Triangulate(geometry);

        List<Vertex> verticesRaw = meshRepresentation.Vertices.ToList();
        List<int> verticesInt = new List<int>();

        print(verticesRaw.Count);

        for (int i = 0; i < verticesRaw.Count; i++)
        {
            verticesInt.Add(verticesRaw[i].ID);
        }

        List<Edge> edgesRaw = meshRepresentation.Edges.ToList();
        List<Graph.Edge> edgesInt = new List<Graph.Edge>();

        print(edgesRaw.Count);

        for (int i = 0; i < edgesRaw.Count; i++)
        {
            edgesInt.Add(new Graph.Edge(edgesRaw[i].P0, edgesRaw[i].P1, Vector2.Distance(new Vector2(verticesRaw[edgesRaw[i].P0].x, verticesRaw[edgesRaw[i].P0].y), new Vector2(verticesRaw[edgesRaw[i].P1].x, verticesRaw[edgesRaw[i].P1].y))));
        }

        Graph graph = new Graph(verticesInt, edgesInt);

        spanningTree = graph.KruskalMST();

        connectedRooms = new List<Pair<Room, Room>>();

        for (int i = 0; i < spanningTree.Length; i++)
        {
            Room room0;
            Room room1;

            roomDict.TryGetValue(new Pair<int, int>((int)verticesRaw[spanningTree[i].src].x, (int)verticesRaw[spanningTree[i].src].y), out room0);
            roomDict.TryGetValue(new Pair<int, int>((int)verticesRaw[spanningTree[i].dest].x, (int)verticesRaw[spanningTree[i].dest].y), out room1);

            connectedRooms.Add(new Pair<Room, Room>(room0, room1));
        }


        print(connectedRooms.Count);
        //spanningTree = MinimumSpanningTree(edgeWeights);
    }

    //private List<Edge2D> MinimumSpanningTree(List<Vertex> vertices, List<Edge> edges)
    //{
    //List<Edge2D> tree = new List<Edge2D>();

    //List<Vector2> vertices = new List<Vector2>();

    //edges.Sort((x, y) => x.Second.CompareTo(y.Second));

    //for (int i = 0; i < edges.Count; i++)
    //{
    //    vertices.Add(edges[i].First[0]);
    //    vertices.Add(edges[i].First[1]);
    //}

    //vertices = vertices.Distinct().ToList();

    //print(vertices.Count);

    //for (int i = 0; i < edges.Count; i++)
    //{
    //    if (vertices.Contains(edges[i].First[0]) || vertices.Contains(edges[i].First[1]))
    //    {
    //        tree.Add(edges[i].First);
    //        vertices.Remove(edges[i].First[0]);
    //        vertices.Remove(edges[i].First[1]);
    //    }
    //}

    //print(tree.Count);

    //}
}

public class Room
{
    private GameObject gameObject;
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
        gameObject = new GameObject();
        gameObject.transform.position = position;
        gameObject.transform.parent = parent;

        gameObject.name = size.ToString();
        collider = gameObject.AddComponent<BoxCollider2D>();
        collider.size = size;

        rigidbody = gameObject.AddComponent<Rigidbody2D>();
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