using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
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

    private List<CellNode> pathNodes;

    private CellNode m_rootNode;

    private enum CellState
    {
        EMPTY = 0,
        WALL = 1,
        ROOM = 2,
        CORRIDOR = 3
    }

    private CellNode[,] m_map;

    private List<Box> m_rooms;
    private List<Box> m_failedRooms;

    private Box m_nextRoom;

    void Start()
    {
        m_map = new CellNode[m_width, m_height];

        for (int y = 0; y < m_height; y++)
        {
            for (int x = 0; x < m_width; x++)
            {
                m_map[x, y] = new CellNode(x, y);
            }
        }
        path = new List<Line>();

        int index = 0;

        bool placementGood = false;

        m_rooms = new List<Box>();
        m_failedRooms = new List<Box>();

        StartCoroutine(PlaceRooms());
        //BuildMaze();
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
        BuildMaze();
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

    void BuildMaze()
    {
        //TODO: Space the corridors 1 cell away from each other (THIS IS DONE BY MULTIPLYING EVERYTHING BY TWO)
        //TODO: Unwind the corridors to create a MST

        m_rootNode = RandomCellNode();
        Carve(m_rootNode, 0);

        pathNodes = new List<CellNode>();
        m_rootNode.AllChildren(ref pathNodes);

        //while (cellNodeStack.Count > 0)
        //{
        //    currentNode.children = GetUnvisitedNeighbours(currentNode);

        //    int result = currentNode.CalculateNeigbours();

        //    if (result == 0) // if there are no free adjacent cells, need to backtrack
        //    {
        //        CellNode junction = cellNodeStack.Pop();
        //        currentNode = junction;
        //    }
        //    else // Continue path to free adjacent cell
        //    {
        //        //Need to set the children first;

        //        CellNode nextNode = currentNode.GetRandomChild(ref prevDir);

        //        if ((result & (result - 1)) == 0) // if there is only one neighbour
        //        {

        //        }
        //        else // there is more than one neighbour so we need to add a new node to the tree
        //        {
        //            cellNodeStack.Push(currentNode);
        //        }

        //        path.Add(new Line(currentNode.cell.Vector2 + (Vector2.one * 0.5f), nextNode.cell.Vector2 + (Vector2.one * 0.5f)));

        //        currentNode = nextNode;

        //        m_map[currentNode.cell.x, currentNode.cell.y].state = CellState.CORRIDOR;
        //    }
        //}
        //yield return new WaitForSeconds(0.001f);
    }

    //void Carve(ref CellNode targetNode, ref int prevDir, ref int escape)
    //{
    //    //if (escape > 100000)
    //    //{
    //    //    return;
    //    //}
    //    targetNode.children = GetUnvisitedNeighbours(targetNode);

    //    targetNode.CalculateNeigbours();

    //    if (targetNode.neighbours == 0) // if there are no free adjacent cells, need to backtrack
    //    {
    //        return;
    //    }

    //    CellNode nextNode = targetNode.GetRandomChild(ref prevDir);

    //    path.Add(new Line(targetNode.cell.Vector2 + (Vector2.one * 0.5f), nextNode.cell.Vector2 + (Vector2.one * 0.5f)));

    //    escape++;

    //    m_map[targetNode.cell.x, targetNode.cell.y].state = CellState.CORRIDOR;

    //    Carve(ref nextNode, ref prevDir, ref escape);
    //}

    void Carve(CellNode targetNode, int prevDir)
    {
        //ADD NODE
        //IF HAS CHILDREN

        //FOREACH CHILD THAT HASNT BEEN ADDED
        //SELECT ONE CHILD AT RANDOM
        //RECURSE

        //WHEN NO CHILDREN ARE LEFT
        //RETURN

        targetNode.added = true;
        targetNode.state = CellState.CORRIDOR;

        CellNode[] possibleChildren = GetAvailableNeighbours(targetNode);
        CellNode nextNode;
        List<int> randomIndices = new List<int>();
        int rand;
        for (int i = 0; i < possibleChildren.Length; i++)
        {
            if (CheckNode(possibleChildren[i]))
            {
                randomIndices.Add(i);
            }
        }

        while (randomIndices.Count > 0)
        {
            rand = Random.value > 0.85 ? randomIndices[Random.Range(0, randomIndices.Count)] : prevDir;
            randomIndices.Remove(rand);

            nextNode = possibleChildren[rand];

            if (CheckNode(nextNode))
            {
                targetNode.SetChild(rand, nextNode);
                path.Add(new Line(targetNode.cell.Vector2, nextNode.cell.Vector2));

                Carve(nextNode, rand);
            }
        }


    }

    bool CheckNode(CellNode node)
    {
        return node != null && !node.added && node.state == CellState.EMPTY;
    }

    //List<CellNode> GetRandom(CellNode[] possible, ref int dir)
    //{
    //    int randCell = -1;
    //    List<int> setBits = new List<int>();

    //    for (int i = 0; i < 4; i++)
    //    {
    //        if (possible[i] != null && !possible[i].added)
    //        {
    //            setBits.Add(i);
    //        }
    //    }

    //    if (setBits.Contains(dir) && Random.value < 0.85f)
    //    {
    //        randCell = dir;
    //    }
    //    else
    //    {
    //        randCell = setBits[Random.Range(0, setBits.Count)];
    //    }

    //    dir = randCell;

    //    return new Pair<CellNode, int>(possible[randCell], randCell);
    //}

    List<CellNode> GetNeighbours(int x, int y)
    {
        int result = 0;

        List<CellNode> neighbours = new List<CellNode>();
        if (y < m_height - 1)
        {
            result |= (1 << 0); // N
            CellNode node = m_map[x, y + 1];

            if (!node.visited)
            {
                neighbours.Add(node);
                node.visited = true;
            }
        }

        if (x < m_width - 1)
        {
            result |= (1 << 1); // E
            CellNode node = m_map[x + 1, y];

            if (!node.visited)
            {
                neighbours.Add(node);
                node.visited = true;
            }
        }

        if (y > 1)
        {
            result |= (1 << 2); // S
            CellNode node = m_map[x, y - 1];

            if (!node.visited)
            {
                neighbours.Add(node);
                node.visited = true;
            }
        }

        if (x > 1)
        {
            result |= (1 << 3); // W
            CellNode node = m_map[x - 1, y];

            if (!node.visited)
            {
                neighbours.Add(node);
                node.visited = true;
            }
        }

        //if ((result & 3) == 3)
        //{
        //    neighbours.Add(new Cell(x + 1, y + 1));
        //}
        //if ((result & 6) == 6)
        //{
        //    neighbours.Add(new Cell(x + 1, y - 1));
        //}
        //if ((result & 12) == 12)
        //{
        //    neighbours.Add(new Cell(x - 1, y - 1));
        //}
        //if ((result & 9) == 9)
        //{
        //    neighbours.Add(new Cell(x - 1, y + 1));
        //}

        return neighbours;
    }

    int CheckAdjacentMatches(int x, int y, CellState matchState)
    {
        int result = 0;

        if (y < m_height - 1 && m_map[x, y + 1].state == matchState)
        {
            result |= (1 << 0); // N
        }

        if (x < m_width - 1 && m_map[x + 1, y].state == matchState)
        {
            result |= (1 << 1); // E
        }

        if (y > 1 && m_map[x, y - 1].state == matchState)
        {
            result |= (1 << 2); // S
        }

        if (x > 1 && m_map[x - 1, y].state == matchState)
        {
            result |= (1 << 3); // W
        }
        return result;
    }

    int GetPossibleDirections(CellNode node)
    {
        int result = 0;
        int x = node.cell.x, y = node.cell.y;

        if (y < m_height - 1 && !m_map[x, y + 1].added)
        {
            result |= (1 << 0); // N
        }

        if (x < m_width - 1 && !m_map[x + 1, y].added)
        {
            result |= (1 << 1); // E
        }

        if (y > 1 && !m_map[x, y - 1].added)
        {
            result |= (1 << 2); // S
        }

        if (x > 1 && !m_map[x - 1, y].added)
        {
            result |= (1 << 3); // W
        }
        return result;
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
        if (y > 1)
        {
            neighbours[2] = m_map[x, y - 1];
        }
        if (x > 1)
        {
            neighbours[3] = m_map[x - 1, y];
        }
        return neighbours;
    }

    CellNode GetRandomCell(int x, int y, int result, ref int dir)
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
            return m_map[x, y + 1];
        }
        if (randCell == 1)
        {
            return m_map[x + 1, y];
        }
        if (randCell == 2)
        {
            return m_map[x, y - 1];
        }
        if (randCell == 3)
        {
            return m_map[x - 1, y];
        }
        return null;
    }

    CellNode RandomCellNode()
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

        return new CellNode(x, y);
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
                m_map[i, j].state = CellState.ROOM;
            }
        }
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
    }

    void DrawTile(int x, int y, Color fill)
    {
        Handles.DrawSolidRectangleWithOutline(new Rect(x, y, 1, 1), fill, Color.clear);
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            //for (int y = 0; y < m_height; y++)
            //{
            //    for (int x = 0; x < m_width; x++)
            //    {
            //        if (m_map[x, y].Second == CellState.ROOM)
            //        {
            //            //DrawTile(Mathf.FloorToInt(x), Mathf.FloorToInt(y), Color.red);
            //            //DrawTile(Mathf.FloorToInt(x * 2 + 1), Mathf.FloorToInt(y * 2), Color.red);
            //            //DrawTile(Mathf.FloorToInt(x * 2), Mathf.FloorToInt(y * 2 + 1), Color.red);
            //            //DrawTile(Mathf.FloorToInt(x * 2 + 1), Mathf.FloorToInt(y * 2 + 1), Color.red);
            //
            //        }
            //
            //        if (m_map[x, y].Second == CellState.WALL)
            //            //DrawTile(x * 2, y * 2, Color.green);
            //
            //            //if (m_map[x, y].Second == CellState.CORRIDOR)
            //            //DrawTile(x * 2, y * 2, Color.blue);
            //    }
            //}

            for (int i = 0; i < m_rooms.Count; i++)
            {
                Rect r = new Rect(m_rooms[i].x, m_rooms[i].y, m_rooms[i].w, m_rooms[i].h);
                Handles.DrawSolidRectangleWithOutline(new Rect(r), Color.red, Color.red);
            }
            //for (int i = 0; i < path.Count; i++)
            //{
            //    //DrawTile(Mathf.FloorToInt(path[i].start.x ), Mathf.FloorToInt(path[i].start.y * 2), Color.blue);
            //    //DrawTile(Mathf.FloorToInt(path[i].end.x ), Mathf.FloorToInt(path[i].end.y * 2), Color.blue);
            //    //DrawTile(Mathf.FloorToInt(Mathf.Lerp(path[i].start.x * 2, path[i].end.x * 2, 0.5f)), Mathf.FloorToInt(Mathf.Lerp(path[i].start.y * 2, path[i].end.y * 2, 0.5f)), Color.blue);

            //    Gizmos.DrawLine(path[i].start, path[i].end);
            //}

            Gizmos.color = Color.green;
            if (pathNodes != null)
            {
                for (int i = 0; i < pathNodes.Count; i++)
                {
                    if (pathNodes[i].parent != null)
                    {
                        Gizmos.DrawLine(pathNodes[i].cell.Vector2, pathNodes[i].parent.cell.Vector2);
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

    class CellNode
    {
        public Cell cell;
        public CellState state;
        public bool visited;
        public bool added;

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
        }

        public bool IsLeaf
        {
            get { return children == null; }
        }

        public void SetChild(int index, CellNode child)
        {
            if (IsLeaf)
            { children = new CellNode[4]; }

            children[index] = child;
            child.parent = this;
        }

        public int CalculateNeigbours()
        {
            neighbours = 0;
            for (int i = 0; i < 4; i++)
            {
                if (children[i] != null)
                {
                    neighbours |= (1 << i); // N
                }
            }
            return neighbours;
        }

        public List<CellNode> Children()
        {
            List<CellNode> ch = new List<CellNode>();
            for (int i = 0; i < 4; i++)
            {
                if (children[i] != null)
                {
                    ch.Add(children[i]);
                }
            }

            return ch;
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
    }
}