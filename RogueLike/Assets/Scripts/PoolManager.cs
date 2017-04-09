using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : SingletonBehaviour<PoolManager>
{
    private static Dictionary<Type, MonoPool<MonoBehaviour>> m_pooledMonoBehaviours;

    public override void Setup()
    {
        base.Setup();
        m_pooledMonoBehaviours = new Dictionary<Type, MonoPool<MonoBehaviour>>();
    }

    public void Add<T>(T objectToPool, int count) where T : MonoBehaviour
    {
        m_pooledMonoBehaviours.Add(typeof(T), new MonoPool<MonoBehaviour>(count, objectToPool));

        Debug.Log("ADDED " + count + " " + typeof(T).Name + " to pool");
    }

    public T Next<T>() where T : MonoBehaviour
    {
        MonoPool<MonoBehaviour> pool;
        for (int i = 0; i < m_pooledMonoBehaviours.Count; i++)
        {
            m_pooledMonoBehaviours.TryGetValue(typeof(T), out pool);
            if (pool != null)
            {
                T returnObject = pool.Next<T>();
                returnObject.gameObject.SetActive(true);
                return returnObject;
            }
        }
        throw new Exception("Pool does not contain a " + typeof(T).Name);
    }
}