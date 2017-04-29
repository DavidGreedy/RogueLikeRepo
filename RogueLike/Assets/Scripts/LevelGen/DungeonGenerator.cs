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

        int index = 0;

        while (index < 1000)
        {
            m_nextRoom = CreateRandomRoom();
            bool mapContainsRoom = mapBox.Contains(m_nextRoom);
            bool roomOk = CheckRoomPlacement(m_nextRoom, m_rooms);

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

        Cell currentCell = RandomCell();

        Stack<Cell> cellStack = new Stack<Cell>();

        Tree<Cell> maze = new Tree<Cell>(new Node<Cell>(currentCell, null));

        //path.Add(new Line(xPos, yPos, 1, 1));

        cellStack.Push(currentCell);

        int checkDir = 0;
        int result = 0;

        int prevDir = 1;

        print("BEGIN");
        while (cellStack.Count > 0)
        {
            result = CheckAdjacentMatches(currentCell.x, currentCell.y, CellState.EMPTY);
            if (result == 0) // if there are no free adjacent cells, need to backtrack
            {
                Cell junction = cellStack.Pop();
                currentCell.x = junction.x;
                currentCell.y = junction.y;
            }
            else // Continue path to free adjacent cell
            {
                Cell nextCell = GetRandomCell(currentCell.x, currentCell.y, result, ref prevDir);

                if ((result & (result - 1)) != 0) // if there is only one neighbour
                {
                    cellStack.Push(new Cell(currentCell.x, currentCell.y));
                }
                else // there is more than one neighbour so we need to add a new node to the tree
                {

                }

                path.Add(new Line(currentCell.Vector2 + (Vector2.one * 0.5f), nextCell.Vector2 + (Vector2.one * 0.5f)));

                currentCell.x = nextCell.x;
                currentCell.y = nextCell.y;

                m_map[currentCell.x, currentCell.y] = CellState.CORRIDOR;
            }
        }
        yield return new WaitForSeconds(0.001f);
    }

    int CheckAdjacentMatches(int x, int y, CellState matchState)
    {
        int result = 0;

        if (y < m_height - 1 && m_map[x, y + 1] == matchState)
        {
            result |= (1 << 0); // N
        }

        if (x < m_width - 1 && m_map[x + 1, y] == matchState)
        {
            result |= (1 << 1); // E
        }

        if (y > 1 && m_map[x, y - 1] == matchState)
        {
            result |= (1 << 2); // S
        }

        if (x > 1 && m_map[x - 1, y] == matchState)
        {
            result |= (1 << 3); // W
        }
        return result;
    }

    Cell GetRandomCell(int x, int y, int result, ref int dir)
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

        if (setBits.Contains(dir) && Random.value < 0.85f)
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
            return new Cell(x, y + 1);
        }
        if (randCell == 1)
        {
            return new Cell(x + 1, y);
        }
        if (randCell == 2)
        {
            return new Cell(x, y - 1);
        }
        if (randCell == 3)
        {
            return new Cell(x - 1, y);
        }
        return null;
    }

    Cell RandomCell()
    {
        bool pointOk = false;

        int x = 0;
        int y = 0;

        while (!pointOk)
        {
            x = (int)Random.Range(0, m_width);
            y = (int)Random.Range(0, m_height);

            pointOk = CheckPoint(x, y, m_rooms);
        }

        return new Cell(x, y);
    }

    bool CheckPoint(int x, int y, List<Box> boxes)
    {
        for (int i = 0; i < boxes.Count; i++)
        {
            if (boxes[i].Contains(x, y))
            {
                return false;
            }
        }
        return true;
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

    class Cell
    {
        public int x;
        public int y;

        public Cell(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public Vector2 Vector2 { get { return new Vector2(x, y); } }
    }
}