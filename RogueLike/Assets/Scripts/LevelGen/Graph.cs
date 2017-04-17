using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Graph<T>
{
    private List<Node<T>> nodes;

    public List<Pair<Node<T>, Node<T>>> Connections()
    {
        List<Pair<Node<T>, Node<T>>> connections = new List<Pair<Node<T>, Node<T>>>();

        for (int i = 0; i < nodes.Count; i++)
        {
            List<Node<T>> nodeConnections = nodes[i].Connections;

            for (int j = 0; j < nodeConnections.Count; j++)
            {
                connections.Add(new Pair<Node<T>, Node<T>>(nodes[i], nodeConnections[j]));
            }
        }
        return connections;
    }

    public List<Node<T>> Nodes()
    {
        return nodes;
    }

    public List<T> Data()
    {
        List<T> data = new List<T>();

        for (int i = 0; i < nodes.Count; i++)
        {
            data.Add(nodes[i].Data);
        }

        return data;
    }
}

[System.Serializable]
public class Node<T>
{
    private T data;
    private List<Node<T>> connections;

    public List<Node<T>> Connections { get { return connections; } }

    public T Data { get { return data; } }

    public Node(T data)
    {
        this.data = data;
        connections = new List<Node<T>>();
    }

    public void Connect(Node<T> other)
    {
        if (!connections.Contains(other))
        {
            connections.Add(other);
        }
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

    public T1 First { get; set; }
    public T2 Second { get; set; }
}