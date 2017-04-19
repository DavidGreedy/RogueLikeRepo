using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters;

[System.Serializable]
public class Graph<T>
{
    private List<Node<T>> nodes;

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

public class Tree<T>
{
    private Node<T> rootNode;

    public Tree(Node<T> rootNode)
    {
        this.rootNode = rootNode;
    }

    public List<Node<T>> GetAll()
    {
        List<Node<T>> children = new List<Node<T>>();
        rootNode.AllChildren(ref children);
        return children;
    }
}

[System.Serializable]
public class Node<T>
{
    private T data;
    private Node<T> parent;
    private List<Node<T>> children;

    public T Data
    {
        get { return data; }
    }

    public Node<T> Parent
    {
        get { return parent; }
        private set { parent = value; }
    }

    public List<Node<T>> Children
    {
        get { return children; }
    }

    public void AllChildren(ref List<Node<T>> allChildren)
    {
        allChildren.AddRange(children);
        if (children != null)
        {
            foreach (Node<T> child in children)
            {
                child.AllChildren(ref allChildren);
            }
        }
    }

    public Node(T data, Node<T> parent)
    {
        this.data = data;
        this.parent = parent;


        if (parent != null)
        {
            parent.AddChild(this);
        }
    }

    public Node<T> Root()
    {
        return parent != null ? parent.Root() : this;
    }

    public void AddChild(Node<T> child)
    {
        if (children == null)
        {
            this.children = new List<Node<T>>();
        }
        children.Add(child);
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