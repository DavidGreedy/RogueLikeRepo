using System;
using System.Collections.Generic;

class Graph
{
    public class Edge : IComparable<Edge>
    {
        public int src, dest;
        public float weight;

        public Edge()
        { }

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

    class Subset
    {
        public int parent, rank;
    };

    int vertexCount, edgeCount;
    Edge[] edges;

    public Graph(List<int> vs, List<Edge> es)
    {
        vertexCount = vs.Count;
        edgeCount = es.Count;
        edges = es.ToArray();
    }

    int Find(Subset[] subsets, int i)
    {
        if (subsets[i].parent != i)
        {
            subsets[i].parent = Find(subsets, subsets[i].parent);
        }

        return subsets[i].parent;
    }

    // A function that does union of two sets of x and y
    // (uses union by rank)
    void Union(Subset[] subsets, int x, int y)
    {
        int xroot = Find(subsets, x);
        int yroot = Find(subsets, y);

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
        Edge[] kruskalMst = new Edge[vertexCount];
        int resultIndex = 0;
        int sortedIndex = 0;

        for (sortedIndex = 0; sortedIndex < vertexCount; ++sortedIndex)
        {
            kruskalMst[sortedIndex] = new Edge();
        }

        // Step 1:  Sort all the edges in non-decreasing order of their
        // weight.  If we are not allowed to change the given graph, we
        // can create a copy of array of edges
        Array.Sort(edges);

        // Allocate memory for creating vertexCount ssubsets
        Subset[] subsets = new Subset[vertexCount];
        for (sortedIndex = 0; sortedIndex < vertexCount; ++sortedIndex)
        {
            subsets[sortedIndex] = new Subset();
        }

        // Create vertexCount subsets with single elements
        for (int v = 0; v < vertexCount; ++v)
        {
            subsets[v].parent = v;
            subsets[v].rank = 0;
        }

        sortedIndex = 0; // Index used to pick next edges

        // Number of edges to be taken is equal to vertexCount-1
        while (resultIndex < vertexCount - 1)
        {
            // Step 2: Pick the smallest edges. And increment the index
            // for next iteration
            Edge next_edge = edges[sortedIndex++];

            int x = Find(subsets, next_edge.src);
            int y = Find(subsets, next_edge.dest);

            // If including this edges does't cause cycle, include it
            // in result and increment the index of result for next edges
            if (x != y)
            {
                kruskalMst[resultIndex++] = next_edge;
                Union(subsets, x, y);
            }
            // Else discard the next_edge
        }

        return kruskalMst;
    }
}

public class Pair<T1, T2>
{
    public Pair() { }

    public Pair(T1 first, T2 second)
    {
        this.First = first;
        this.Second = second;
    }

    public override int GetHashCode()
    {
        int hash = 17;
        // Suitable nullity checks etc, of course :)
        hash = hash * 23 + First.GetHashCode();
        hash = hash * 23 + Second.GetHashCode();
        return hash;
    }

    public override bool Equals(object obj)
    {
        return GetHashCode() == obj.GetHashCode();
    }

    public T1 First { get; set; }
    public T2 Second { get; set; }
}