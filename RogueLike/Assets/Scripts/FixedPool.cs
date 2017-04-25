using System.Collections.Generic;
using UnityEngine;

public class MonoPool<T> where T : MonoBehaviour
{
    private T[] m_objectQueue;
    private int m_head;

    public MonoPool(int size, T initialValue)
    {
        m_objectQueue = new T[size];
        m_head = 0;

        for (int i = 0; i < size; i++)
        {
            T poolObject = GameObject.Instantiate(initialValue);
            poolObject.gameObject.hideFlags = HideFlags.HideInHierarchy;
            poolObject.gameObject.name = typeof(T).Name + " " + i;
            poolObject.gameObject.SetActive(false);
            m_objectQueue[i] = poolObject;
        }
    }

    public virtual T Next<T>() where T : MonoBehaviour
    {
        T returnObject = m_objectQueue[m_head] as T;

        int tempHead = m_head + 1;
        m_head = tempHead >= m_objectQueue.Length ? 0 : m_head + 1;
        return returnObject;
    }
}

public class FixedPool<T>
{
    private Queue<T> m_objectQueue;

    public FixedPool(int size, T initialValue)
    {
        m_objectQueue = new Queue<T>(size);

        for (int i = 0; i < size; i++)
        {
            m_objectQueue.Enqueue(initialValue);
        }
    }

    public virtual T Next()
    {
        return m_objectQueue.Dequeue();
    }
}