using UnityEngine;
using System.Collections;

public class DungeonManager : MonoBehaviour {

    public int seed = System.Environment.TickCount;
    public int nRows = 39;
    public int nCols = 39;
    public DUNGEON_LAYOUT dungeonLayout = DUNGEON_LAYOUT.SQUARE;
    public int roomMin = 3;
    public int roomMax = 9;
    public DUNGEON_ROOM_LAYOUT roomLayout = DUNGEON_ROOM_LAYOUT.PACKED;
    public DUNGEON_CORRIDOR_LAYOUT corridorLayout = DUNGEON_CORRIDOR_LAYOUT.STRAIGHT;
    public int removeDeadends = 100;
    public int addStairs = 0;
    public int tileResolution = 12;

    public GameObject stoneDoorTile;
    public GameObject stoneBossDoorTile;
    public GameObject stoneArchTile;
    public GameObject stoneStairsUpTile;
    public GameObject stoneStairsDownTile;
    public GameObject[] stoneFloorTiles;
    public GameObject[] stoneWallTiles;
    public GameObject[] corridorTiles;

    public Transform boardHolder;									//A variable to store a reference to the transform of our Board object.

    public Dungeon GenerateDungeon() {
        return new Dungeon(seed, nRows, nCols, dungeonLayout, roomMin, roomMax, roomLayout, corridorLayout, removeDeadends, addStairs);
    }

    public void BuildDungeonGOs(Dungeon d) {
        boardHolder = new GameObject("Board").transform;

        for(int r = 0; r <= d.nRows; r++) {
            for(int c = 0; c <= d.nCols; c++) {
                GameObject toInstantiate = stoneWallTiles[Random.Range(0, stoneWallTiles.Length)];

                if((d.cell[r][c] & Dungeon.ARCH) != Dungeon.NOTHING)
                    toInstantiate = stoneArchTile;
                else if((d.cell[r][c] & Dungeon.CLOSED) != Dungeon.NOTHING)
                    toInstantiate = stoneBossDoorTile;
                else if((d.cell[r][c] & Dungeon.OPEN) != Dungeon.NOTHING)
                    toInstantiate = stoneDoorTile;
                else if((d.cell[r][c] & Dungeon.ROOM) != Dungeon.NOTHING)
                    toInstantiate = stoneFloorTiles[Random.Range(0, stoneFloorTiles.Length)];
                else if((d.cell[r][c] & Dungeon.CORRIDOR) != Dungeon.NOTHING)
                    toInstantiate = corridorTiles[Random.Range(0, corridorTiles.Length)];
                else if((d.cell[r][c] & Dungeon.STAIR_UP) != Dungeon.NOTHING)
                    toInstantiate = stoneStairsUpTile;
                else if((d.cell[r][c] & Dungeon.STAIR_DN) != Dungeon.NOTHING)
                    toInstantiate = stoneStairsDownTile;
                else
                    toInstantiate = stoneWallTiles[Random.Range(0, stoneWallTiles.Length)];

                GameObject instance = Instantiate(toInstantiate, new Vector3(r, c, 0f), Quaternion.identity) as GameObject;
                instance.transform.SetParent(boardHolder);
            }
        }
    }
}
