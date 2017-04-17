using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;


public class LevelGenerator : MonoBehaviour
{
    private List<PhysicsRoom> m_pRooms;
    private List<PhysicsRoom> m_selectedRooms;

    [SerializeField]
    private int numRooms;

    [SerializeField]
    private int targetWidth;

    [SerializeField]
    private int targetHeight;

    [SerializeField]
    private bool isDone = false;

    void Start()
    {
        CreateRoomsInCircle();
        OnPhysicsComplete += SelectRooms;
    }

    void CreateRoomsInCircle()
    {
        m_pRooms = new List<PhysicsRoom>();
        for (int i = 0; i < numRooms; i++)
        {
            Vector2 randomPosition = GetRandomPointInCircle(30f);
            m_pRooms.Add(new PhysicsRoom());
            m_pRooms[i].Init(transform, randomPosition, new Vector2(Random.Range(targetWidth - 5, targetWidth + 5), Random.Range(targetHeight - 5, targetHeight + 5)));
        }
    }

    void SelectRooms()
    {
        for (int i = 0; i < m_pRooms.Count; i++)
        {
            if (Random.value > 0.1f)
            {
                m_pRooms.RemoveAt(i);
            }
        }
    }

    public event Action OnPhysicsComplete;

    private void Update()
    {
        foreach (PhysicsRoom physicsRoom in m_pRooms)
        {
            physicsRoom.Draw();
        }

        if (isDone)
        {
            //for (int i = 0; i < m_pRooms.Count; i++)
            //{
            //    for (int j = i; j < m_pRooms.Count; j++)
            //    {
            //        Debug.DrawLine(m_pRooms[i].Position, m_pRooms[j].Position);
            //    }
            //}
        }

        if (!isDone)
        {
            foreach (PhysicsRoom physicsRoom in m_pRooms)
            {
                if (!physicsRoom.rigidbody.IsSleeping())
                {
                    return;
                }
            }
            foreach (PhysicsRoom physicsRoom in m_pRooms)
            {
                Destroy(physicsRoom.rigidbody);
            }
            isDone = true;

            if (OnPhysicsComplete != null)
            {
                OnPhysicsComplete.Invoke();
            }
        }
    }

    public static int Round(float n, int m)
    {
        return Mathf.FloorToInt(((n + m - 1) / m)) * m;
    }

    Vector2 GetRandomPointInCircle(float radius)
    {
        float t = 2 * Mathf.PI * Random.value;
        float u = Random.value + Random.value;
        float r = u > 1 ? 2 - u : u;

        return new Vector2(Round(radius * r * Mathf.Cos(t), 1), Round(radius * r * Mathf.Sin(t), 1));
    }
}

public class PhysicsRoom : MonoBehaviour
{
    private BoxCollider2D collider;
    public Rigidbody2D rigidbody;

    public Vector3 Position
    {
        get
        {
            return new Vector2(LevelGenerator.Round(collider.transform.position.x, 1), LevelGenerator.Round(collider.transform.position.y, 1));
        }
    }

    public void Init(Transform parent, Vector3 position, Vector2 size)
    {
        GameObject g = new GameObject();
        g.transform.position = position;
        g.transform.parent = parent;
        collider = g.AddComponent<BoxCollider2D>();
        collider.size = size;

        rigidbody = g.AddComponent<Rigidbody2D>();
        rigidbody.Sleep();
        rigidbody.gravityScale = 0;
        rigidbody.freezeRotation = true;
    }

    public void Draw()
    {
        float xMin = LevelGenerator.Round(collider.transform.position.x - (collider.size.x / 2f), 1);
        float xMax = LevelGenerator.Round(collider.transform.position.x + (collider.size.x / 2f), 1);
        float yMin = LevelGenerator.Round(collider.transform.position.y - (collider.size.y / 2f), 1);
        float yMax = LevelGenerator.Round(collider.transform.position.y + (collider.size.y / 2f), 1);

        Debug.DrawLine(new Vector2(xMin, yMax), new Vector3(xMax, yMax));
        Debug.DrawLine(new Vector2(xMax, yMax), new Vector3(xMax, yMin));
        Debug.DrawLine(new Vector2(xMax, yMin), new Vector3(xMin, yMin));
        Debug.DrawLine(new Vector2(xMin, yMin), new Vector3(xMin, yMax));
    }

}

public class Room
{
    private Rect m_rect;

    public Rect Rect
    {
        get { return m_rect; }
    }

    public Room(int x, int y, int w, int h)
    {
        m_rect = new Rect(x, y, w, h);
    }

    public void Draw()
    {
        Debug.DrawLine(new Vector2(m_rect.xMin, m_rect.yMax), new Vector3(m_rect.xMax, m_rect.yMax));
        Debug.DrawLine(new Vector2(m_rect.xMax, m_rect.yMax), new Vector3(m_rect.xMax, m_rect.yMin));
        Debug.DrawLine(new Vector2(m_rect.xMax, m_rect.yMin), new Vector3(m_rect.xMin, m_rect.yMin));
        Debug.DrawLine(new Vector2(m_rect.xMin, m_rect.yMin), new Vector3(m_rect.xMin, m_rect.yMax));
    }
}