using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    [SerializeField]
    private SpriteSheet m_dungeonSprites;

    private SpriteRenderer[,] sprites;

    private enum CellState
    {
        EMPTY = 0,
        WALL = 1,
        ROOM = 2,
        CORRIDOR = 3,
        DOOR = 4
    }

    private CellMap m_cellMap;

    private class CellMap
    {
        private CellNode[,] m_cells;
        private int m_width, m_height;

        public bool wrap = false;

        int mod(int x, int m)
        {
            int r = x % m;
            return r < 0 ? r + m : r;
        }

        private int WrapX(int x)
        {
            return wrap ? mod(x, m_width) : x;
        }

        private int WrapY(int y)
        {
            return wrap ? mod(y, m_height) : y;
        }

        public CellMap(int width, int height)
        {
            this.m_width = width;
            this.m_height = height;

            m_cells = new CellNode[width, height];
            ClearMap();
        }

        public void ClearMap()
        {
            for (int y = 0; y < m_height; y++)
            {
                for (int x = 0; x < m_width; x++)
                {
                    m_cells[WrapX(x), WrapY(y)] = new CellNode(x, y);
                }
            }
        }

        public CellNode GetCell(int x, int y)
        {
            if (ContainsElement(x, y))
            {
                return m_cells[x, y];
            }
            return null;
        }

        public void SetCellState(int x, int y, CellState state)
        {
            if (ContainsElement(x, y))
            {
                m_cells[x, y].state = state;
            }
        }

        public CellNode GetCellNeighbour(int x, int y, int index) // 0 = N, 1 = E, 2 = S, 3 = W
        {
            if (ContainsElement(x, y) && index < 4)
            {
                switch (index)
                {
                    case 0:
                    {
                        if (y < m_height - 1)
                        {
                            return m_cells[WrapX(x), WrapY(y + 1)];
                        }
                    }
                    break;
                    case 1:
                    {
                        if (x < m_width - 1)
                        {
                            return m_cells[WrapX(x + 1), WrapY(y)];
                        }
                    }
                    break;
                    case 2:
                    {
                        if (y > 0)
                        {
                            return m_cells[WrapX(x), WrapY(y - 1)];
                        }
                    }
                    break;
                    case 3:
                    {
                        if (x > 0)
                        {
                            return m_cells[WrapX(x - 1), WrapY(y)];
                        }
                    }
                    break;
                }
            }
            return null;
        }

        public CellNode[] GetImmediateCellNeighbours(int x, int y)
        {
            CellNode[] neighbours = new CellNode[4];

            neighbours[0] = m_cells[WrapX(x), WrapY(y + 1)];

            neighbours[1] = m_cells[WrapX(x + 1), WrapY(y)];

            neighbours[2] = m_cells[WrapX(x), WrapY(y - 1)];

            neighbours[3] = m_cells[WrapX(x - 1), WrapY(y)];

            return neighbours;
        }

        public CellNode[] GetCellNeighbours(int x, int y)
        {
            CellNode[] neighbours = new CellNode[8];

            neighbours[0] = m_cells[WrapX(x), WrapY(y + 1)];

            neighbours[2] = m_cells[WrapX(x + 1), WrapY(y)];

            neighbours[4] = m_cells[WrapX(x), WrapY(y - 1)];

            neighbours[6] = m_cells[WrapX(x - 1), WrapY(y)];

            if (neighbours[0] != null && neighbours[2] != null)
            {
                neighbours[1] = m_cells[WrapX(x + 1), WrapY(y + 1)];
            }

            if (neighbours[2] != null && neighbours[4] != null)
            {
                neighbours[3] = m_cells[WrapX(x + 1), WrapY(y - 1)];
            }

            if (neighbours[4] != null && neighbours[6] != null)
            {
                neighbours[5] = m_cells[WrapX(x - 1), WrapY(y - 1)];
            }

            if (neighbours[6] != null && neighbours[0] != null)
            {
                neighbours[7] = m_cells[WrapX(x - 1), WrapY(y + 1)];
            }

            return neighbours;
        }

        public CellNode[] GetMatchingCellNeighbours(int x, int y)
        {
            CellNode[] allNeighbours = new CellNode[8];

            allNeighbours[0] = m_cells[WrapX(x), WrapY(y + 1)];
            allNeighbours[1] = m_cells[WrapX(x + 1), WrapY(y + 1)];
            allNeighbours[2] = m_cells[WrapX(x + 1), WrapY(y)];
            allNeighbours[3] = m_cells[WrapX(x + 1), WrapY(y - 1)];
            allNeighbours[4] = m_cells[WrapX(x), WrapY(y - 1)];
            allNeighbours[5] = m_cells[WrapX(x - 1), WrapY(y - 1)];
            allNeighbours[6] = m_cells[WrapX(x - 1), WrapY(y)];
            allNeighbours[7] = m_cells[WrapX(x - 1), WrapY(y + 1)];

            CellNode[] matchingNeighbours = new CellNode[8];
            CellState state = m_cells[x, y].state;

            matchingNeighbours[0] = (state == allNeighbours[0].state) ? allNeighbours[0] : null;
            matchingNeighbours[2] = (state == allNeighbours[2].state) ? allNeighbours[2] : null;
            matchingNeighbours[4] = (state == allNeighbours[4].state) ? allNeighbours[4] : null;
            matchingNeighbours[6] = (state == allNeighbours[6].state) ? allNeighbours[6] : null;

            if (matchingNeighbours[0] != null && matchingNeighbours[2] != null && allNeighbours[1] != null && allNeighbours[1].state == state)
            {
                matchingNeighbours[1] = allNeighbours[1];
            }

            if (matchingNeighbours[2] != null && matchingNeighbours[4] != null && allNeighbours[3] != null && allNeighbours[3].state == state)
            {
                matchingNeighbours[3] = allNeighbours[3];
            }

            if (matchingNeighbours[4] != null && matchingNeighbours[6] != null && allNeighbours[5] != null && allNeighbours[5].state == state)
            {
                matchingNeighbours[5] = allNeighbours[5];
            }

            if (matchingNeighbours[6] != null && matchingNeighbours[0] != null && allNeighbours[7] != null && allNeighbours[7].state == state)
            {
                matchingNeighbours[7] = allNeighbours[7];
            }

            return matchingNeighbours;
        }

        public void SetBox(int x, int y, int w, int h, CellState state)
        {
            for (int j = y; j < y + h; j++)
            {
                for (int i = x; i < x + w; i++)
                {
                    m_cells[WrapX(i), WrapY(j)].state = state;
                }
            }
        }

        public bool ContainsElement(int x, int y) // if cells[x,y] is within the array bounds
        {
            return (x >= 0 && y >= 0 && x < m_width && y < m_height);
        }

        public bool ContainsElements(int xStart, int yStart, int xEnd, int yEnd)
        {
            return ContainsElement(xStart, yStart) && ContainsElement(xEnd, yEnd);
        }

        public bool CheckState(int xStart, int yStart, int xEnd, int yEnd, CellState state)
        {
            for (int y = yStart; y < yEnd; y++)
            {
                for (int x = xStart; x < xEnd; x++)
                {
                    int t = WrapX(x);
                    int p = WrapY(y);
                    if (p < 0 || t < 0)
                    {
                        print(1);
                    }
                    if (m_cells[t, p].state == state)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void DebugDraw()
        {
            for (int y = 0; y < m_height; y++)
            {
                for (int x = 0; x < m_width; x++)
                {
                    m_cells[x, y].Draw();
                }
            }
        }

        public CellNode RandomCell(CellState targetState)
        {
            bool pointOk = false;

            int x = 0;
            int y = 0;

            while (!pointOk)
            {
                x = Random.Range(0, m_width);
                y = Random.Range(0, m_height);

                pointOk = ContainsElement(x, y) && m_cells[x, y].state == targetState;
            }
            return m_cells[x, y];
        }
    }

    void Start()
    {
        PlaceMap();
    }

    void PlaceMap()
    {
        DateTime startTime = DateTime.Now;

        m_cellMap = new CellMap((int)m_width, (int)m_height);
        m_cellMap.wrap = true;

        PlaceRooms();
        PlacePath();

        print(DateTime.Now - startTime);

        sprites = new SpriteRenderer[m_width, m_height];

        int cellValue = 0;
        int index = 0;
        try
        {
            for (int y = 0; y < m_height; y++)
            {
                for (int x = 0; x < m_width; x++)
                {
                    CellNode cell = m_cellMap.GetCell(x, y);
                    if (cell != null)
                    {
                        cellValue = 0;
                        CellNode[] neighbours = m_cellMap.GetMatchingCellNeighbours(x, y);
                        if (cell.state == CellState.CORRIDOR)
                        {
                            for (int i = 0; i < neighbours.Length; i++)
                            {
                                if (neighbours[i] != null && ((cell.parent != null && neighbours[i] == cell.parent) || (!cell.IsLeaf && cell.children.ToList().Contains(neighbours[i]))))
                                {
                                    cellValue |= (1 << i);
                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i < neighbours.Length; i++)
                            {
                                if (neighbours[i] != null)
                                {
                                    cellValue |= (1 << i);
                                }
                            }
                        }

                        GameObject g = new GameObject();
                        g.transform.position = new Vector3(x + 0.5f, y + 0.5f, 0);
                        sprites[x, y] = g.AddComponent<SpriteRenderer>();

                        if (cell.state == CellState.CORRIDOR)
                        {
                            if (!pathNodes.Contains(cell))
                            {
                                cellValue = 0;
                            }
                        }
                        index = SpritePicker.GetSpriteNumber(cellValue);
                        sprites[x, y].sprite = m_dungeonSprites[index];

                    }
                }
            }
        }
        catch (Exception)
        {

            throw;
        }
        //LoadMap();
    }

    void PlaceRooms()
    {
        List<Box> rooms = new List<Box>();
        Box nextRoom;

        int index = 0;

        while (index < 10000)
        {
            nextRoom = CreateRandomRoom((int)m_width, (int)m_height);
            //bool mapContainsRoom = m_cellMap.ContainsElements(nextRoom.x - 1, nextRoom.y - 1, nextRoom.x + nextRoom.w + 1, nextRoom.y + nextRoom.h + 1);
            bool roomOk = !m_cellMap.CheckState(nextRoom.x - 1, nextRoom.y - 1, nextRoom.x + nextRoom.w + 1, nextRoom.y + nextRoom.h + 1, CellState.ROOM);

            index = 0;
            while (!roomOk && index < 10000)
            {
                nextRoom = CreateRandomRoom((int)m_width, (int)m_height);
                //mapContainsRoom = m_cellMap.ContainsElements(nextRoom.x - 1, nextRoom.y - 1, nextRoom.x + nextRoom.w + 1, nextRoom.y + nextRoom.h + 1);
                roomOk = !m_cellMap.CheckState(nextRoom.x - 1, nextRoom.y - 1, nextRoom.x + nextRoom.w + 1, nextRoom.y + nextRoom.h + 1, CellState.ROOM);
                index++;
            }
            if (index < 10000)
            {
                rooms.Add(nextRoom);
                m_cellMap.SetBox(nextRoom.x, nextRoom.y, nextRoom.w, nextRoom.h, CellState.ROOM);
            }
        }

        //PLACE DOORS HERE USING THE LIST OF ROOMS.
        PlaceDoors(rooms);
    }

    void PlaceDoors(List<Box> rooms)
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            int doorx = Random.Range(rooms[i].x + 1, rooms[i].x + rooms[i].w - 1);
            int doory = Random.Range(rooms[i].y + 1, rooms[i].y + rooms[i].h - 1);

            float r = Random.value;
            if (r > 0.75f)
            {
                m_cellMap.SetCellState(doorx, rooms[i].y, CellState.DOOR);
            }
            else if (r > 0.5f)
            {
                m_cellMap.SetCellState(rooms[i].x, doory, CellState.DOOR);
            }
            else if (r > 0.25f)
            {
                m_cellMap.SetCellState(doorx, rooms[i].y + rooms[i].h - 1, CellState.DOOR);
            }
            else
            {
                m_cellMap.SetCellState(rooms[i].x + rooms[i].w - 1, doory, CellState.DOOR);
            }
        }
    }

    void PlacePath()
    {
        //TODO: Space the corridors 1 cell away from each other and be able to save the result

        //m_rootNode = m_cellMap.GetCell(0, 0);
        m_rootNode = m_cellMap.RandomCell(CellState.EMPTY);
        Carve(m_rootNode, 0);
        m_rootNode.Reduce(CellState.DOOR);

        pathNodes = new List<CellNode>();
        m_rootNode.AllChildren(ref pathNodes);
    }

    void Carve(CellNode targetNode, int prevDir)
    {
        /*
         * TODO: Need to check to see if the root node only has one child and recurse until it has more than one as the root node is often a dead end.
         */

        /* 
         * TODO: Maybe prioratise nodes with more neighbours / ones with the most connections to increase the amount of cross / t junctions
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

        CellNode[] possibleChildren = m_cellMap.GetImmediateCellNeighbours(targetNode.x, targetNode.y);

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
                Carve(nextNode, rand);
            }
        }
    }

    bool CheckNode(CellNode node)
    {
        return node != null && !node.added && (node.state == CellState.EMPTY || node.state == CellState.DOOR);
    }

    Box CreateRandomRoom(int width, int height)
    {
        int x, y, w, h;

        w = Random.Range(5, 15);
        h = Random.Range(5, 15);

        x = Random.Range(0, width) % width;
        y = Random.Range(0, height) % height;

        return new Box(x, y, w, h);
    }

    //void ExportMap()
    //{
    //    StreamWriter sw = new StreamWriter("MAP.txt");
    //    sw.WriteLine(m_width);
    //    sw.WriteLine(m_height);
    //    string[] line = new string[m_width];

    //    for (int y = (int)m_height - 1; y >= 0; y--)
    //    {
    //        for (int x = 0; x < m_width; x++)
    //        {
    //            line[x] = ((int)m_map[x, y].state).ToString();
    //        }
    //        sw.WriteLine(String.Join(" ", line));
    //    }
    //    sw.Close();
    //}

    //void LoadMap()
    //{
    //    StreamReader sr = new StreamReader("MAP.txt");

    //    m_width = (uint)int.Parse(sr.ReadLine());
    //    m_height = (uint)int.Parse(sr.ReadLine());
    //    m_map = new CellNode[m_width, m_height];

    //    string[] line = new string[m_width];
    //    for (int y = (int)m_height - 1; y >= 0; y--)
    //    {
    //        line = sr.ReadLine().Split(' ');
    //        for (int x = 0; x < m_width; x++)
    //        {
    //            m_map[x, y] = new CellNode(x, y);
    //            int state = int.Parse(line[x]);
    //            m_map[x, y].state = (CellState)state;
    //        }
    //    }
    //    sr.Close();
    //}

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            //m_cellMap.DebugDraw();

            Gizmos.color = Color.green;
            if (pathNodes != null)
            {
                for (int i = 0; i < pathNodes.Count; i++)
                {
                    if (pathNodes[i].parent != null)
                    {
                        //Gizmos.DrawLine((pathNodes[i].Vector2 * 2f) + Vector2.one, (pathNodes[i].parent.Vector2 * 2f) + Vector2.one);
                        Gizmos.DrawLine((pathNodes[i].Vector2) + Vector2.one * 0.5f, (pathNodes[i].parent.Vector2) + Vector2.one * 0.5f);
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

    class CellNode
    {
        public int x;
        public int y;

        public Vector2 Vector2 { get { return new Vector2(x, y); } }

        public CellState state;
        public bool added;

        public CellNode parent;

        public  CellNode[] children;

        public int cellNeighbourValue;

        public CellNode(int x, int y)
        {
            this.x = x;
            this.y = y;
            state = CellState.EMPTY;
            children = new CellNode[4];
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
        }

        public void RemoveChildren()
        {
            if (IsLeaf)
            {
                return;
            }
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i] != null)
                {
                    children[i].RemoveChildren();
                    children[i] = null;
                }
            }
            children = null;
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

        public bool Reduce(CellState checkState)
        {
            bool save = false;
            if (state == checkState)
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
                        if (!children[i].Reduce(checkState))
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

        public void Draw()
        {
            Color color = Color.clear;
            switch (state)
            {
                case CellState.EMPTY:
                {
                    return;
                }
                break;
                case CellState.ROOM:
                {
                    color = Color.red;
                }
                break;
                case CellState.CORRIDOR:
                {
                    color = Color.blue;
                }
                break;
                case CellState.DOOR:
                {
                    color = Color.black;
                }
                break;
            }
            if (state == CellState.CORRIDOR)
            {
                Handles.DrawSolidRectangleWithOutline(new Rect(x, y, 1, 1), color, color);
                //Handles.DrawSolidRectangleWithOutline(new Rect(x * 2, y * 2, 1, 1), color, color);

            }
            else
            {
                Handles.DrawSolidRectangleWithOutline(new Rect(x, y, 1, 1), color, color);
                //Handles.DrawSolidRectangleWithOutline(new Rect(x * 2, y * 2, 1, 1), color, color);

            }
        }
    }
}