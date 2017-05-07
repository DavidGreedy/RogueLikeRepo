using System;
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

    [SerializeField]
    private bool m_wrapMap;

    private List<CellNode> pathNodes;
    private List<Box> m_rooms;

    private CellNode m_rootNode;

    [SerializeField]
    private Material material;

    public enum CellState
    {
        EMPTY = 0,
        WALL = 1,
        ROOM = 2,
        CORRIDOR = 3,
        DOOR = 4
    }

    public CellMap m_cellMap;

    public World world;

    public int scale;

    public class CellMap
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
            if (InRange(x, y))
            {
                return m_cells[x, y];
            }
            return null;
        }

        public CellNode[,] GetCells()
        {
            return m_cells;
        }

        public void SetCellState(int x, int y, CellState state)
        {
            if (InRange(x, y))
            {
                m_cells[x, y].state = state;
            }
        }

        public CellNode GetCellNeighbour(int x, int y, int index) // 0 = N, 1 = E, 2 = S, 3 = W
        {
            int lx, nx, ux, ly, ny, uy;

            lx = WrapX(x - 1);
            nx = WrapX(x);
            ux = WrapX(x + 1);

            ly = WrapY(y - 1);
            ny = WrapY(y);
            uy = WrapY(y + 1);

            BitSet checkBitSet = new BitSet();

            checkBitSet.Set(0, uy < m_height);
            checkBitSet.Set(2, ux < m_width);
            checkBitSet.Set(4, ly > 0);
            checkBitSet.Set(6, lx > 0);

            checkBitSet.Set(1, checkBitSet.Get(0) && checkBitSet.Get(2));
            checkBitSet.Set(3, checkBitSet.Get(2) && checkBitSet.Get(4));
            checkBitSet.Set(5, checkBitSet.Get(4) && checkBitSet.Get(6));
            checkBitSet.Set(7, checkBitSet.Get(6) && checkBitSet.Get(0));

            try
            {

                switch (index)
                {
                    case 0:
                    {
                        if (checkBitSet.Get(0))
                        {
                            return m_cells[nx, uy];
                        }
                    }
                    break;
                    case 1:
                    {
                        if (checkBitSet.Get(1))
                        {
                            return m_cells[ux, uy];
                        }
                    }
                    break;
                    case 2:
                    {
                        if (checkBitSet.Get(2))
                        {
                            return m_cells[ux, ny];
                        }
                    }
                    break;
                    case 3:
                    {
                        if (checkBitSet.Get(3))
                        {
                            return m_cells[ux, ly];
                        }
                    }
                    break;
                    case 4:
                    {
                        if (checkBitSet.Get(4))
                        {
                            return m_cells[nx, ly];
                        }
                    }
                    break;
                    case 5:
                    {
                        if (checkBitSet.Get(5))
                        {
                            return m_cells[lx, ly];
                        }
                    }
                    break;
                    case 6:
                    {
                        if (checkBitSet.Get(6))
                        {
                            return m_cells[lx, ny];
                        }
                    }
                    break;
                    case 7:
                    {
                        if (checkBitSet.Get(7))
                        {
                            return m_cells[lx, uy];
                        }
                    }
                    break;
                }
            }
            catch (Exception)
            {

                throw;
            }
            return null;
        }

        public void SetCellValue(int x, int y)
        {
            int lx, nx, ux, ly, ny, uy;

            lx = WrapX(x - 1);
            nx = WrapX(x);
            ux = WrapX(x + 1);

            ly = WrapY(y - 1);
            ny = WrapY(y);
            uy = WrapY(y + 1);

            BitSet checkBitSet = new BitSet();

            checkBitSet.Set(0, uy < m_height);
            checkBitSet.Set(2, ux < m_width);
            checkBitSet.Set(4, ly > 0);
            checkBitSet.Set(6, lx > 0);

            checkBitSet.Set(1, checkBitSet.Get(0) && checkBitSet.Get(2));
            checkBitSet.Set(3, checkBitSet.Get(2) && checkBitSet.Get(4));
            checkBitSet.Set(5, checkBitSet.Get(4) && checkBitSet.Get(6));
            checkBitSet.Set(7, checkBitSet.Get(6) && checkBitSet.Get(0));

            checkBitSet.Set(0, m_cells[nx, uy] == null);
            checkBitSet.Set(0, m_cells[ux, uy] == null);
            checkBitSet.Set(0, m_cells[ux, ny] == null);
            checkBitSet.Set(0, m_cells[ux, ly] == null);
            checkBitSet.Set(0, m_cells[nx, ly] == null);
            checkBitSet.Set(0, m_cells[lx, ly] == null);
            checkBitSet.Set(0, m_cells[lx, ny] == null);
            checkBitSet.Set(0, m_cells[lx, uy] == null);

            m_cells[nx, ny].cellNeighbourValue = checkBitSet.Bits;

        }

        public void SetCellValues()
        {
            for (int y = 0; y < m_height; y++)
            {
                for (int x = 0; x < m_width; x++)
                {
                    SetCellValue(x, y);
                }
            }
        }

        public CellNode[] GetImmediateCellNeighbours(int x, int y)
        {
            CellNode[] neighbours = new CellNode[4];

            neighbours[0] = (y >= m_height - 1 && !wrap) ? null : m_cells[WrapX(x), WrapY(y + 1)];
            neighbours[1] = (x >= m_width - 1 && !wrap) ? null : m_cells[WrapX(x + 1), WrapY(y)];
            neighbours[2] = (y <= 1 && !wrap) ? null : m_cells[WrapX(x), WrapY(y - 1)];
            neighbours[3] = (x < 1 && !wrap) ? null : m_cells[WrapX(x - 1), WrapY(y)];

            return neighbours;
        }

        private bool InRange(int x, int y) // if cells[x,y] is within the array bounds
        {
            return (x >= 0 && y >= 0 && x < m_width && y < m_height);
        }

        public void SetBox(int x, int y, int w, int h, CellState state)
        {
            if (wrap || (InRange(x, y) && InRange(x + w, y + h)))
            {
                for (int j = y; j < y + h; j++)
                {
                    for (int i = x; i < x + w; i++)
                    {
                        m_cells[WrapX(i), WrapY(j)].state = state;
                    }
                }
            }
        }

        public bool ContainsElements(int xStart, int yStart, int xEnd, int yEnd)
        {
            return InRange(xStart, yStart) && InRange(xEnd, yEnd);
        }

        public bool CheckState(int xStart, int yStart, int xEnd, int yEnd, CellState state)
        {
            if (!wrap && (!InRange(xStart, yStart) || !InRange(xEnd, yEnd)))
            {
                return true;
            }
            for (int y = yStart; y < yEnd; y++)
            {
                for (int x = xStart; x < xEnd; x++)
                {
                    if (m_cells[WrapX(x), WrapY(y)].state == state)
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

                pointOk = InRange(x, y) && m_cells[x, y].state == targetState;
            }
            return m_cells[x, y];
        }
    }

    void Start()
    {
        world.OnGenerationComplete += PlaceMap;
    }

    void PlaceMap()
    {
        DateTime startTime = DateTime.Now;

        m_cellMap = new CellMap((int)m_width, (int)m_height);
        m_cellMap.wrap = m_wrapMap;

        PlaceRooms();
        PlacePath();

        CellNode[,] nodes = m_cellMap.GetCells();

        CellNode[,] scaledNodes = new CellNode[nodes.GetLength(0) * scale, nodes.GetLength(1) * scale];

        //for (int y = 0; y < nodes.GetLength(1); y++)
        //{
        //    for (int x = 0; x < nodes.GetLength(0); x++)
        //    {
        //        for (int i = 0; i < scale; i++)
        //        {
        //            for (int j = 0; j < scale; j++)
        //            {
        //                scaledNodes[(x * scale) + i, (y * scale) + j] = nodes[x, y];
        //            }
        //        }
        //    }
        //}

        for (int y = 0; y < nodes.GetLength(1); y++)
        {
            for (int x = 0; x < nodes.GetLength(0); x++)
            {
                if (nodes[x, y].state == CellState.ROOM || nodes[x, y].state == CellState.DOOR)
                {
                    world.SetBlock(x, 1, y, new BlockAir());
                    world.SetBlock(x, 2, y, new BlockAir());
                    //for (int i = 0; i < scale; i++)
                    //{
                    //    for (int j = 0; j < scale; j++)
                    //    {
                    //        world.SetBlock((x * scale) + i, 1, (y * scale) + j, new BlockAir());
                    //        world.SetBlock((x * scale) + i, 2, (y * scale) + j, new BlockAir());
                    //    }
                    //}
                }
            }
        }

        //m_cellMap.SetCellValues();

        //print(DateTime.Now - startTime);

        //print(m_rooms.Count + " ROOMS");

        //for (int i = 0; i < pathNodes.Count; i++)
        //{
        //    float midx = (pathNodes[i].x + (pathNodes[i].parent.x - pathNodes[i].x) * 0.5f) * 2;
        //    float midy = (pathNodes[i].y + (pathNodes[i].parent.y - pathNodes[i].y) * 0.5f) * 2;

        //    if (pathNodes[i].state != CellState.DOOR)
        //    {
        //        float localx = pathNodes[i].x * 2;
        //        float localy = pathNodes[i].y * 2;

        //        world.SetBlock(Mathf.RoundToInt(localx), 1, Mathf.RoundToInt(localy), new BlockAir());
        //        world.SetBlock(Mathf.RoundToInt(localx), 2, Mathf.RoundToInt(localy), new BlockAir());
        //    }
        //    if (pathNodes[i].parent == null)
        //    {
        //        continue;
        //    }
        //    world.SetBlock(Mathf.RoundToInt(midx), 1, Mathf.RoundToInt(midy), new BlockAir());
        //    world.SetBlock(Mathf.RoundToInt(midx), 2, Mathf.RoundToInt(midy), new BlockAir());
        //}

        //for (int i = 0; i < m_rooms.Count; i++)
        //{
        //    int sizex = (m_rooms[i].w);
        //    int sizey = (m_rooms[i].h);

        //    int localx = (m_rooms[i].x) * 2;
        //    int localy = (m_rooms[i].y) * 2;

        //    for (int y = localy; y < localy + sizey * 2 - 1; y++)
        //    {
        //        for (int x = localx; x < localx + sizex * 2 - 1; x++)
        //        {
        //            world.SetBlock(x, 1, y, new BlockAir());
        //            world.SetBlock(x, 2, y, new BlockAir());

        //        }
        //    }
        //}
    }


    //bool TryAddRoom(int x, int y, int w, int h)
    //{

    //}

    void PlaceRooms()
    {
        m_rooms = new List<Box>();
        Box nextRoom;

        int index = 0;

        while (index < 10000)
        {
            nextRoom = CreateRandomRoom((int)m_width, (int)m_height);
            bool roomOk = !m_cellMap.CheckState(nextRoom.x - 1, nextRoom.y - 1, nextRoom.x + nextRoom.w + 1, nextRoom.y + nextRoom.h + 1, CellState.ROOM);

            while (!roomOk && index < 10000)
            {
                nextRoom = CreateRandomRoom((int)m_width, (int)m_height);
                roomOk = !m_cellMap.CheckState(nextRoom.x - 1, nextRoom.y - 1, nextRoom.x + nextRoom.w + 1, nextRoom.y + nextRoom.h + 1, CellState.ROOM);
                index++;
            }
            if (roomOk)
            {
                m_rooms.Add(nextRoom);
                m_cellMap.SetBox(nextRoom.x, nextRoom.y, nextRoom.w, nextRoom.h, CellState.ROOM);
            }
        }

        //PLACE DOORS HERE USING THE LIST OF ROOMS.
        PlaceDoors(m_rooms);
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
        //m_rootNode = m_cellMap.GetCell(0, 0);
        m_rootNode = m_cellMap.RandomCell(CellState.EMPTY);
        Carve(m_rootNode, 0);
        m_rootNode.Reduce(CellState.DOOR);

        pathNodes = new List<CellNode>();
        m_rootNode.AllChildren(ref pathNodes);
    }

    void Carve(CellNode targetNode, int prevDir)
    {
        targetNode.added = true;
        if (targetNode.state != CellState.DOOR)
        {
            targetNode.state = CellState.CORRIDOR;
        }

        CellNode[] possibleChildren = new CellNode[4];
        for (int i = 0; i < possibleChildren.Length; i++)
        {
            possibleChildren[i] = m_cellMap.GetCellNeighbour(targetNode.x, targetNode.y, i * 2);
        }

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

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            if (pathNodes != null)
            {
                for (int i = 0; i < pathNodes.Count; i++)
                {
                    if (pathNodes[i].parent != null)
                    {
                        Gizmos.DrawLine((pathNodes[i].Vector2 * 2f) + Vector2.one, (pathNodes[i].parent.Vector2 * 2f) + Vector2.one);
                        //Gizmos.DrawLine((pathNodes[i].Vector2) + Vector2.one * 0.5f, (pathNodes[i].parent.Vector2) + Vector2.one * 0.5f);
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

    public class CellNode
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
                //Handles.DrawSolidRectangleWithOutline(new Rect(x, y, 1, 1), color, color);
                Handles.DrawSolidRectangleWithOutline(new Rect(x * 2, y * 2, 1, 1), color, color);

            }
            else
            {
                //Handles.DrawSolidRectangleWithOutline(new Rect(x, y, 1, 1), color, color);
                Handles.DrawSolidRectangleWithOutline(new Rect(x * 2, y * 2, 1, 1), color, color);

            }
        }

        public Vector3[] Quad()
        {
            Vector3[] quad = new Vector3[6];
            float localx = x * 2;
            float localy = y * 2;

            quad[0] = new Vector3(localx, localy + 1, 0);
            quad[1] = new Vector3(localx + 1, localy + 1, 0);
            quad[2] = new Vector3(localx + 1, localy, 0);
            quad[3] = new Vector3(localx, localy + 1, 0);
            quad[4] = new Vector3(localx + 1, localy, 0);
            quad[5] = new Vector3(localx, localy, 0);

            return quad;
        }

        public List<Vector3> PathQuad()
        {
            List<Vector3> quad = new List<Vector3>();

            float midx = (x + (parent.x - x) * 0.5f) * 2;
            float midy = (y + (parent.y - y) * 0.5f) * 2;

            if (state != CellState.DOOR)
            {
                float localx = x * 2;
                float localy = y * 2;

                quad.Add(new Vector3(localx, localy + 1, 0));
                quad.Add(new Vector3(localx + 1, localy + 1, 0));
                quad.Add(new Vector3(localx + 1, localy, 0));
                quad.Add(new Vector3(localx, localy + 1, 0));
                quad.Add(new Vector3(localx + 1, localy, 0));
                quad.Add(new Vector3(localx, localy, 0));
            }
            else
            {
                //Init door positions here.
            }

            if (parent == null)
            {
                return quad;
            }

            quad.Add(new Vector3(midx, midy + 1, 0));
            quad.Add(new Vector3(midx + 1, midy + 1, 0));
            quad.Add(new Vector3(midx + 1, midy, 0));
            quad.Add(new Vector3(midx, midy + 1, 0));
            quad.Add(new Vector3(midx + 1, midy, 0));
            quad.Add(new Vector3(midx, midy, 0));

            return quad;
        }
    }
}