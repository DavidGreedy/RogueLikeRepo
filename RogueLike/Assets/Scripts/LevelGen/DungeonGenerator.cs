using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField]
    private uint m_width, m_height;

    [SerializeField]
    private uint m_roomWidthAverage, m_roomHeightAverage;

    private List<CellNode> pathNodes;

    private CellNode m_rootNode;

    private enum CellState
    {
        EMPTY = 0,
        WALL = 1,
        ROOM = 2,
        CORRIDOR = 3,
        DOOR = 4
    }

    private CellNode[,] m_map;

    void Start()
    {
        PlaceMap();
    }

    void PlaceMap()
    {
        DateTime startTime = DateTime.Now;
        m_map = new CellNode[m_width, m_height];

        for (int y = 0; y < m_height; y++)
        {
            for (int x = 0; x < m_width; x++)
            {
                m_map[x, y] = new CellNode(x, y);
            }
        }

        PlaceRooms();
        PlacePath();

        print(DateTime.Now - startTime);

        //LoadMap();
    }

    void PlaceRooms()
    {
        List<Box> rooms = new List<Box>();
        Box mapBox = new Box(0, 0, (int)m_width, (int)m_height);

        Box nextRoom;

        int index = 0;

        while (index < 1000)
        {
            nextRoom = CreateRandomRoom();
            bool mapContainsRoom = mapBox.Contains(nextRoom);
            bool roomOk = CheckRoomPlacement(nextRoom, rooms);

            index = 0;
            while (!mapContainsRoom || !roomOk)
            {
                nextRoom = CreateRandomRoom();
                mapContainsRoom = mapBox.Contains(nextRoom);
                roomOk = CheckRoomPlacement(nextRoom, rooms);
                index++;
            }
            rooms.Add(nextRoom);
            AddRoomToMap(nextRoom);
        }
    }

    void PlacePath()
    {
        //TODO: Space the corridors 1 cell away from each other (THIS IS DONE BY MULTIPLYING EVERYTHING BY TWO)
        //TODO: Unwind the corridors to create a MST

        m_rootNode = RandomEmptyCell();
        Carve(m_rootNode, 0);
        m_rootNode.RemoveDeadEnds();

        pathNodes = new List<CellNode>();
        m_rootNode.AllChildren(ref pathNodes);
    }

    void Carve(CellNode targetNode, int prevDir)
    {
        /*
         * TODO: Need to check to see if the root node only has one child and recurse until it has more than one as the root node is often a dead end.
        */

        //ADD NODE
        //IF HAS CHILDREN

        //FOREACH CHILD THAT HASNT BEEN ADDED
        //SELECT ONE CHILD AT RANDOM
        //RECURSE

        //WHEN NO CHILDREN ARE LEFT
        //RETURN

        targetNode.added = true;
        if (targetNode.state != CellState.DOOR)
        {
            targetNode.state = CellState.CORRIDOR;
        }

        CellNode[] possibleChildren = GetAvailableNeighbours(targetNode);

        List<int> randomIndices = new List<int>();

        for (int i = 0; i < possibleChildren.Length; i++)
        {
            if (CheckNode(possibleChildren[i]))
            {
                randomIndices.Add(i);
            }
        }

        while (randomIndices.Count > 0)
        {
            int rand = Random.value > 0.85f ? randomIndices[Random.Range(0, randomIndices.Count)] : prevDir;
            randomIndices.Remove(rand);

            CellNode nextNode = possibleChildren[rand];

            if (CheckNode(nextNode))
            {
                targetNode.AddChild(rand, nextNode);
                //path.Add(new Line(targetNode.cell.Vector2, nextNode.cell.Vector2));

                Carve(nextNode, rand);
            }
        }
    }

    bool CheckNode(CellNode node)
    {
        return node != null && !node.added && (node.state == CellState.EMPTY || node.state == CellState.DOOR);
    }

    CellNode[] GetAvailableNeighbours(CellNode node)
    {
        CellNode[] neighbours = new CellNode[4];

        int x = node.cell.x, y = node.cell.y;

        if (y < m_height - 1)
        {
            neighbours[0] = m_map[x, y + 1];
        }
        if (x < m_width - 1)
        {
            neighbours[1] = m_map[x + 1, y];
        }
        if (y > 0)
        {
            neighbours[2] = m_map[x, y - 1];
        }
        if (x > 0)
        {
            neighbours[3] = m_map[x - 1, y];
        }
        return neighbours;
    }

    CellNode RandomEmptyCell()
    {
        bool pointOk = false;

        int x = 0;
        int y = 0;

        while (!pointOk)
        {
            x = (int)Random.Range(0, m_width);
            y = (int)Random.Range(0, m_height);

            pointOk = CheckPoint(x, y);
        }

        return new CellNode(x, y);
    }

    bool CheckPoint(int x, int y)
    {
        if (m_map[x, y].state != CellState.EMPTY)
        {
            return false;
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

    void AddRoomToMap(Box room)
    {
        for (int j = room.y; j < room.y + room.h; j++)
        {
            for (int i = room.x; i < room.x + room.w; i++)
            {
                m_map[i, j].state = CellState.ROOM;
            }
        }

        AddRandomDoor(room);
        //if (room.w * room.h > (m_roomWidthAverage * m_roomHeightAverage) || Random.value > 0.9f)
        //{
        //    AddRandomDoor(room);
        //}
    }

    void AddRandomDoor(Box room)
    {
        int doorx = Random.Range(room.x + 1, room.x + room.w - 1);
        int doory = Random.Range(room.y + 1, room.y + room.h - 1);

        float r = Random.value;
        if (r > 0.75f)
        {
            m_map[doorx, room.y].state = CellState.DOOR;
        }
        else if (r > 0.5f)
        {
            m_map[room.x, doory].state = CellState.DOOR;
        }
        else if (r > 0.25f)
        {
            m_map[doorx, room.y + room.h - 1].state = CellState.DOOR;
        }
        else
        {
            m_map[room.x + room.w - 1, doory].state = CellState.DOOR;
        }
    }

    void ExportMap()
    {
        StreamWriter sw = new StreamWriter("MAP.txt");
        sw.WriteLine(m_width);
        sw.WriteLine(m_height);
        string[] line = new string[m_width];

        for (int y = (int)m_height - 1; y >= 0; y--)
        {
            for (int x = 0; x < m_width; x++)
            {
                line[x] = ((int)m_map[x, y].state).ToString();
            }
            sw.WriteLine(String.Join(" ", line));
        }
        sw.Close();
    }

    void LoadMap()
    {
        StreamReader sr = new StreamReader("MAP.txt");

        m_width = (uint)int.Parse(sr.ReadLine());
        m_height = (uint)int.Parse(sr.ReadLine());
        m_map = new CellNode[m_width, m_height];

        string[] line = new string[m_width];
        try
        {
            for (int y = (int)m_height - 1; y >= 0; y--)
            {
                line = sr.ReadLine().Split(' ');
                for (int x = 0; x < m_width; x++)
                {
                    m_map[x, y] = new CellNode(x, y);
                    int state = int.Parse(line[x]);
                    m_map[x, y].state = (CellState)state;
                }
            }
            sr.Close();

        }
        catch (Exception)
        {

            throw;
        }
    }

    void DrawTile(int x, int y, Color fill)
    {
        Handles.DrawSolidRectangleWithOutline(new Rect(x, y, 1, 1), fill, Color.clear);
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            for (int y = 0; y < m_height; y++)
            {
                for (int x = 0; x < m_width; x++)
                {
                    if (m_map[x, y].state == CellState.ROOM)
                        DrawTile(x, y, Color.red);

                    if (m_map[x, y].state == CellState.WALL)
                        DrawTile(x, y, Color.green);

                    //if (m_map[x, y].state == CellState.CORRIDOR)
                    //    DrawTile(x, y, Color.blue);

                    if (m_map[x, y].state == CellState.DOOR)
                        DrawTile(x, y, Color.black);
                }
            }

            Gizmos.color = Color.green;
            if (pathNodes != null)
            {
                for (int i = 0; i < pathNodes.Count; i++)
                {
                    if (pathNodes[i].parent != null)
                    {
                        Gizmos.DrawLine(pathNodes[i].cell.Vector2 + Vector2.one * 0.5f,
                            pathNodes[i].parent.cell.Vector2 + Vector2.one * 0.5f);
                    }
                }
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

        public bool Intersects(Box other) // NOTE: Make Equals than to allow rooms to touch
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

    class CellNode
    {
        public Cell cell;
        public CellState state;
        public bool visited;
        public bool added;

        public int childCount;

        public CellNode parent;

        private CellNode[] children;

        public int neighbours;

        public CellNode(int x, int y)
        {
            cell = new Cell(x, y);
            state = CellState.EMPTY;
            visited = false;
            children = new CellNode[4];
            neighbours = 0;
            childCount = 0;
        }

        public bool IsLeaf
        {
            get { return children == null; }
        }

        public void AddChild(int index, CellNode child)
        {
            if (IsLeaf)
            {
                children = new CellNode[4];
            }

            children[index] = child;
            child.parent = this;
            childCount++;
        }

        public void RemoveChildren()
        {
            if (IsLeaf)
            {
                return;
            }
            for (int i = 0; i < 4; i++)
            {
                if (children[i] != null)
                {
                    children[i].RemoveChildren();
                    children[i] = null;
                }
            }
            children = null;
            childCount = 0;
        }

        public void AllChildren(ref List<CellNode> allChildren)
        {
            if (!IsLeaf)
            {
                for (int i = 0; i < children.Length; i++)
                {
                    if (children[i] != null)
                    {
                        allChildren.Add(children[i]);
                        children[i].AllChildren(ref allChildren);
                    }
                }
            }
        }

        public bool RemoveDeadEnds()
        {
            bool save = false;
            if (state == CellState.DOOR)
            {
                return true;
            }
            // Need to traverse the tree and remove any nodes that dont have any doors as children
            if (!IsLeaf)
            {
                for (int i = 0; i < children.Length; i++)
                {
                    if (children[i] != null)
                    {
                        if (!children[i].RemoveDeadEnds())
                        {
                            children[i].RemoveChildren();
                            children[i] = null;
                        }
                        else
                        {
                            save = true;
                        }
                    }
                }
            }
            return save;
        }
    }
}