using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoomGeneration : MonoBehaviour
{
    [SerializeField]
    private int numRooms;

    private int maxConnections;

    private List<Vector2> points;

    private List<PhysicsRoom> pRooms;
    [SerializeField]
    private bool isComplete;

    private void Start()
    {
        Create();
    }

    void Create()
    {
        points = new List<Vector2>();

        Room outRoom;
        int numAttempts = 0;

        pRooms = new List<PhysicsRoom>();
        Time.timeScale = 100f;
        for (int i = 0; i < numRooms; i++)
        {
            Vector2 randomPosition = Random.insideUnitCircle * 100;

            pRooms.Add(new PhysicsRoom());
            pRooms[i].Init(transform, randomPosition.x, randomPosition.y, Random.Range(5, 25), Random.Range(5, 25));
        }

        //while (Room.Count < numRooms || numAttempts < 100)
        //{
        //    Room.TryPlace(Random.Range(0, 100), Random.Range(0, 100), Random.Range(5, 10), Random.Range(5, 10), out outRoom);
        //    numAttempts++;
        //}

        points = Room.Points();
        print(Room.Count);
    }

    private void FixedUpdate()
    {
        if (pRooms != null)
        {
            for (int i = 0; i < numRooms; i++)
            {
                pRooms[i].Attract(Vector3.zero, 0.01f);
            }
            isComplete = true;
        }
    }

    private void Update()
    {
        if (points != null)
        {
            for (int i = 0; i < (points.Count / 4); i++)
            {
                int index = i * 4;
                Debug.DrawLine(points[index], points[index + 1]);
                Debug.DrawLine(points[index + 1], points[index + 2]);
                Debug.DrawLine(points[index + 2], points[index + 3]);
                Debug.DrawLine(points[index + 3], points[index]);
            }
        }
    }

    class PhysicsRoom
    {
        private GameObject gameObject;
        private BoxCollider2D collider;
        public Rigidbody2D rigidbody;

        public void Init(Transform parent, float x, float y, int w, int h)
        {
            gameObject = new GameObject();
            gameObject.transform.position = new Vector3(x, y, 0);
            gameObject.transform.parent = parent;

            collider = gameObject.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(w, h);

            rigidbody = gameObject.AddComponent<Rigidbody2D>();
            rigidbody.mass = 0.1f;
            rigidbody.Sleep();
            rigidbody.gravityScale = 0;
            rigidbody.freezeRotation = true;
        }

        public Room GetRoom()
        {
            int positionX = RoundToGrid(gameObject.transform.position.x, 1);
            int positionY = RoundToGrid(gameObject.transform.position.y, 1);
            int sizeX = RoundToGrid(collider.size.x, 1);
            int sizeY = RoundToGrid(collider.size.y, 1);

            return new Room(positionX, positionY, sizeX, sizeY);
        }

        public static int RoundToGrid(float number, int gridSize)
        {
            return Mathf.FloorToInt(((number + gridSize - 1) / gridSize)) * gridSize;
        }

        public void Attract(Vector3 position, float dt)
        {
            rigidbody.AddForce((position - gameObject.transform.position).normalized * dt);
        }
    }

    class Room
    {
        public int x, y, w, h;
        static int minRoomSize, maxRoomSize;
        static int doorOffset;
        private static int doorWidth;
        private static int wallThickness;

        private static List<Room> rooms = new List<Room>();

        public static int Count
        {
            get { return rooms.Count; }
        }

        public Room(int x, int y, int w, int h)
        {
            this.x = x;
            this.y = y;
            this.w = w;
            this.h = h;

            rooms.Add(this);
        }

        ~Room()
        {
            rooms.Remove(this);
        }

        /*public Room(Room connectedRoom, int orientation)
        {
            switch (orientation)
            {
                case 0:
                {
                    this.x = Random.Range(connectedRoom.x, x + w);
                }
                break;

                case 1:
                {
                    this.y = Random.Range(connectedRoom.x, x + w);
                }
                break;
                case 2:
                {
                    this.x = Random.Range(connectedRoom.x, x + w);
                }
                break;

                case 3:
                {
                    this.y = Random.Range(connectedRoom.x, x + w);
                }
                break;
            }
        }
        */
        public static bool TryPlace(int x, int y, int w, int h, out Room room)
        {
            for (int i = 0; i < rooms.Count; i++)
            {
                if (rooms[i].Overlaps(x, y, w, h, 1))
                {
                    room = null;
                    return false;
                }
            }
            room = new Room(x, y, w, h);
            return true;
        }

        //public static bool TryPlaceTouching(int x, int y, int w, int h, out Room room)
        //{
        //    if (rooms.Count == 0)
        //    {
        //        room = new Room(x, y, w, h);
        //        return true;
        //    }
        //    for (int i = 0; i < rooms.Count; i++)
        //    {
        //        if (rooms[i].Touches(x, y, w, h) && rooms[i].Overlaps(x, y, w, h, 0))
        //        {
        //            room = new Room(x, y, w, h);
        //            return true;
        //        }
        //    }
        //    room = null;
        //    return false;
        //}

        public bool Overlaps(int x, int y, int w, int h, int overlap)
        {
            return !(this.x + this.w - overlap < x ||
                     this.x + overlap > x + w ||
                     this.y + this.h - overlap < y ||
                     this.y + overlap > y + h);
        }

        public bool Touches(int x, int y, int w, int h)
        {
            return (this.x + this.w == x || this.x == x + w || this.y + this.h == y || this.y == y + h);
        }

        public void Connnect(Room other)
        {

            //if (x + doorOffset > x + w - doorOffset - doorWidth || y + doorOffset > y + h - doorOffset - doorWidth)
            //{
            //    throw new Exception("Door cannot be placed with given parameters!");
            //}
            //
            //switch (side)
            //{
            //    case 0:
            //    {
            //        int randomX = Random.Range(x + doorOffset, x + w - doorOffset - doorWidth);
            //
            //        doors.Add(new Door(randomX, y + h, doorWidth, side));
            //    }
            //    return;
            //    case 1:
            //    {
            //        int randomY = Random.Range(y + doorOffset, y + h - doorOffset - doorWidth);
            //
            //        doors.Add(new Door(x + w, randomY, doorWidth, side));
            //    }
            //    return;
            //    case 2:
            //    {
            //        int randomX = Random.Range(x + doorOffset, x + w - doorOffset - doorWidth);
            //
            //        doors.Add(new Door(randomX, y, doorWidth, side));
            //    }
            //    return;
            //    case 3:
            //    {
            //        int randomY = Random.Range(y + doorOffset, y + h - doorOffset - doorWidth);
            //
            //        doors.Add(new Door(x, randomY, doorWidth, side));
            //    }
            //    return;
            //}
        }

        public Vector2[] GetPoints()
        {
            Vector2[] points =
            {
                new Vector2(x, y),
                new Vector2(x + w, y),
                new Vector2(x + w, y + h),
                new Vector2(x, y + h)
            };

            return points;
        }

        public static List<Vector2> Points()
        {
            List<Vector2> points = new List<Vector2>();

            for (int i = 0; i < Count; i++)
            {
                points.AddRange(rooms[i].GetPoints());
            }

            return points;
        }
    }

    class Door
    {
        private int x, y, w, o;
        private Room roomA, roomB;
        public Door(int x, int y, int w, int o)
        {
            this.x = x;
            this.y = y;
            this.w = w;
            this.o = o;
        }
    }
}