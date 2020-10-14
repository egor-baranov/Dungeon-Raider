using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using UnityEngine.Serialization;

public class Generation : MonoBehaviour
{
    [FormerlySerializedAs("tilemap")] public Tilemap tileMap;
    public Tile wallTile;
    public Tile groundTile;
    public Tile voidTile;
    [SerializeField] public int[,] level;

    public Vector2Int size;
    public Vector2 tileSize;

    public Vector2Int[] roomSizes;
    [Range(0, 100)] public int minRooms;
    [Range(0, 100)] public int maxRooms;

    public int tunnelWidth;

    private int _roomAmount;
    public Vector4[] rooms;
    [FormerlySerializedAs("parrent")] public int[] parentOf;

    public Vector2Int freeSpace;

    public List<Vector3> roomsSorted;


    public int attempts;

    private bool Intersects(Vector4 current, IList<Vector4> previous)
    {
        var cur = new Rect(current.x - freeSpace.x, current.y - freeSpace.y, current.z + freeSpace.x * 2,
            current.w + freeSpace.y * 2);

        for (var i = 0; i < previous.Count; i++)
        {
            var tmp = new Rect(previous[i].x - freeSpace.x, previous[i].y - freeSpace.y,
                previous[i].z + freeSpace.x * 2,
                previous[i].w + freeSpace.y * 2);
            if (tmp.Overlaps(cur))
                return true;
        }

        return false;
    }

    private static int Closest(Vector4 current, IList<Vector4> previous)
    {
        var min = 0;
        float minDist = -1;
        for (var i = 0; i < previous.Count; i++)
        {
            if ((Vector2.Distance(new Vector2(current.x + current.z / 2, current.y + current.w / 2),
                     new Vector2(previous[i].x + previous[i].z / 2, previous[i].y + previous[i].w / 2)) < minDist &&
                 (current.x != previous[i].x || current.y != previous[i].y)) || minDist == -1)
            {
                minDist = Vector2.Distance(new Vector2(current.x, current.y),
                    new Vector2(previous[i].x, previous[i].y));
                min = i;
            }
        }

        return min;
    }

    private void GenerateArena()
    {
        var roomType = 0;
        var x = (int) (size.x / 2 - roomSizes[roomType].x / 2);
        var y = (int) (size.y / 2 - roomSizes[roomType].y / 2);

        var tmp = new Vector4(x, y, roomSizes[roomType].x, roomSizes[roomType].y);

        var temp = new Vector4[rooms.Length + 1];
        rooms.CopyTo(temp, 0);
        rooms = temp;
        rooms[rooms.Length - 1] = tmp;


        for (var y1 = (int) tmp.y; y1 < (int) tmp.y + tmp.w; y1++)
        {
            for (var x1 = (int) tmp.x; x1 < (int) tmp.x + tmp.z; x1++)
            {
                if ((x1 == (int) tmp.x) || (x1 == (int) tmp.x + tmp.z - 1) || (y1 == (int) tmp.y) ||
                    (y1 == (int) tmp.y + tmp.w - 1))
                    level[x1, y1] = 1;
                else
                    level[x1, y1] = 2;
            }
        }
    }

    private void GenerateStarterRooms()
    {
        for (var i = 0; i < 4; i++)
        {
            var roomType = 1;
            var x = 0;
            var y = 0;
            switch (i)
            {
                case 0:
                    x = 0;
                    y = 0;
                    break;
                case 1:
                    x = size.x - roomSizes[roomType].x;
                    y = 0;
                    break;
                case 2:
                    x = 0;
                    y = size.y - roomSizes[roomType].y;
                    break;
                case 3:
                    x = size.x - roomSizes[roomType].x;
                    y = size.y - roomSizes[roomType].y;
                    break;
            }

            var tmp = new Vector4(x, y, roomSizes[roomType].x, roomSizes[roomType].y);

            var tempi = new int[parentOf.Length + 1];
            parentOf.CopyTo(tempi, 0);
            parentOf = tempi;

            parentOf[parentOf.Length - 1] = Closest(tmp, rooms);

            var temp = new Vector4[rooms.Length + 1];
            rooms.CopyTo(temp, 0);
            rooms = temp;
            rooms[rooms.Length - 1] = tmp;


            for (var y1 = (int) tmp.y; y1 < (int) tmp.y + tmp.w; y1++)
            {
                for (var x1 = (int) tmp.x; x1 < (int) tmp.x + tmp.z; x1++)
                {
                    if ((x1 == (int) tmp.x) || (x1 == (int) tmp.x + tmp.z - 1) || (y1 == (int) tmp.y) ||
                        (y1 == (int) tmp.y + tmp.w - 1))
                        level[x1, y1] = 1;
                    else
                        level[x1, y1] = 2;
                }
            }
        }
    }

    private void FindStarterRoomParents()
    {
        for (int i = 0; i < 4; i++)
        {
            var tmp = rooms[i + 1];
            parentOf[i + 1] = Closest(tmp, rooms);
        }
    }

    private void FindParents()
    {
        for (var i = 0; i < parentOf.Length; ++i)
        {
            var tmp = rooms[i];
            parentOf[i] = Closest(tmp, rooms);
        }
    }

    private void GenerateRooms()
    {
        for (var i = 0; i < attempts; ++i)
        {
            var roomType = Random.Range(2, roomSizes.Length);
            var x = Random.Range(5, size.x - roomSizes[roomType].x - 5);
            var y = Random.Range(5, size.y - roomSizes[roomType].y - 5);
            Debug.Log(roomType);
            var tmp = new Vector4(x, y, roomSizes[roomType].x, roomSizes[roomType].y);
            if (rooms.Length == 0 || !Intersects(tmp, rooms))
            {
                var tempi = new int[parentOf.Length + 1];
                parentOf.CopyTo(tempi, 0);
                parentOf = tempi;
                if (rooms.Length > 1)
                    parentOf[parentOf.Length - 1] = Closest(tmp, rooms);
                else
                    parentOf[parentOf.Length - 1] = 0;

                var temp = new Vector4[rooms.Length + 1];
                rooms.CopyTo(temp, 0);
                rooms = temp;
                rooms[rooms.Length - 1] = tmp;


                for (var y1 = (int) tmp.y; y1 < (int) tmp.y + tmp.w; y1++)
                {
                    for (var x1 = (int) tmp.x; x1 < (int) tmp.x + tmp.z; x1++)
                    {
                        if ((x1 == (int) tmp.x) || (x1 == (int) tmp.x + tmp.z - 1) || (y1 == (int) tmp.y) ||
                            (y1 == (int) tmp.y + tmp.w - 1))
                            level[x1, y1] = 1;
                        else
                            level[x1, y1] = 2;
                    }
                }
            }
        }
    }

    private void BuildPaths()
    {
        for (var i = 1; i < parentOf.Length; ++i)
        {
            var beginX = (int) (rooms[i].x + rooms[i].z / 2);
            var beginY = (int) (rooms[i].y + rooms[i].w / 2);
            var endX = (int) (rooms[parentOf[i]].x + rooms[parentOf[i]].z / 2);
            var endY = (int) (rooms[parentOf[i]].y + rooms[parentOf[i]].w / 2);

            if (Mathf.Abs(beginX - endX) > Mathf.Abs(beginY - endY))
            {
                for (var y = -tunnelWidth; y <= tunnelWidth; y++)
                {
                    for (var x = Mathf.Min(beginX, endX); x <= Mathf.Max(beginX, endX); x++)
                    {
                        var tmp = 2;
                        if (y == -tunnelWidth || y == tunnelWidth) tmp = 1;
                        if (level[x, beginY + y] == 0 || level[x, beginY + y] == 1)
                            level[x, beginY + y] = tmp;
                    }
                }

                for (var x = -tunnelWidth; x <= tunnelWidth; x++)
                {
                    for (var y = -tunnelWidth; y <= tunnelWidth; y++)
                    {
                        var tmp = 2;
                        if (Mathf.Abs(x) == tunnelWidth || Mathf.Abs(y) == tunnelWidth) tmp = 1;
                        if (level[endX + x, beginY + y] == 0 || (tmp == 2 && level[endX + x, beginY + y] == 1))
                            level[endX + x, beginY + y] = tmp;
                    }
                }

                for (var x = -tunnelWidth; x <= tunnelWidth; x++)
                for (var y = Mathf.Min(beginY, endY); y <= Mathf.Max(beginY, endY); y++)
                {
                    var tmp = 2;
                    if (x == -tunnelWidth || x == tunnelWidth) tmp = 1;
                    if (level[endX + x, y] == 0 || level[endX + x, y] == 1)
                        level[endX + x, y] = tmp;
                }
            }
            else
            {
                for (var x = -tunnelWidth; x <= tunnelWidth; x++)
                {
                    for (var y = Mathf.Min(beginY, endY); y <= Mathf.Max(beginY, endY); y++)
                    {
                        var tmp = 2;
                        if (x == -tunnelWidth || x == tunnelWidth) tmp = 1;
                        if (level[beginX + x, y] == 0 || level[beginX + x, y] == 1)
                            level[beginX + x, y] = tmp;
                    }
                }

                for (var x = -tunnelWidth; x <= tunnelWidth; x++)
                {
                    for (var y = -tunnelWidth; y <= tunnelWidth; y++)
                    {
                        var tmp = 2;
                        if (Mathf.Abs(x) == tunnelWidth || Mathf.Abs(y) == tunnelWidth) tmp = 1;
                        if (level[beginX + x, endY + y] == 0 || (tmp == 2 && level[beginX + x, endY + y] == 1))
                            level[beginX + x, endY + y] = tmp;
                    }
                }

                for (var y = -tunnelWidth; y <= tunnelWidth; y++)
                {
                    for (var x = Mathf.Min(beginX, endX); x <= Mathf.Max(beginX, endX); x++)
                    {
                        var tmp = 2;
                        if (y == -tunnelWidth || y == tunnelWidth) tmp = 1;
                        if (level[x, endY + y] == 0 || level[x, endY + y] == 1)
                            level[x, endY + y] = tmp;
                    }
                }
            }
        }
    }

    private void BuildTilemap()
    {
        for (var y = 0; y < size.y; y++)
        {
            for (var x = 0; x < size.x; x++)
            {
                switch (level[x, y])
                {
                    case 0:
                        tileMap.SetTile(new Vector3Int(x, y, 0), voidTile);
                        break;
                    case 1:
                        tileMap.SetTile(new Vector3Int(x, y, 0), wallTile);
                        break;
                    case 2:
                        tileMap.SetTile(new Vector3Int(x, y, 0), groundTile);
                        break;
                }
            }
        }
    }

    private int CompareToTarget(Vector3 a, Vector3 b)
    {
        Vector3 target = new Vector3(0f, 0f, a.z);
        float da = (a - target).sqrMagnitude;
        target.z = b.z;
        float db = (b - target).sqrMagnitude;

        if (da < db)
            return -1;
        else if (db < da)
            return 1;
        return 0;
    }

    private void SortRooms()
    {
        for (int i = 0; i < rooms.Length; i++)
        {
            roomsSorted.Add(new Vector3(rooms[i].x + rooms[i].z / 2, rooms[i].y + rooms[i].w / 2, i));
        }

        roomsSorted.Sort(CompareToTarget);
    }

    private void Start()
    {
        level = new int[size.x, size.y];

        //generateArena();

        //generateStarterRooms();

        //generateRooms();

        //sortRooms();

        //findParents();

        //buildPaths();

        BuildTilemap();
    }

    private void Update()
    {
        for (int i = 0; i < roomsSorted.Count - 3; i += 3)
        {
            Debug.DrawLine(tileMap.CellToWorld(new Vector3Int((int) roomsSorted[i].x, (int) roomsSorted[i].y, 0)),
                tileMap.CellToWorld(new Vector3Int((int) roomsSorted[i + 1].x, (int) roomsSorted[i + 1].y, 0)));
            Debug.DrawLine(
                tileMap.CellToWorld(new Vector3Int((int) roomsSorted[i + 2].x, (int) roomsSorted[i + 2].y, 0)),
                tileMap.CellToWorld(new Vector3Int((int) roomsSorted[i + 1].x, (int) roomsSorted[i + 1].y, 0)));
            Debug.DrawLine(tileMap.CellToWorld(new Vector3Int((int) roomsSorted[i].x, (int) roomsSorted[i].y, 0)),
                tileMap.CellToWorld(new Vector3Int((int) roomsSorted[i + 2].x, (int) roomsSorted[i + 2].y, 0)));
        }
    }
}