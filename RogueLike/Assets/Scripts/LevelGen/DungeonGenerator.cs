using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField]
    private uint m_width, m_height;

    [SerializeField]
    private uint m_roomWidthAverage, m_roomHeightAverage;

    private List<Line> path;

    private enum CellState
    {
        EMPTY = 0,
        WALL = 1,
        ROOM = 2,
        CORRIDOR = 3
    }

    private CellState[,] m_map;

    private List<Box> m_rooms;
    private List<Box> m_failedRooms;

    private Box m_nextRoom;

    void Start()
    {
        m_map = new CellState[m_width, m_height];

        for (int y = 0; y < m_height; y++)
        {
            for (int x = 0; x < m_width; x++)
            {
                m_map[x, y] = CellState.EMPTY;
            }
        }
        path = new List<Line>();

        int index = 0;

        bool placementGood = false;

        m_rooms = new List<Box>();
        m_failedRooms = new List<Box>();

        StartCoroutine(PlaceRooms());
        //StartCoroutine(BuildMaze());
    }

    IEnumerator PlaceRooms()
    {
        Box mapBox = new Box(0, 0, (int)m_width, (int)m_height);

        //m_nextRoom = CreateRandomRoom();

        bool mapContainsRoom = false;
        bool roomOk = false;

        int index = 0;

        while (index < 1000)
        {
            m_nextRoom = CreateRandomRoom();
            mapContainsRoom = mapBox.Contains(m_nextRoom);
            roomOk = CheckRoomPlacement(m_nextRoom, m_rooms);

            index = 0;
            while (!mapContainsRoom || !roomOk)
            {
                m_failedRooms.Add(m_nextRoom);
                m_nextRoom = CreateRandomRoom();
                mapContainsRoom = mapBox.Contains(m_nextRoom);
                roomOk = CheckRoomPlacement(m_nextRoom, m_rooms);
                //yield return new WaitForSeconds(0);
                index++;
            }
            m_rooms.Add(m_nextRoom);
        }
        print("LEVEL GENERATION COPMPLETE " + m_rooms.Count + " ROOMS CREATED");

        for (int i = 0; i < m_rooms.Count; i++)
        {
            AddRoomToMap(m_rooms[i]);
        }
        StartCoroutine(BuildMaze());
        yield return null;
    }

    void Update()
    {
        //for (int i = 0; i < m_failedRooms.Count; i++)
        //{
        //    Color c = new Color(0, 0, 1, (i / (float)m_failedRooms.Count) * i);
        //    m_failedRooms[i].Draw(c, c);
        //}

        //if (m_failedRooms.Count > 20)
        //{
        //    m_failedRooms.RemoveAt(0);
        //}

        //for (int i = 0; i < m_rooms.Count; i++)
        //{
        //    //m_rooms[i].Draw(Color.clear, Color.red);
        //}

        //if (m_nextRoom != null)
        //{
        //    //m_nextRoom.Draw(Color.clear, Color.green);
        //}
    }

    IEnumerator BuildMaze()
    {
        //TODO: Space the corridors 1 cell away from each other
        //TODO: Unwind the corridors to create a MST
        int xPos = 0, yPos = 0;

        Stack<Pair<int, int>> junctions = new Stack<Pair<int, int>>();

        Tree<Pair<int, int>> maze =
            new Tree<Pair<int, int>>(new Node<Pair<int, int>>(new Pair<int, int>(xPos, yPos), null));

        //path.Add(new Line(xPos, yPos, 1, 1));

        junctions.Push(new Pair<int, int>(xPos, yPos));

        int checkDir = 0;
        int result = 0;

        int prevDir = 1;

        print("BEGIN");
        while (junctions.Count > 0)
        {
            result = 0;
            if (yPos < m_height - 1 && m_map[xPos, yPos + 1] == CellState.EMPTY)
            {
                result |= (1 << 0); // N
            }

            if (xPos < m_width - 1 && m_map[xPos + 1, yPos] == CellState.EMPTY)
            {
                result |= (1 << 1); // E
            }

            if (yPos > 1 && m_map[xPos, yPos - 1] == CellState.EMPTY)
            {
                result |= (1 << 2); // S
            }

            if (xPos > 1 && m_map[xPos - 1, yPos] == CellState.EMPTY)
            {
                result |= (1 << 3); // W
            }
            if (result == 0) // if there are no free adjacent cells, need to backtrack
            {
                Pair<int, int> junction = junctions.Pop();
                xPos = junction.First;
                yPos = junction.Second;
            }
            else // Continue path to free adjacent cell
            {
                Pair<int, int> nextCell;
                int changeDir = prevDir;

                nextCell = GetRandomCell(xPos, yPos, result, ref prevDir);

                //if (changeDir != prevDir)
                {
                    junctions.Push(new Pair<int, int>(xPos, yPos));
                }

                path.Add(new Line(new Vector2(xPos, yPos), new Vector2(nextCell.First, nextCell.Second)));

                xPos = nextCell.First;
                yPos = nextCell.Second;


                m_map[xPos, yPos] = CellState.CORRIDOR;
            }
        }
        yield return new WaitForSeconds(0.001f);
    }

    Pair<int, int> GetRandomCell(int x, int y, int result, ref int dir)
    {
        int randCell = 0;
        List<int> setBits = new List<int>();

        if (((result >> 0) & 1) == 1)
        {
            setBits.Add(0);
        }
        if (((result >> 1) & 1) == 1)
        {
            setBits.Add(1);
        }
        if (((result >> 2) & 1) == 1)
        {
            setBits.Add(2);
        }
        if (((result >> 3) & 1) == 1)
        {
            setBits.Add(3);
        }

        if (setBits.Contains(dir) && Random.value < 0.65f)
        {
            randCell = dir;
        }
        else
        {
            randCell = setBits[Random.Range(0, setBits.Count)];
        }

        dir = randCell;
        if (randCell == 0)
        {
            return new Pair<int, int>(x, y + 1);
        }
        if (randCell == 1)
        {
            return new Pair<int, int>(x + 1, y);
        }
        if (randCell == 2)
        {
            return new Pair<int, int>(x, y - 1);
        }
        if (randCell == 3)
        {
            return new Pair<int, int>(x - 1, y);
        }
        return null;
    }

    int CheckPosition(int x, int y, int dir)
    {
        int result = 0;
        if ((dir >> 0 & 1) == 1)
        {
            result |= m_map[x, y - 1] == CellState.EMPTY ? 0 : 1 << 1; // N
        }
        if ((dir >> 1 & 1) == 1)
        {
            result |= m_map[x + 1, y] == CellState.EMPTY ? 0 : 1 << 2; // E
        }
        if ((dir >> 2 & 1) == 1)
        {
            result |= m_map[x, y + 1] == CellState.EMPTY ? 0 : 1 << 3; // S
        }
        if ((dir >> 3 & 1) == 1)
        {
            result |= m_map[x - 1, y] == CellState.EMPTY ? 0 : 1 << 4; // W
        }
        return result;
    }

    bool CheckRoomPlacement(Box box, List<Box> boxes)
    {
        for (int i = 0; i < boxes.Count; i++)
        {
            if (boxes[i].Intersects(box))
            {
                return false;
            }
        }
        return true;
    }

    Box CreateRandomRoom()
    {
        int x, y, w, h;

        w = Random.Range(5, 15);
        h = Random.Range(5, 15);

        x = (int)Random.Range(1, m_width - w);
        y = (int)Random.Range(1, m_height - h);

        return new Box(x, y, w, h);
    }

    void CreateRoomAt(int x, int y, int w, int h, bool force = false)
    {
        if (x > m_width || y > m_height)
        {
            Debug.Log("Out of range");
            return;
        }
        for (int j = y; j < y + h; j++)
        {
            for (int i = x; i < x + w; i++)
            {
                m_map[i, j] = CellState.ROOM;
            }
        }
    }

    void AddRoomToMap(Box room)
    {
        for (int j = room.y; j < room.y + room.h; j++)
        {
            for (int i = room.x; i < room.x + room.w; i++)
            {
                m_map[i, j] = CellState.ROOM;
            }
        }
    }

    void DrawTile(int x, int y)
    {
        Color c = m_map[x, y] == CellState.ROOM ? Color.red : Color.blue;
        Handles.DrawSolidRectangleWithOutline(new Rect(x, y, 1, 1), c, Color.clear);
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            for (int y = 0; y < m_height; y++)
            {
                for (int x = 0; x < m_width; x++)
                {
                    if (m_map[x, y] == CellState.ROOM)
                        DrawTile(x, y);
                }
            }

            for (int i = 0; i < path.Count; i++)
            {
                Gizmos.DrawLine(path[i].start, path[i].end);
            }
        }
    }

    private class Box
    {
        public int x, y, w, h;

        public Box(int x, int y, int w, int h)
        {
            this.x = x;
            this.y = y;
            this.w = w;
            this.h = h;
        }

        public bool Contains(int x, int y)
        {
            return (x > this.x && x < this.x + this.w && y > this.y && y < this.y + this.h);
        }

        public bool LiesOnEdge(int x, int y)
        {
            return (x >= this.x || x <= this.x + this.w || y >= this.y || y <= this.y + this.h);
        }

        public bool LiesOnEdge(Box other)
        {
            return (this.x == other.x + other.w || this.x + this.w == other.x ||
                    this.y == other.y + other.h || this.y + this.h == other.y);
        }

        public bool Contains(Box other)
        {
            return (this.x < other.x
                 && this.y < other.y
                 && this.x + this.w > other.x + other.w
                 && this.y + this.h > other.y + other.h);
        }

        public bool Intersects(Box other)
        {
            return !(this.x + this.w < other.x ||
                     this.x > other.x + other.w ||
                     this.y + this.h < other.y ||
                     this.y > other.y + other.h);
        }

        public Box Intersection(Box other)
        {
            int xMin = Mathf.Max(this.x, other.x);
            int yMin = Mathf.Max(this.y, other.y);
            int xMax = Mathf.Min(this.x + this.w, other.x + other.w);
            int yMax = Mathf.Min(this.y + this.h, other.y + other.h);

            return new Box(xMin, yMin, xMax - xMin, yMax - yMin);
        }

        public void Draw(Color fill, Color outline)
        {
            //Handles.DrawSolidRectangleWithOutline(new Rect(x, y, w, h), fill, outline);

            Debug.DrawLine(new Vector2(x, y + h), new Vector2(x + w, y + h), outline);
            Debug.DrawLine(new Vector2(x + w, y + h), new Vector2(x + w, y), outline);
            Debug.DrawLine(new Vector2(x + w, y), new Vector2(x, y), outline);
            Debug.DrawLine(new Vector2(x, y), new Vector2(x, y + h), outline);
        }
    }
}