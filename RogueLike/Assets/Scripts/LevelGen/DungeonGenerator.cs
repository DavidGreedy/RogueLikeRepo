using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField]
    private uint m_width, m_height;

    [SerializeField]
    private uint m_roomWidthAverage, m_roomHeightAverage;

    private enum TileState
    {
        EMPTY = 0,
        WALL = 1,
        ROOM = 2,
        CORRIDOR = 3
    }

    private TileState[,] m_map;

    private List<Box> m_rooms;
    private List<Box> m_failedRooms;

    private Box m_nextRoom;

    void Start()
    {
        m_map = new TileState[m_width, m_height];

        for (int y = 0; y < m_height; y++)
        {
            for (int x = 0; x < m_width; x++)
            {
                m_map[x, y] = TileState.EMPTY;
            }
        }

        int index = 0;

        bool placementGood = false;

        m_rooms = new List<Box>();
        m_failedRooms = new List<Box>();
        //for (int i = 0; i < m_roomCount; i++)
        //{
        //    room = CreateRandomRoom();

        //    while (!placementGood && index < 10000)
        //    {
        //        for (int j = 0; j < rooms.Count; j++)
        //        {
        //            if (!mapBox.Contains(room) || rooms[i].Touches(room))
        //            {
        //                break;
        //            }
        //            if (j == rooms.Count)
        //            {
        //                placementGood = true;
        //            }
        //        }
        //        if (!placementGood)
        //        {
        //            room = CreateRandomRoom();
        //        }
        //        index++;
        //    }
        //    rooms.Add(room);
        //    placementGood = false;
        //}
        StartCoroutine(PlaceRooms());

        //for (int i = 0; i < m_rooms.Count; i++)
        //{
        //    AddRoomToMap(m_rooms[i]);
        //}
    }

    IEnumerator PlaceRooms()
    {
        Box mapBox = new Box(0, 0, (int)m_width, (int)m_height);

        //m_nextRoom = CreateRandomRoom();

        bool mapContainsRoom = false;
        bool roomOk = false;

        int index = 0;

        while (index < 1000)
        {
            m_nextRoom = CreateRandomRoom();
            mapContainsRoom = mapBox.Contains(m_nextRoom);
            roomOk = CheckRoomPlacement(m_nextRoom, m_rooms);

            while (!mapContainsRoom || !roomOk)
            {
                m_failedRooms.Add(m_nextRoom);
                m_nextRoom = CreateRandomRoom();
                mapContainsRoom = mapBox.Contains(m_nextRoom);
                roomOk = CheckRoomPlacement(m_nextRoom, m_rooms);
                yield return new WaitForSeconds(0.01f);
                index++;
            }
            m_rooms.Add(m_nextRoom);
            yield return new WaitForSeconds(0.1f);
        }
        print("LEVEL GENERATION COPMPLETE");
    }

    void Update()
    {
        for (int i = 0; i < m_failedRooms.Count; i++)
        {
            m_failedRooms[i].Draw(new Color(0, 0, 1, (i / (float)m_failedRooms.Count) * i));
        }

        if (m_failedRooms.Count > 20)
        {
            m_failedRooms.RemoveAt(0);
        }

        for (int i = 0; i < m_rooms.Count; i++)
        {
            m_rooms[i].Draw(Color.red);
        }

        if (m_nextRoom != null)
        {
            m_nextRoom.Draw(Color.green);
        }

    }

    bool CheckRoomPlacement(Box box, List<Box> boxes)
    {
        for (int i = 0; i < boxes.Count; i++)
        {
            if (boxes[i].Intersects(box))
            {
                return false;
            }
        }
        return true;
    }

    Box CreateRandomRoom()
    {
        int x, y, w, h;

        w = Random.Range(5, 15);
        h = Random.Range(5, 15);

        x = (int)Random.Range(1, m_width - w);
        y = (int)Random.Range(1, m_height - h);

        return new Box(x, y, w, h);
    }

    void CreateRoomAt(int x, int y, int w, int h, bool force = false)
    {
        if (x > m_width || y > m_height)
        {
            Debug.Log("Out of range");
            return;
        }
        for (int j = y; j < y + h; j++)
        {
            for (int i = x; i < x + w; i++)
            {
                m_map[i, j] = TileState.ROOM;
            }
        }
    }

    void AddRoomToMap(Box room)
    {
        for (int j = room.y; j < room.y + room.h; j++)
        {
            for (int i = room.x; i < room.x + room.w; i++)
            {
                m_map[i, j] = TileState.ROOM;
            }
        }
    }

    void DrawTile(int x, int y)
    {
        Color c = m_map[x, y] == TileState.ROOM ? Color.red : Color.blue;
        Handles.DrawSolidRectangleWithOutline(new Rect(x, y, 1, 1), c, Color.clear);
    }

    private void OnDrawGizmos()
    {
        //if (Application.isPlaying)
        //{
        //    for (int y = 0; y < m_height; y++)
        //    {
        //        for (int x = 0; x < m_width; x++)
        //        {
        //            DrawTile(x, y);
        //        }
        //    }
        //}
    }

    private class Box
    {
        public int x, y, w, h;

        public Box(int x, int y, int w, int h)
        {
            this.x = x;
            this.y = y;
            this.w = w;
            this.h = h;
        }

        public bool Contains(int x, int y)
        {
            return (x > this.x && x < this.x + this.w && y > this.y && y < this.y + this.h);
        }

        public bool LiesOnEdge(int x, int y)
        {
            return (x >= this.x || x <= this.x + this.w || y >= this.y || y <= this.y + this.h);
        }

        public bool LiesOnEdge(Box other)
        {
            return (this.x == other.x + other.w || this.x + this.w == other.x ||
                    this.y == other.y + other.h || this.y + this.h == other.y);
        }

        public bool Contains(Box other)
        {
            return (this.x < other.x
                 && this.y < other.y
                 && this.x + this.w > other.x + other.w
                 && this.y + this.h > other.y + other.h);
        }

        public bool Intersects(Box other)
        {
            return !(this.x + this.w < other.x ||
                     this.x > other.x + other.w ||
                     this.y + this.h < other.y ||
                     this.y > other.y + other.h);
        }

        public Box Intersection(Box other)
        {
            int xMin = Mathf.Max(this.x, other.x);
            int yMin = Mathf.Max(this.y, other.y);
            int xMax = Mathf.Min(this.x + this.w, other.x + other.w);
            int yMax = Mathf.Min(this.y + this.h, other.y + other.h);

            return new Box(xMin, yMin, xMax - xMin, yMax - yMin);
        }

        public void Draw(Color color)
        {
            Debug.DrawLine(new Vector2(x, y + h), new Vector2(x + w, y + h), color);
            Debug.DrawLine(new Vector2(x + w, y + h), new Vector2(x + w, y), color);
            Debug.DrawLine(new Vector2(x + w, y), new Vector2(x, y), color);
            Debug.DrawLine(new Vector2(x, y), new Vector2(x, y + h), color);
        }
    }
}