using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralGeneration : MonoBehaviour
{
    public int[,] roomTypes =
    {
        {6, 11, 9, 12, 14, 10, 7, 13, 8}, {6, 9, 0, 7, 8, 0, 0, 0, 0}, {0, 4, 0, 5, 8, 0, 0, 0, 0},
        {0, 0, 4, 0, 6, 8, 0, 2, 0}, {0, 0, 0, 0, 1, 0, 0, 0, 0}, {0, 0, 0, 0, 6, 3, 0, 2, 0},
        {0, 0, 0, 5, 9, 0, 0, 7, 3}, {5, 11, 3, 0, 2, 0, 0, 0, 0}, {0, 4, 0, 0, 2, 0, 0, 0, 0},
        {0, 0, 0, 5, 3, 0, 0, 0, 0}
    };

    public Vector2Int size;
    public int[,] level;
    public GameObject[] Tiles;

    public Vector2 tileSize;

    public int attempts;

    public float fillPercentage;

    private int[,] replace()
    {
        var x = Random.Range(0, size.x - 3);
        var y = Random.Range(0, size.y - 3);
        var roomType = Random.Range(1, roomTypes.GetLength(0));
        int[,] tmp = (int[,]) level.Clone();

        for (int y1 = 0; y1 < 3; y1++)
        {
            for (int x1 = 0; x1 < 3; x1++)
            {
                if (tmp[x1 + x, y1 + y] != 0)
                {
                    return level;
                }

                int ttp = roomTypes[roomType, y1 * 3 + x1];
                tmp[x1 + x, y1 + y] = ttp;
            }
        }

        return tmp;
    }

    private void fill()
    {
        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                if (level[x, y] == 0 && Random.Range(0, 100) <= fillPercentage)
                {
                    level[x, y] = 1;
                }
            }
        }
    }

    private void createArena()
    {
        level = new int[size.x, size.y];
        int x = (int) (size.x / 2) - 1;
        int y = (int) (size.y / 2) - 1;
        int roomType = 0;

        for (int y1 = 0; y1 < 3; y1++)
        {
            for (int x1 = 0; x1 < 3; x1++)
            {
                int ttp = roomTypes[roomType, y1 * 3 + x1];
                level[x1 + x, y1 + y] = ttp;
            }
        }
    }

    private void createStarterRooms()
    {
        level[0, 0] = 1;
        level[0, size.y - 1] = 1;
        level[size.x - 1, 0] = 1;
        level[size.x - 1, size.y - 1] = 1;
    }

    private void spawnRooms()
    {
        for (int y1 = 0; y1 < size.y; y1++)
        {
            for (int x1 = 0; x1 < size.x; x1++)
            {
                Tiles[level[x1, y1]].transform.position = new Vector3(x1 * tileSize.x, y1 * tileSize.y, 0);
                Instantiate(Tiles[level[x1, y1]]);
            }
        }
    }

    private void createPathToArena()
    {
        for (int i = 0; i < size.x; i++)
        {
            if (level[i, i] == 0) level[i, i] = 1;
            if (level[i, size.y - 1 - i] == 0) level[i, size.y - 1 - i] = 1;
            if (i > 0 && i < size.x - 1)
            {
                if (level[i + 1, i] == 0) level[i + 1, i] = 1;
                if (level[i, size.y - i] == 0) level[i, size.y - i] = 1;
            }
        }
    }

    void Start()
    {
        createArena();

        createStarterRooms();

        for (int i = 0; i < attempts; i++)
        {
            level = replace();
        }

        fill();

        createPathToArena();

        spawnRooms();
    }
}