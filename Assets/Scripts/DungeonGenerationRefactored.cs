using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Tilemap))]
public class DungeonGenerationRefactored : MonoBehaviour {
    public Vector3Int mapSize, roomSize, roomMargin;

    public List<GameObject> roomPrefabs;

    public GameObject verticalConnector, horizontalConnector;

    public GameObject aStarPrefab;

    private Transform _grid;

    private List<int> _parent, _weight;

    private void Awake() {
        _grid = transform.GetChild(0);

        // init singleton SeedManager with system time
        SeedManager.Init((int) DateTime.Now.Ticks);
    }

    private void Start() {
        Process();
    }

    // Wrapper function for level generation
    private void Process() {
        SeedManager.Refresh();

        // initialization of two lists which will be used in union–find data structure
        _parent = new List<int>(mapSize.x * mapSize.y);
        _weight = new List<int>(mapSize.x * mapSize.y);

        for (var i = 0; i < mapSize.x * mapSize.y; ++i) {
            _parent.Add(i);
            _weight.Add(1);
        }

        // arrangement of rooms
        foreach (var y in Enumerable.Range(0, mapSize.y)) {
            foreach (var x in Enumerable.Range(0, mapSize.x)) {
                var roomPosition =
                    new Vector2Int(x * (roomSize.x + roomMargin.x), y * (roomSize.y + roomMargin.y));
                AddRoom(roomPrefabs[Random.Range(0, roomPrefabs.Capacity)], roomPosition);
            }
        }

        // building a minimal connected graph
        while (_weight[ParentOf(0, 0)] != mapSize.x * mapSize.y) {
            int x = Random.Range(0, mapSize.x), y = Random.Range(0, mapSize.y);

            if (x > 0 && ParentOf(x, y) != ParentOf(x - 1, y)) {
                ConnectRooms(new Vector2Int(x, y), new Vector2Int(x - 1, y));
            }

            if (y > 0 && ParentOf(x, y) != ParentOf(x, y - 1)) {
                ConnectRooms(new Vector2Int(x, y), new Vector2Int(x, y - 1));
            }
        }

        // adding random connectors between rooms
        foreach (var y in Enumerable.Range(0, mapSize.y)) {
            foreach (var x in Enumerable.Range(0, mapSize.x)) {
                if (x > 0 && Random.Range(0, 5) == 0) {
                    ConnectRooms(new Vector2Int(x, y), new Vector2Int(x - 1, y));
                }

                if (y > 0 && Random.Range(0, 5) == 0) {
                    ConnectRooms(new Vector2Int(x, y), new Vector2Int(x, y - 1));
                }
            }
        }

        Instantiate(aStarPrefab, new Vector2(
                mapSize.x * (roomSize.x + roomMargin.x) / 2,
                mapSize.y * (roomSize.y + roomMargin.y) / 2),
            Quaternion.identity);

        StartCoroutine(ScanWithDelay());
    }

    private static IEnumerator ScanWithDelay() {
        yield return new WaitForSeconds(0.02F);
        AstarPath.active.Scan();
    }

    // A function that creates selected room and fills it with random objects
    private void AddRoom(GameObject room, Vector2Int p) {
        BoundsInt
            boundsAdd = new BoundsInt(new Vector3Int(p.x, p.y, 0), roomSize),
            boundsGet = new BoundsInt(Vector3Int.zero, roomSize);

        for (var i = 0; i <= 4; ++i) {
            _grid.GetChild(i).GetComponent<Tilemap>().SetTilesBlock(boundsAdd,
                room.transform.GetChild(i).GetComponent<Tilemap>().GetTilesBlock(boundsGet));
        }

        // objects inside of room
        if (room.transform.childCount > 4) {
            foreach (Transform obj in room.transform.GetChild(4)) {
                var newObj = Instantiate(obj, GetComponent<Transform>());

                // Debug.Log($"{room.transform.GetChild(4).position.x}, {room.transform.GetChild(4).position.y}");

                newObj.transform.position = obj.transform.position +
                                            new Vector3(p.x - 1, p.y, 0);
            }
        }
    }

    // A function that creates a connector between two selected rooms
    private void ConnectRooms(Vector2Int first, Vector2Int second) {
        Assert.IsTrue(first.x == second.x || first.y == second.y);

        if (AreConnected(first, second)) return;

        if (ParentOf(first) != ParentOf(second)) {
            _weight[ParentOf(second)] += _weight[ParentOf(first)];
            _parent[ParentOf(first)] = ParentOf(second);
        }

        // connector coordinates
        int x = Mathf.Min(first.x, second.x), y = Mathf.Min(first.y, second.y);

        if (first.y == second.y) { // horizontal connector
            var hBounds = new BoundsInt(Vector3Int.zero, new Vector3Int(6, 8, 1));

            var connectorPosition = new Vector3Int(
                x * (roomSize.x + roomMargin.x) + roomSize.x - 2,
                y * (roomSize.y + roomMargin.y) + (roomSize.y - hBounds.size.y) / 2,
                0
            );

            var boundsAdd = new BoundsInt(connectorPosition, new Vector3Int(6, 8, 1));

            for (var i = 0; i < 3; ++i) {
                _grid.GetChild(i).GetComponent<Tilemap>().SetTilesBlock(boundsAdd,
                    horizontalConnector.transform.GetChild(i).GetComponent<Tilemap>().GetTilesBlock(hBounds));
            }
        }
        else { // vertical connector
            var vBounds = new BoundsInt(Vector3Int.zero, new Vector3Int(6, 6, 1));

            var connectorPosition = new Vector3Int(
                x * (roomSize.x + roomMargin.x) + (roomSize.x - vBounds.size.x) / 2,
                y * (roomSize.y + roomMargin.y) + roomSize.y - 2,
                0
            );

            var boundsAdd = new BoundsInt(connectorPosition, new Vector3Int(6, 6, 1));

            for (var i = 0; i < 3; ++i) {
                _grid.GetChild(i).GetComponent<Tilemap>().SetTilesBlock(boundsAdd,
                    verticalConnector.transform.GetChild(i).GetComponent<Tilemap>().GetTilesBlock(vBounds));
            }
        }
    }

    private int ParentOf(int x, int y) {
        if (x < 0 || y < 0) return -1;

        var n = FormatCoordinates(x, y);

        while (_parent[n] != n) n = _parent[n];
        return n;
    }

    private int ParentOf(Vector2Int a) => ParentOf(a.x, a.y);

    // This function only works for rooms that are connected during the construction of a minimal connected graph
    private bool AreConnected(Vector2Int first, Vector2Int second) {
        int a = FormatCoordinates(first), b = FormatCoordinates(second);
        return _parent[a] == b || _parent[b] == a;
    }

    // Coordinates of room to it's index
    private int FormatCoordinates(Vector2Int a) => FormatCoordinates(a.x, a.y);

    private int FormatCoordinates(int x, int y) => x + y * mapSize.x;
}