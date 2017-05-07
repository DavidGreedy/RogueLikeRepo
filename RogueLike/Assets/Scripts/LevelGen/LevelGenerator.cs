using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Random = UnityEngine.Random;

public struct LevelData
{
    public List<Room> m_rooms;
    public LevelDebugView.SearchGrid<NodeState> data;
}

public class LevelGenerator : MonoBehaviour
{
    private List<Room> m_rooms;
    private List<Pair<Room, Room>> connectedRooms;

    private LevelData m_levelData;

    private List<Room> m_selectedRooms;

    private Graph.Edge[] spanningTree;

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
    private bool m_isPhysicsComplete;

    [SerializeField]
    private bool m_isGenerationComplete;

    public event Action OnGenerationComplete;

    void Start()
    {
        Time.timeScale = 100;
        CreateRoomsInCircle();
        OnPhysicsComplete += RoomLogic;
    }

    void CreateRoomsInCircle()
    {
        m_rooms = new List<Room>();

        for (int i = 0; i < m_numInitialRooms; i++)
        {
            Vector2 randomPosition = Random.insideUnitCircle * 30.0f;

            m_rooms.Add(new Room());
            m_rooms[i].Init(transform, randomPosition, new Vector2(ExtensionMethods.ToEvenGridPosition(Random.Range(4, 10), 2), ExtensionMethods.ToEvenGridPosition(Random.Range(4, 10), 2)));
        }
    }

    void RoomLogic()
    {
        Time.timeScale = 1;
        SelectRooms();

        foreach (Room room in m_rooms)
        {
            room.RemoveCollider();
        }

        m_isGenerationComplete = true;

        if (OnGenerationComplete != null)
        {
            OnGenerationComplete.Invoke();
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
        float meanWidth = m_rooms.Average(x => x.Rect.width);
        float meanHeight = m_rooms.Average(x => x.Rect.height);

        m_selectedRooms = m_rooms.Where(x => x.Rect.width * x.Rect.height > (meanWidth * meanHeight)).ToList();
        m_selectedRooms = m_selectedRooms.Take(m_numMainRooms).ToList();

        foreach (Room room in m_selectedRooms)
        {
            room.FinalisePosition();
        }
    }

    public event Action OnPhysicsComplete;

    private void Update()
    {
        if (!m_isPhysicsComplete)
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
            m_isPhysicsComplete = true;

            if (OnPhysicsComplete != null)
            {
                OnPhysicsComplete.Invoke();
            }
        }
    }

    private Rect GetBounds(List<Room> rooms)
    {
        float xMin = float.MaxValue;
        float xMax = float.MinValue;
        float yMin = float.MaxValue;
        float yMax = float.MinValue;

        foreach (Room Room in rooms)
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

    public LevelData BuildLevelData()
    {
        m_levelData = new LevelData();

        m_levelData.m_rooms = m_selectedRooms;

        Rect boundingRect = GetBounds(m_selectedRooms);

        int width = Mathf.RoundToInt(boundingRect.size.x);
        int height = Mathf.RoundToInt(boundingRect.size.y);

        int spacing = 8;
        int halfSpacing = spacing / 2;

        m_levelData.data = new LevelDebugView.SearchGrid<NodeState>(width + spacing, height + spacing, NodeState.Empty);

        foreach (Room Room in m_selectedRooms)
        {
            int xStart = Mathf.RoundToInt(Room.Rect.xMin - boundingRect.xMin + halfSpacing);
            int xEnd = Mathf.RoundToInt(Room.Rect.xMax - boundingRect.xMin + halfSpacing);
            int yStart = Mathf.RoundToInt(Room.Rect.yMin - boundingRect.yMin + halfSpacing);
            int yEnd = Mathf.RoundToInt(Room.Rect.yMax - boundingRect.yMin + halfSpacing);

            Room.Rect = new Rect(Rect.MinMaxRect(xStart, yStart, xEnd, yEnd));

            for (int y = yStart - 1; y < yEnd + 1; y++)
            {
                for (int x = xStart - 1; x < xEnd + 1; x++)
                {
                    if (x < xStart || y < yStart || x >= xEnd || y >= yEnd)
                    {
                        m_levelData.data.SetNode(x, y, NodeState.Wall);
                    }
                    else
                    {
                        m_levelData.data.SetNode(x, y, NodeState.Room);
                    }
                }
            }
        }
        return m_levelData;
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
            //if (spanningTree[i].weight > 0)
            {
                connectedRooms.Add(new Pair<Room, Room>(m_selectedRooms[spanningTree[i].src], m_selectedRooms[spanningTree[i].dest]));
            }
        }

        int maxConnections = Mathf.FloorToInt(roomEdges.Count * 0.1f);//10% of all edges
        int currentConnections = 0;
        for (int i = 0; i < roomEdges.Count; i++)
        {
            if (roomEdges[i].weight < m_connectionThresholdDistance)
            {
                connectedRooms.Add(new Pair<Room, Room>(m_selectedRooms[roomEdges[i].src], m_selectedRooms[roomEdges[i].dest]));
                currentConnections++;
            }
            if (currentConnections > maxConnections)
            {
                break;
            }
        }
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
        set { rect = value; }
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
        rect.xMin = ExtensionMethods.ToOddGridPosition(collider.transform.position.x - (collider.size.x / 2f), 1) + 1;
        rect.xMax = ExtensionMethods.ToOddGridPosition(collider.transform.position.x + (collider.size.x / 2f), 1);
        rect.yMin = ExtensionMethods.ToOddGridPosition(collider.transform.position.y - (collider.size.y / 2f), 1) + 1;
        rect.yMax = ExtensionMethods.ToOddGridPosition(collider.transform.position.y + (collider.size.y / 2f), 1);
    }

    public void FinalisePosition()
    {
        UpdatePosition();
        collider.size = rect.size;
        //collider.transform.position = rect.center;
        GameObject.Destroy(rigidbody);
        gameObject.name = collider.size.ToString();
    }

    public void RemoveCollider()
    {
        //GameObject.Destroy(collider);
    }
}