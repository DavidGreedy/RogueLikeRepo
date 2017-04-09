using UnityEngine;

public abstract class SingletonBehaviour<T> : SingletonBehaviour where T : MonoBehaviour
{
    public static T Instance
    {
        get { return instance; }
    }

    private static T instance = default(T);

    public override void Setup()
    {
        if (instance != this) instance = this as T;
    }

    public override void Clear()
    {
        if (instance == this) instance = null;
    }
}

public abstract class SingletonBehaviour : MonoBehaviour
{
    public abstract void Setup();
    public abstract void Clear();

    protected virtual void Awake()
    {
        Setup();
    }

    protected virtual void OnDestroy()
    {
        Clear();
    }
}