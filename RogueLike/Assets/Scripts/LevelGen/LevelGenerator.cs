using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelGenerator : MonoBehaviour
{
    private List<Room> m_rooms;
    private List<Room> m_selectedRooms;

    private Graph.Edge[] spanningTree;
    private List<Pair<Room, Room>> connectedRooms;


    private List<Pair<Vector2, Vector2>> path;
    private List<Pair<Vector2, Vector2>> border;
    private List<Pair<Vector2, Vector2>> doors;

    private List<Rect> pathAreas;

    [SerializeField]
    private int m_numInitialRooms;

    [SerializeField]
    private int m_numMainRooms;

    [SerializeField]
    private float m_connectionThresholdDistance;

    [SerializeField]
    private int targetWidth;

    [SerializeField]
    private int targetHeight;

    [SerializeField]
    private float m_pathSize;

    [SerializeField]
    private float doorSize;

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

        for (int i = 0; i < m_numInitialRooms; i++)
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
        m_selectedRooms = m_selectedRooms.Take(m_numMainRooms).ToList();

        foreach (Room room in m_rooms)
        {
            room.FinalisePosition();
        }

        Connect();
        //TODO: Reintroduce Delauny to remove small angled edges

        GeneratePaths();
    }

    private Pair<Vector2, Vector2> GetDoor(Pair<Vector2, Vector2> line)
    {
        Vector2 doorPos;
        float xStart, xEnd, yStart, yEnd;
        float pathSizeHalf = m_pathSize / 2.0f;

        if (Math.Abs(line.First.y - line.Second.y) < 0.01f)
        {
            xStart = Mathf.Max(line.First.x, line.Second.x) - pathSizeHalf;
            xEnd = Mathf.Min(line.First.x, line.Second.x) + pathSizeHalf;
            doorPos = new Vector2(Random.Range(xStart, xEnd), line.First.y);
            return new Pair<Vector2, Vector2>(doorPos - new Vector2(pathSizeHalf, 0), doorPos + new Vector2(pathSizeHalf, 0));
        }
        else if (Math.Abs(line.First.x - line.Second.x) < 0.01f)
        {
            yStart = Mathf.Max(line.First.y, line.Second.y) - pathSizeHalf;
            yEnd = Mathf.Min(line.First.y, line.Second.y) + pathSizeHalf;
            doorPos = new Vector2(line.First.x, Random.Range(yStart, yEnd));
            return new Pair<Vector2, Vector2>(doorPos - new Vector2(0, pathSizeHalf), doorPos + new Vector2(0, pathSizeHalf));
        }
        throw new Exception("DOOR NOT CREATED");
    }

    private void GeneratePaths()
    {
        // NEED TO WORK OUT THE DIRECTION
        float pathSize = m_pathSize / 2.0f;
        path = new List<Pair<Vector2, Vector2>>();
        border = new List<Pair<Vector2, Vector2>>();
        doors = new List<Pair<Vector2, Vector2>>();

        for (int i = 0; i < connectedRooms.Count; i++)
        {
            Room r0 = connectedRooms[i].First;
            Room r1 = connectedRooms[i].Second;

            float xDiff = r0.Position.x - r1.Position.x; // - (Mathf.Abs(r0.Size.x) + Mathf.Abs(r0.Size.x));
            float yDiff = r0.Position.y - r1.Position.y; // - (Mathf.Abs(r0.Size.y) + Mathf.Abs(r1.Size.y));

            //path.Add(r0.Position + ((Vector2)r0.Size / 2.0f));

            int xMin = Mathf.Max((int)r0.Rect.xMin, (int)r1.Rect.xMin);
            int xMax = Mathf.Min((int)r0.Rect.xMax, (int)r1.Rect.xMax);

            int yMin = Mathf.Max((int)r0.Rect.yMin, (int)r1.Rect.yMin);
            int yMax = Mathf.Min((int)r0.Rect.yMax, (int)r1.Rect.yMax);

            int xCommon = xMax - xMin;
            int yCommon = yMax - yMin;

            Pair<Vector2, Vector2> bP = r0.Rect.Border(r1.Rect);

            //xCommon = Mathf.Abs(bP.First.x - bP.Second.x);
            //yCommon = Mathf.Abs(bP.First.y - bP.Second.y);

            //border.Add(bP);

            // Trivial straight line cases
            if (xCommon > m_pathSize || yCommon > m_pathSize)
            {
                float randX = Mathf.Lerp(Mathf.RoundToInt(xMin + pathSize), Mathf.RoundToInt(xMax - pathSize), 0.5f);
                float randY = Mathf.Lerp(Mathf.RoundToInt(yMin + pathSize), Mathf.RoundToInt(yMax - pathSize), 0.5f);

                Vector2 randomPathStart = new Vector2(xCommon > pathSize ? randX : xMin, yCommon > pathSize ? randY : yMin);
                Vector2 randomPathEnd = new Vector2(xCommon > pathSize ? randX : xMax, yCommon > pathSize ? randY : yMax);

                if (r0.Rect.IsTouching(r1.Rect))
                {
                    Pair<Vector2, Vector2> borderPair = r0.Rect.Border(r1.Rect);
                    border.Add(borderPair);

                    doors.Add(GetDoor(borderPair));
                }
                else
                {
                    path.Add(new Pair<Vector2, Vector2>(randomPathStart, randomPathEnd));
                }
            }
            //More complicated bends
            //TODO: Check for corners that intersect rooms
            else
            {
                if (Random.value > 0.5f)
                {
                    path.Add(r0.GetHorizontalTo(r1));
                    path.Add(r1.GetVerticalTo(r0));
                }
                else
                {
                    path.Add(r1.GetHorizontalTo(r0));
                    path.Add(r0.GetVerticalTo(r1));
                }
            }
        }
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
            //foreach (Room Room in m_rooms)
            //{
            //    DrawRect(Room.Rect, Color.clear, Color.white);
            //}
            DrawRect(GetBounds(), Color.clear, isDone ? Color.green : Color.red);
        }
        if (m_selectedRooms != null)
        {
            foreach (Room Room in m_selectedRooms)
            {
                DrawRect(Room.Rect, Color.clear, Color.red);
            }
        }
        if (connectedRooms != null)
        {
            //for (int i = 0; i < connectedRooms.Count; i++)
            //{
            //    Gizmos.DrawLine(connectedRooms[i].First.Position, connectedRooms[i].Second.Position);
            //}
        }

        if (path != null)
        {
            Gizmos.color = Color.magenta;
            for (int i = 0; i < path.Count; i++)
            {
                //Rect pathRect = new Rect();
                //pathRect.xMin = Mathf.Min(path[i].First.x, path[i].Second.x);
                //pathRect.xMax = Mathf.Max(path[i].First.x, path[i].Second.x);
                //pathRect.yMin = Mathf.Min(path[i].First.y, path[i].Second.y);
                //pathRect.yMax = Mathf.Max(path[i].First.y, path[i].Second.y);

                //pathRect.size = new Vector2(Mathf.Max(pathRect.size.x, m_pathSize), Mathf.Max(pathRect.size.y, m_pathSize));

                //DrawRect(pathRect, Color.red, Color.red);
                //Gizmos.DrawSphere(path[i].First, 1f);
                //Gizmos.DrawSphere(path[i].Second, 1f);

                Gizmos.DrawLine(path[i].First, path[i].Second);
            }
        }

        if (border != null)
        {
            Gizmos.color = Color.white;
            for (int i = 0; i < border.Count; i++)
            {
                //Rect pathRect = new Rect();
                //pathRect.xMin = Mathf.Min(path[i].First.x, path[i].Second.x);
                //pathRect.xMax = Mathf.Max(path[i].First.x, path[i].Second.x);
                //pathRect.yMin = Mathf.Min(path[i].First.y, path[i].Second.y);
                //pathRect.yMax = Mathf.Max(path[i].First.y, path[i].Second.y);

                //pathRect.size = new Vector2(Mathf.Max(pathRect.size.x, m_pathSize), Mathf.Max(pathRect.size.y, m_pathSize));

                //DrawRect(pathRect, Color.red, Color.red);
                //Gizmos.DrawSphere(path[i].First, 1f);
                //Gizmos.DrawSphere(path[i].Second, 1f);

                Gizmos.DrawLine(border[i].First, border[i].Second);

            }
        }

        if (doors != null)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < doors.Count; i++)
            {
                //Rect pathRect = new Rect();
                //pathRect.xMin = Mathf.Min(path[i].First.x, path[i].Second.x);
                //pathRect.xMax = Mathf.Max(path[i].First.x, path[i].Second.x);
                //pathRect.yMin = Mathf.Min(path[i].First.y, path[i].Second.y);
                //pathRect.yMax = Mathf.Max(path[i].First.y, path[i].Second.y);

                //pathRect.size = new Vector2(Mathf.Max(pathRect.size.x, m_pathSize), Mathf.Max(pathRect.size.y, m_pathSize));

                //DrawRect(pathRect, Color.red, Color.red);
                //Gizmos.DrawSphere(path[i].First, 1f);
                //Gizmos.DrawSphere(path[i].Second, 1f);

                Gizmos.DrawLine(doors[i].First, doors[i].Second);

            }
        }
    }

    public static int Round(float n, int m)
    {
        return Mathf.FloorToInt(((n + m - 1) / m)) * m;
    }

    public void DrawRect(Rect rect, Color fill, Color outline)
    {
        Handles.DrawSolidRectangleWithOutline(rect, fill, outline);
        //Gizmos.DrawLine(new Vector2(rect.xMin, rect.yMax), new Vector2(rect.xMax, rect.yMax));
        //Gizmos.DrawLine(new Vector2(rect.xMax, rect.yMax), new Vector2(rect.xMax, rect.yMin));
        //Gizmos.DrawLine(new Vector2(rect.xMax, rect.yMin), new Vector2(rect.xMin, rect.yMin));
        //Gizmos.DrawLine(new Vector2(rect.xMin, rect.yMin), new Vector2(rect.xMin, rect.yMax));
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
        }
    }

    public void Connect()
    {
        List<Graph.Edge> roomEdges = new List<Graph.Edge>();

        for (int i = 0; i < m_selectedRooms.Count; i++)
        {
            for (int j = i; j < m_selectedRooms.Count; j++)
            {
                roomEdges.Add(new Graph.Edge(i, j, Vector2.Distance(m_selectedRooms[i].Position, m_selectedRooms[j].Position)));
            }
        }

        roomEdges = roomEdges.Distinct().ToList();
        //Sort room edges by weight
        roomEdges.Sort((x, y) => x.weight.CompareTo(y.weight));

        List<int> roomIndices = new List<int>();

        for (int i = 0; i < m_selectedRooms.Count; i++)
        {
            roomIndices.Add(i);
        }

        Graph graph = new Graph(roomIndices, roomEdges);

        spanningTree = graph.KruskalMST();

        connectedRooms = new List<Pair<Room, Room>>();

        for (int i = 0; i < spanningTree.Length; i++)
        {
            if (spanningTree[i].weight > 0)
            {
                connectedRooms.Add(new Pair<Room, Room>(m_selectedRooms[spanningTree[i].src], m_selectedRooms[spanningTree[i].dest]));
            }
        }

        //Adds any edge whose weight is less than the threshold value
        //for (int i = 0; i < roomEdges.Count; i++)
        //{
        //    if (roomEdges[i].weight < m_connectionThresholdDistance)
        //    {
        //        connectedRooms.Add(new Pair<Room, Room>(m_selectedRooms[roomEdges[i].src], m_selectedRooms[roomEdges[i].dest]));
        //    }
        //    else
        //    {
        //        break;
        //    }
        //}

        connectedRooms = connectedRooms.Distinct().ToList();
    }
}

public class Room
{
    private GameObject gameObject;
    private BoxCollider2D collider;
    public Rigidbody2D rigidbody;
    private Rect rect;

    public Rect Rect
    {
        get { return rect; }
    }

    public Vector2 Position
    {
        get { return Rect.center; }
    }

    public Vector3 Size
    {
        get { return Rect.size; }
    }

    public void Init(Transform parent, Vector3 position, Vector2 size)
    {
        gameObject = new GameObject();
        gameObject.transform.position = position;
        gameObject.transform.parent = parent;

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

    public void FinalisePosition()
    {
        UpdatePosition();
        collider.size = rect.size;
        collider.transform.position = rect.center;
        GameObject.Destroy(rigidbody);
        gameObject.name = collider.size.ToString();
    }

    public Pair<Vector2, Vector2> GetHorizontalTo(Room other)
    {
        return new Pair<Vector2, Vector2>(
            new Vector2(other.Position.x > Position.x ? Rect.xMax : Rect.xMin, Position.y),
            new Vector2(other.Position.x, Position.y));
    }

    public Pair<Vector2, Vector2> GetVerticalTo(Room other)
    {
        return
            new Pair<Vector2, Vector2>(
                new Vector2(Position.x, other.Position.y > Position.y ? Rect.yMax : Rect.yMin),
                new Vector2(Position.x, other.Position.y));
    }
}