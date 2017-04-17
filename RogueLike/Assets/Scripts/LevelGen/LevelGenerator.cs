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
        //OnPhysicsComplete += SelectRooms;
    }

    void CreateRoomsInCircle()
    {
        m_pRooms = new List<PhysicsRoom>();
        for (int i = 0; i < numRooms; i++)
        {
            Vector2 randomPosition = Random.insideUnitCircle * 30.0f;

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
                physicsRoom.UpdatePosition();
            }
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

    private Rect GetBounds()
    {
        float xMin = float.MaxValue;
        float xMax = float.MinValue;
        float yMin = float.MaxValue;
        float yMax = float.MinValue;

        foreach (PhysicsRoom physicsRoom in m_pRooms)
        {
            xMin = Mathf.Min(physicsRoom.Rect.xMin, xMin);
            xMax = Mathf.Max(physicsRoom.Rect.xMax, xMax);
            yMin = Mathf.Min(physicsRoom.Rect.yMin, yMin);
            yMax = Mathf.Max(physicsRoom.Rect.yMax, yMax);
        }

        Rect bounds = new Rect();
        bounds.xMin = xMin;
        bounds.xMax = xMax;
        bounds.yMin = yMin;
        bounds.yMax = yMax;

        return bounds;
    }

    private void OnDrawGizmos()
    {
        if (m_pRooms == null)
        {
            return;
        }

        foreach (PhysicsRoom physicsRoom in m_pRooms)
        {
            DrawRect(physicsRoom.Rect, Color.white);
        }

        DrawRect(GetBounds(), Color.red);
    }

    public static int Round(float n, int m)
    {
        return Mathf.FloorToInt(((n + m - 1) / m)) * m;
    }

    public void DrawRect(Rect rect, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawLine(new Vector2(rect.xMin, rect.yMax), new Vector2(rect.xMax, rect.yMax));
        Gizmos.DrawLine(new Vector2(rect.xMax, rect.yMax), new Vector2(rect.xMax, rect.yMin));
        Gizmos.DrawLine(new Vector2(rect.xMax, rect.yMin), new Vector2(rect.xMin, rect.yMin));
        Gizmos.DrawLine(new Vector2(rect.xMin, rect.yMin), new Vector2(rect.xMin, rect.yMax));
    }
}

public class PhysicsRoom : MonoBehaviour
{
    private BoxCollider2D collider;
    public Rigidbody2D rigidbody;
    private Rect rect;

    public Rect Rect { get { return rect; } }

    public Vector3 Position { get { return new Vector2(LevelGenerator.Round(collider.transform.position.x, 1), LevelGenerator.Round(collider.transform.position.y, 1)); } }

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

    public void UpdatePosition()
    {
        rect.xMin = LevelGenerator.Round(collider.transform.position.x - (collider.size.x / 2f), 1);
        rect.xMax = LevelGenerator.Round(collider.transform.position.x + (collider.size.x / 2f), 1);
        rect.yMin = LevelGenerator.Round(collider.transform.position.y - (collider.size.y / 2f), 1);
        rect.yMax = LevelGenerator.Round(collider.transform.position.y + (collider.size.y / 2f), 1);
    }
}