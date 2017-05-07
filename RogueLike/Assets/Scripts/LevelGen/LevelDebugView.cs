using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[System.Serializable]
public enum NodeState
{
    Empty = 0, Wall = 1, Path = 2, Room = 3, Door = 4
}

[System.Serializable]
public class LevelDebugView : MonoBehaviour
{
    public LevelGenerator levelGenerator;

    private SearchNode<NodeState> rootNode;

    [SerializeField]
    private SearchGrid<NodeState> grid;

    private List<SearchNode<NodeState>> path;

    public World world;

    public Texture2D texture;

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
        //SetBlocks();
        InitDebugTexture();
    }

    void SetBlocks()
    {
        //if (grid != null)
        //{
        //    for (int y = 0; y < grid.Height; y++)
        //    {
        //        for (int x = 0; x < grid.Width; x++)
        //        {
        //            if (grid.GetNode(x, y).Data == NodeState.Room)
        //            {
        //                world.SetBlock(x, 2, y, new BlockAir());
        //            }
        //            //for (int i = 0; i < scale; i++)
        //            //{
        //            //    for (int j = 0; j < scale; j++)
        //            //    {
        //            //        if (grid.GetNode(x, y).Data == NodeState.Room)
        //            //        {
        //            //            world.SetBlock((x * scale) + i, 1, (y * scale) + j, new BlockAir());
        //            //            world.SetBlock((x * scale) + i, 2, (y * scale) + j, new BlockAir());
        //            //
        //            //        }
        //            //    }
        //            //}
        //        }
        //    }
        //}
        List<SearchNode<NodeState>> nodes = new List<SearchNode<NodeState>>();
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

            int doorCount = Random.Range(1, 2);

            for (int j = 0; j < doorCount; j++)
            {
                for (int y = yStart - 1; y < yEnd + 1; y++)
                {
                    for (int x = xStart - 1; x < xEnd + 1; x++)
                    {
                        int conditions = 0;
                        conditions += x < xStart ? 1 : 0;
                        conditions += y < yStart ? 1 : 0;
                        conditions += x >= xEnd ? 1 : 0;
                        conditions += y >= yEnd ? 1 : 0;

                        if (conditions == 3)
                        {
                            levelData.data.SetNode(x, y, NodeState.Wall);
                        }
                    }
                }
            }
        }
    }

    void Update()
    {
        //if (rootNode != null)
        //{
        //    List<SearchNode<NodeState>> nodes = new List<SearchNode<NodeState>>();
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
    public class SearchNode<T>
    {
        public bool Visited = false;
        public GridPosition position;
        public T Data;

        public List<SearchNode<T>> connections = new List<SearchNode<T>>();

        public SearchNode(int x, int y, T data)
        {
            position = new GridPosition(x, y);
            this.Data = data;
        }

        public void Connect(SearchNode<T> node)
        {
            if (node == this)
            { return; }
            if (!connections.Contains(node))
            {
                connections.Add(node);
            }
        }

        public void GetConnections(ref List<SearchNode<T>> nodes)
        {
            //if (nodes.Contains(this))
            //{ return; }
            for (int i = 0; i < connections.Count; i++)
            {
                nodes.Add(this);
                //Debug.DrawLine(new Vector3(position.x, 0, position.y), new Vector3(connections[i].position.x, 0, connections[i].position.y));
                connections[i].GetConnections(ref nodes);
            }
        }
    }

    [System.Serializable]
    public class SearchGrid<T>
    {
        private SearchNode<T>[,] SearchNodes;
        public int Width { get; private set; }
        public int Height { get; private set; }

        public SearchGrid(int w, int h, T init)
        {
            Width = w;
            Height = h;
            SearchNodes = new SearchNode<T>[w, h];

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    SearchNodes[x, y] = new SearchNode<T>(x, y, init);
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
            SearchNodes[x, y] = new SearchNode<T>(x, y, data);
        }

        public SearchNode<T> GetNode(int x, int y)
        {
            if (InRangeX(x) && InRangeY(y))
            {
                return SearchNodes[x, y];
            }
            return null;
        }

        public SearchNode<T> GetNeighbour(int x, int y, int index) // 0 = N, 2 = E, 4 = S, 6 = W
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
                        return SearchNodes[nx, uy];
                    }
                }
                break;
                case 1:
                {
                    if (checkBitSet.Get(1))
                    {
                        return SearchNodes[ux, uy];
                    }
                }
                break;
                case 2:
                {
                    if (checkBitSet.Get(2))
                    {
                        return SearchNodes[ux, ny];
                    }
                }
                break;
                case 3:
                {
                    if (checkBitSet.Get(3))
                    {
                        return SearchNodes[ux, ly];
                    }
                }
                break;
                case 4:
                {
                    if (checkBitSet.Get(4))
                    {
                        return SearchNodes[nx, ly];
                    }
                }
                break;
                case 5:
                {
                    if (checkBitSet.Get(5))
                    {
                        return SearchNodes[lx, ly];
                    }
                }
                break;
                case 6:
                {
                    if (checkBitSet.Get(6))
                    {
                        return SearchNodes[lx, ny];
                    }
                }
                break;
                case 7:
                {
                    if (checkBitSet.Get(7))
                    {
                        return SearchNodes[lx, uy];
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

        meshFilter.sharedMesh = mesh;
        meshRenderer.material.mainTexture = texture;
    }

    //void PlaceDoors() // For a door to be valid 
    //{
    //for (int y = 0; y < grid.Height; y++)
    //{
    //for (int x = 0; x < grid.Width; x++)
    //{
    //SearchNode<NodeState> targetNode = grid.GetNode(x, y);
    //SearchNode<NodeState>[] neighbours = new SearchNode<NodeState>[8];

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
    //Sounds retarded but it works. Although for it to look proper the rooms must be positioned on an even tile and be evenly sized

    void BuildPath(ref SearchGrid<NodeState> searchGrid, SearchNode<NodeState> currentNode, int prevDir)
    {
        currentNode.Visited = true;

        currentNode.Data = NodeState.Path;

        SearchNode<NodeState>[] possibleChildren = new SearchNode<NodeState>[4];
        for (int i = 0; i < possibleChildren.Length; i++)
        {
            SearchNode<NodeState> tmp = searchGrid.GetNeighbour(currentNode.position.x, currentNode.position.y, i * 2);
            if (tmp != null)
            {
                possibleChildren[i] = searchGrid.GetNeighbour(tmp.position.x, tmp.position.y, i * 2);
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

            SearchNode<NodeState> nextNode = possibleChildren[rand];

            if (nextNode != null && !nextNode.Visited && nextNode.Data == NodeState.Empty)
            {
                currentNode.Connect(nextNode);
                nextNode.Connect(currentNode);
                searchGrid.GetNeighbour(currentNode.position.x, currentNode.position.y, rand * 2).Visited = true;
                searchGrid.GetNeighbour(currentNode.position.x, currentNode.position.y, rand * 2).Data = NodeState.Path;

                BuildPath(ref searchGrid, nextNode, rand);
            }
        }
    }

    //TODO: Add Door placement
}