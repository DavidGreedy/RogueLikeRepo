using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LevelGenerator : MonoBehaviour
{
    private List<PhysicsRoom> m_pRooms;


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
        StartCoroutine(CreateRoomsInCircle());
    }

    IEnumerator CreateRoomsInCircle()
    {
        m_pRooms = new List<PhysicsRoom>();
        for (int i = 0; i < numRooms; i++)
        {
            Vector2 randomPosition = Random.insideUnitCircle * 30f;
            m_pRooms.Add(new PhysicsRoom());
            m_pRooms[i].Init(transform, randomPosition, new Vector2(Random.Range(targetWidth - 5, targetWidth + 5), Random.Range(targetHeight - 5, targetHeight + 5)));
            //m_rooms.Add(new Room(Mathf.RoundToInt(randomPosition.x), Mathf.RoundToInt(randomPosition.y), targetWidth, targetHeight));
        }
        yield return new WaitForSeconds(0.00f);
    }

    private void Update()
    {
        if (!isDone)
        {
            foreach (PhysicsRoom physicsRoom in m_pRooms)
            {
                if (!physicsRoom.rigidbody.IsSleeping())
                {
                    return;
                }
            }
            // ALL OF THE ROOMS HAVE REACHED THEIR FINAL POSITIONS SO ROUND THEM TO THE NEAREST INTEGER
            foreach (PhysicsRoom physicsRoom in m_pRooms)
            {
                Vector3 position = physicsRoom.rigidbody.transform.position;
                physicsRoom.rigidbody.transform.position = new Vector3(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
                Destroy(physicsRoom.rigidbody);
            }
            isDone = true;
        }
    }
}

public class PhysicsRoom : MonoBehaviour
{
    private BoxCollider2D collider;
    public Rigidbody2D rigidbody;

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