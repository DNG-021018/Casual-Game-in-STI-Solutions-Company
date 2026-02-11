using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileMapController : MonoBehaviour
{
    [Header("Tilemap")]
    public Grid grid;
    public Tilemap tilemapGround;
    public Tilemap tilemapWall;

    [Header("Spawn Points")]
    public Transform spawnPoint1;
    public Transform spawnPoint2;

    [Header("Runtime Tile Data")]
    public Dictionary<Vector3Int, GameObject> tilemapGroundDict = new();
    public Dictionary<Vector3Int, GameObject> tilemapWallDict = new();
    public Dictionary<Vector3Int, TileMapPointTeleport> tileMapPointTeleport = new();

    public List<Vector3Int> tilemapGroundList = new();
    public List<Vector3Int> tilemapWallList = new();
    public List<TileMapPointTeleport> tileMapPointTeleportList = new();

    private void Start()
    {
        DrawTilemapWall();
        HideTileMapGround();
    }

    public void InitializeTileMapGround(Tilemap tileMap)
    {
        tilemapGroundDict.Clear();
        tilemapGroundList.Clear();

        tilemapGround = tileMap;
        tilemapGround.gameObject.SetActive(true);

        DrawTilemapGround(tileMap);
        StartCoroutine(SetupTeleport());
    }

    public void InitializeSpawnPoints(Transform spawnA, Transform spawnB)
    {
        spawnPoint1 = spawnA;
        spawnPoint2 = spawnB;
    }

    [ContextMenu("Setup Teleport Points")]
    public IEnumerator SetupTeleport()
    {
        yield return new WaitForSeconds(0.2f);
        tileMapPointTeleport.Clear();

        foreach (TileMapPointTeleport teleport in tileMapPointTeleportList)
        {
            // Bỏ qua nếu object null hoặc đang bị ẩn
            if (teleport == null ||
                teleport.fromObject == null || !teleport.fromObject.gameObject.activeInHierarchy ||
                teleport.toObject == null || !teleport.toObject.gameObject.activeInHierarchy)
            {
                Debug.LogWarning("Bỏ qua teleport vì from/to bị null hoặc đang bị ẩn");
                continue;
            }

            Vector3Int fromCell = tilemapGround.WorldToCell(teleport.fromObject.position);
            Vector3Int toCell = tilemapGround.WorldToCell(teleport.toObject.position);

            if (tileMapPointTeleport.TryAdd(fromCell, teleport))
            {
                Debug.Log($"✅ Đã thêm teleport từ {fromCell} → {toCell}");
            }
            else
            {
                Debug.LogWarning($"⚠️ Trùng vị trí teleport tại {fromCell}, bỏ qua.");
            }
        }
    }


    private void DrawTilemapGround(Tilemap tilemap)
    {
        foreach (Transform child in tilemap.transform)
        {
            Vector3Int cellPos = tilemap.WorldToCell(child.position);
            tilemapGroundDict[cellPos] = child.gameObject;
            tilemapGroundList.Add(cellPos);
        }
    }

    private void DrawTilemapWall()
    {
        foreach (Transform child in tilemapWall.transform)
        {
            Vector3Int cellPos = tilemapWall.WorldToCell(child.position);
            if (tilemapWallDict.TryAdd(cellPos, child.gameObject))
                tilemapWallList.Add(cellPos);
        }
    }

    private void HideTileMapGround()
    {
        GameObject[] tileMaps = GameObject.FindGameObjectsWithTag("TileMapLevel");

        foreach (GameObject obj in tileMaps)
        {
            obj.SetActive(false);
        }

        Debug.Log($"Đã ẩn {tileMaps.Length} TileMapGround có tag 'TileMapLevel'");
    }

}

[Serializable]
public class TileMapPointTeleport
{
    public Transform fromObject;
    public Transform toObject;

    public TileMapPointTeleport(Transform from, Transform to)
    {
        fromObject = from;
        toObject = to;
    }
}
