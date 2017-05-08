using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[System.Serializable]
public enum NodeState
{
    Empty = 0, Wall = 1, Path = 2, Room = 3, Door = 4, Corner = 5
}

[System.Serializable]
public class LevelDebugView : MonoBehaviour
{
    public LevelGenerator levelGenerator;

    private TileNode<NodeState> rootNode;

    [SerializeField]
    private TileGrid<NodeState> grid;

    private List<TileNode<NodeState>> path;

    public World world;

    private Texture2D texture;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    private LevelData levelData;

    private void Start()
    {
        levelGenerator.OnGenerationComplete += GetLevel;
    }

    private void GetLevel()
    {
        levelData = levelGenerator.BuildLevelData();
        grid = levelData.data;

        rootNode = null;

        while (rootNode == null)
        {
            rootNode = grid.GetNode(ExtensionMethods.ToEvenGridPosition(Random.Range(0, grid.Width), 1), ExtensionMethods.ToEvenGridPosition(Random.Range(0, grid.Height), 1));
            if (rootNode.Data != NodeState.Empty)
            {
                rootNode = null;
            }
        }

        print(rootNode.position.x + " " + rootNode.position.y);

        BuildPath(ref grid, rootNode, 0);

        PlaceDoors();

        rootNode.Reduce(ref grid, NodeState.Door, NodeState.Empty);

        SetBlocks();
        InitDebugTexture();
    }

    void SetBlocks()
    {
        if (grid != null)
        {
            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    if (grid.GetNode(x, y).Data == NodeState.Room || grid.GetNode(x, y).Data == NodeState.Path || grid.GetNode(x, y).Data == NodeState.Door)
                    {
                        world.SetBlock(x + 16, 2, y + 16, new BlockAir());
                    }
                }
            }
        }
        List<TileNode<NodeState>> nodes = new List<TileNode<NodeState>>();
        //rootNode.GetConnections(ref nodes);
        print(nodes.Count);
        //for (int i = 0; i < nodes.Count; i++)
        //{
        //    world.SetBlock((nodes[i].x), 2, nodes[i].y, new BlockAir());
        //}
    }

    void PlaceDoors()
    {
        for (int i = 0; i < levelData.m_rooms.Count; i++)
        {
            Room room = levelData.m_rooms[i];

            int xStart = (int)room.Rect.xMin;
            int yStart = (int)room.Rect.yMin;
            int xEnd = (int)room.Rect.xMax;
            int yEnd = (int)room.Rect.yMax;
            for (int y = yStart - 1; y < yEnd + 1; y++)
            {
                for (int x = xStart - 1; x < xEnd + 1; x++)
                {
                    int conditions = 0;
                    conditions += x < xStart ? 1 : 0;
                    conditions += y < yStart ? 1 : 0;
                    conditions += x >= xEnd ? 1 : 0;
                    conditions += y >= yEnd ? 1 : 0;

                    if (conditions == 2)
                    {
                        levelData.data.SetNode(x, y, NodeState.Corner);

                        for (int j = 0; j < 4; j++)
                        {
                            grid.GetNeighbour(x, y, j * 2);
                            if (grid.GetNeighbour(x, y, j * 2) != null && grid.GetNeighbour(x, y, j * 2).Data == NodeState.Wall)
                            {
                                grid.GetNeighbour(x, y, j * 2).Data = NodeState.Corner;
                            }
                        }
                    }
                }
            }

            int placedDoors = 0;

            BitSet directions = new BitSet();

            while (placedDoors < 2)
            {
                int doorx = Random.Range(xStart, xEnd);
                int doory = Random.Range(yStart, yEnd);

                float r = Random.value;
                TileNode<NodeState> currentNode;

                if (r > 0.75f && grid.GetNode(doorx, yStart - 1).Data != NodeState.Door && grid.GetNode(doorx, yStart - 1).Data != NodeState.Corner && !directions.Get(0))
                {
                    currentNode = grid.GetNode(doorx, yStart - 1);

                    if (currentNode.Data != NodeState.Door && currentNode.Data != NodeState.Corner && !directions.Get(0))
                    {
                        grid.SetNode(doorx, yStart - 1, NodeState.Door);
                        directions.Set(0, 1);
                        placedDoors++;
                    }
                }
                else if (r > 0.5f)
                {
                    grid.SetNode(xStart - 1, doory, NodeState.Door);
                    placedDoors++;
                    directions.Set(1, 1);
                }
                else if (r > 0.25f)
                {
                    grid.SetNode(doorx, yEnd, NodeState.Door);
                    placedDoors++;
                    directions.Set(2, 1);
                }
                else
                {
                    grid.SetNode(xEnd, doory, NodeState.Door);
                    placedDoors++;
                    directions.Set(3, 1);
                }
            }
        }
        //for (int i = 0; i < levelData.m_rooms.Count; i++)
        //{
        //    Room room = levelData.m_rooms[i];

        //    float mergeRoomChance = Random.value;

        //    int xStart = (int)room.Rect.xMin;
        //    int yStart = (int)room.Rect.yMin;
        //    int xEnd = (int)room.Rect.xMax;
        //    int yEnd = (int)room.Rect.yMax;

        //    int doorCount = Random.Range(1, 2);

        //    List<TileNode<NodeState>> doorPositions = new List<TileNode<NodeState>>();

        //    for (int y = yStart - 1; y < yEnd + 1; y++)
        //    {
        //        for (int x = xStart - 1; x < xEnd + 1; x++)
        //        {
        //            int conditions = 0;
        //            conditions += x < xStart ? 1 : 0;
        //            conditions += y < yStart ? 1 : 0;
        //            conditions += x >= xEnd ? 1 : 0;
        //            conditions += y >= yEnd ? 1 : 0;

        //            TileNode<NodeState>[] neighbours = new TileNode<NodeState>[4];

        //            for (int j = 0; j < 4; j++)
        //            {
        //                neighbours[j] = grid.GetNeighbour(x, y, j * 2);
        //                //if (neighbours[j] != null && neighbours[j].Data == NodeState.Wall)
        //                //{
        //                //    neighbours[j].Data = NodeState.Corner;
        //                //}
        //            }

        //            if (conditions == 2)
        //            {
        //                levelData.data.SetNode(x, y, NodeState.Corner);
        //            }

        //            //if (conditions == 1 && mergeRoomChance > 0.5f)
        //            //{
        //            //    if (neighbours[0] != null && neighbours[2] != null && neighbours[0].Data == NodeState.Room && neighbours[2].Data == NodeState.Room)
        //            //    {
        //            //        levelData.data.SetNode(x, y, NodeState.Room);
        //            //    }
        //            //    if (neighbours[1] != null && neighbours[3] != null && neighbours[1].Data == NodeState.Room && neighbours[3].Data == NodeState.Room)
        //            //    {
        //            //        levelData.data.SetNode(x, y, NodeState.Room);
        //            //    }
        //            //}
        //            //else
        //            {
        //                if (levelData.data.GetNode(x, y).Data == NodeState.Wall && neighbours[0] != null && neighbours[2] != null && (neighbours[0].Data == NodeState.Room || neighbours[0].Data == NodeState.Path) && (neighbours[2].Data == NodeState.Room || neighbours[2].Data == NodeState.Path))
        //                {
        //                    doorPositions.Add(levelData.data.GetNode(x, y));
        //                }
        //                if (levelData.data.GetNode(x, y).Data == NodeState.Wall && neighbours[1] != null && neighbours[3] != null && (neighbours[1].Data == NodeState.Room || neighbours[1].Data == NodeState.Path) && (neighbours[3].Data == NodeState.Room || neighbours[3].Data == NodeState.Path))
        //                {
        //                    doorPositions.Add(levelData.data.GetNode(x, y));
        //                }
        //            }

        //            //if (conditions == 1 && levelData.data.GetNode(x, y).Data != NodeState.Corner)
        //            //{
        //            //    if (levelData.data.GetNeighbour(x, y, 0).Data == NodeState.Room && levelData.data.GetNeighbour(x, y, 4).Data == NodeState.Room)
        //            //    {
        //            //        levelData.data.SetNode(x, y, NodeState.Room);
        //            //    }
        //            //    if (levelData.data.GetNeighbour(x, y, 2).Data == NodeState.Room && levelData.data.GetNeighbour(x, y, 6).Data == NodeState.Room)
        //            //    {
        //            //        levelData.data.SetNode(x, y, NodeState.Room);
        //            //    }
        //            //    //    //levelData.data.SetNode(x, y, NodeState.Door);
        //            //    //    doorPositions.Add(grid.GetNode(x, y));
        //            //}
        //        }
        //    }

        //    for (int j = 0; j < doorPositions.Count; j++)
        //    {
        //        doorPositions[j].Data = NodeState.Door;
        //    }
        //}
    }

    void Update()
    {
        //if (rootNode != null)
        //{
        //    List<TileNode<NodeState>> nodes = new List<TileNode<NodeState>>();
        //    rootNode.GetConnections(ref nodes);
        //}
    }

    [System.Serializable]
    public struct GridPosition
    {
        public int x;
        public int y;

        public GridPosition(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public GridPosition North { get { return new GridPosition(x, y + 1); } }
        public GridPosition NorthEast { get { return new GridPosition(x + 1, y + 1); } }
        public GridPosition East { get { return new GridPosition(x + 1, y); } }
        public GridPosition SouthEast { get { return new GridPosition(x + 1, y - 1); } }
        public GridPosition South { get { return new GridPosition(x, y - 1); } }
        public GridPosition SouthWest { get { return new GridPosition(x - 1, y - 1); } }
        public GridPosition West { get { return new GridPosition(x - 1, y); } }
        public GridPosition NorthWest { get { return new GridPosition(x - 1, y + 1); } }

        public GridPosition Neighbour(int dir)
        {
            switch (dir)
            {
                case 0:
                return North;
                case 1:
                return NorthEast;
                case 2:
                return East;
                case 3:
                return SouthEast;
                case 4:
                return South;
                case 5:
                return SouthWest;
                case 6:
                return West;
                case 7:
                return NorthWest;
            }
            throw new Exception("Neighbour index out of range:" + dir);
        }
    }

    [System.Serializable]
    public class TileNode<T> where T : struct
    {
        public bool Visited = false;
        public GridPosition position;
        public T Data;

        public TileNode<T> parent;
        public List<TileNode<T>> children;

        public TileNode(int x, int y, T data)
        {
            position = new GridPosition(x, y);
            this.Data = data;
        }

        public bool IsLeaf
        {
            get { return children == null; }
        }

        public void AddChild(TileNode<T> child)
        {
            if (child == this)
            { return; }

            if (IsLeaf)
            { children = new List<TileNode<T>>(); }

            children.Add(child);
            child.parent = this;
        }

        public void RemoveChildren()
        {
            if (IsLeaf)
            {
                return;
            }
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] != null)
                {
                    children[i].RemoveChildren();
                    children[i] = null;
                }
            }
            children = null;
        }

        public void AllChildren(ref List<TileNode<T>> allChildren)
        {
            if (!IsLeaf)
            {
                for (int i = 0; i < children.Count; i++)
                {
                    if (children[i] != null)
                    {
                        allChildren.Add(children[i]);
                        children[i].AllChildren(ref allChildren);
                    }
                }
            }
        }

        public virtual bool Reduce(ref TileGrid<T> grid, T checkState, T setState)
        {
            bool save = false;

            for (int i = 0; i < 4; i++)
            {
                TileNode<T> node = grid.GetNeighbour(position.x, position.y, i * 2);
                if (node != null && node.Data.Equals(checkState))
                {
                    save = true;
                }
                //if (Data.Equals(checkState))
                //{
                //    return true;
                //}

            }
            if (children != null)
            {
                for (int i = 0; i < children.Count; i++)
                {
                    if (children[i] != null)
                    {
                        if (!children[i].Reduce(ref grid, checkState, setState))
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
            if (!save)
            {
                Data = setState;
            }
            return save;
        }
    }

    [System.Serializable]
    public class TileGrid<T> where T : struct
    {
        private TileNode<T>[,] tileNodes;
        public int Width { get; private set; }
        public int Height { get; private set; }

        public TileGrid(int w, int h, T init)
        {
            Width = w;
            Height = h;
            tileNodes = new TileNode<T>[w, h];

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    tileNodes[x, y] = new TileNode<T>(x, y, init);
                }
            }
        }

        private bool InRangeX(int x)
        {
            return x >= 0 && x < Width;
        }

        private bool InRangeY(int y)
        {
            return y >= 0 && y < Height;
        }

        public void SetNode(int x, int y, T data)
        {
            tileNodes[x, y] = new TileNode<T>(x, y, data);
        }

        public TileNode<T> GetNode(int x, int y)
        {
            if (InRangeX(x) && InRangeY(y))
            {
                return tileNodes[x, y];
            }
            return null;
        }

        public TileNode<T> GetNeighbour(int x, int y, int index) // 0 = N, 2 = E, 4 = S, 6 = W
        {
            int lx, nx, ux, ly, ny, uy;

            lx = x - 1;
            nx = x;
            ux = x + 1;

            ly = y - 1;
            ny = y;
            uy = y + 1;

            BitSet checkBitSet = new BitSet();

            checkBitSet.Set(0, InRangeY(uy));
            checkBitSet.Set(2, InRangeX(ux));
            checkBitSet.Set(4, InRangeY(ly));
            checkBitSet.Set(6, InRangeX(lx));

            checkBitSet.Set(1, checkBitSet.Get(0) && checkBitSet.Get(2));
            checkBitSet.Set(3, checkBitSet.Get(2) && checkBitSet.Get(4));
            checkBitSet.Set(5, checkBitSet.Get(4) && checkBitSet.Get(6));
            checkBitSet.Set(7, checkBitSet.Get(6) && checkBitSet.Get(0));

            switch (index)
            {
                case 0:
                {
                    if (checkBitSet.Get(0))
                    {
                        return tileNodes[nx, uy];
                    }
                }
                break;
                case 1:
                {
                    if (checkBitSet.Get(1))
                    {
                        return tileNodes[ux, uy];
                    }
                }
                break;
                case 2:
                {
                    if (checkBitSet.Get(2))
                    {
                        return tileNodes[ux, ny];
                    }
                }
                break;
                case 3:
                {
                    if (checkBitSet.Get(3))
                    {
                        return tileNodes[ux, ly];
                    }
                }
                break;
                case 4:
                {
                    if (checkBitSet.Get(4))
                    {
                        return tileNodes[nx, ly];
                    }
                }
                break;
                case 5:
                {
                    if (checkBitSet.Get(5))
                    {
                        return tileNodes[lx, ly];
                    }
                }
                break;
                case 6:
                {
                    if (checkBitSet.Get(6))
                    {
                        return tileNodes[lx, ny];
                    }
                }
                break;
                case 7:
                {
                    if (checkBitSet.Get(7))
                    {
                        return tileNodes[lx, uy];
                    }
                }
                break;
            }
            return null;
        }
    }

    void InitDebugTexture()
    {
        texture = new Texture2D(grid.Width, grid.Height)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        for (int y = 0; y < grid.Height; y++)
        {
            for (int x = 0; x < grid.Width; x++)
            {
                NodeState state = grid.GetNode(x, y).Data;
                switch (state)
                {
                    case NodeState.Empty:
                    if (grid.GetNode(x, y).Visited)
                    {
                        texture.SetPixel(x, y, Color.black);
                    }
                    else
                    {
                        texture.SetPixel(x, y, Color.grey);
                    }
                    break;

                    case NodeState.Wall:
                    texture.SetPixel(x, y, Color.cyan);
                    break;

                    case NodeState.Door:
                    texture.SetPixel(x, y, Color.yellow);
                    break;

                    case NodeState.Room:
                    texture.SetPixel(x, y, Color.red);
                    break;

                    case NodeState.Path:
                    texture.SetPixel(x, y, Color.magenta);
                    break;

                    case NodeState.Corner:
                    texture.SetPixel(x, y, Color.white);
                    break;
                }
            }
        }

        texture.Apply();

        meshFilter = gameObject.GetComponent<MeshFilter>();
        meshRenderer = gameObject.GetComponent<MeshRenderer>();

        MeshData meshData = new MeshData();

        meshData.AddVertex(new Vector3(0, 0, grid.Height));
        meshData.AddVertex(new Vector3(grid.Width, 0, grid.Height));
        meshData.AddVertex(new Vector3(grid.Width, 0, 0));
        meshData.AddVertex(new Vector3(0, 0, 0));

        meshData.uvs.Add(new Vector2(0, 1));
        meshData.uvs.Add(new Vector2(1, 1));
        meshData.uvs.Add(new Vector2(1, 0));
        meshData.uvs.Add(new Vector2(0, 0));

        meshData.CreateQuadFromLast();

        Mesh mesh = new Mesh()
        {
            vertices = meshData.vertices.ToArray(),
            triangles = meshData.indices.ToArray(),
            uv = meshData.uvs.ToArray()
        };
        mesh.RecalculateNormals();

        meshFilter.sharedMesh = mesh;
        meshRenderer.material.mainTexture = texture;
    }

    //void PlaceDoors() // For a door to be valid 
    //{
    //for (int y = 0; y < grid.Height; y++)
    //{
    //for (int x = 0; x < grid.Width; x++)
    //{
    //TileNode<NodeState> targetNode = grid.GetNode(x, y);
    //TileNode<NodeState>[] neighbours = new TileNode<NodeState>[8];

    //for (int i = 0; i < 8; i++)
    //{
    //neighbours[i] = grid.GetNeighbour(x, y, i);
    //}
    //if ()
    //}
    //}
    //}

    //NOTE (David) Ok , so this was overcomplicated to the extreme.
    //Simple solution: Create non colliding paths by just checking two neighbours away instead of one.
    //Sounds retarded but it works. Although for it to look proper the rooms must be positioned on an odd tile and be odd sizes

    private static void BuildPath(ref TileGrid<NodeState> tileGrid, TileNode<NodeState> currentNode, int prevDir)
    {
        currentNode.Visited = true;

        if (currentNode.Data != NodeState.Door)
        {
            currentNode.Data = NodeState.Path;
        }

        TileNode<NodeState>[] possibleChildren = new TileNode<NodeState>[4];
        for (int i = 0; i < possibleChildren.Length; i++)
        {
            TileNode<NodeState> tmp = tileGrid.GetNeighbour(currentNode.position.x, currentNode.position.y, i * 2);
            if (tmp != null)
            {
                possibleChildren[i] = tileGrid.GetNeighbour(tmp.position.x, tmp.position.y, i * 2);
            }
        }

        List<int> randomIndices = new List<int>();

        for (int i = 0; i < possibleChildren.Length; i++)
        {
            if (possibleChildren[i] != null && !possibleChildren[i].Visited)
            {
                randomIndices.Add(i);
            }
        }

        while (randomIndices.Count > 0)
        {
            int rand = Random.value > 0.85f ? randomIndices[Random.Range(0, randomIndices.Count)] : prevDir;
            randomIndices.Remove(rand);

            TileNode<NodeState> nextNode = possibleChildren[rand];

            if (nextNode != null && !nextNode.Visited && nextNode.Data == NodeState.Empty)
            {

                TileNode<NodeState> midNode = tileGrid.GetNeighbour(currentNode.position.x, currentNode.position.y, rand * 2);
                currentNode.AddChild(midNode);
                midNode.AddChild(nextNode);

                tileGrid.GetNeighbour(currentNode.position.x, currentNode.position.y, rand * 2).Visited = true;
                tileGrid.GetNeighbour(currentNode.position.x, currentNode.position.y, rand * 2).Data = NodeState.Path;

                BuildPath(ref tileGrid, nextNode, rand);
            }
        }
    }
    //TODO: Add Door placement
}