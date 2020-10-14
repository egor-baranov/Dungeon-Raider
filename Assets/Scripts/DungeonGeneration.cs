using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Pathfinding;

[RequireComponent(typeof(Tilemap))]
public class DungeonGeneration : MonoBehaviour {
    public int[] parents = { };

    public Vector2Int size;
    public Vector2Int roomSize;
    public Vector2Int roomMargins;

    public long seedSave;
    [SerializeField] private long seed;

    [Range(0, 100)] public float connectionChance;

    private int _completed = 1;
    private readonly Stack<int> _done = new Stack<int>();

    public GameObject[] rooms;

    [SerializeField] private Tilemap tilemapFg;
    [SerializeField] private Tilemap tilemapBase;
    [SerializeField] private Tilemap tilemapBg;

    public GameObject connectorHorizontal;
    public GameObject connectorVertical;

    public Tile[] tileNames;
    public GameObject[] tileObjects;
    public Tile[] doorTiles;
    public GameObject[] doorObjects;
    public GameObject[] roomManagers;
    public GameObject roomManager;

    public GameObject aStar;

    private void Start() {
        seed = Random.Range(3853, 2143223457);
        seed *= seed;
        seedSave = seed;

        // building foreground
        for (var y = 0; y < size.y; y++) {
            for (var x = 0; x < size.x; x++) {
                Debug.Log("seed: " + seed);

                var roomType = (int) (seed % rooms.Length);
                seed /= rooms.Length;
                if (seed < 100) seed = seedSave;

                var bounds = new BoundsInt(new Vector3Int(0, 0, 0), new Vector3Int(roomSize.x, roomSize.y, 1));

                roomManagers[x + y * size.x] = Instantiate(roomManager,
                    new Vector3(x * (roomSize.x + roomMargins.x), y * (roomSize.y + roomMargins.y), 0),
                    Quaternion.identity);


                var allTiles =
                    rooms[roomType].transform.GetChild(0).GetComponent<Tilemap>().GetTilesBlock(bounds);

                for (var y1 = 0; y1 < roomSize.y; y1++) {
                    for (var x1 = 0; x1 < roomSize.x; x1++) {
                        var tile = allTiles[x1 + y1 * bounds.size.x];
                        tilemapFg.SetTile(
                            new Vector3Int(x * (roomSize.x + roomMargins.x) + x1, y * (roomSize.y + roomMargins.y) + y1,
                                0), tile);
                        for (var i = 0; i < tileNames.Length; i++) {
                            if (tile != tileNames[i]) continue;

                            Instantiate(tileObjects[i], tilemapFg.CellToWorld(new Vector3Int(
                                                            x * (roomSize.x + roomMargins.x) + x1,
                                                            y * (roomSize.y + roomMargins.y) + y1, 0)) +
                                                        new Vector3(tilemapFg.cellSize.x / 2, tilemapFg.cellSize.y / 2,
                                                            0),
                                Quaternion.identity);
                            tilemapFg.SetTile(
                                new Vector3Int(x * (roomSize.x + roomMargins.x) + x1,
                                    y * (roomSize.y + roomMargins.y) + y1, 0), null);
                        }
                    }
                }

                allTiles = rooms[roomType].transform.GetChild(1).GetComponent<Tilemap>().GetTilesBlock(bounds);
                for (var y1 = 0; y1 < roomSize.y; y1++) {
                    for (var x1 = 0; x1 < roomSize.x; x1++) {
                        TileBase tile = allTiles[x1 + y1 * bounds.size.x];
                        tilemapBase.SetTile(
                            new Vector3Int(x * (roomSize.x + roomMargins.x) + x1, y * (roomSize.y + roomMargins.y) + y1,
                                0), tile);
                    }
                }

                allTiles = rooms[roomType].transform.GetChild(2).GetComponent<Tilemap>().GetTilesBlock(bounds);
                for (var y1 = 0; y1 < roomSize.y; y1++) {
                    for (var x1 = 0; x1 < roomSize.x; x1++) {
                        var tile = allTiles[x1 + y1 * bounds.size.x];
                        tilemapBg.SetTile(
                            new Vector3Int(x * (roomSize.x + roomMargins.x) + x1, y * (roomSize.y + roomMargins.y) + y1,
                                0), tile);
                    }
                }
            }
        }

        // generating maze
        for (var i = 0; i < size.x * size.y; i++) {
            parents[i] = -1;
        }

        _done.Push(0);
        while (_completed <= size.x * size.y) {
            if (seed < 100) seed = seedSave;
            var count = 0;
            var posCase = 0;

            if (_done.Peek() + size.x < size.x * size.y && parents[_done.Peek() + size.x] == -1) {
                count++;
                posCase += 1;
            }

            posCase *= 2;
            if (_done.Peek() - size.x > 0 && parents[_done.Peek() - size.x] == -1) {
                count++;
                posCase += 1;
            }

            posCase *= 2;
            if ((_done.Peek() + 1) % size.x != 0 && parents[_done.Peek() + 1] == -1) {
                count++;
                posCase += 1;
            }

            posCase *= 2;
            if (_done.Peek() % size.x != 0 && parents[_done.Peek() - 1] == -1) {
                count++;
                posCase += 1;
            }

            if (count == 0) _done.Pop();
            else {
                var chance = (int) (seed % count);
                var chanceSave = chance;
                seed /= count;
                int move = 0;

                // Debug.Log("room: " + done.Peek() + ", came from: " + parents[done.Peek()] + ", chance: " + chance + ", posCase: " + posCase);

                while (chance > 0 || posCase % 2 == 0) {
                    chance -= posCase % 2;
                    move++;
                    posCase /= 2;
                }

                // Debug.Log("move: " + move + ", case: " + posCase + ", chance: " + chanceSave + ", current tile: " + done.Peek() + ", count: " + count);

                switch (move) {
                    case 0:
                        parents[_done.Peek() - 1] = _done.Peek();
                        _done.Push(_done.Peek() - 1);
                        _completed++;
                        break;
                    case 1:
                        parents[_done.Peek() + 1] = _done.Peek();
                        _done.Push(_done.Peek() + 1);
                        _completed++;
                        break;
                    case 2:
                        parents[_done.Peek() - size.x] = _done.Peek();
                        _done.Push(_done.Peek() - size.x);
                        _completed++;
                        break;
                    case 3:
                        parents[_done.Peek() + size.x] = _done.Peek();
                        _done.Push(_done.Peek() + size.x);
                        _completed++;
                        break;
                }
            }
        }


        for (var y = 0; y < size.y; y++) {
            for (var x = 0; x < size.x; x++) {
                // horizontal connector
                if (x != size.x - 1 && (seed % 100 < connectionChance ||
                                        parents[x + y * size.x] == x + y * size.x + 1 ||
                                        parents[x + y * size.x + 1] == x + y * size.x)) {
                    var bounds = new BoundsInt(new Vector3Int(0, 0, 0), new Vector3Int(6, 4, 1));
                    TileBase[] allTilesFg = connectorHorizontal.transform.GetChild(0).GetComponent<Tilemap>()
                            .GetTilesBlock(bounds),
                        allTilesBase = connectorHorizontal.transform.GetChild(1).GetComponent<Tilemap>()
                            .GetTilesBlock(bounds),
                        allTilesBg = connectorHorizontal.transform.GetChild(2).GetComponent<Tilemap>()
                            .GetTilesBlock(bounds);

                    for (var y1 = 0; y1 < bounds.size.y; y1++) {
                        for (var x1 = 0; x1 < bounds.size.x; x1++) {
                            TileBase tile = allTilesBase[x1 + y1 * bounds.size.x],
                                tilef = allTilesFg[x1 + y1 * bounds.size.x],
                                tileb = allTilesBg[x1 + y1 * bounds.size.x];

                            tilemapBg.SetTile(
                                new Vector3Int(x * (roomSize.x + roomMargins.x) + roomSize.x + x1 - 2,
                                    y * (roomSize.y + roomMargins.y) + (roomSize.y - bounds.size.y) / 2 + y1, 0),
                                tileb);

                            tilemapBase.SetTile(
                                new Vector3Int(x * (roomSize.x + roomMargins.x) + roomSize.x + x1 - 2,
                                    y * (roomSize.y + roomMargins.y) + (roomSize.y - bounds.size.y) / 2 + y1, 0), tile);
                            tilemapFg.SetTile(
                                new Vector3Int(x * (roomSize.x + roomMargins.x) + roomSize.x + x1 - 2,
                                    y * (roomSize.y + roomMargins.y) + (roomSize.y - bounds.size.y) / 2 + y1, 0),
                                tilef);
                        }
                    }
                }

                seed /= 13;
                if (seed < 100) seed = seedSave;

                // vertical connector
                if (y != size.y - 1 &&
                    (seed % 100 < connectionChance || parents[x + y * size.x] == x + y * size.x + size.x ||
                     parents[x + y * size.x + size.x] == x + y * size.x)) {
                    var bounds =
                        new BoundsInt(new Vector3Int(0, 0, 0), new Vector3Int(4, 6, 1));

                    TileBase[] allTilesFg = connectorVertical.transform.GetChild(0).GetComponent<Tilemap>()
                            .GetTilesBlock(bounds),
                        allTiles = connectorVertical.transform.GetChild(1).GetComponent<Tilemap>()
                            .GetTilesBlock(bounds),
                        allTilesBg = connectorVertical.transform.GetChild(2).GetComponent<Tilemap>()
                            .GetTilesBlock(bounds);

                    for (var y1 = 0; y1 < bounds.size.y; y1++) {
                        for (var x1 = 0; x1 < bounds.size.x; x1++) {
                            TileBase tile = allTiles[x1 + y1 * bounds.size.x],
                                tileFg = allTilesFg[x1 + y1 * bounds.size.x],
                                tileBg = allTilesBg[x1 + y1 * bounds.size.x];

                            tilemapBg.SetTile(
                                new Vector3Int(x * (roomSize.x + roomMargins.x) + (roomSize.x - bounds.size.x) / 2 + x1,
                                    y * (roomSize.y + roomMargins.y) + roomSize.y + y1 - 2, 0), tileBg);

                            tilemapBase.SetTile(
                                new Vector3Int(x * (roomSize.x + roomMargins.x) + (roomSize.x - bounds.size.x) / 2 + x1,
                                    y * (roomSize.y + roomMargins.y) + roomSize.y + y1 - 2, 0), tile);

                            tilemapFg.SetTile(
                                new Vector3Int(x * (roomSize.x + roomMargins.x) + (roomSize.x - bounds.size.x) / 2 + x1,
                                    y * (roomSize.y + roomMargins.y) + roomSize.y + y1 - 2, 0), tileFg);
                        }
                    }
                }

                seed /= 13;
                if (seed < 100) seed = seedSave;
            }
        }

        for (var y = 0; y < size.y; y++) {
            for (var x = 0; x < size.x; x++) {
                var pos = new Vector3Int(x * (roomSize.x + roomMargins.x),
                    y * (roomSize.y + roomMargins.y) + (roomSize.x - 4) / 2 + 1, 0);
                if (tilemapFg.GetTile(pos) == doorTiles[0]) {
                    roomManagers[x + y * size.x].GetComponent<RoomController>().doors[0] = Instantiate(doorObjects[0],
                        tilemapFg.CellToWorld(pos) + new Vector3(0.5f, 0.5f, 0), Quaternion.identity);
                    tilemapFg.SetTile(pos, null);
                }

                pos = new Vector3Int(x * (roomSize.x + roomMargins.x) + (roomSize.y - 4) / 2 + 1,
                    y * (roomSize.y + roomMargins.y), 0);
                if (tilemapFg.GetTile(pos) == doorTiles[1]) {
                    roomManagers[x + y * size.x].GetComponent<RoomController>().doors[1] = Instantiate(doorObjects[1],
                        tilemapFg.CellToWorld(pos) + new Vector3(0.5f, 0.5f, 0), Quaternion.identity);
                    tilemapFg.SetTile(pos, null);
                }

                pos = new Vector3Int(x * (roomSize.x + roomMargins.x) + (roomSize.y - 4) / 2 + 1,
                    y * (roomSize.y + roomMargins.y) + roomSize.y - 1, 0);
                if (tilemapFg.GetTile(pos) == doorTiles[2]) {
                    roomManagers[x + y * size.x].GetComponent<RoomController>().doors[2] = Instantiate(doorObjects[2],
                        tilemapFg.CellToWorld(pos) + new Vector3(0.5f, 0.5f, 0), Quaternion.identity);
                    tilemapFg.SetTile(pos, null);
                }

                pos = new Vector3Int(x * (roomSize.x + roomMargins.x) + roomSize.x - 1,
                    y * (roomSize.y + roomMargins.y) + (roomSize.x - 4) / 2 + 1, 0);
                Debug.Log(tilemapFg.GetTile(pos) + " " + pos);
                if (tilemapFg.GetTile(pos) == doorTiles[3]) {
                    roomManagers[x + y * size.x].GetComponent<RoomController>().doors[3] = Instantiate(doorObjects[3],
                        tilemapFg.CellToWorld(pos) + new Vector3(0.5f, 0.5f, 0), Quaternion.identity);
                    tilemapFg.SetTile(pos, null);
                }
            }
        }

        var newAStar = Instantiate(aStar, new Vector2(15, 16), Quaternion.identity);
        StartCoroutine(ScanWithDelay());
    }

    private static IEnumerator ScanWithDelay() {
        yield return new WaitForSeconds(0.02F);
        // this should work    
        AstarPath.active.Scan();
    }
}