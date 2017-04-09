using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public ProjectileBehaviour projectile;

    void Start()
    {
        if (PoolManager.Instance)
        {
            PoolManager.Instance.Add(projectile, 100);
        }
    }
}